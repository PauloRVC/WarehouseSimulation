using Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    public class SystemStats
    {
        public double AvgTimeInSystem { get; private set; }
        public Dictionary<ProcessType, double> AvgTimeRecirc { get; private set; }
        public double AvgTimeInQueue { get; private set; }
        public double AvgTimeInProcess { get; private set; }
        public Dictionary<ProcessType, double> AvgNumTimesRecirc { get; private set; }
        public Dictionary<ProcessType, int> ArrivalCount { get; set; }
        public int ExitCount { get; set; }

        public SystemStats(DateTime day)
        {
            var data = new WarehouseData();

            ArrivalCount = CountArrivals(data.FirstArrivals(data.Scanner901, day), data);
            ExitCount = data.GetNPuts(day);

            AvgTimeInSystem = CalcAvgTimeInSystem(data, day);
            AvgTimeInProcess = CalcAvgTimeInProcess(data, day);
            AvgTimeInQueue = CalcAvgTimeInQueue(data, day);

            var recircGroups = data.GetRecircGroups(day);

            var putWallRecircGroups = recircGroups.Where(x => x.Item2.Select(y => y.Item1).Contains(data.P06.LocationID));
            var nonPutwallRecircGroups = recircGroups.Where(x => !x.Item2.Select(y => y.Item1).Contains(data.P06.LocationID));

            AvgNumTimesRecirc = new Dictionary<ProcessType, double>();
            AvgNumTimesRecirc.Add(ProcessType.Putwall, putWallRecircGroups.Select(x => Math.Max(0, x.Item2.Where(y => y.Item1 == data.P06.LocationID).Count())).Average());
            AvgNumTimesRecirc.Add(ProcessType.NonPutwall, nonPutwallRecircGroups.Select(x => Math.Max(0, x.Item2.Count)).Average());

            AvgTimeRecirc = new Dictionary<ProcessType, double>();
            AvgTimeRecirc.Add(ProcessType.Putwall, putWallRecircGroups.Select(x => 
            sumDateDiffs( x.Item2.Where(y => y.Item1 == data.P06.LocationID).Select(y => y.Item2).ToList() )).Average());
            AvgTimeRecirc.Add(ProcessType.NonPutwall, nonPutwallRecircGroups.Select(x => sumDateDiffs(x.Item2.Select(y => y.Item2).ToList())).Average());
        }
        public SystemStats(DateTime day, double anomolyLimit)
        {
            var data = new WarehouseData();

            ArrivalCount = CountArrivals(data.FirstArrivals(data.Scanner901, day), data);
            ExitCount = data.GetNPuts(day);

            AvgTimeInSystem = CalcAvgTimeInSystem(data, day);
            AvgTimeInProcess = CalcAvgTimeInProcess(data, day);
            AvgTimeInQueue = CalcAvgTimeInQueue(data, day);

            var recircGroups = data.GetRecircGroups(day);

            var putWallRecircGroups = recircGroups.Where(x => x.Item2.Select(y => y.Item1).Contains(data.P06.LocationID));
            var nonPutwallRecircGroups = recircGroups.Where(x => !x.Item2.Select(y => y.Item1).Contains(data.P06.LocationID));

            AvgNumTimesRecirc = new Dictionary<ProcessType, double>();
            AvgNumTimesRecirc.Add(ProcessType.Putwall, putWallRecircGroups.Select(x => Math.Max(0, x.Item2.Where(y => y.Item1 == data.P06.LocationID).Count())).Average());
            AvgNumTimesRecirc.Add(ProcessType.NonPutwall, nonPutwallRecircGroups.Select(x => Math.Max(0, x.Item2.Count)).Average());

            AvgTimeRecirc = new Dictionary<ProcessType, double>();
            AvgTimeRecirc.Add(ProcessType.Putwall, putWallRecircGroups.Select(x =>
                                    sumDateDiffs(x.Item2.Where(y => y.Item1 == data.P06.LocationID).Select(y => y.Item2).ToList(), anomolyLimit)).Average());
            AvgTimeRecirc.Add(ProcessType.NonPutwall, nonPutwallRecircGroups.Select(x => sumDateDiffs(x.Item2.Select(y => y.Item2).ToList(), anomolyLimit)).Average());
        }
        private double sumDateDiffs(List<DateTime> times)
        {
            double sum = 0;
            for(int i = 1; i < times.Count; i++)
            {
                sum += times[i].Subtract(times[i - 1]).TotalSeconds;
            }
            return sum;
        }
        private double sumDateDiffs(List<DateTime> times, double anomolyLimit)
        {
            double sum = 0;
            for (int i = 1; i < times.Count; i++)
            {
                if(times[i].Subtract(times[i - 1]).TotalSeconds <= anomolyLimit)
                    sum += times[i].Subtract(times[i - 1]).TotalSeconds;
            }
            return sum;
        }
        private Dictionary<ProcessType, int> CountArrivals(List<BatchScan> arrivals, WarehouseData data)
        {
            var results = new Dictionary<ProcessType, int>();

            int nPutwallBatches = arrivals.Where(x => x.IntendedDestinationID == data.P06.LocationID).Count();
            int nNonPutwallBatches = arrivals.Where(x => x.IntendedDestinationID != data.P06.LocationID).Count();

            results.Add(ProcessType.Putwall, nPutwallBatches);
            results.Add(ProcessType.NonPutwall, nNonPutwallBatches);

            return results;
        }
        private double CalcAvgTimeInSystem(WarehouseData data, DateTime day)
        {
            var arrivals = data.FirstArrivals(data.Scanner901, day);
            var putTimes = data.LastPutTimes(day, arrivals.Select(x => x.BatchID).Distinct().ToList());

            return putTimes.Where(x => arrivals.Select(y => y.BatchID).Contains(x.Item1)).
                            Select(x => x.Item2.Subtract(arrivals.Where(y => y.BatchID == x.Item1).FirstOrDefault().Timestamp).TotalSeconds).Average();
        }
        private double CalcAvgTimeInProcess(WarehouseData data, DateTime day)
        {
            //Get the last batchscan at 901
            var todaysArrivals = data.LastArrivals(data.Scanner901, day);

            //Get the time the first item in batch was put
            var firstPutTimes = data.FirstPutTimes(day, todaysArrivals.Select(x => x.BatchID).ToList());

            //Get the time the last item in the batch was put
            var lastPutTimes = data.LastPutTimes(day, todaysArrivals.Select(x => x.BatchID).ToList());

            //FIX THIS GARBAGE WHEN YOU HAVE TIME
            var validBatches = firstPutTimes.Select(x => x.Item1).Where(x => lastPutTimes.Select(y => y.Item1).Contains(x));

            var times = validBatches.Select(x => lastPutTimes.Where(y => y.Item1 == x).
                                                FirstOrDefault().Item2.Subtract(firstPutTimes.Where(z => z.Item1 == x).
                                                FirstOrDefault().Item2).TotalSeconds);
            return times.Average();
        }
        private double CalcAvgTimeInQueue(WarehouseData data, DateTime day)
        {
            //Get the last batchscan at 901
            var lastArrivals = data.LastArrivals(data.Scanner901, day);

            //Get the time the first item in batch was put
            var firstPutTimes = data.FirstPutTimes(day, lastArrivals.Select(x => x.BatchID).ToList());

            return firstPutTimes.Where(x => lastArrivals.Select(y => y.BatchID).Contains(x.Item1)).
                                        Select(x => x.Item2.Subtract(lastArrivals.Where(y => y.BatchID == x.Item1).FirstOrDefault().Timestamp).TotalSeconds).Average();
        }
        
    }
}
