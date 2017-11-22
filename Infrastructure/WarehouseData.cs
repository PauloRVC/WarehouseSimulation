using Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    public enum WarehouseDataType
    {
        AllData,
        PutwallOnlyData
    }
    public class WarehouseData
    {
        public ILogger Logger { get; set; } = new NullLogger();
        public Location Scanner901 { get; set; }
        public Location P06 { get; set; }
        internal WarehouseContext db;
        public WarehouseData()
        {
            db = new WarehouseContext();
            Scanner901 = db.Locations.Where(x => x.ScannerIndicator == "901").First();
            P06 = db.Locations.Where(x => x.ScannerIndicator == "P06").First();
        }
        public WarehouseData(WarehouseDataType wDataType)
        {
            switch (wDataType)
            {
                case WarehouseDataType.AllData:
                    db = new WarehouseContext();
                    break;
                case WarehouseDataType.PutwallOnlyData:
                    db = new PutwallOnlyData();
                    break;
                default:
                    throw new NotImplementedException();
            }
            
            Scanner901 = db.Locations.Where(x => x.ScannerIndicator == "901").First();
            P06 = db.Locations.Where(x => x.ScannerIndicator == "P06").First();
        }
        public List<BatchScan> FirstArrivals(Location location, DateTime day)
        {
            
            var daysBatchScans = db.BatchScan.Where(x => DbFunctions.TruncateTime(x.Timestamp) == day.Date
                                                            && x.CurrentLocation.LocationID == location.LocationID);

            //Keep only the first arrival for each batch
            return daysBatchScans.GroupBy(x => x.BatchID).Select(x => x.OrderBy(y => y.Timestamp).FirstOrDefault()).OrderBy(x => x.Timestamp).
                Include(x => x.ActualDestination).Include(x => x.CurrentLocation).Include(x => x.IntendedDestination).ToList();
        }
        public List<BatchScan> FirstArrivals(Location location, DateTime day, Tuple<TimeSpan, TimeSpan> interval)
        {
            
            var daysBatchScans = db.BatchScan.Where(x => DbFunctions.TruncateTime(x.Timestamp) == day.Date
                                                            && x.CurrentLocation.LocationID == location.LocationID
                                                            && DbFunctions.CreateTime(x.Timestamp.Hour, x.Timestamp.Minute, x.Timestamp.Second) >= interval.Item1
                                                            && DbFunctions.CreateTime(x.Timestamp.Hour, x.Timestamp.Minute, x.Timestamp.Second) <= interval.Item2);

            //Keep only the first arrival for each batch
            return daysBatchScans.GroupBy(x => x.BatchID).Select(x => x.OrderBy(y => y.Timestamp).FirstOrDefault()).OrderBy(x => x.Timestamp).
                Include(x => x.ActualDestination).Include(x => x.CurrentLocation).Include(x => x.IntendedDestination).ToList();
        }
        public List<BatchScan> LastArrivals(Location location, DateTime day)
        {
            
            var daysBatchScans = db.BatchScan.Where(x => DbFunctions.TruncateTime(x.Timestamp) == day.Date
                                                            && x.CurrentLocation.LocationID == location.LocationID);

            //Keep only the first arrival for each batch
            return daysBatchScans.GroupBy(x => x.BatchID).Select(x => x.OrderByDescending(y => y.Timestamp).FirstOrDefault()).OrderBy(x => x.Timestamp).ToList();
        }
        public List<Tuple<int, DateTime>> FirstPutTimes(DateTime day, List<int> batchIDs)
        {
            
            var puts = db.OrderItems.Where(x => x.PutTimestamp.HasValue
                                            && DbFunctions.TruncateTime(x.PutTimestamp) == day.Date
                                            && batchIDs.Contains(x.Order.BatchID));


            return puts.GroupBy(x => x.Order.BatchID).ToList().Select(x => new Tuple<int, DateTime>(x.Key, (DateTime)x.OrderBy(y => y.PutTimestamp).First().PutTimestamp)).ToList();
        }
       
        public List<Tuple<int, DateTime>> FirstPutTimes(DateTime day, List<int> batchIDs, Tuple<TimeSpan, TimeSpan> interval)
        {
            
            var puts = db.OrderItems.Where(x => x.PutTimestamp.HasValue
                                            && DbFunctions.TruncateTime(x.PutTimestamp) == day.Date
                                            && batchIDs.Contains(x.Order.BatchID)
                                            && DbFunctions.CreateTime(x.PutTimestamp.Value.Hour, x.PutTimestamp.Value.Minute, x.PutTimestamp.Value.Second) >= interval.Item1
                                            && DbFunctions.CreateTime(x.PutTimestamp.Value.Hour, x.PutTimestamp.Value.Minute, x.PutTimestamp.Value.Second) <= interval.Item2);


            return puts.GroupBy(x => x.Order.BatchID).ToList().Select(x => new Tuple<int, DateTime>(x.Key, (DateTime)x.OrderBy(y => y.PutTimestamp).First().PutTimestamp)).ToList();
        }
        public Dictionary<int, DateTime> FirstPutTimesDict(DateTime day, List<int> batchIDs)
        {
            
            var puts = db.OrderItems.Where(x => x.PutTimestamp.HasValue
                                            && DbFunctions.TruncateTime(x.PutTimestamp) == day.Date
                                            && batchIDs.Contains(x.Order.BatchID));

            return puts.GroupBy(x => x.Order.BatchID).ToDictionary(x => x.Key, x => x.OrderBy(y => y.PutTimestamp).First().PutTimestamp.Value);
        }
        public Dictionary<int, DateTime> FirstPutTimesDict(DateTime day, List<int> batchIDs, Tuple<TimeSpan, TimeSpan> interval)
        {
            var puts = db.OrderItems.Where(x => x.PutTimestamp.HasValue
                                            && DbFunctions.TruncateTime(x.PutTimestamp) == day.Date
                                            && batchIDs.Contains(x.Order.BatchID)
                                            && DbFunctions.CreateTime(x.PutTimestamp.Value.Hour, x.PutTimestamp.Value.Minute, x.PutTimestamp.Value.Second) >= interval.Item1
                                            && DbFunctions.CreateTime(x.PutTimestamp.Value.Hour, x.PutTimestamp.Value.Minute, x.PutTimestamp.Value.Second) <= interval.Item2);

            return puts.GroupBy(x => x.Order.BatchID).ToDictionary(x => x.Key, x => x.OrderBy(y => y.PutTimestamp).First().PutTimestamp.Value);
        }
        public List<Tuple<int, DateTime>> LastPutTimes(DateTime day, List<int> batchIDs)
        {
            
            var puts = db.OrderItems.Where(x => x.PutTimestamp.HasValue
                                            && DbFunctions.TruncateTime(x.PutTimestamp) == day.Date
                                            && batchIDs.Contains(x.Order.BatchID));


            return puts.GroupBy(x => x.Order.BatchID).ToList().Select(x => new Tuple<int, DateTime>(x.Key, (DateTime)x.OrderByDescending(y => y.PutTimestamp).First().PutTimestamp)).ToList();
        }
        
        public List<Tuple<int, DateTime>> LastPutTimes(DateTime day, List<int> batchIDs, Tuple<TimeSpan, TimeSpan> interval)
        {
            var puts = db.OrderItems.Where(x => x.PutTimestamp.HasValue
                                            && DbFunctions.TruncateTime(x.PutTimestamp) == day.Date
                                            && batchIDs.Contains(x.Order.BatchID)
                                            && DbFunctions.CreateTime(x.PutTimestamp.Value.Hour, x.PutTimestamp.Value.Minute, x.PutTimestamp.Value.Second) >= interval.Item1
                                            && DbFunctions.CreateTime(x.PutTimestamp.Value.Hour, x.PutTimestamp.Value.Minute, x.PutTimestamp.Value.Second) <= interval.Item2);


            return puts.GroupBy(x => x.Order.BatchID).ToList().Select(x => new Tuple<int, DateTime>(x.Key, (DateTime)x.OrderByDescending(y => y.PutTimestamp).First().PutTimestamp)).ToList();
        }
        public Dictionary<int, DateTime> LastPutTimesDict(DateTime day, List<int> batchIDs)
        {
           
            var puts = db.OrderItems.Where(x => x.PutTimestamp.HasValue
                                            && DbFunctions.TruncateTime(x.PutTimestamp) == day.Date
                                            && batchIDs.Contains(x.Order.BatchID));


            return puts.GroupBy(x => x.Order.BatchID).ToDictionary(x => x.Key, x => x.OrderByDescending(y => y.PutTimestamp).First().PutTimestamp.Value);
        }
        public Dictionary<int, DateTime> LastPutTimesDict(DateTime day, List<int> batchIDs, Tuple<TimeSpan, TimeSpan> interval)
        {
            var puts = db.OrderItems.Where(x => x.PutTimestamp.HasValue
                                            && DbFunctions.TruncateTime(x.PutTimestamp) == day.Date
                                            && batchIDs.Contains(x.Order.BatchID)
                                            && DbFunctions.CreateTime(x.PutTimestamp.Value.Hour, x.PutTimestamp.Value.Minute, x.PutTimestamp.Value.Second) >= interval.Item1
                                            && DbFunctions.CreateTime(x.PutTimestamp.Value.Hour, x.PutTimestamp.Value.Minute, x.PutTimestamp.Value.Second) <= interval.Item2);


            return puts.GroupBy(x => x.Order.BatchID).ToDictionary(x => x.Key, x => x.OrderByDescending(y => y.PutTimestamp).First().PutTimestamp.Value);
        }
        public int GetNPuts(DateTime day)
        {
            var puts = db.OrderItems.Where(x => x.PutTimestamp.HasValue
                                            && DbFunctions.TruncateTime(x.PutTimestamp) == day.Date);
            return puts.GroupBy(x => x.Order.BatchID).Count();
        }
        public int GetNPuts(DateTime day, Tuple<TimeSpan, TimeSpan> interval)
        {
            var puts = db.OrderItems.Where(x => x.PutTimestamp.HasValue
                                            && DbFunctions.TruncateTime(x.PutTimestamp) == day.Date
                                            && DbFunctions.CreateTime(x.PutTimestamp.Value.Hour, x.PutTimestamp.Value.Minute, x.PutTimestamp.Value.Second) >= interval.Item1
                                            && DbFunctions.CreateTime(x.PutTimestamp.Value.Hour, x.PutTimestamp.Value.Minute, x.PutTimestamp.Value.Second) <= interval.Item2);
            return puts.GroupBy(x => x.Order.BatchID).Count();
        }


        public List<DateTime> GetBatchScanAvailability()
        {
            return db.BatchScan.Select(x => DbFunctions.TruncateTime(x.Timestamp)).Distinct().ToList().Select(x => ((DateTime)x).Date).ToList();
        }
        public List<DateTime> GetPutTimeAvailability()
        {
            return db.OrderItems.Where(x => x.PutTimestamp.HasValue).Select(x => DbFunctions.TruncateTime(x.PutTimestamp)).Distinct().ToList().Select(x => ((DateTime)x).Date).ToList();
        }
        public List<DateTime> GetOverallAvailability()
        {
            return GetBatchScanAvailability().Intersect(GetPutTimeAvailability()).ToList();
        }
        public Dictionary<int, int> GetPutsPerHour(DateTime day)
        {
            var lastPuts = db.OrderItems.Where(x => x.PutTimestamp.HasValue
                                            && DbFunctions.TruncateTime(x.PutTimestamp) == day.Date).
                                            GroupBy(x => x.Order.BatchID).Select(x => x.OrderByDescending(y => y.PutTimestamp).FirstOrDefault()).
                                            GroupBy(x => x.PutTimestamp.Value.Hour).
                                            Select(x => new { hour = x.Key, count = x.Count() });
            var pph = new Dictionary<int, int>();

            foreach (var a in lastPuts)
            {
                pph.Add(a.hour, a.count);
            }

            for (int i = 0; i < 24; i++)
            {
                if (!pph.ContainsKey(i))
                {
                    pph.Add(i, 0);
                }
            }
            return pph;
        }
        public List<int> GetRecircTimes(DateTime day)
        {
            var res = db.BatchScan.Where(x => x.CurrentLocation.LocationID == db.Scanner901.LocationID &&
                                            DbFunctions.TruncateTime(x.Timestamp) == day.Date).
                                            GroupBy(x => x.BatchID).Where(x => x.Count() > 1).
                                            Select(x => x.Select(y => y.Timestamp).OrderBy(y => y).ToList());

            List<int> recircTimes = new List<int>();

            foreach (var group in res)
            {
                for (int i = 1; i < group.Count; i++)
                {
                    recircTimes.Add((int)group[i].Subtract(group[i - 1]).TotalSeconds);
                }
            }

            return recircTimes;
        }
        public Dictionary<int, int> GetPutsPerZMins(DateTime day, int z)
        {
            Dictionary<DateTime, int> indexLookup = new Dictionary<DateTime, int>();
            int i = 0;
            DateTime time = new DateTime(day.Year, day.Month, day.Day, 0, 0, 0);

            indexLookup.Add(time, i);

            do
            {
                i++;
                time = time.AddMinutes(z);
                indexLookup.Add(time, i);
            } while (time.Day == day.Day);
            var intermed = db.OrderItems.Where(x => x.PutTimestamp.HasValue
                                            && DbFunctions.TruncateTime(x.PutTimestamp) == day.Date).
                                            GroupBy(x => x.Order.BatchID).Select(x => x.OrderByDescending(y => y.PutTimestamp).FirstOrDefault()).
                                            ToList();

            var lastPuts = intermed.GroupBy(x => GetTimeIndex(indexLookup, x.PutTimestamp.Value)).
                                                Select(x => new { timeIndex = x.Key, count = x.Count() });

            var pph = new Dictionary<int, int>();
            foreach (int k in indexLookup.Values)
            {
                pph.Add(k, 0);
            }

            foreach (var a in lastPuts)
            {
                pph[a.timeIndex] += a.count;
            }

            return pph;
        }
        public Dictionary<int, int> GetOperatorsPerZMins(DateTime day, int z)
        {
            Dictionary<DateTime, int> indexLookup = new Dictionary<DateTime, int>();
            int i = 0;
            DateTime time = new DateTime(day.Year, day.Month, day.Day, 0, 0, 0);

            indexLookup.Add(time, i);

            do
            {
                i++;
                time = time.AddMinutes(z);
                indexLookup.Add(time, i);
            } while (time.Day == day.Day);
            var intermed = db.OrderItems.Where(x => x.PutTimestamp.HasValue
                                            && DbFunctions.TruncateTime(x.PutTimestamp) == day.Date).
                                            GroupBy(x => x.Order.BatchID).Select(x => x.OrderByDescending(y => y.PutTimestamp).FirstOrDefault()).
                                            ToList();

            var lastPuts = intermed.GroupBy(x => GetTimeIndex(indexLookup, x.PutTimestamp.Value)).
                                                Select(x => new { timeIndex = x.Key, count = x.Select(y => y.OperatorID).Distinct().Count() });

            var operatorsPerZ = new Dictionary<int, int>();
            foreach (int k in indexLookup.Values)
            {
                operatorsPerZ.Add(k, 0);
            }

            foreach (var a in lastPuts)
            {
                operatorsPerZ[a.timeIndex] += a.count;
            }

            return operatorsPerZ;
        }
        private int GetTimeIndex(Dictionary<DateTime, int> indexLookup, DateTime time)
        {
            return indexLookup[indexLookup.Keys.Where(x => x <= time).Max()];
        }
        public List<Tuple<int, List<Tuple<int, DateTime>>>> GetRecircGroups(DateTime day)
        {
            var dbresults = db.BatchScan.Where(x => x.CurrentLocation.LocationID == db.Scanner901.LocationID &&
                                            DbFunctions.TruncateTime(x.Timestamp) == day.Date).
                                            GroupBy(x => x.BatchID).
                                            Select(x => new
                                            {
                                                bid = x.Key,
                                                pair = x.Select(y => new { y.IntendedDestinationID, y.Timestamp }).ToList()
                                            }).ToList();

            return dbresults.
                Select(x => new Tuple<int, List<Tuple<int, DateTime>>>(x.bid, x.pair.Select(y => new Tuple<int, DateTime>(y.IntendedDestinationID, y.Timestamp)).
                ToList())).ToList();
        }
        public List<Tuple<int, List<Tuple<int, DateTime>>>> GetRecircGroups(DateTime day, Tuple<TimeSpan, TimeSpan> interval)
        {
            var dbresults = db.BatchScan.Where(x => x.CurrentLocation.LocationID == db.Scanner901.LocationID &&
                                            DbFunctions.TruncateTime(x.Timestamp) == day.Date
                                            && DbFunctions.CreateTime(x.Timestamp.Hour, x.Timestamp.Minute, x.Timestamp.Second) >= interval.Item1
                                            && DbFunctions.CreateTime(x.Timestamp.Hour, x.Timestamp.Minute, x.Timestamp.Second) <= interval.Item2).
                                            GroupBy(x => x.BatchID).
                                            Select(x => new
                                            {
                                                bid = x.Key,
                                                pair = x.Select(y => new { y.IntendedDestinationID, y.Timestamp }).ToList()
                                            }).ToList();

            return dbresults.
                Select(x => new Tuple<int, List<Tuple<int, DateTime>>>(x.bid, x.pair.Select(y => new Tuple<int, DateTime>(y.IntendedDestinationID, y.Timestamp)).
                ToList())).ToList();
        }
        public List<Tuple<DateTime, int>> GetInterarrivalTimesOverTime(DateTime day)
        {
            var results = new List<Tuple<DateTime, int>>();

            var batchScans = FirstArrivals(Scanner901, day);

            var consecutiveTimes = batchScans.Select(x => x.Timestamp).OrderBy(x => x).ToList();

            for (int i = 1; i < consecutiveTimes.Count; i++)
            {
                results.Add(new Tuple<DateTime, int>(consecutiveTimes[i], (int)consecutiveTimes[i].Subtract(consecutiveTimes[i - 1]).TotalSeconds));
            }

            return results;
        }
        public int[] GetQueueSizeOverTime(DateTime day)
        {
            var QueueSize = new int[(int)(new TimeSpan(24, 0, 0)).TotalSeconds];

            var last901 = LastArrivals(Scanner901, day);

            var batchIDs = last901.Select(x => x.BatchID).ToList();

            var puts = db.OrderItems.Where(x => x.PutTimestamp.HasValue
                                                && DbFunctions.TruncateTime(x.PutTimestamp) == day.Date
                                                && batchIDs.Contains(x.Order.BatchID)).GroupBy(x => x.Order.BatchID).ToList().
                                                ToDictionary(x => x.Key, x => x.Select(y => y.PutTimestamp).Min());

            foreach (var lastArrival in last901)
            {
                if (puts.ContainsKey(lastArrival.BatchID))
                {
                    var t1 = lastArrival.Timestamp;
                    var t2 = puts[lastArrival.BatchID].Value;

                    int start = (int)t1.TimeOfDay.TotalSeconds;
                    int end = (int)t2.TimeOfDay.TotalSeconds;

                    for (int i = start; i <= end; i++)
                    {
                        QueueSize[i]++;
                    }
                }
            }

            return QueueSize;
        }
        public int[] GetItemsInRecircOverTime(List<DateTime> data, DateTime day)
        {
            var dates = data.Select(x => x.Date).ToList();
            var ItemsInRecirc = new int[(int)(new TimeSpan(24, 0, 0)).TotalSeconds];
            var groups = db.BatchScan.Where(x => x.CurrentLocation.LocationID == db.Scanner901.LocationID &&
                                                data.Contains(DbFunctions.TruncateTime(x.Timestamp).Value)).
                                                GroupBy(x => x.BatchID).Where(x => x.Count() > 1).Select(x => x.OrderBy(y => y.Timestamp));

            var dayStart = new DateTime(day.Year, day.Month, day.Day, 0, 0, 0);
            var dayEnd = new DateTime(day.Year, day.Month, day.Day, 23, 59, 59);
            foreach (var group in groups)
            {
                var t1 = group.First().Timestamp;
                var t2 = group.Last().Timestamp;

                if (!(t2 < dayStart | t1 > dayEnd))
                {
                    t1 = new DateTime(Math.Max(t1.Ticks, dayStart.Ticks));
                    t2 = new DateTime(Math.Min(t2.Ticks, dayEnd.Ticks));

                    int start = (int)t1.TimeOfDay.TotalSeconds;
                    int end = (int)t2.TimeOfDay.TotalSeconds;

                    for (int i = start; i <= end; i++)
                    {
                        ItemsInRecirc[i]++;
                    }
                }

            }

            return ItemsInRecirc;
        }
        public int[] GetQueueSizeOverTime(List<DateTime> data, DateTime day)
        {
            var QueueSize = new int[(int)(new TimeSpan(24, 0, 0)).TotalSeconds];

            var last901 = new List<BatchScan>();

            foreach (DateTime d in data)
            {
                last901.AddRange(LastArrivals(Scanner901, d));
            }

            last901 = last901.GroupBy(x => x.BatchID).Select(x => x.OrderBy(y => y.Timestamp).Last()).ToList();

            var batchIDs = last901.Select(x => x.BatchID).ToList();


            var dates = data.Select(y => y.Date).ToList();


            var p = db.OrderItems.Where(x => x.PutTimestamp.HasValue
                                            && batchIDs.Contains(x.Order.BatchID)).Include(x => x.Order).ToList();

            var puts = p.Where(x => dates.Contains(x.PutTimestamp.Value.Date)).
                                GroupBy(x => x.Order.BatchID).
                                ToDictionary(x => x.Key, x => x.Select(y => y.PutTimestamp).Min());

            var dayStart = new DateTime(day.Year, day.Month, day.Day, 0, 0, 0);
            var dayEnd = new DateTime(day.Year, day.Month, day.Day, 23, 59, 59);
            foreach (var lastArrival in last901)
            {
                if (puts.ContainsKey(lastArrival.BatchID))
                {
                    var t1 = lastArrival.Timestamp;
                    var t2 = puts[lastArrival.BatchID].Value;

                    if (!(t2 < dayStart | t1 > dayEnd))
                    {
                        t1 = new DateTime(Math.Max(t1.Ticks, dayStart.Ticks));
                        t2 = new DateTime(Math.Min(t2.Ticks, dayEnd.Ticks));

                        int start = (int)t1.TimeOfDay.TotalSeconds;
                        int end = (int)t2.TimeOfDay.TotalSeconds;

                        for (int i = start; i <= end; i++)
                        {
                            QueueSize[i]++;
                        }
                    }
                }
            }

            return QueueSize;
        }
        public List<int> GetTimeInQueue(DateTime day)
        {
            var qTimes = new List<int>();

            var last901 = LastArrivals(Scanner901, day);

            var batchIDs = last901.Select(x => x.BatchID).ToList();
            
            var puts = db.OrderItems.Where(x => x.PutTimestamp.HasValue
                                            && DbFunctions.TruncateTime(x.PutTimestamp) == day.Date
                                            && batchIDs.Contains(x.Order.BatchID)).GroupBy(x => x.Order.BatchID).ToList().
                                            ToDictionary(x => x.Key, x => x.Select(y => y.PutTimestamp).Min());

            foreach (var lastArrival in last901)
            {
                if (puts.ContainsKey(lastArrival.BatchID))
                {
                    var t1 = lastArrival.Timestamp;
                    var t2 = puts[lastArrival.BatchID].Value;

                    if (t2.Subtract(t1).TotalSeconds < 0)
                        throw new InvalidOperationException();

                    qTimes.Add((int)t2.Subtract(t1).TotalSeconds);

                }
            }

            return qTimes;
        }
        public List<int> GetTimeInQueue(DateTime day, Tuple<TimeSpan, TimeSpan> interval)
        {
            var qTimes = new List<int>();

            var last901 = LastArrivals(Scanner901, day).Where(x => x.Timestamp.TimeOfDay >= interval.Item1 &
                                                                x.Timestamp.TimeOfDay <= interval.Item2);

            var batchIDs = last901.Select(x => x.BatchID).ToList();
            
            var puts = db.OrderItems.Where(x => x.PutTimestamp.HasValue
                                            && DbFunctions.TruncateTime(x.PutTimestamp) == day.Date
                                            && batchIDs.Contains(x.Order.BatchID)
                                            && DbFunctions.CreateTime(x.PutTimestamp.Value.Hour, x.PutTimestamp.Value.Minute, x.PutTimestamp.Value.Second) >= interval.Item1
                                            && DbFunctions.CreateTime(x.PutTimestamp.Value.Hour, x.PutTimestamp.Value.Minute, x.PutTimestamp.Value.Second) <= interval.Item2).
                                            GroupBy(x => x.Order.BatchID).ToList().
                                            ToDictionary(x => x.Key, x => x.Select(y => y.PutTimestamp).Min());

            foreach (var lastArrival in last901)
            {
                if (puts.ContainsKey(lastArrival.BatchID))
                {
                    var t1 = lastArrival.Timestamp;
                    var t2 = puts[lastArrival.BatchID].Value;

                    if (t2.Subtract(t1).TotalSeconds < 0)
                        throw new InvalidOperationException();

                    qTimes.Add((int)t2.Subtract(t1).TotalSeconds);

                }
            }

            return qTimes;
        }
        public int[] GetItemsInRecircOverTime(DateTime day)
        {
            var ItemsInRecirc = new int[(int)(new TimeSpan(24, 0, 0)).TotalSeconds];
            var groups = db.BatchScan.Where(x => x.CurrentLocation.LocationID == db.Scanner901.LocationID &&
                                                DbFunctions.TruncateTime(x.Timestamp) == day.Date).
                                                GroupBy(x => x.BatchID).Where(x => x.Count() > 1).Select(x => x.OrderBy(y => y.Timestamp));

            foreach (var group in groups)
            {
                var t1 = group.First().Timestamp;
                var t2 = group.Last().Timestamp;

                int start = (int)t1.TimeOfDay.TotalSeconds;
                int end = (int)t2.TimeOfDay.TotalSeconds;

                for (int i = start; i <= end; i++)
                {
                    ItemsInRecirc[i]++;
                }
            }

            return ItemsInRecirc;
        }
        public List<Tuple<DateTime, DateTime>> FindBreaks(DateTime day, int minBreakTime)
        {
            var breakTimes = new List<Tuple<DateTime, DateTime>>();

            var batchScans = FirstArrivals(Scanner901, day);

            var consecutiveTimes = batchScans.Select(x => x.Timestamp).OrderBy(x => x).ToList();

            for (int i = 1; i < consecutiveTimes.Count; i++)
            {
                if (consecutiveTimes[i].Subtract(consecutiveTimes[i - 1]).TotalSeconds > minBreakTime)
                {
                    breakTimes.Add(new Tuple<DateTime, DateTime>(consecutiveTimes[i - 1], consecutiveTimes[i]));
                }
            }

            return breakTimes;
        }
        public Dictionary<int, List<int>> QueueObservationsPerInterval(DateTime day, List<DateTime> qData, int interval, int startSecond, int duration)
        {
            int[] qSizeOverTime = GetQueueSizeOverTime(qData, day);

            var QueueObservations = new Dictionary<int, List<int>>();

            for (int i = startSecond; i <= startSecond + duration; i += interval)
            {
                var obs = new List<int>();
                for (int j = 0; j < interval; j++)
                {
                    obs.Add(qSizeOverTime[i + j]);
                }
                QueueObservations.Add(i, obs);
            }

            return QueueObservations;
        }
        public Dictionary<int, Tuple<int, int>> RecirculationVSQueueSize(DateTime day, List<DateTime> qData, Tuple<TimeSpan, TimeSpan> interval)
        {
            var RecircVsQSize = new Dictionary<int, Tuple<int, int>>();

            int[] qSizeOverTime = GetQueueSizeOverTime(qData, day);

            var dates = qData.Select(x => x.Date).ToList();
            
            var recircs = db.BatchScan.Where(x => x.CurrentLocation.LocationID == db.Scanner901.LocationID
                                            && dates.Contains(DbFunctions.TruncateTime(x.Timestamp).Value)
                                            && DbFunctions.CreateTime(x.Timestamp.Hour, x.Timestamp.Minute, x.Timestamp.Second) >= interval.Item1
                                            && DbFunctions.CreateTime(x.Timestamp.Hour, x.Timestamp.Minute, x.Timestamp.Second) <= interval.Item2).
                                            GroupBy(x => x.BatchID).ToDictionary(x => x.Key, x => x.Where(y => y.Timestamp != x.Max(z => z.Timestamp))).SelectMany(x => x.Value);
            var nonRecircs = db.BatchScan.Where(x => x.CurrentLocation.LocationID == db.Scanner901.LocationID
                                            && dates.Contains(DbFunctions.TruncateTime(x.Timestamp).Value)
                                            && DbFunctions.CreateTime(x.Timestamp.Hour, x.Timestamp.Minute, x.Timestamp.Second) >= interval.Item1
                                            && DbFunctions.CreateTime(x.Timestamp.Hour, x.Timestamp.Minute, x.Timestamp.Second) <= interval.Item2).
                                            GroupBy(x => x.BatchID).ToDictionary(x => x.Key, x => x.Where(y => y.Timestamp == x.Max(z => z.Timestamp))).SelectMany(x => x.Value);
            foreach (var batchScan in recircs)
            {
                int time = (int)batchScan.Timestamp.TimeOfDay.TotalSeconds;

                int qSize = qSizeOverTime[time];

                if (RecircVsQSize.ContainsKey(qSize))
                {
                    RecircVsQSize[qSize] = new Tuple<int, int>(RecircVsQSize[qSize].Item1 + 1, RecircVsQSize[qSize].Item2);
                }
                else
                {
                    RecircVsQSize[qSize] = new Tuple<int, int>(1, 0);
                }
            }

            foreach (var batchScan in nonRecircs)
            {
                int time = (int)batchScan.Timestamp.TimeOfDay.TotalSeconds;

                int qSize = qSizeOverTime[time];

                if (RecircVsQSize.ContainsKey(qSize))
                {
                    RecircVsQSize[qSize] = new Tuple<int, int>(RecircVsQSize[qSize].Item1, RecircVsQSize[qSize].Item2 + 1);
                }
                else
                {
                    RecircVsQSize[qSize] = new Tuple<int, int>(0, 1);
                }
            }

            return RecircVsQSize;
        }
        public Dictionary<int, int> BuildSmoothedCapacity(DateTime day, int outerInterval, int innerInterval)
        {
            var unSmoothed = GetPutsPerZMins(day, outerInterval);

            var smoothed = new Dictionary<int, int>();
            int nBlocks = outerInterval / innerInterval;

            for (int i = 0; i < unSmoothed.Count; i++)
            {
                int putsPerBlock = unSmoothed[i] / nBlocks;

                for (int j = 0; j < nBlocks - 1; j++)
                {
                    smoothed.Add(i * outerInterval + j * innerInterval, putsPerBlock);
                }
                if (unSmoothed[i] % nBlocks == 0)
                {
                    smoothed.Add(i * outerInterval + (nBlocks - 1) * innerInterval, putsPerBlock);
                }
                else
                {
                    smoothed.Add(i * outerInterval + (nBlocks - 1) * innerInterval, putsPerBlock + (unSmoothed[i] % nBlocks));
                }
            }

            return smoothed;
        }


    }
}
