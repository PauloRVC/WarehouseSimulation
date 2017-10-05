using Infrastructure;
using Infrastructure.Models;
using SimulationObjects.Distributions;
using SimulationObjects.Resources;
using SimulationObjects.Results;
using SimulationObjects.SimBlocks;
using SimulationObjects.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulationObjects
{
    public enum Process
    {
        Default
    }
    public static class SimulationFactory
    {
        public static Simulation WIPSimWithBreaksAndQTime(List<DateTime> selectedDays,
                                                            int x,
                                                            int startMin,
                                                            int endTime,
                                                            int queueSize,
                                                            int warmUpDays,
                                                            ILogger logger,
                                                            List<Tuple<TimeSpan, TimeSpan>> intervals,
                                                            int arrivalAnomolyLimit,
                                                            List<Tuple<DateTime, DateTime>> breaks,
                                                            Tuple<TimeSpan, TimeSpan> interval)
        {
            int minsInShift = endTime / 60;
            var results = new ResultsWithWarmup(endTime * warmUpDays, endTime * (warmUpDays + 1));

            var breakTimes = new List<Tuple<int, int>>();

            foreach (var brk in breaks)
            {
                if (brk.Item1.TimeOfDay.TotalMinutes > startMin)
                {
                    for (int i = 0; i <= warmUpDays; i++)
                    {
                        int t1 = ((int)brk.Item1.TimeOfDay.TotalSeconds - startMin * 60) + (endTime * i);
                        int t2 = (int)brk.Item2.TimeOfDay.TotalSeconds - startMin * 60 + (endTime * i);

                        breakTimes.Add(new Tuple<int, int>(t1, t2));
                    }
                }
            }

            Simulation simulation = new Simulation(results, endTime * (warmUpDays + 1));

            var Operators = new List<Processor>();

            var data = new WarehouseData();


            Operators.Add(new Processor());

            RealDistributionBuilder distBuilder = new RealDistributionBuilder(simulation);

            distBuilder.Logger = logger;

            var ppxMinutes = distBuilder.GetPutsPerX(selectedDays.First(), x);

            var ppxSchedule = new Dictionary<int, int>();



            //for (int min = startMin; min < startMin + minsInShift * (warmUpDays + 1); min += x)
            //{
            //    int startSecond = (min - startMin) * 60;
            //    ppxSchedule.Add(startSecond, ppxMinutes[((min - startMin) % (minsInShift + startMin) + startMin) / x]);
            //}

            for (int newX = 0; newX <= minsInShift * (warmUpDays + 1); newX += x)
            {
                int newStartSecond = newX * 60;
                ppxSchedule.Add(newStartSecond, ppxMinutes[(((newX % minsInShift) + startMin) / x)]);
            }

            logger.LogPutsPerHour("PPX_Schedule_" + x, ppxSchedule);

            IDestinationBlock disposalBlock = new DisposalBlock(simulation);


            IDestinationBlock P06 = new PutwallWithPPHScheduleAndTimeInQ(queueSize,
                                                               ppxSchedule,
                                                               Operators,
                                                               distBuilder.BuildProcessTimeDist(selectedDays, Process.Default),
                                                               distBuilder.BuildRecircTimeDist(selectedDays, 300, interval),
                                                               simulation,
                                                                disposalBlock,
                                                                distBuilder.BuildTimeInQueueDistribution(selectedDays, int.MaxValue, interval),
                                                                results);
            Location P06L = data.P06;

            Dictionary<int, IDestinationBlock> locationDict = new Dictionary<int, IDestinationBlock>()
            {
                { P06L.LocationID, P06 }
            };


            var arrivalDists = distBuilder.BuildArrivalDist(selectedDays, arrivalAnomolyLimit, intervals);

            var newArrivalDists = new Dictionary<Tuple<int, int>, IDistribution<int>>();
            for (int j = 0; j < warmUpDays + 1; j++)
            {
                for (int i = 0; i < intervals.Count; i++)
                {

                    var t = new Tuple<int, int>((int)(intervals[i].Item1.TotalMinutes - startMin) * 60 + 60 * minsInShift * j,
                        (int)(intervals[i].Item2.TotalMinutes - startMin) * 60 + 60 * minsInShift * j);
                    newArrivalDists.Add(t, arrivalDists[i]);
                }

            }



            var ArrivalBlock = new InterarrivalBlockWithBreaks(newArrivalDists, distBuilder.BuildDestinationDist(selectedDays, locationDict, disposalBlock), simulation, breakTimes);

            var firstArrival = ArrivalBlock.GetNextEvent();

            simulation.Initialize(ArrivalBlock, firstArrival);

            return simulation;
        }
        public static Simulation WIPSimWithBreaksAndQTime(List<DateTime> selectedDays,
                                                            int x,
                                                            int startMin,
                                                            int endTime,
                                                            int queueSize,
                                                            int warmUpDays,
                                                            ILogger logger,
                                                            List<Tuple<TimeSpan, TimeSpan>> intervals,
                                                            int arrivalAnomolyLimit,
                                                            List<Tuple<DateTime, DateTime>> breaks)
        {
            int minsInShift = endTime / 60;
            var results = new ResultsWithWarmup(endTime * warmUpDays, endTime * (warmUpDays + 1));

            var breakTimes = new List<Tuple<int, int>>();

            foreach (var brk in breaks)
            {
                if (brk.Item1.TimeOfDay.TotalMinutes > startMin)
                {
                    for (int i = 0; i <= warmUpDays; i++)
                    {
                        int t1 = ((int)brk.Item1.TimeOfDay.TotalSeconds - startMin * 60) + (endTime * i);
                        int t2 = (int)brk.Item2.TimeOfDay.TotalSeconds - startMin * 60 + (endTime * i);

                        breakTimes.Add(new Tuple<int, int>(t1, t2));
                    }
                }
            }

            Simulation simulation = new Simulation(results, endTime * (warmUpDays + 1));

            var Operators = new List<Processor>();

            var data = new WarehouseData();

            
            Operators.Add(new Processor());

            RealDistributionBuilder distBuilder = new RealDistributionBuilder(simulation);

            distBuilder.Logger = logger;

            var ppxMinutes = distBuilder.GetPutsPerX(selectedDays.First(), x);

            var ppxSchedule = new Dictionary<int, int>();



            //for (int min = startMin; min < startMin + minsInShift * (warmUpDays + 1); min += x)
            //{
            //    int startSecond = (min - startMin) * 60;
            //    ppxSchedule.Add(startSecond, ppxMinutes[((min - startMin) % (minsInShift + startMin) + startMin) / x]);
            //}

            for (int newX = 0; newX <= minsInShift * (warmUpDays + 1); newX += x)
            {
                int newStartSecond = newX * 60;
                ppxSchedule.Add(newStartSecond, ppxMinutes[(((newX % minsInShift) + startMin) / x)]);
            }

            logger.LogPutsPerHour("PPX_Schedule_" + x, ppxSchedule);

            IDestinationBlock disposalBlock = new DisposalBlock(simulation);


            IDestinationBlock P06 = new PutwallWithPPHScheduleAndTimeInQ(queueSize,
                                                               ppxSchedule,
                                                               Operators,
                                                               distBuilder.BuildProcessTimeDist(selectedDays, Process.Default),
                                                               distBuilder.BuildRecircTimeDist(selectedDays, 300),
                                                               simulation,
                                                                disposalBlock,
                                                                distBuilder.BuildTimeInQueueDistribution(selectedDays, 40000),
                                                                results);
            Location P06L = data.P06;

            Dictionary<int, IDestinationBlock> locationDict = new Dictionary<int, IDestinationBlock>()
            {
                { P06L.LocationID, P06 }
            };


            var arrivalDists = distBuilder.BuildArrivalDist(selectedDays, arrivalAnomolyLimit, intervals);

            var newArrivalDists = new Dictionary<Tuple<int, int>, IDistribution<int>>();
            for (int j = 0; j < warmUpDays + 1; j++)
            {
                for (int i = 0; i < intervals.Count; i++)
                {

                    var t = new Tuple<int, int>((int)(intervals[i].Item1.TotalMinutes - startMin) * 60 + 60 * minsInShift * j,
                        (int)(intervals[i].Item2.TotalMinutes - startMin) * 60 + 60 * minsInShift * j);
                    newArrivalDists.Add(t, arrivalDists[i]);
                }

            }



            var ArrivalBlock = new InterarrivalBlockWithBreaks(newArrivalDists, distBuilder.BuildDestinationDist(selectedDays, locationDict, disposalBlock), simulation, breakTimes);

            var firstArrival = ArrivalBlock.GetNextEvent();

            simulation.Initialize(ArrivalBlock, firstArrival);

            return simulation;
        }
        public static Simulation WIPSimWithBreaksAndOperators(List<DateTime> selectedDays,
                                                    int x,
                                                    int startMin,
                                                    int endTime,
                                                    int queueSize,
                                                    int warmUpDays,
                                                    ILogger logger,
                                                    List<Tuple<TimeSpan, TimeSpan>> intervals,
                                                    int arrivalAnomolyLimit,
                                                    List<Tuple<DateTime, DateTime>> breaks,
                                                    int nOperators)
        {
            int minsInShift = endTime / 60;
            var results = new ResultsWithWarmup(endTime * warmUpDays, endTime * (warmUpDays + 1));

            var breakTimes = new List<Tuple<int, int>>();

            foreach (var brk in breaks)
            {
                if (brk.Item1.TimeOfDay.TotalMinutes > startMin)
                {
                    for (int i = 0; i <= warmUpDays; i++)
                    {
                        int t1 = ((int)brk.Item1.TimeOfDay.TotalSeconds - startMin * 60) + (endTime * i);
                        int t2 = (int)brk.Item2.TimeOfDay.TotalSeconds - startMin * 60 + (endTime * i);

                        breakTimes.Add(new Tuple<int, int>(t1, t2));
                    }
                }
            }

            Simulation simulation = new Simulation(results, endTime * (warmUpDays + 1));

            var Operators = new List<Processor>();

            var data = new WarehouseData();

            for(int i = 0; i < nOperators; i++)
            {
                Operators.Add(new Processor());
            }

            RealDistributionBuilder distBuilder = new RealDistributionBuilder(simulation);

            distBuilder.Logger = logger;

            var ppxMinutes = distBuilder.GetPutsPerX(selectedDays.First(), x);

            var ppxSchedule = new Dictionary<int, int>();



            //for (int min = startMin; min < startMin + minsInShift * (warmUpDays + 1); min += x)
            //{
            //    int startSecond = (min - startMin) * 60;
            //    ppxSchedule.Add(startSecond, ppxMinutes[((min - startMin) % (minsInShift + startMin) + startMin) / x]);
            //}

            for (int newX = 0; newX <= minsInShift * (warmUpDays + 1); newX += x)
            {
                int newStartSecond = newX * 60;
                ppxSchedule.Add(newStartSecond, ppxMinutes[(((newX % minsInShift) + startMin) / x)]);
            }

            logger.LogPutsPerHour("PPX_Schedule_" + x, ppxSchedule);

            IDestinationBlock disposalBlock = new DisposalBlock(simulation);


            IDestinationBlock P06 = new PutwallWithPPHSchedule (queueSize,
                                                               ppxSchedule,
                                                               Operators,
                                                               distBuilder.BuildProcessTimeDist(selectedDays, Process.Default),
                                                               distBuilder.BuildRecircTimeDist(selectedDays, 300),
                                                               simulation,
                                                                disposalBlock);
            Location P06L = data.P06;

            Dictionary<int, IDestinationBlock> locationDict = new Dictionary<int, IDestinationBlock>()
            {
                { P06L.LocationID, P06 }
            };


            var arrivalDists = distBuilder.BuildArrivalDist(selectedDays, arrivalAnomolyLimit, intervals);

            var newArrivalDists = new Dictionary<Tuple<int, int>, IDistribution<int>>();
            for (int j = 0; j < warmUpDays + 1; j++)
            {
                for (int i = 0; i < intervals.Count; i++)
                {

                    var t = new Tuple<int, int>((int)(intervals[i].Item1.TotalMinutes - startMin) * 60 + 60 * minsInShift * j,
                        (int)(intervals[i].Item2.TotalMinutes - startMin) * 60 + 60 * minsInShift * j);
                    newArrivalDists.Add(t, arrivalDists[i]);
                }

            }



            var ArrivalBlock = new InterarrivalBlockWithBreaks(newArrivalDists, distBuilder.BuildDestinationDist(selectedDays, locationDict, disposalBlock), simulation, breakTimes);

            var firstArrival = ArrivalBlock.GetNextEvent();

            simulation.Initialize(ArrivalBlock, firstArrival);

            return simulation;
        }
        public static Simulation WIPSimWithBreaks(List<DateTime> selectedDays,
                                                    int x,
                                                    int startMin,
                                                    int endTime,
                                                    int queueSize,
                                                    int warmUpDays,
                                                    ILogger logger,
                                                    List<Tuple<TimeSpan, TimeSpan>> intervals,
                                                    int arrivalAnomolyLimit,
                                                    List<Tuple<DateTime, DateTime>> breaks)
        {
            int minsInShift = endTime / 60;
            var results = new ResultsWithWarmup(endTime * warmUpDays, endTime * (warmUpDays + 1));

            var breakTimes = new List<Tuple<int, int>>();

            foreach (var brk in breaks)
            {
                if (brk.Item1.TimeOfDay.TotalMinutes > startMin)
                {
                    for(int i = 0; i <= warmUpDays; i++)
                    {
                        int t1 = ((int)brk.Item1.TimeOfDay.TotalSeconds - startMin * 60) + (endTime*i);
                        int t2 = (int)brk.Item2.TimeOfDay.TotalSeconds - startMin * 60 + (endTime * i);

                        breakTimes.Add(new Tuple<int, int>(t1, t2));
                    }
                }
            }

            Simulation simulation = new Simulation(results, endTime * (warmUpDays + 1));

            var Operators = new List<Processor>();

            var data = new WarehouseData();

            Operators.Add(new Processor());

            RealDistributionBuilder distBuilder = new RealDistributionBuilder(simulation);

            distBuilder.Logger = logger;

            var ppxMinutes = distBuilder.GetPutsPerX(selectedDays.First(), x);

            var ppxSchedule = new Dictionary<int, int>();



            //for (int min = startMin; min < startMin + minsInShift * (warmUpDays + 1); min += x)
            //{
            //    int startSecond = (min - startMin) * 60;
            //    ppxSchedule.Add(startSecond, ppxMinutes[((min - startMin) % (minsInShift + startMin) + startMin) / x]);
            //}

            for (int newX = 0; newX <= minsInShift * (warmUpDays + 1); newX += x)
            {
                int newStartSecond = newX * 60;
                ppxSchedule.Add(newStartSecond, ppxMinutes[(((newX % minsInShift) + startMin) / x)]);
            }

            logger.LogPutsPerHour("PPX_Schedule_" + x, ppxSchedule);

            IDestinationBlock disposalBlock = new DisposalBlock(simulation);


            IDestinationBlock P06 = new PutwallWithPPHSchedule(queueSize,
                                                               ppxSchedule,
                                                               Operators,
                                                               distBuilder.BuildProcessTimeDist(selectedDays, Process.Default),
                                                               distBuilder.BuildRecircTimeDist(selectedDays, 300),
                                                               simulation,
                                                                disposalBlock);
            Location P06L = data.P06;

            Dictionary<int, IDestinationBlock> locationDict = new Dictionary<int, IDestinationBlock>()
            {
                { P06L.LocationID, P06 }
            };


            var arrivalDists = distBuilder.BuildArrivalDist(selectedDays, arrivalAnomolyLimit, intervals);

            var newArrivalDists = new Dictionary<Tuple<int, int>, IDistribution<int>>();
            for (int j = 0; j < warmUpDays + 1; j++)
            {
                for (int i = 0; i < intervals.Count; i++)
                {

                    var t = new Tuple<int, int>((int)(intervals[i].Item1.TotalMinutes - startMin) * 60 + 60 * minsInShift * j,
                        (int)(intervals[i].Item2.TotalMinutes - startMin) * 60 + 60 * minsInShift * j);
                    newArrivalDists.Add(t, arrivalDists[i]);
                }

            }



            var ArrivalBlock = new InterarrivalBlockWithBreaks(newArrivalDists, distBuilder.BuildDestinationDist(selectedDays, locationDict, disposalBlock), simulation, breakTimes);

            var firstArrival = ArrivalBlock.GetNextEvent();

            simulation.Initialize(ArrivalBlock, firstArrival);

            return simulation;
        }
        public static Simulation WIPSim(List<DateTime> selectedDays,
                                                    int x,
                                                    int startMin,
                                                    int endTime,
                                                    int queueSize,
                                                    int warmUpDays,
                                                    ILogger logger,
                                                    List<Tuple<TimeSpan, TimeSpan>> intervals,
                                                    int arrivalAnomolyLimit)
        {
            int minsInShift = endTime / 60;
            var results = new ResultsWithWarmup(endTime * warmUpDays, endTime*(warmUpDays+1));

            Simulation simulation = new Simulation(results, endTime * (warmUpDays + 1));
            
            var Operators = new List<Processor>();

            var data = new WarehouseData();

            Operators.Add(new Processor());

            RealDistributionBuilder distBuilder = new RealDistributionBuilder(simulation);

            distBuilder.Logger = logger;

            var ppxMinutes = distBuilder.GetPutsPerX(selectedDays.First(), x);

            var ppxSchedule = new Dictionary<int, int>();

            

            //for (int min = startMin; min < startMin + minsInShift * (warmUpDays + 1); min += x)
            //{
            //    int startSecond = (min - startMin) * 60;
            //    ppxSchedule.Add(startSecond, ppxMinutes[((min - startMin) % (minsInShift + startMin) + startMin) / x]);
            //}

            for(int newX = 0; newX <= minsInShift * (warmUpDays + 1); newX += x)
            {
                int newStartSecond = newX * 60;
                ppxSchedule.Add(newStartSecond, ppxMinutes[(((newX % minsInShift)+startMin) / x) ]);
            }

            logger.LogPutsPerHour("PPX_Schedule_" + x, ppxSchedule);

            IDestinationBlock disposalBlock = new DisposalBlock(simulation);


            IDestinationBlock P06 = new PutwallWithPPHSchedule(queueSize,
                                                               ppxSchedule,
                                                               Operators,
                                                               distBuilder.BuildProcessTimeDist(selectedDays, Process.Default),
                                                               distBuilder.BuildRecircTimeDist(selectedDays, 300),
                                                               simulation,
                                                                disposalBlock);
            Location P06L = data.P06;

            Dictionary<int, IDestinationBlock> locationDict = new Dictionary<int, IDestinationBlock>()
            {
                { P06L.LocationID, P06 }
            };


            var arrivalDists = distBuilder.BuildArrivalDist(selectedDays, arrivalAnomolyLimit, intervals);

            var newArrivalDists = new Dictionary<Tuple<int, int>, IDistribution<int>>();
            for (int j = 0; j < warmUpDays + 1; j++)
            {
                for (int i = 0; i < intervals.Count; i++)
                {
                
                    var t = new Tuple<int, int>((int)(intervals[i].Item1.TotalMinutes - startMin) * 60 + 60*minsInShift*j, 
                        (int)(intervals[i].Item2.TotalMinutes - startMin) * 60 + 60*minsInShift * j) ;
                    newArrivalDists.Add(t, arrivalDists[i]);
                }
                
            }



            var ArrivalBlock = new InterarrivalBlock2(newArrivalDists, distBuilder.BuildDestinationDist(selectedDays, locationDict, disposalBlock), simulation);

            var firstArrival = ArrivalBlock.GetNextEvent();

            simulation.Initialize(ArrivalBlock, firstArrival);

            return simulation;
        }
        public static Simulation SimWithPPXScheduleAndInterval2(List<DateTime> selectedDays,
                                                    int x,
                                                    int startMin,
                                                    int endTime,
                                                    int queueSize,
                                                    ILogger logger,
                                                    List<Tuple<TimeSpan, TimeSpan>> intervals,
                                                    int arrivalAnomolyLimit)
        {
            Simulation simulation = new Simulation(endTime);



            var Operators = new List<Processor>();

            var data = new WarehouseData();

            Operators.Add(new Processor());

            RealDistributionBuilder distBuilder = new RealDistributionBuilder(simulation);

            distBuilder.Logger = logger;

            var ppxMinutes = distBuilder.GetPutsPerX(selectedDays.First(), x);

            var ppxSchedule = new Dictionary<int, int>();

            //for (int min = startMin; min < startMin + 1440; min += x)
            //{
            //    int startSecond = (min - startMin) * 60;
            //    ppxSchedule.Add(startSecond, ppxMinutes[(min % 1440) / x]);
            //}
            int minsInShift = endTime / 60;

            for (int newX = 0; newX <= minsInShift; newX += x)
            {
                int newStartSecond = newX * 60;
                ppxSchedule.Add(newStartSecond, ppxMinutes[(((newX % minsInShift) + startMin) / x)]);
            }

            logger.LogPutsPerHour("PPX_Schedule_" + x, ppxSchedule);

            IDestinationBlock disposalBlock = new DisposalBlock(simulation);


            IDestinationBlock P06 = new PutwallWithPPHSchedule(queueSize,
                                                               ppxSchedule,
                                                               Operators,
                                                               distBuilder.BuildProcessTimeDist(selectedDays, Process.Default),
                                                               distBuilder.BuildRecircTimeDist(selectedDays, 300),
                                                               simulation,
                                                                disposalBlock);
            Location P06L = data.P06;

            Dictionary<int, IDestinationBlock> locationDict = new Dictionary<int, IDestinationBlock>()
            {
                { P06L.LocationID, P06 }
            };


            var arrivalDists = distBuilder.BuildArrivalDist(selectedDays, arrivalAnomolyLimit, intervals);

            var newArrivalDists = new Dictionary<Tuple<int,int>, IDistribution<int>>();

            for(int i = 0; i < intervals.Count; i++)
            {
                var t = new Tuple<int, int>((int)(intervals[i].Item1.TotalMinutes - startMin) * 60, (int)(intervals[i].Item2.TotalMinutes - startMin) * 60);
                newArrivalDists.Add(t, arrivalDists[i]);
            }



            var ArrivalBlock = new InterarrivalBlock2(newArrivalDists, distBuilder.BuildDestinationDist(selectedDays, locationDict, disposalBlock), simulation);

            var firstArrival = ArrivalBlock.GetNextEvent();

            simulation.Initialize(ArrivalBlock, firstArrival);

            return simulation;
        }
        public static Simulation SimWithPPXScheduleAndInterval(List<DateTime> selectedDays,
                                                    int x,
                                                    int startMin,
                                                    int endTime,
                                                    int queueSize,
                                                    ILogger logger)
        {
            Simulation simulation = new Simulation(endTime);



            var Operators = new List<Processor>();

            var data = new WarehouseData();

            Operators.Add(new Processor());

            RealDistributionBuilder distBuilder = new RealDistributionBuilder(simulation);

            distBuilder.Logger = logger;

            var ppxMinutes = distBuilder.GetPutsPerX(selectedDays.First(), x);

            var ppxSchedule = new Dictionary<int, int>();

            for (int min = startMin; min < startMin + 1440; min += x)
            {
                int startSecond = (min - startMin) * 60;
                ppxSchedule.Add(startSecond, ppxMinutes[(min % 1440) / x]);
            }

            logger.LogPutsPerHour("PPX_Schedule_" + x, ppxSchedule);

            IDestinationBlock disposalBlock = new DisposalBlock(simulation);


            IDestinationBlock P06 = new PutwallWithPPHSchedule(queueSize,
                                                               ppxSchedule,
                                                               Operators,
                                                               distBuilder.BuildProcessTimeDist(selectedDays, Process.Default),
                                                               distBuilder.BuildRecircTimeDist(selectedDays, 300),
                                                               simulation,
                                                                disposalBlock);
            Location P06L = data.P06;

            Dictionary<int, IDestinationBlock> locationDict = new Dictionary<int, IDestinationBlock>()
            {
                { P06L.LocationID, P06 }
            };


            Dictionary<DateTime, int> breakpoints = new Dictionary<DateTime, int>()
            {
                { new DateTime(1,1,1,7,0,0), 0 },
                { new DateTime(1,1,1,21,0,0), 3600 },
                { new DateTime(1,1,1,22,0,0), 54000 },
            };
            

            var ArrivalBlock = new IntervalArrivalBlock(distBuilder.BuildIntervalDists(breakpoints, selectedDays, 600), distBuilder.BuildDestinationDist(selectedDays, locationDict, disposalBlock), simulation);

            var firstArrival = ArrivalBlock.GetNextEvent();

            simulation.Initialize(ArrivalBlock, firstArrival);

            return simulation;
        }
        public static Simulation SimWithPPXSchedule(List<DateTime> selectedDays,
                                                    int x,
                                                    int startMin,
                                                    int endTime,
                                                    int queueSize,
                                                    ILogger logger)
        {
            Simulation simulation = new Simulation(endTime);



            var Operators = new List<Processor>();

            var data = new WarehouseData();

            Operators.Add(new Processor());

            RealDistributionBuilder distBuilder = new RealDistributionBuilder(simulation);

            distBuilder.Logger = logger;

            var ppxMinutes = distBuilder.GetPutsPerX(selectedDays.First(), x);

            var ppxSchedule = new Dictionary<int, int>();

            for (int min = startMin; min < startMin + 1440; min+=x)
            {
                int startSecond = (min - startMin) * 60;
                ppxSchedule.Add(startSecond, ppxMinutes[(min % 1440)/x]);
            }

            logger.LogPutsPerHour("PPX_Schedule_" + x, ppxSchedule);

            IDestinationBlock disposalBlock = new DisposalBlock(simulation);


            IDestinationBlock P06 = new PutwallWithPPHSchedule(queueSize,
                                                               ppxSchedule,
                                                               Operators,
                                                               distBuilder.BuildProcessTimeDist(selectedDays, Process.Default),
                                                               distBuilder.BuildRecircTimeDist(selectedDays, 300),
                                                               simulation,
                                                                disposalBlock);
            Location P06L = data.P06;

            Dictionary<int, IDestinationBlock> locationDict = new Dictionary<int, IDestinationBlock>()
            {
                { P06L.LocationID, P06 }
            };

            var ArrivalBlock = new ArrivalBlock(distBuilder.BuildArrivalDist(selectedDays, 300), distBuilder.BuildDestinationDist(selectedDays, locationDict, disposalBlock), simulation);

            var firstArrival = ArrivalBlock.GetNextEvent();

            simulation.Initialize(ArrivalBlock, firstArrival);

            return simulation;
        }
        public static Simulation SimWithPPHSchedule(List<DateTime> selectedDays, 
                                                    int startHour,
                                                    int endTime, 
                                                    int queueSize,
                                                    ILogger logger)
        {
            Simulation simulation = new Simulation(endTime);

            

            var Operators = new List<Processor>();

            var data = new WarehouseData();
            
            Operators.Add(new Processor());

            RealDistributionBuilder distBuilder = new RealDistributionBuilder(simulation);

            distBuilder.Logger = logger;

            var pphHourly = distBuilder.GetPutsPerHour(selectedDays.First());

            var pphSchedule = new Dictionary<int, int>();

            for(int hour=startHour;hour<startHour + 24; hour++)
            {
                int startSecond = (hour - startHour) * 3600;
                pphSchedule.Add(startSecond, pphHourly[hour % 24]);
            }

            logger.LogPutsPerHour("PPH_Schedule", pphSchedule);

            IDestinationBlock disposalBlock = new DisposalBlock(simulation);


            IDestinationBlock P06 = new PutwallWithPPHSchedule(queueSize,
                                                               pphSchedule,
                                                               Operators,
                                                               distBuilder.BuildProcessTimeDist(selectedDays, Process.Default),
                                                               distBuilder.BuildRecircTimeDist(selectedDays, 300),
                                                               simulation,
                                                                disposalBlock);
            Location P06L = data.P06;

            Dictionary<int, IDestinationBlock> locationDict = new Dictionary<int, IDestinationBlock>()
            {
                { P06L.LocationID, P06 }
            };

            var ArrivalBlock = new ArrivalBlock(distBuilder.BuildArrivalDist(selectedDays, 300), distBuilder.BuildDestinationDist(selectedDays, locationDict, disposalBlock), simulation);

            var firstArrival = ArrivalBlock.GetNextEvent();

            simulation.Initialize(ArrivalBlock, firstArrival);

            return simulation;
        }
        public static Simulation SimWithOperatorSchedule(List<DateTime> selectedDays, int endTime, Dictionary<int, int> operatorSchedule, ILogger logger)
        {
            Simulation simulation = new Simulation(endTime);

            var Operators = new List<Processor>();

            var data = new WarehouseData();

            for (int i = 1; i <= operatorSchedule.Values.Max(); i++)
            {
                Operators.Add(new Processor());
            }

            IDistributionBuilder distBuilder = new RealDistributionBuilder(simulation);

            distBuilder.Logger = logger;


            IDestinationBlock disposalBlock = new DisposalBlock(simulation);


            IDestinationBlock P06 = new PutwallWithOperatorSchedule(operatorSchedule, 
                                                                    Operators, 
                                                                    distBuilder.BuildProcessTimeDist(selectedDays, Process.Default), 
                                                                    distBuilder.BuildRecircTimeDist(selectedDays), 
                                                                    simulation, 
                                                                    disposalBlock);
            Location P06L = data.P06;

            Dictionary<int, IDestinationBlock> locationDict = new Dictionary<int, IDestinationBlock>()
            {
                { P06L.LocationID, P06 }
            };

            var ArrivalBlock = new ArrivalBlock(distBuilder.BuildArrivalDist(selectedDays), distBuilder.BuildDestinationDist(selectedDays, locationDict, disposalBlock), simulation);

            var firstArrival = ArrivalBlock.GetNextEvent();

            simulation.Initialize(ArrivalBlock, firstArrival);

            return simulation;
        }
        public static Simulation DefaultSimulation(List<DateTime> selectedDays, int endTime, int nProcessors, ILogger logger)
        {
            Simulation simulation = new Simulation(endTime);

            var Operators = new List<Processor>();

            var data = new WarehouseData();

            for(int i = 1; i <= nProcessors;i++)
            {
                Operators.Add(new Processor());
            }

            IDistributionBuilder distBuilder = new RealDistributionBuilder(simulation);

            distBuilder.Logger = logger;


            IDestinationBlock disposalBlock = new DisposalBlock(simulation);


            IDestinationBlock P06 = new Putwall(Operators, distBuilder.BuildProcessTimeDist(selectedDays, Process.Default), distBuilder.BuildRecircTimeDist(selectedDays), simulation, disposalBlock);
            Location P06L = data.P06;

            Dictionary<int, IDestinationBlock> locationDict = new Dictionary<int, IDestinationBlock>()
            {
                { P06L.LocationID, P06 }
            };
            
            var ArrivalBlock = new ArrivalBlock(distBuilder.BuildArrivalDist(selectedDays), distBuilder.BuildDestinationDist(selectedDays, locationDict, disposalBlock), simulation);

            var firstArrival = ArrivalBlock.GetNextEvent();

            simulation.Initialize(ArrivalBlock, firstArrival);

            return simulation;
        }
        public static Simulation FakeSimulation(List<DateTime> selectedDays)
        {
            int endTime = 100000;
            Simulation simulation = new Simulation(endTime);

            
            IDistributionBuilder distBuilder = new FakeDataBuilder();
            FakeDataBuilder destDistBuilder = new FakeDataBuilder();

            IDestinationBlock disposalBlock = new DisposalBlock(simulation);
            

            var Operators = new List<Processor>()
            {
                new Processor(),
                new Processor(),
                new Processor(),
                new Processor(),
                new Processor(),
                new Processor(),
                new Processor(),
                new Processor()
            };

            IDestinationBlock P06 = new Putwall(Operators, distBuilder.BuildProcessTimeDist(selectedDays, Process.Default), distBuilder.BuildRecircTimeDist(selectedDays), simulation, disposalBlock);


            List<Tuple<double, IDestinationBlock>> DestinationData = new List<Tuple<double, IDestinationBlock>>()
            {
                new Tuple<double, IDestinationBlock>(0.1, new NonPutwallLane(simulation, disposalBlock)),
                new Tuple<double, IDestinationBlock>(0.1, new NonPutwallLane(simulation, disposalBlock)),
                new Tuple<double, IDestinationBlock>(0.1, new NonPutwallLane(simulation, disposalBlock)),
                new Tuple<double, IDestinationBlock>(0.1, new NonPutwallLane(simulation, disposalBlock)),
                new Tuple<double, IDestinationBlock>(0.6, P06),
            };

            destDistBuilder.FakeDestData = DestinationData;

            var ArrivalBlock = new ArrivalBlock(destDistBuilder.BuildArrivalDist(selectedDays), destDistBuilder.BuildDestinationDist(selectedDays, null, disposalBlock), simulation);

            List<IDestinationBlock> putwallLanes = DestinationData.Select(x => x.Item2).ToList();
            putwallLanes.Add(P06);

            var firstArrival = ArrivalBlock.GetNextEvent();

            simulation.Initialize(ArrivalBlock, firstArrival);

            return simulation;
        }
    }
}
