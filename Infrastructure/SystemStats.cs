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
        public Dictionary<int, int> CumulativeArrivalCount { get; set; }

        public double StdDevTimeInSystem { get; private set; }
        public Dictionary<ProcessType, double> StdDevTimeRecirc { get; private set; }
        public double StdDevTimeInQueue { get; private set; }
        public double StdDevTimeInProcess { get; private set; }
        public Dictionary<ProcessType, double> StdDevNumTimesRecirc { get; private set; }

        public ILogger Logger = new NullLogger();

        public SystemStats(DateTime day)
        {
            var data = new WarehouseData();

            ArrivalCount = CountArrivals(data.FirstArrivals(data.Scanner901, day), data);
            ExitCount = data.GetNPuts(day);

            AvgTimeInSystem = CalcAvgTimeInSystem(data, day);
            AvgTimeInProcess = CalcAvgTimeInProcess(data, day);
            AvgTimeInQueue = CalcAvgTimeInQueue(data, day);

            StdDevTimeInSystem = CalcStdDevTimeInSystem(data, day);
            StdDevTimeInProcess = CalcStdDevTimeInProcess(data, day);
            StdDevTimeInQueue = CalcStdDevTimeInQueue(data, day);

            var recircGroups = data.GetRecircGroups(day);

            var putWallRecircGroups = recircGroups.Where(x => x.Item2.Select(y => y.Item1).Contains(data.P06.LocationID));
            var nonPutwallRecircGroups = recircGroups.Where(x => !x.Item2.Select(y => y.Item1).Contains(data.P06.LocationID));

            AvgNumTimesRecirc = new Dictionary<ProcessType, double>();
            AvgNumTimesRecirc.Add(ProcessType.Putwall, putWallRecircGroups.Select(x => Math.Max(0, x.Item2.Where(y => y.Item1 == data.P06.LocationID).Count())).Average());
            AvgNumTimesRecirc.Add(ProcessType.NonPutwall, nonPutwallRecircGroups.Select(x => Math.Max(0, x.Item2.Count)).Average());

            StdDevNumTimesRecirc = new Dictionary<ProcessType, double>();
            StdDevNumTimesRecirc.Add(ProcessType.Putwall, putWallRecircGroups.Select(x => Math.Max(0, x.Item2.Where(y => y.Item1 == data.P06.LocationID).Count())).StandardDeviation());

            AvgTimeRecirc = new Dictionary<ProcessType, double>();
            AvgTimeRecirc.Add(ProcessType.Putwall, putWallRecircGroups.Select(x => 
            sumDateDiffs( x.Item2.Where(y => y.Item1 == data.P06.LocationID).Select(y => y.Item2).ToList() )).Average());
            AvgTimeRecirc.Add(ProcessType.NonPutwall, nonPutwallRecircGroups.Select(x => sumDateDiffs(x.Item2.Select(y => y.Item2).ToList())).Average());

            StdDevTimeRecirc = new Dictionary<ProcessType, double>();
            StdDevTimeRecirc.Add(ProcessType.Putwall, putWallRecircGroups.Select(x =>
            sumDateDiffs(x.Item2.Where(y => y.Item1 == data.P06.LocationID).Select(y => y.Item2).ToList())).StandardDeviation());
            

        }
        public SystemStats(DateTime day, double anomolyLimit)
        {
            var data = new WarehouseData();

            ArrivalCount = CountArrivals(data.FirstArrivals(data.Scanner901, day), data);
            ExitCount = data.GetNPuts(day);

            AvgTimeInSystem = CalcAvgTimeInSystem(data, day);
            AvgTimeInProcess = CalcAvgTimeInProcess(data, day);
            AvgTimeInQueue = CalcAvgTimeInQueue(data, day);

            StdDevTimeInSystem = CalcStdDevTimeInSystem(data, day);
            StdDevTimeInProcess = CalcStdDevTimeInProcess(data, day);
            StdDevTimeInQueue = CalcStdDevTimeInQueue(data, day);

            var recircGroups = data.GetRecircGroups(day);

            var putWallRecircGroups = recircGroups.Where(x => x.Item2.Select(y => y.Item1).Contains(data.P06.LocationID));
            var nonPutwallRecircGroups = recircGroups.Where(x => !x.Item2.Select(y => y.Item1).Contains(data.P06.LocationID));

            AvgNumTimesRecirc = new Dictionary<ProcessType, double>();
            AvgNumTimesRecirc.Add(ProcessType.Putwall, putWallRecircGroups.Select(x => Math.Max(0, x.Item2.Where(y => y.Item1 == data.P06.LocationID).Count())).Average());
            AvgNumTimesRecirc.Add(ProcessType.NonPutwall, nonPutwallRecircGroups.Select(x => Math.Max(0, x.Item2.Count)).Average());

            StdDevNumTimesRecirc = new Dictionary<ProcessType, double>();
            StdDevNumTimesRecirc.Add(ProcessType.Putwall, putWallRecircGroups.Select(x => Math.Max(0, x.Item2.Where(y => y.Item1 == data.P06.LocationID).Count())).StandardDeviation());

            AvgTimeRecirc = new Dictionary<ProcessType, double>();
            AvgTimeRecirc.Add(ProcessType.Putwall, putWallRecircGroups.Select(x =>
                                    sumDateDiffs(x.Item2.Where(y => y.Item1 == data.P06.LocationID).Select(y => y.Item2).ToList(), anomolyLimit)).Average());
            AvgTimeRecirc.Add(ProcessType.NonPutwall, nonPutwallRecircGroups.Select(x => sumDateDiffs(x.Item2.Select(y => y.Item2).ToList(), anomolyLimit)).Average());

            StdDevTimeRecirc = new Dictionary<ProcessType, double>();
            StdDevTimeRecirc.Add(ProcessType.Putwall, putWallRecircGroups.Select(x =>
            sumDateDiffs(x.Item2.Where(y => y.Item1 == data.P06.LocationID).Select(y => y.Item2).ToList())).StandardDeviation());
            
        }
        public SystemStats(DateTime day, double anomolyLimit, Tuple<TimeSpan, TimeSpan> interval)
        {
            var data = new WarehouseData();

            ArrivalCount = CountArrivals(data.FirstArrivals(data.Scanner901, day).
                Where(x => x.Timestamp.TimeOfDay >= interval.Item1 & 
                    x.Timestamp.TimeOfDay <= interval.Item2).ToList(), data);

            ExitCount = data.GetNPuts(day, interval);

            AvgTimeInSystem = CalcAvgTimeInSystem(data, day, interval);
            AvgTimeInProcess = CalcAvgTimeInProcess(data, day, interval);
            AvgTimeInQueue = CalcAvgTimeInQueue(data, day, interval);

            StdDevTimeInSystem = CalcStdDevTimeInSystem(data, day);
            StdDevTimeInProcess = CalcStdDevTimeInProcess(data, day);
            StdDevTimeInQueue = CalcStdDevTimeInQueue(data, day);

            var recircGroups = data.GetRecircGroups(day, interval);

            var putWallRecircGroups = recircGroups.Where(x => x.Item2.Select(y => y.Item1).Contains(data.P06.LocationID));
            var nonPutwallRecircGroups = recircGroups.Where(x => !x.Item2.Select(y => y.Item1).Contains(data.P06.LocationID));

            AvgNumTimesRecirc = new Dictionary<ProcessType, double>();
            AvgNumTimesRecirc.Add(ProcessType.Putwall, putWallRecircGroups.Select(x => Math.Max(0, x.Item2.Where(y => y.Item1 == data.P06.LocationID).Count())).Average());
            AvgNumTimesRecirc.Add(ProcessType.NonPutwall, nonPutwallRecircGroups.Select(x => Math.Max(0, x.Item2.Count)).Average());


            StdDevNumTimesRecirc = new Dictionary<ProcessType, double>();
            StdDevNumTimesRecirc.Add(ProcessType.Putwall, putWallRecircGroups.Select(x => Math.Max(0, x.Item2.Where(y => y.Item1 == data.P06.LocationID).Count())).StandardDeviation());
            

            Logger.LogDistribution("RecircDist_Real", putWallRecircGroups.Select(x =>
                                    (int)sumDateDiffs(x.Item2.Where(y => y.Item1 == data.P06.LocationID).Select(y => y.Item2).ToList(), anomolyLimit)).ToList());
            AvgTimeRecirc = new Dictionary<ProcessType, double>();
            AvgTimeRecirc.Add(ProcessType.Putwall, putWallRecircGroups.Select(x =>
                                    sumDateDiffs(x.Item2.Where(y => y.Item1 == data.P06.LocationID).Select(y => y.Item2).ToList(), anomolyLimit)).Average());
            AvgTimeRecirc.Add(ProcessType.NonPutwall, nonPutwallRecircGroups.Select(x => sumDateDiffs(x.Item2.Select(y => y.Item2).ToList(), anomolyLimit)).Average());

            StdDevTimeRecirc = new Dictionary<ProcessType, double>();
            StdDevTimeRecirc.Add(ProcessType.Putwall, putWallRecircGroups.Select(x =>
                                    sumDateDiffs(x.Item2.Where(y => y.Item1 == data.P06.LocationID).Select(y => y.Item2).ToList(), anomolyLimit)).StandardDeviation());

            CumulativeArrivalCount = CalcCumArrivalCount(data, day, interval);
        }
        public SystemStats(DateTime day, double anomolyLimit, Tuple<TimeSpan, TimeSpan> interval, ILogger logger)
        {
            Logger = logger;
            var data = new WarehouseData();

            ArrivalCount = CountArrivals(data.FirstArrivals(data.Scanner901, day).
                Where(x => x.Timestamp.TimeOfDay >= interval.Item1 &
                    x.Timestamp.TimeOfDay <= interval.Item2).ToList(), data);

            ExitCount = data.GetNPuts(day, interval);

            AvgTimeInSystem = CalcAvgTimeInSystem(data, day, interval);
            AvgTimeInProcess = CalcAvgTimeInProcess(data, day, interval);
            AvgTimeInQueue = CalcAvgTimeInQueue(data, day, interval);

            StdDevTimeInSystem = CalcStdDevTimeInSystem(data, day);
            StdDevTimeInProcess = CalcStdDevTimeInProcess(data, day);
            StdDevTimeInQueue = CalcStdDevTimeInQueue(data, day);

            var recircGroups = data.GetRecircGroups(day, interval);

            var putWallRecircGroups = recircGroups.Where(x => x.Item2.Select(y => y.Item1).Contains(data.P06.LocationID));
            var nonPutwallRecircGroups = recircGroups.Where(x => !x.Item2.Select(y => y.Item1).Contains(data.P06.LocationID));

            AvgNumTimesRecirc = new Dictionary<ProcessType, double>();
            AvgNumTimesRecirc.Add(ProcessType.Putwall, putWallRecircGroups.Select(x => Math.Max(0, x.Item2.Where(y => y.Item1 == data.P06.LocationID).Count() - 1)).Average());
            AvgNumTimesRecirc.Add(ProcessType.NonPutwall, nonPutwallRecircGroups.Select(x => Math.Max(0, x.Item2.Count - 1)).Average());

            Logger.LogDistribution("RecircDist_Real", putWallRecircGroups.Select(x =>
                                    (int)sumDateDiffs(x.Item2.Where(y => y.Item1 == data.P06.LocationID).Select(y => y.Item2).ToList(), anomolyLimit)).ToList());
            AvgTimeRecirc = new Dictionary<ProcessType, double>();
            AvgTimeRecirc.Add(ProcessType.Putwall, putWallRecircGroups.Select(x =>
                                    sumDateDiffs(x.Item2.Where(y => y.Item1 == data.P06.LocationID).Select(y => y.Item2).ToList(), anomolyLimit)).Average());
            AvgTimeRecirc.Add(ProcessType.NonPutwall, nonPutwallRecircGroups.Select(x => sumDateDiffs(x.Item2.Select(y => y.Item2).ToList(), anomolyLimit)).Average());

            CumulativeArrivalCount = CalcCumArrivalCount(data, day, interval);

            StdDevNumTimesRecirc = new Dictionary<ProcessType, double>();
            StdDevNumTimesRecirc.Add(ProcessType.Putwall, putWallRecircGroups.Select(x => Math.Max(0, x.Item2.Where(y => y.Item1 == data.P06.LocationID).Count() - 1)).StandardDeviation());
            StdDevTimeRecirc = new Dictionary<ProcessType, double>();
            StdDevTimeRecirc.Add(ProcessType.Putwall, putWallRecircGroups.Select(x =>
                                    sumDateDiffs(x.Item2.Where(y => y.Item1 == data.P06.LocationID).Select(y => y.Item2).ToList(), anomolyLimit)).StandardDeviation());
           
        }
        public SystemStats(DateTime day, double anomolyLimit, Tuple<TimeSpan, TimeSpan> interval, ILogger logger, WarehouseDataType wDataType)
        {
            Logger = logger;
            var data = new WarehouseData(wDataType);

            ArrivalCount = CountArrivals(data.FirstArrivals(data.Scanner901, day).
                Where(x => x.Timestamp.TimeOfDay >= interval.Item1 &
                    x.Timestamp.TimeOfDay <= interval.Item2).ToList(), data);

            ExitCount = data.GetNPuts(day, interval);

            AvgTimeInSystem = CalcAvgTimeInSystem(data, day, interval);
            AvgTimeInProcess = CalcAvgTimeInProcess(data, day, interval);
            AvgTimeInQueue = CalcAvgTimeInQueue(data, day, interval);

            StdDevTimeInSystem = CalcStdDevTimeInSystem(data, day);
            StdDevTimeInProcess = CalcStdDevTimeInProcess(data, day);
            StdDevTimeInQueue = CalcStdDevTimeInQueue(data, day);

            var recircGroups = data.GetRecircGroups(day, interval);

            var putWallRecircGroups = recircGroups.Where(x => x.Item2.Select(y => y.Item1).Contains(data.P06.LocationID));
            var nonPutwallRecircGroups = recircGroups.Where(x => !x.Item2.Select(y => y.Item1).Contains(data.P06.LocationID));

            AvgNumTimesRecirc = new Dictionary<ProcessType, double>();
            AvgNumTimesRecirc.Add(ProcessType.Putwall, putWallRecircGroups.Select(x => Math.Max(0, x.Item2.Where(y => y.Item1 == data.P06.LocationID).Count() - 1)).Average());
            AvgNumTimesRecirc.Add(ProcessType.NonPutwall,
                nonPutwallRecircGroups.Count() > 0 ? nonPutwallRecircGroups.Select(x => Math.Max(0, x.Item2.Count - 1)).Average():0);

            Logger.LogDistribution("RecircDist_Real", putWallRecircGroups.Select(x =>
                                    (int)sumDateDiffs(x.Item2.Where(y => y.Item1 == data.P06.LocationID).Select(y => y.Item2).ToList(), anomolyLimit)).ToList());
            AvgTimeRecirc = new Dictionary<ProcessType, double>();
            AvgTimeRecirc.Add(ProcessType.Putwall, putWallRecircGroups.Select(x =>
                                    sumDateDiffs(x.Item2.Where(y => y.Item1 == data.P06.LocationID).Select(y => y.Item2).ToList(), anomolyLimit)).Average());
            AvgTimeRecirc.Add(ProcessType.NonPutwall,
                nonPutwallRecircGroups.Count() > 0 ? nonPutwallRecircGroups.Select(x => sumDateDiffs(x.Item2.Select(y => y.Item2).ToList(), anomolyLimit)).Average(): 0);

            CumulativeArrivalCount = CalcCumArrivalCount(data, day, interval);

            StdDevNumTimesRecirc = new Dictionary<ProcessType, double>();
            StdDevNumTimesRecirc.Add(ProcessType.Putwall, putWallRecircGroups.Select(x => Math.Max(0, x.Item2.Where(y => y.Item1 == data.P06.LocationID).Count() - 1)).StandardDeviation());

            StdDevTimeRecirc = new Dictionary<ProcessType, double>();
            StdDevTimeRecirc.Add(ProcessType.Putwall, putWallRecircGroups.Select(x =>
                                    sumDateDiffs(x.Item2.Where(y => y.Item1 == data.P06.LocationID).Select(y => y.Item2).ToList(), anomolyLimit)).StandardDeviation());


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
        private double CalcStdDevTimeInSystem(WarehouseData data, DateTime day)
        {
            var arrivals = data.FirstArrivals(data.Scanner901, day);
            var putTimes = data.LastPutTimes(day, arrivals.Select(x => x.BatchID).Distinct().ToList());

            return putTimes.Where(x => arrivals.Select(y => y.BatchID).Contains(x.Item1)).
                            Select(x => x.Item2.Subtract(arrivals.Where(y => y.BatchID == x.Item1).FirstOrDefault().Timestamp).TotalSeconds).StandardDeviation();
        }
        private double CalcAvgTimeInSystem(WarehouseData data, DateTime day, Tuple<TimeSpan, TimeSpan> interval)
        {
            var arrivals = data.FirstArrivals(data.Scanner901, day).Where(x => x.Timestamp.TimeOfDay >= interval.Item1 &
                    x.Timestamp.TimeOfDay <= interval.Item2);

            var putTimes = data.LastPutTimes(day, arrivals.Select(x => x.BatchID).Distinct().ToList(), interval);

            return putTimes.Where(x => arrivals.Select(y => y.BatchID).Contains(x.Item1)).
                            Select(x => x.Item2.Subtract(arrivals.Where(y => y.BatchID == x.Item1).FirstOrDefault().Timestamp).TotalSeconds).Average();
        }
        private double CalcStdDevTimeInSystem(WarehouseData data, DateTime day, Tuple<TimeSpan, TimeSpan> interval)
        {
            var arrivals = data.FirstArrivals(data.Scanner901, day).Where(x => x.Timestamp.TimeOfDay >= interval.Item1 &
                    x.Timestamp.TimeOfDay <= interval.Item2);

            var putTimes = data.LastPutTimes(day, arrivals.Select(x => x.BatchID).Distinct().ToList(), interval);

            return putTimes.Where(x => arrivals.Select(y => y.BatchID).Contains(x.Item1)).
                            Select(x => x.Item2.Subtract(arrivals.Where(y => y.BatchID == x.Item1).FirstOrDefault().Timestamp).TotalSeconds).StandardDeviation();
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
        private double CalcStdDevTimeInProcess(WarehouseData data, DateTime day)
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
            return times.StandardDeviation();
        }
        private double CalcAvgTimeInProcess(WarehouseData data, DateTime day, Tuple<TimeSpan, TimeSpan> interval)
        {
            //Get the last batchscan at 901
            var todaysArrivals = data.LastArrivals(data.Scanner901, day).Where(x => x.Timestamp.TimeOfDay >= interval.Item1 &
                    x.Timestamp.TimeOfDay <= interval.Item2);

            //Get the time the first item in batch was put
            var firstPutTimes = data.FirstPutTimes(day, todaysArrivals.Select(x => x.BatchID).ToList(), interval);

            //Get the time the last item in the batch was put
            var lastPutTimes = data.LastPutTimes(day, todaysArrivals.Select(x => x.BatchID).ToList(), interval);

            //FIX THIS GARBAGE WHEN YOU HAVE TIME
            var validBatches = firstPutTimes.Select(x => x.Item1).Where(x => lastPutTimes.Select(y => y.Item1).Contains(x));

            var times = validBatches.Select(x => lastPutTimes.Where(y => y.Item1 == x).
                                                FirstOrDefault().Item2.Subtract(firstPutTimes.Where(z => z.Item1 == x).
                                                FirstOrDefault().Item2).TotalSeconds);
            return times.Average();
        }
        private double CalcStdDevTimeInProcess(WarehouseData data, DateTime day, Tuple<TimeSpan, TimeSpan> interval)
        {
            //Get the last batchscan at 901
            var todaysArrivals = data.LastArrivals(data.Scanner901, day).Where(x => x.Timestamp.TimeOfDay >= interval.Item1 &
                    x.Timestamp.TimeOfDay <= interval.Item2);

            //Get the time the first item in batch was put
            var firstPutTimes = data.FirstPutTimes(day, todaysArrivals.Select(x => x.BatchID).ToList(), interval);

            //Get the time the last item in the batch was put
            var lastPutTimes = data.LastPutTimes(day, todaysArrivals.Select(x => x.BatchID).ToList(), interval);

            //FIX THIS GARBAGE WHEN YOU HAVE TIME
            var validBatches = firstPutTimes.Select(x => x.Item1).Where(x => lastPutTimes.Select(y => y.Item1).Contains(x));

            var times = validBatches.Select(x => lastPutTimes.Where(y => y.Item1 == x).
                                                FirstOrDefault().Item2.Subtract(firstPutTimes.Where(z => z.Item1 == x).
                                                FirstOrDefault().Item2).TotalSeconds);
            return times.StandardDeviation();
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
        private double CalcStdDevTimeInQueue(WarehouseData data, DateTime day)
        {
            //Get the last batchscan at 901
            var lastArrivals = data.LastArrivals(data.Scanner901, day);

            //Get the time the first item in batch was put
            var firstPutTimes = data.FirstPutTimes(day, lastArrivals.Select(x => x.BatchID).ToList());

            return firstPutTimes.Where(x => lastArrivals.Select(y => y.BatchID).Contains(x.Item1)).
                                        Select(x => x.Item2.Subtract(lastArrivals.Where(y => y.BatchID == x.Item1).FirstOrDefault().Timestamp).TotalSeconds).StandardDeviation();
        }
        private double CalcAvgTimeInQueue(WarehouseData data, DateTime day, Tuple<TimeSpan, TimeSpan> interval)
        {
            //Get the last batchscan at 901
            var lastArrivals = data.LastArrivals(data.Scanner901, day).Where(x => x.Timestamp.TimeOfDay >= interval.Item1 &
                    x.Timestamp.TimeOfDay <= interval.Item2); 

            //Get the time the first item in batch was put
            var firstPutTimes = data.FirstPutTimes(day, lastArrivals.Select(x => x.BatchID).ToList(), interval);

            return firstPutTimes.Where(x => lastArrivals.Select(y => y.BatchID).Contains(x.Item1)).
                                        Select(x => x.Item2.Subtract(lastArrivals.Where(y => y.BatchID == x.Item1).FirstOrDefault().Timestamp).TotalSeconds).Average();
        }
        private double CalcStdDevTimeInQueue(WarehouseData data, DateTime day, Tuple<TimeSpan, TimeSpan> interval)
        {
            //Get the last batchscan at 901
            var lastArrivals = data.LastArrivals(data.Scanner901, day).Where(x => x.Timestamp.TimeOfDay >= interval.Item1 &
                    x.Timestamp.TimeOfDay <= interval.Item2);

            //Get the time the first item in batch was put
            var firstPutTimes = data.FirstPutTimes(day, lastArrivals.Select(x => x.BatchID).ToList(), interval);

            return firstPutTimes.Where(x => lastArrivals.Select(y => y.BatchID).Contains(x.Item1)).
                                        Select(x => x.Item2.Subtract(lastArrivals.Where(y => y.BatchID == x.Item1).FirstOrDefault().Timestamp).TotalSeconds).StandardDeviation();
        }
        private Dictionary<int, int> CalcCumArrivalCount(WarehouseData data, DateTime day, Tuple<TimeSpan, TimeSpan> interval)
        {
            var CumArrivalCount = new Dictionary<int, int>();

            var arrivals = data.FirstArrivals(data.Scanner901, day).
                            Where(x => x.Timestamp.TimeOfDay >= interval.Item1 &
                            x.Timestamp.TimeOfDay <= interval.Item2).ToList();

            foreach(var arv in arrivals)
            {
                int t = (int)arv.Timestamp.TimeOfDay.Subtract(interval.Item1).TotalSeconds;

                if (CumArrivalCount.ContainsKey(t))
                {
                    CumArrivalCount[t]++;
                }
                else
                {
                    CumArrivalCount.Add(t, 1);
                }
            }

            var keys = CumArrivalCount.Keys.OrderBy(x => x).ToList();

            for(int i = 1; i < CumArrivalCount.Count; i++)
            {
                CumArrivalCount[keys[i]] = CumArrivalCount[keys[i]] + CumArrivalCount[keys[i - 1]];
            }

            return CumArrivalCount;
        }

    }
}
