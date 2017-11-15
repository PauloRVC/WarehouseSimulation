using Infrastructure;
using Infrastructure.Models;
using SimulationObjects.SimBlocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulationObjects.Distributions
{
    public class NewDistBuilder
    {
        private WarehouseData data = new WarehouseData();

        private ILogger logger;

        public ILogger Logger
        {
            get { return logger; }
            set
            {
                logger = value;
                data.Logger = value;
            }
        }

        public NewDistBuilder()
        {
            Logger = new NullLogger();
        }
        public List<IDistribution<int>>BuildProcessTimeDists(List<DateTime> selectedDays, int anomolyLimit, List<Tuple<TimeSpan, TimeSpan>> periods)
        {
            var pTimeList = new List<List<int>>();

            var scanner901 = data.Scanner901;

            var distributions = new List<IDistribution<int>>();

            for (int k = 0; k < periods.Count; k++)
            {
                //For each day, find the ordered list of the time first arrival of each batch at scanner 901
                int j = 1;
                foreach (DateTime day in selectedDays)
                {
                    //Get the last batchscan at 901
                    var todaysArrivals = data.LastArrivals(scanner901, day);

                    //Get the time the first item in batch was put
                    var firstPutTimesDict = data.FirstPutTimesDict(day, todaysArrivals.Select(x => x.BatchID).ToList(), periods[k]);

                    //Get the time the last item in the batch was put
                    var lastPutTimesDict = data.LastPutTimesDict(day, todaysArrivals.Select(x => x.BatchID).ToList(), periods[k]);

                    //Get the batches that exist in both
                    var validBatches = firstPutTimesDict.Keys.Intersect(lastPutTimesDict.Keys);

                    var times = validBatches.Select(x => (int)lastPutTimesDict[x].Subtract(firstPutTimesDict[x]).TotalSeconds).ToList();

                    pTimeList.Add(times);

                    Logger.LogDistribution("ProcessTimeDist" + j + "_" + k, times);

                    j++;
                }

                //var allObservations = interArrivalList.SelectMany(x => x).Where(x => x < 70);

                var allObservations = pTimeList.SelectMany(x => x).Where(x => x < anomolyLimit);

                if (allObservations.Any(x => x < 0))
                    throw new InvalidOperationException();

                int obsCount = allObservations.Count();

                var probs = allObservations.GroupBy(x => x).Select(x => new Tuple<double, int>((double)x.Count() / obsCount, x.Key)).ToList();

                if (probs.Count == 0)
                {
                    probs.Add(new Tuple<double, int>(1, (int)periods[k].Item2.Subtract(periods[k].Item1).TotalSeconds));
                }
                if (probs.Select(x => x.Item1).Sum() < 0.99)
                    throw new InvalidOperationException();

                var arrivalDist = new EmpiricalDist(probs);

                distributions.Add(arrivalDist);
            }


            return distributions;
        }
        public IndependentInterarrivalBatchSizeDist BuildInterarrivalBatchSizeDist(List<DateTime> selectedDays, 
                                                                                   int anomolyLimit,
                                                                                   Tuple<TimeSpan, TimeSpan> interval)
        {
            var interarrivalList = new List<int>();
            var batchSizeList = new List<int>();

            var scanner901 = data.Scanner901;

            foreach(DateTime day in selectedDays)
            {
                var todaysArrivals = data.FirstArrivals(scanner901, day).
                                                Select(x => x.Timestamp).
                                                Where(x => x.TimeOfDay >= interval.Item1 && x.TimeOfDay <= interval.Item2).
                                                OrderBy(x => x).ToList();

                var todaysInterArrivalTimes = new List<int>();
                
                for (int i = 1; i < todaysArrivals.Count; i++)
                {
                    todaysInterArrivalTimes.Add((int)todaysArrivals[i].Subtract(todaysArrivals[i - 1]).TotalSeconds);
                }

                int j = 0;
                while (j < todaysInterArrivalTimes.Count)
                {
                    if(todaysInterArrivalTimes[j] == 0)
                    {
                        throw new InvalidOperationException();
                    }
                    else
                    {
                        interarrivalList.Add(todaysInterArrivalTimes[j]);

                        int k = j + 1;
                        int batchSize = 1;
                        while(k < todaysInterArrivalTimes.Count && todaysInterArrivalTimes[k] == 0)
                        {
                            batchSize++;
                            k++;
                        }

                        batchSizeList.Add(batchSize);

                        j = k;
                    }
                }
            }

            var allArrivalObservations = interarrivalList.Where(x => x < anomolyLimit);
            var allBatchSizeObservations = batchSizeList;

            if (allArrivalObservations.Any(x => x <= 0))
                throw new InvalidOperationException();
            if (allBatchSizeObservations.Any(x => x <= 0))
                throw new InvalidOperationException();

            int arrivalObsCount = allArrivalObservations.Count();
            int batchSizeObsCount = allBatchSizeObservations.Count();

            var arrivalProbs = allArrivalObservations.GroupBy(x => x).Select(x => new Tuple<double, int>((double)x.Count() / arrivalObsCount, x.Key)).ToList();
            var batchSizeProbs = allBatchSizeObservations.GroupBy(x => x).Select(x => new Tuple<double, int>((double)x.Count() / batchSizeObsCount, x.Key)).ToList();

            if (arrivalProbs.Count == 0 || batchSizeProbs.Count == 0)
            {
                throw new InvalidOperationException();
            }

            if (arrivalProbs.Select(x => x.Item1).Sum() < 0.99 || batchSizeProbs.Select(x => x.Item1).Sum() < 0.99)
                throw new InvalidOperationException();

            var arrivalDist = new EmpiricalDist(arrivalProbs);
            var batchSizeDist = new EmpiricalDist(batchSizeProbs);

            return new IndependentInterarrivalBatchSizeDist(arrivalDist, batchSizeDist);
        }
        public ConditionalInterarrivalBatchSizeDist BuildConditionalInterarrivalBatchSizeDist(List<DateTime> selectedDays,
                                                                                   int anomolyLimit,
                                                                                   Tuple<TimeSpan, TimeSpan> interval)
        {
            var interarrivalList = new List<int>();
            var batchSizeDict = new Dictionary<int, List<int>>();

            var scanner901 = data.Scanner901;

            foreach (DateTime day in selectedDays)
            {
                var todaysArrivals = data.FirstArrivals(scanner901, day).
                                                Select(x => x.Timestamp).
                                                Where(x => x.TimeOfDay >= interval.Item1 && x.TimeOfDay <= interval.Item2).
                                                OrderBy(x => x).ToList();

                var todaysInterArrivalTimes = new List<int>();

                for (int i = 1; i < todaysArrivals.Count; i++)
                {
                    todaysInterArrivalTimes.Add((int)todaysArrivals[i].Subtract(todaysArrivals[i - 1]).TotalSeconds);
                }

                int j = 0;
                while (j < todaysInterArrivalTimes.Count)
                {
                    if (todaysInterArrivalTimes[j] == 0)
                    {
                        throw new InvalidOperationException();
                    }
                    else
                    {
                        interarrivalList.Add(todaysInterArrivalTimes[j]);

                        int k = j + 1;
                        int batchSize = 1; 
                        while (k < todaysInterArrivalTimes.Count && todaysInterArrivalTimes[k] == 0)
                        {
                            batchSize++;
                            k++;
                        }

                        if (batchSizeDict.ContainsKey(todaysInterArrivalTimes[j]))
                        {
                            batchSizeDict[todaysInterArrivalTimes[j]].Add(batchSize);
                        }
                        else
                        {
                            batchSizeDict.Add(todaysInterArrivalTimes[j], new List<int>() { batchSize });
                        }

                        j = k;
                    }
                }
            }

            var allArrivalObservations = interarrivalList.Where(x => x < anomolyLimit);
            

            if (allArrivalObservations.Any(x => x <= 0))
                throw new InvalidOperationException();

            int arrivalObsCount = allArrivalObservations.Count();

            var arrivalProbs = allArrivalObservations.GroupBy(x => x).Select(x => new Tuple<double, int>((double)x.Count() / arrivalObsCount, x.Key)).ToList();

            if (arrivalProbs.Count == 0)
            {
                throw new InvalidOperationException();
            }

            if (arrivalProbs.Select(x => x.Item1).Sum() < 0.99)
                throw new InvalidOperationException();

            var arrivalDist = new EmpiricalDist(arrivalProbs);

            var conditionalBatchSizeDists = new Dictionary<int, IDistribution<int>>();
            
            foreach(int interarrivalT in batchSizeDict.Keys)
            {
                var conditionalBatchSizes = batchSizeDict[interarrivalT];

                if(conditionalBatchSizes.Any(x => x <= 0))
                {
                    throw new InvalidOperationException();
                }

                int obsCount = conditionalBatchSizes.Count;

                var conditionalBatchSizeProbs = conditionalBatchSizes.GroupBy(x => x).Select(x => new Tuple<double, int>((double)x.Count() / obsCount, x.Key)).ToList();

                if (conditionalBatchSizeProbs.Count == 0)
                {
                    throw new InvalidOperationException();
                }

                if (conditionalBatchSizeProbs.Select(x => x.Item1).Sum() < 0.99)
                    throw new InvalidOperationException();

                var conditionalBatchSizeDist = new EmpiricalDist(conditionalBatchSizeProbs);

                conditionalBatchSizeDists.Add(interarrivalT, conditionalBatchSizeDist);
            }

            return new ConditionalInterarrivalBatchSizeDist(arrivalDist, conditionalBatchSizeDists);
        }
        public List<IDistribution<int>> BuildArrivalDist(List<DateTime> selectedDays, int anomolyLimit, List<Tuple<TimeSpan, TimeSpan>> periods)
        {

            var interArrivalList = new List<List<int>>();

            var scanner901 = data.Scanner901;

            var distributions = new List<IDistribution<int>>();

            for (int k = 0; k < periods.Count; k++)
            {
                //For each day, find the ordered list of the time first arrival of each batch at scanner 901
                int j = 1;
                foreach (DateTime day in selectedDays)
                {
                    var todaysArrivals = data.FirstArrivals(scanner901, day).
                                                Select(x => x.Timestamp).
                                                Where(x => x.TimeOfDay >= periods[k].Item1 && x.TimeOfDay <= periods[k].Item2).
                                                OrderBy(x => x).ToList();

                    var todaysInterArrivalTimes = new List<int>();

                    for (int i = 1; i < todaysArrivals.Count; i++)
                    {
                        todaysInterArrivalTimes.Add((int)todaysArrivals[i].Subtract(todaysArrivals[i - 1]).TotalSeconds);
                    }

                    interArrivalList.Add(todaysInterArrivalTimes);

                    //Logger.LogDistribution("ArrivalDistDay." + k + "." + j, todaysInterArrivalTimes);

                    j++;
                }

                //var allObservations = interArrivalList.SelectMany(x => x).Where(x => x < 70);

                var allObservations = interArrivalList.SelectMany(x => x).Where(x => x < anomolyLimit);

                if (allObservations.Any(x => x < 0))
                    throw new InvalidOperationException();

                int obsCount = allObservations.Count();

                var probs = allObservations.GroupBy(x => x).Select(x => new Tuple<double, int>((double)x.Count() / obsCount, x.Key)).ToList();

                if(probs.Count == 0)
                {
                    probs.Add(new Tuple<double, int>(1, (int)periods[k].Item2.Subtract(periods[k].Item1).TotalSeconds));
                }
                if (probs.Select(x => x.Item1).Sum() < 0.99)
                    throw new InvalidOperationException();

                var arrivalDist = new EmpiricalDist(probs);

                distributions.Add(arrivalDist);
            }


            return distributions;

        }
        public IDistribution<int> BuildProcessTimeDist(List<DateTime> selectedDays, int anomolyLimit, Tuple<TimeSpan, TimeSpan> interval)
        {
            var pTimeList = new List<List<int>>();

            var scanner901 = data.Scanner901;

            int j = 1;

            foreach (DateTime day in selectedDays)
            {
                //Get the last batchscan at 901
                var todaysArrivals = data.LastArrivals(scanner901, day);

                //Get the time the first item in batch was put
                var firstPutTimesDict = data.FirstPutTimesDict(day, todaysArrivals.Select(x => x.BatchID).ToList(), interval);

                //Get the time the last item in the batch was put
                var lastPutTimesDict = data.LastPutTimesDict(day, todaysArrivals.Select(x => x.BatchID).ToList(), interval);

                //Get the batches that exist in both
                var validBatches = firstPutTimesDict.Keys.Intersect(lastPutTimesDict.Keys);

                var times = validBatches.Select(x => (int)lastPutTimesDict[x].Subtract(firstPutTimesDict[x]).TotalSeconds).ToList();

                pTimeList.Add(times);

                Logger.LogDistribution("ProcessTimeDist" + j, times);

                j++;
            }

            var allObservations = pTimeList.SelectMany(x => x).Where(x => x < anomolyLimit).ToList();

            if (allObservations.Any(x => x < 0))
            {
                int failCount = allObservations.Where(x => x < 0).Count();
                if (failCount == 1)
                {
                    allObservations.Remove(allObservations.Where(x => x < 0).First());
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }


            int obsCount = allObservations.Count();

            var probs = allObservations.GroupBy(x => x).Select(x => new Tuple<double, int>((double)x.Count() / obsCount, x.Key)).ToList();

            if (probs.Select(x => x.Item1).Sum() < 0.99)
            {
                var sum = probs.Select(x => x.Item1).Sum();
                throw new InvalidOperationException();
            }


            var processTimeDist = new EmpiricalDist(probs);

            return processTimeDist;
        }
        public IDistribution<int> BuildRecircTimeDist(List<DateTime> selectedDays, int anomolyLimit, Tuple<TimeSpan, TimeSpan> interval)
        {
            List<List<int>> sTimeList = new List<List<int>>();

            int j = 1;

            foreach (DateTime d in selectedDays)
            {
                var obs = data.GetRecircGroups(d, interval).Where(x => x.Item2.Select(y => y.Item1).Contains(data.P06.LocationID)).Select(x =>
                                    CalcTimeDiffs(x.Item2.Where(y => y.Item1 == data.P06.LocationID).Select(y => y.Item2).ToList(), anomolyLimit));
                sTimeList.Add(obs.SelectMany(x => x).ToList());

                logger.LogDistribution("RecircTimeDist" + j, obs.SelectMany(x => x).ToList());

                j++;
            }

            var allObservations = sTimeList.SelectMany(x => x).Where(x => x < anomolyLimit).ToList();

            if (allObservations.Any(x => x < 0))
            {
                int failCount = allObservations.Where(x => x < 0).Count();
                if (failCount == 1)
                {
                    allObservations.Remove(allObservations.Where(x => x < 0).First());
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }

            int obsCount = allObservations.Count();

            var probs = allObservations.GroupBy(x => x).Select(x => new Tuple<double, int>((double)x.Count() / obsCount, x.Key)).ToList();

            if (probs.Select(x => x.Item1).Sum() < 0.99)
            {
                var sum = probs.Select(x => x.Item1).Sum();
                throw new InvalidOperationException();
            }


            var recircTimeDist = new EmpiricalDist(probs);

            return recircTimeDist;

            //Faking recirc time with constant 30s
            //return new EmpiricalDist(new List<Tuple<double, int>>() { new Tuple<double, int>(1, 30) });
        }

        public IDistribution<int> BuildTimeInQueueDistribution(List<DateTime> selectedDays, int anomolyLimit, Tuple<TimeSpan, TimeSpan> interval)
        {
            List<List<int>> sTimeList = new List<List<int>>();

            int j = 1;

            foreach (DateTime d in selectedDays)
            {
                var obs = data.GetTimeInQueue(d, interval);
                sTimeList.Add(obs);

                logger.LogDistribution("TimeInQueueDist" + j, obs);

                j++;
            }

            var allObservations = sTimeList.SelectMany(x => x).Where(x => x < anomolyLimit).ToList();

            if (allObservations.Any(x => x < 0))
            {
                int failCount = allObservations.Where(x => x < 0).Count();
                if (failCount == 1)
                {
                    allObservations.Remove(allObservations.Where(x => x < 0).First());
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }

            int obsCount = allObservations.Count();

            var probs = allObservations.GroupBy(x => x).Select(x => new Tuple<double, int>((double)x.Count() / obsCount, x.Key)).ToList();

            if (probs.Select(x => x.Item1).Sum() < 0.99)
            {
                var sum = probs.Select(x => x.Item1).Sum();
                throw new InvalidOperationException();
            }


            var qTimeDist = new EmpiricalDist(probs);

            return qTimeDist;
        }

        public LocationDist BuildDestinationDist(List<DateTime> selectedDays,
                                                            Tuple<TimeSpan, TimeSpan> interval)
        {
            var destinationList = new List<List<Location>>();

            var locations = new Dictionary<int, Location>();

            var scanner901 = data.Scanner901;

            int j = 1;
            foreach (DateTime day in selectedDays)
            {
                var todaysArrivals = data.FirstArrivals(scanner901, day, interval).Select(x => x.IntendedDestination).ToList();

                destinationList.Add(todaysArrivals);

                Logger.LogDistribution("DestinationDist" + j, todaysArrivals);

                j++;
            }

            var allObservations = destinationList.SelectMany(x => x);

            int obsCount = allObservations.Count();

            var distinctDests = allObservations.Distinct();
            foreach (Location l in distinctDests)
            {
                locations.Add(l.LocationID, l);
            }

            var probs = allObservations.GroupBy(x => x.LocationID).Select(x => new Tuple<double, Location>((double)x.Count() / obsCount, locations[x.Key])).ToList();

            if (probs.Select(x => x.Item1).Sum() < 0.99)
                throw new InvalidOperationException();

            var destinationDist = new LocationDist(probs);

            return destinationDist;
        }
        
        public Dictionary<int, int> GetPutsPerX(DateTime day, int x)
        {
            var ppx = data.GetPutsPerZMins(day, x);

            logger.LogPutsPerHour("PutsPerX", ppx);

            return ppx;
        }

        private List<int> CalcTimeDiffs(List<DateTime> times, double anomolyLimit)
        {
            var t = new List<int>();

            for (int i = 1; i < times.Count; i++)
            {
                if (times[i].Subtract(times[i - 1]).TotalSeconds <= anomolyLimit)
                    t.Add((int)times[i].Subtract(times[i - 1]).TotalSeconds);
            }
            return t;
        }


    }
}
