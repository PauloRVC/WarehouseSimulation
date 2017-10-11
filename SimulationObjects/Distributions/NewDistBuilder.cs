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

                    Logger.LogDistribution("ArrivalDistDay." + k + "." + j, todaysInterArrivalTimes);

                    j++;
                }

                //var allObservations = interArrivalList.SelectMany(x => x).Where(x => x < 70);

                var allObservations = interArrivalList.SelectMany(x => x).Where(x => x < anomolyLimit);

                if (allObservations.Any(x => x < 0))
                    throw new InvalidOperationException();

                int obsCount = allObservations.Count();

                var probs = allObservations.GroupBy(x => x).Select(x => new Tuple<double, int>((double)x.Count() / obsCount, x.Key)).ToList();

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
