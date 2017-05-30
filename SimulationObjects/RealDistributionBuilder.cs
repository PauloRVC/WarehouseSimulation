using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infrastructure;
using Infrastructure.Models;

namespace SimulationObjects
{
    public class RealDistributionBuilder : IDistributionBuilder
    {
        private WarehouseData data = new WarehouseData();
        public IDistribution<int> BuildArrivalDist(List<DateTime> selectedDays)
        {

            var interArrivalList = new List<List<int>>();

            var scanner901 = data.Scanner901;

            //For each day, find the ordered list of the time first arrival of each batch at scanner 901
            foreach(DateTime day in selectedDays)
            {
                var todaysArrivals = data.FirstArrivals(scanner901, day).Select(x => x.Timestamp).OrderBy(x => x).ToList();

                var todaysInterArrivalTimes = new List<int>();

                for(int i = 1; i < todaysArrivals.Count; i++)
                {
                    todaysInterArrivalTimes.Add((int)todaysArrivals[i].Subtract(todaysArrivals[i - 1]).TotalSeconds);
                }

                interArrivalList.Add(todaysInterArrivalTimes);
            }

            var allObservations = interArrivalList.SelectMany(x => x);

            if(allObservations.Any(x => x < 0))
                throw new InvalidOperationException();

            int obsCount = allObservations.Count();

            var probs = allObservations.GroupBy(x => x).Select(x => new Tuple<double,int>((double)x.Count() / obsCount,  x.Key)).ToList();

            if (probs.Select(x => x.Item1).Sum() < 0.99)
                throw new InvalidOperationException();

            var arrivalDist = new EmpiricalDist(probs);

            return arrivalDist;

        }

        public IDistribution<IProcessBlock> BuildDestinationDist(List<DateTime> selectedDays, Dictionary<Location, IProcessBlock> processBlocks)
        {
            var destinationList = new List<List<Location>>();

            var scanner901 = data.Scanner901;

            foreach (DateTime day in selectedDays)
            {
                var todaysArrivals = data.FirstArrivals(scanner901, day).Select(x => x.IntendedDestination).ToList();
                
                destinationList.Add(todaysArrivals);
            }

            var allObservations = destinationList.SelectMany(x => x);

            int obsCount = allObservations.Count();

            var distinctDests = allObservations.Distinct();
            foreach(Location l in distinctDests)
            {
                if (!processBlocks.ContainsKey(l))
                {
                    processBlocks.Add(l, new NonPutwallLane());
                }
            }

            var probs = allObservations.GroupBy(x => x).Select(x => new Tuple<double, IProcessBlock>((double)x.Count() / obsCount, processBlocks[x.Key])).ToList();

            if (probs.Select(x => x.Item1).Sum() < 0.99)
                throw new InvalidOperationException();

            var destinationDist = new DestinationDist(probs);

            return destinationDist;
        }

        public IDistribution<int> BuildProcessTimeDist(List<DateTime> selectedDays, Process process)
        {
            var pTimeList = new List<List<int>>();

            var scanner901 = data.Scanner901;

            foreach (DateTime day in selectedDays)
            {
                //Get the last batchscan at 901
                var todaysArrivals = data.LastArrivals(scanner901, day);

                //Get the time the last item in the batch was put
                var putTimes = data.LastPutTimes(day, todaysArrivals.Select(x => x.BatchID).ToList());

                todaysArrivals = todaysArrivals.Where(y => putTimes.Select(x => x.Item1).Contains(y.BatchID)).ToList();

                var timePairs = todaysArrivals.Select(x => new { arrivalTime = x.Timestamp, lastPutTime = putTimes.Where(y => y.Item1 == x.BatchID).First().Item2 });

                var processTimes = timePairs.Select(x => (int)x.lastPutTime.Subtract(x.arrivalTime).TotalSeconds).ToList();

                pTimeList.Add(processTimes);
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

        public IDistribution<int> BuildRecircTimeDist(List<DateTime> selectedDays)
        {
            //Faking recirc time with constant 30s
            return new EmpiricalDist(new List<Tuple<double, int>>() { new Tuple<double, int>(1, 30) });
        }
    }
}
