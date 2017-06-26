using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infrastructure;
using Infrastructure.Models;
using SimulationObjects.SimBlocks;

namespace SimulationObjects.Distributions
{
    public class RealDistributionBuilder : IDistributionBuilder
    {
        private WarehouseData data = new WarehouseData();

        private ILogger logger;

        private Simulation Simulation;
        public ILogger Logger
        {
            get { return logger; }
            set
            {
                logger = value;
                data.Logger = value;
            }
        } 

        public RealDistributionBuilder(Simulation simulation)
        {
            logger = new NullLogger();
            Simulation = simulation;
        }
        public IDistribution<int> BuildArrivalDist(List<DateTime> selectedDays)
        {

            var interArrivalList = new List<List<int>>();

            var scanner901 = data.Scanner901;

            //For each day, find the ordered list of the time first arrival of each batch at scanner 901
            int j = 1;
            foreach(DateTime day in selectedDays)
            {
                var todaysArrivals = data.FirstArrivals(scanner901, day).Select(x => x.Timestamp).OrderBy(x => x).ToList();

                var todaysInterArrivalTimes = new List<int>();

                for(int i = 1; i < todaysArrivals.Count; i++)
                {
                    todaysInterArrivalTimes.Add((int)todaysArrivals[i].Subtract(todaysArrivals[i - 1]).TotalSeconds);
                }

                interArrivalList.Add(todaysInterArrivalTimes);

                Logger.LogDistribution("ArrivalDistDay" + j, todaysInterArrivalTimes);

                j++;
            }

            //var allObservations = interArrivalList.SelectMany(x => x).Where(x => x < 70);

            var allObservations = interArrivalList.SelectMany(x => x);

            if (allObservations.Any(x => x < 0))
                throw new InvalidOperationException();

            int obsCount = allObservations.Count();

            var probs = allObservations.GroupBy(x => x).Select(x => new Tuple<double,int>((double)x.Count() / obsCount,  x.Key)).ToList();

            if (probs.Select(x => x.Item1).Sum() < 0.99)
                throw new InvalidOperationException();

            var arrivalDist = new EmpiricalDist(probs);

            return arrivalDist;

        }
        public IDistribution<int> BuildArrivalDist(List<DateTime> selectedDays, int anomolyLimit)
        {

            var interArrivalList = new List<List<int>>();

            var scanner901 = data.Scanner901;

            //For each day, find the ordered list of the time first arrival of each batch at scanner 901
            int j = 1;
            foreach (DateTime day in selectedDays)
            {
                var todaysArrivals = data.FirstArrivals(scanner901, day).Select(x => x.Timestamp).OrderBy(x => x).ToList();

                var todaysInterArrivalTimes = new List<int>();

                for (int i = 1; i < todaysArrivals.Count; i++)
                {
                    todaysInterArrivalTimes.Add((int)todaysArrivals[i].Subtract(todaysArrivals[i - 1]).TotalSeconds);
                }

                interArrivalList.Add(todaysInterArrivalTimes);

                Logger.LogDistribution("ArrivalDistDay" + j, todaysInterArrivalTimes);

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

            return arrivalDist;

        }

        public IDistribution<IDestinationBlock> BuildDestinationDist(List<DateTime> selectedDays, 
                                                            Dictionary<int, IDestinationBlock> processBlocks, 
                                                            IDestinationBlock nextDestination)
        {
            var destinationList = new List<List<Location>>();

            var scanner901 = data.Scanner901;

            int j = 1;
            foreach (DateTime day in selectedDays)
            {
                var todaysArrivals = data.FirstArrivals(scanner901, day).Select(x => x.IntendedDestination).ToList();
                
                destinationList.Add(todaysArrivals);

                Logger.LogDistribution("DestinationDist" + j, todaysArrivals);

                j++;
            }

            var allObservations = destinationList.SelectMany(x => x);

            int obsCount = allObservations.Count();

            var distinctDests = allObservations.Distinct();
            foreach(Location l in distinctDests)
            {
                if (!processBlocks.ContainsKey(l.LocationID))
                {
                    processBlocks.Add(l.LocationID, new NonPutwallLane(Simulation, nextDestination));
                }
            }

            var probs = allObservations.GroupBy(x => x.LocationID).Select(x => new Tuple<double, IDestinationBlock>((double)x.Count() / obsCount, processBlocks[x.Key])).ToList();

            if (probs.Select(x => x.Item1).Sum() < 0.99)
                throw new InvalidOperationException();

            var destinationDist = new DestinationDist(probs);

            return destinationDist;
        }

        public IDistribution<int> BuildProcessTimeDist(List<DateTime> selectedDays, Process process)
        {
            var pTimeList = new List<List<int>>();

            var scanner901 = data.Scanner901;

            int j = 1;

            foreach (DateTime day in selectedDays)
            {
                //Get the last batchscan at 901
                var todaysArrivals = data.LastArrivals(scanner901, day);

                //Get the time the first item in batch was put
                var firstPutTimes = data.FirstPutTimes(day, todaysArrivals.Select(x => x.BatchID).ToList());

                //Get the time the last item in the batch was put
                var lastPutTimes = data.LastPutTimes(day, todaysArrivals.Select(x => x.BatchID).ToList());

                //FIX THIS GARBAGE WHEN YOU HAVE TIME
                var validBatches = firstPutTimes.Select(x => x.Item1).Where(x => lastPutTimes.Select(y => y.Item1).Contains(x));

                var times = validBatches.Select(x => (int)lastPutTimes.Where(y => y.Item1 == x).
                                                    FirstOrDefault().Item2.Subtract(firstPutTimes.Where(z => z.Item1 == x).
                                                    FirstOrDefault().Item2).TotalSeconds).ToList();

                //todaysArrivals = todaysArrivals.Where(y => lastPutTimes.Select(x => x.Item1).Contains(y.BatchID)).ToList();

                //var timePairs = todaysArrivals.Select(x => new { arrivalTime = x.Timestamp, lastPutTime = lastPutTimes.Where(y => y.Item1 == x.BatchID).First().Item2 });

                //var processTimes = timePairs.Select(x => (int)x.lastPutTime.Subtract(x.arrivalTime).TotalSeconds).ToList();

                //pTimeList.Add(processTimes);
                pTimeList.Add(times);

                //Logger.LogDistribution("ProcessTimeDist" + j, processTimes);
                Logger.LogDistribution("ProcessTimeDist" + j, times);

                j++;
            }

            var allObservations = pTimeList.SelectMany(x => x).ToList();

            if (allObservations.Any(x => x < 0))
            {
                int failCount = allObservations.Where(x => x < 0).Count();
                if(failCount == 1)
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

        public IDistribution<int> BuildRecircTimeDist(List<DateTime> selectedDays, int anomolyLimit)
        {
            List<List<int>> sTimeList = new List<List<int>>();

            int j = 1;

            foreach(DateTime d in selectedDays)
            {
                var obs = data.GetRecircTimes(d);
                sTimeList.Add(obs);

                logger.LogDistribution("RecircTimeDist" + j, obs);

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
        public IDistribution<int> BuildRecircTimeDist(List<DateTime> selectedDays)
        {
            List<List<int>> sTimeList = new List<List<int>>();

            int j = 1;

            foreach (DateTime d in selectedDays)
            {
                var obs = data.GetRecircTimes(d);
                sTimeList.Add(obs);

                logger.LogDistribution("RecircTimeDist" + j, obs);

                j++;
            }

            var allObservations = sTimeList.SelectMany(x => x).ToList();

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
        public Dictionary<int, int> GetPutsPerHour(DateTime day)
        {
            var pph = data.GetPutsPerHour(day);
            
            logger.LogPutsPerHour("PutsPerHour", pph);
            
            return pph;
        }
    }
}
