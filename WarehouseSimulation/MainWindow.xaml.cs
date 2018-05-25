using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SimulationObjects;
using Infrastructure;
using SimulationObjects.Utils;
using SimulationObjects.Entities;
using SimulationObjects.Results;

namespace WarehouseSimulation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public SimulationType SimType { get; set; } = SimulationType.DefaultSim;

        public MainWindow()
        {
            DataContext = this;
            InitializeComponent();

            //RunDefaultSim();

            //RunPPXSim();
            //newPPXSim();


            //wipSim();
            //wipSimWithBreaksAndQTime2();
            //wipSimWithBreaksAndOperators();

            //SimWithFullQ();
            //SimWithDifferentWarmupDay();
            //newSimP();

            //SpecificQueueNoQueueTime();
            //SimWithOperatorCount();
            //ConsumeAllSim();
            //SimWithSmoothedCapacity();
            //SimWithConstantPOfRecirc();
            //int maxInterval = 500;

            string basePath = @"C:\Users\p2decarv\Desktop\SimLog\";
            //string basePath = @"C:\Users\Daniel\Desktop\SimLog\";
            //string basePath = @"C:\Users\Dematic\Desktop\SimLog\";

            //string resultsPath = basePath + "Results.txt";
            //using(var writer = new System.IO.StreamWriter(resultsPath, true))
            //{
            //    writer.WriteLine("Arrival Interval \t Number Created \t Number Disposed \t Time in System \t Time in Queue \t Time in Recirc \t Times Recirced \t Time in Put");
            //}
            //for(int i = 5; i <= maxInterval; i++)
            //{

            //    var results = IHateLife(i, basePath);
            //    using (var writer = new System.IO.StreamWriter(resultsPath, true))
            //    {
            //        writer.WriteLine(i + "\t" + results.PutwallStatistics.NumberCreated.Average + "\t" + 
            //            results.PutwallStatistics.NumberDisposed.Average + "\t" + 
            //            results.PutwallStatistics.TimeInSystem.Average + "\t" + 
            //            results.PutwallStatistics.TimeInQueue.Average + "\t" + 
            //            results.PutwallStatistics.TimeRecirculating.Average +"\t" +
            //            results.PutwallStatistics.TimesRecirculated.Average + "\t" +
            //            results.PutwallStatistics.TimeInProcess.Average);
            //    }
            //}

            //MultiArrivalIntervalSim(basePath);
            //MultiArrivalIntervalSim(basePath);
            GetInterArrivals(basePath);
        }

        private void GetInterArrivals(string basePath)
        {
            var dType = WarehouseDataType.PutwallOnlyData;
            var Data = new WarehouseData(dType);
            var availability = new List<DateTime>()
            {
                new DateTime(2015,11,14)
            };

            var logger = new VerboseLogger(basePath);

            var statsInterval = new Tuple<TimeSpan, TimeSpan>(new TimeSpan(0, 0, 0), new TimeSpan(23, 59, 59));
            
            var arrivalTimesOverTime = Data.GetInterarrivalTimesOverTime(availability[0]);
            using (var writer = new System.IO.StreamWriter(basePath + "InterArrivals_over_time_real.txt"))
            {
                writer.WriteLine("Time \t Interarrival Time");

                foreach (var t in arrivalTimesOverTime)
                {
                    writer.WriteLine(t.Item1 + "\t" + t.Item2);
                }
            }

            var cumulativeArrival = Data.CalcCumArrivalCount(availability[0], statsInterval);
            using(var writer = new System.IO.StreamWriter(basePath + "CumulativeArrivals_real.txt"))
            {
                foreach (var entry in cumulativeArrival)
                    writer.WriteLine("{0} \t {1}", TimeSpan.Parse(entry.Key.ToString()).TotalSeconds, entry.Value);
            }
                 
        }

        


        private void MultiArrivalAutoIntervalSim(string basePath)
        {
            var dType = WarehouseDataType.PutwallOnlyData;
            var Data = new WarehouseData(dType);

            var FinalResults = new MetaResults();

            var availability = new List<DateTime>()
            {
                new DateTime(2015,11,10)
            };

            var qSizeData = new List<DateTime>()
            {
                new DateTime(2015,11,9),
                new DateTime(2015,11,10),
                new DateTime(2015,11,11),
                new DateTime(2015,11,12),
                new DateTime(2015,11,13)
            };
            var statsInterval = new Tuple<TimeSpan, TimeSpan>(new TimeSpan(7, 0, 0), new TimeSpan(23, 0, 0));

            var arrivalTimesOverTime = Data.GetInterarrivalTimesOverTime(availability[0]);
            var recircTimesOverTime = Data.GetItemsInRecircOverTime(availability[0]);
            var queueSizeOverTime = Data.GetQueueSizeOverTime(qSizeData, availability[0]);
            var qTimes = Data.GetTimeInQueue(availability[0], statsInterval);



            using (var writer = new System.IO.StreamWriter(basePath + "InterArrivals_over_time_real.txt"))
            {
                writer.WriteLine("Time \t Interarrival Time");

                foreach (var t in arrivalTimesOverTime)
                {
                    writer.WriteLine(t.Item1 + "\t" + t.Item2);
                }
            }

            using (var writer = new System.IO.StreamWriter(basePath + "ItemsInRecirc_over_time_real.txt"))
            {
                writer.WriteLine("Time of Day (s) \t Items in Reicirculation");

                for (int i = 0; i < recircTimesOverTime.Length; i++)
                {
                    writer.WriteLine(i + "\t" + recircTimesOverTime[i]);
                }
            }

            using (var writer = new System.IO.StreamWriter(basePath + "QueueSize_over_time_real.txt"))
            {
                writer.WriteLine("Time of Day (s) \t Items in Queue");

                for (int i = 0; i < queueSizeOverTime.Length; i++)
                {
                    writer.WriteLine(i + "\t" + queueSizeOverTime[i]);
                }
            }

            using (var writer = new System.IO.StreamWriter(basePath + "QTimes.txt"))
            {
                writer.WriteLine("Time in Queue");

                foreach (double time in qTimes)
                {
                    writer.WriteLine(time);
                }
            }




            var logger = new VerboseLogger(basePath);

            var intervals = Data.DistributionBreakpoints(availability[0], statsInterval, 1000, 100);
            using (var writer = new System.IO.StreamWriter(basePath + "ArrivalIntervals.txt"))
            {
                writer.WriteLine("Interval Start \t Interval End");

                foreach(var t in intervals)
                {
                    writer.WriteLine(t.Item1 + "\t" + t.Item2);
                }
            }


            int startMin = 420;
            int dayLength = 57600;

            



            //logger.LogDBStats("DBStats", availability.First());
            //logger.LogDBStats("DBStats", availability.First(), 600);


            int nIterations = 100;
            List<Tuple<int, int>> throughput = new List<Tuple<int, int>>();
            var iterations = Enumerable.Range(0, nIterations);


            int capacityInterval = 30;
            int innerInterval = 10;





            var factoryParams = new FactoryParams()
            {
                StartMin = startMin,
                //DayLength = 59400,
                DayLength = dayLength,
                Logger = logger,
                NWarmupDays = 0,
                InitialNumberInQueue = queueSizeOverTime[startMin * 60]
                //InitialNumberInQueue = 51
            };

            var breakTimes = Data.FindBreaks(availability[0], factoryParams.ArrivalAnomolyLimit);

            logger.LogDBStats("DBStats2", availability.First(), factoryParams.ProcessTimeAnomolyLimit, statsInterval, dType);

            var distParams = new DistributionSelectionParameters()
            {
                SelectedDaysForData = availability,
                ArrivalDistributionBreakpoints = intervals,
                BreakTimes = breakTimes,
                IntervalForOtherDistributions = statsInterval,
                CapacityIntervalSize = capacityInterval,
                QueueSizeData = qSizeData
            };





            var allDists = new MultiArrivalIntervalDists(factoryParams, distParams, innerInterval, dType, intervals);

            var dists = allDists.CreateNCopies(nIterations);


            Parallel.ForEach(iterations, i =>
            {

                var dist = (MultiArrivalIntervalDists)dists[i];


                var sim = SimulationFactory.MultiArrivalIntervalSim(factoryParams, dist, dType);

                dist.UpDateNoArrivalDists(sim);

                sim.Run();


                logger.LogResults(sim.Results, "Results_" + i, (factoryParams.NWarmupDays + 1) * factoryParams.DayLength);


                FinalResults.AddSimResults(sim.Results);

                var t = sim.Results.CalcNumOut();

                if (t.ContainsKey(ProcessType.NonPutwall))
                {
                    throughput.Add(new Tuple<int, int>(t[ProcessType.Putwall], t[ProcessType.NonPutwall]));
                }
                else
                {
                    throughput.Add(new Tuple<int, int>(t[ProcessType.Putwall], 0));
                }


                OutputTimeSeries(i, basePath, sim.Results);

                OutputTimedQueueSize(i, basePath, sim.Results);

                WriteOutTimeInQ(i, basePath, sim.Results);

                WriteOutLeftoverCapacity(i, basePath, sim.Results.LeftOverCapacity);

                logger.LogPutsPerHour("Cumulative_Arrrivals_Sim_" + i, sim.Results.OutputCumulativeArrivalsOverTime());
            });


            using (var writer = new System.IO.StreamWriter(basePath + "FINALRESULTS.txt"))
            {
                foreach (Tuple<int, int> t2 in throughput)
                {
                    writer.WriteLine(t2.Item1.ToString() + '\t' + t2.Item2.ToString());
                }

                writer.WriteLine();
                writer.WriteLine("Putwall Stats");
                writer.WriteLine("Property \t Average \t StdDev");
                writer.WriteLine("Number Created \t" + FinalResults.PutwallStatistics.NumberCreated.Average + "\t" +
                   FinalResults.PutwallStatistics.NumberCreated.StdDev);
                writer.WriteLine("Number Disposed \t" + FinalResults.PutwallStatistics.NumberDisposed.Average + "\t" +
                  FinalResults.PutwallStatistics.NumberDisposed.StdDev);
                writer.WriteLine("Time in System \t" + FinalResults.PutwallStatistics.TimeInSystem.Average + "\t" +
                    FinalResults.PutwallStatistics.TimeInSystem.StdDev);
                writer.WriteLine("Time in Queue \t" + FinalResults.PutwallStatistics.TimeInQueue.Average + "\t" +
                    FinalResults.PutwallStatistics.TimeInQueue.StdDev);
                writer.WriteLine("Time Recirculating \t" + FinalResults.PutwallStatistics.TimeRecirculating.Average + "\t" +
                    FinalResults.PutwallStatistics.TimeRecirculating.StdDev);
                writer.WriteLine("Times Recirculated \t" + FinalResults.PutwallStatistics.TimesRecirculated.Average + "\t" +
                    FinalResults.PutwallStatistics.TimesRecirculated.StdDev);
                writer.WriteLine("Time in Process \t" + FinalResults.PutwallStatistics.TimeInProcess.Average + "\t" +
                    FinalResults.PutwallStatistics.TimeInProcess.StdDev);


                writer.WriteLine();
                writer.WriteLine("NonPutwall Stats");
                writer.WriteLine("Property \t Average \t StdDev");
                writer.WriteLine("Number Created \t" + FinalResults.NonPutwallStatistics.NumberCreated.Average + "\t" +
                   FinalResults.NonPutwallStatistics.NumberCreated.StdDev);
                writer.WriteLine("Number Disposed \t" + FinalResults.NonPutwallStatistics.NumberDisposed.Average + "\t" +
                  FinalResults.NonPutwallStatistics.NumberDisposed.StdDev);
                writer.WriteLine("Time in system \t" + FinalResults.NonPutwallStatistics.TimeInSystem.Average + "\t" +
                    FinalResults.NonPutwallStatistics.TimeInSystem.StdDev);
                writer.WriteLine("Time in Queue \t" + FinalResults.NonPutwallStatistics.TimeInQueue.Average + "\t" +
                    FinalResults.NonPutwallStatistics.TimeInQueue.StdDev);
                writer.WriteLine("Time Recirculating \t" + FinalResults.NonPutwallStatistics.TimeRecirculating.Average + "\t" +
                    FinalResults.NonPutwallStatistics.TimeRecirculating.StdDev);
                writer.WriteLine("Times Recirculated \t" + FinalResults.NonPutwallStatistics.TimesRecirculated.Average + "\t" +
                    FinalResults.NonPutwallStatistics.TimesRecirculated.StdDev);
                writer.WriteLine("Time in Process \t" + FinalResults.NonPutwallStatistics.TimeInProcess.Average + "\t" +
                    FinalResults.NonPutwallStatistics.TimeInProcess.StdDev);


            }
        }
        private void MultiArrivalIntervalSim(string basePath)
        {
            var dType = WarehouseDataType.PutwallOnlyData;
            var Data = new WarehouseData(dType);

            var FinalResults = new MetaResults();

            var availability = new List<DateTime>()
            {
                new DateTime(2015,11,10)
            };

            var qSizeData = new List<DateTime>()
            {
                new DateTime(2015,11,6),
                new DateTime(2015,11,7),
                new DateTime(2015,11,9),
                new DateTime(2015,11,10),
                new DateTime(2015,11,11),
                new DateTime(2015,11,12),
                new DateTime(2015,11,13),
                new DateTime(2015,11,14),
                new DateTime(2015,11,15),
                new DateTime(2015,11,16)

            };
            var statsInterval = new Tuple<TimeSpan, TimeSpan>(new TimeSpan(0, 0, 1), new TimeSpan(23, 59, 59));

            var arrivalTimesOverTime = Data.GetInterarrivalTimesOverTime(availability[0]);
            var recircTimesOverTime = Data.GetItemsInRecircOverTime(availability[0]);
            var queueSizeOverTime = Data.GetQueueSizeOverTime(qSizeData, availability[0]);
            var qTimes = Data.GetTimeInQueue(availability[0], statsInterval);



            using (var writer = new System.IO.StreamWriter(basePath + "InterArrivals_over_time_real.txt"))
            {
                writer.WriteLine("Time \t Interarrival Time");

                foreach (var t in arrivalTimesOverTime)
                {
                    writer.WriteLine(t.Item1 + "\t" + t.Item2);
                }
            }

            using (var writer = new System.IO.StreamWriter(basePath + "ItemsInRecirc_over_time_real.txt"))
            {
                writer.WriteLine("Time of Day (s) \t Items in Reicirculation");

                for (int i = 0; i < recircTimesOverTime.Length; i++)
                {
                    writer.WriteLine(i + "\t" + recircTimesOverTime[i]);
                }
            }

            using (var writer = new System.IO.StreamWriter(basePath + "QueueSize_over_time_real.txt"))
            {
                writer.WriteLine("Time of Day (s) \t Items in Queue");

                for (int i = 0; i < queueSizeOverTime.Length; i++)
                {
                    writer.WriteLine(i + "\t" + queueSizeOverTime[i]);
                }
            }

            using (var writer = new System.IO.StreamWriter(basePath + "QTimes.txt"))
            {
                writer.WriteLine("Time in Queue");

                foreach (double time in qTimes)
                {
                    writer.WriteLine(time);
                }
            }




            var logger = new VerboseLogger(basePath);

            var intervals = new List<Tuple<TimeSpan, TimeSpan>>()
            {
                new Tuple<TimeSpan, TimeSpan>(new TimeSpan(0,0,25200),new TimeSpan(0,0,29328)),
                new Tuple<TimeSpan, TimeSpan>(new TimeSpan(0,0,29328),new TimeSpan(0,0,36229)),
                new Tuple<TimeSpan, TimeSpan>(new TimeSpan(0,0,36229),new TimeSpan(0,0,36609)),
                new Tuple<TimeSpan, TimeSpan>(new TimeSpan(0,0,36609),new TimeSpan(0,0,39814)),
                new Tuple<TimeSpan, TimeSpan>(new TimeSpan(0,0,39814),new TimeSpan(0,0,39894)),
                new Tuple<TimeSpan, TimeSpan>(new TimeSpan(0,0,39894),new TimeSpan(0,0,45643)),
                new Tuple<TimeSpan, TimeSpan>(new TimeSpan(0,0,45643),new TimeSpan(0,0,49189)),
                new Tuple<TimeSpan, TimeSpan>(new TimeSpan(0,0,49189),new TimeSpan(0,0,49430)),
                new Tuple<TimeSpan, TimeSpan>(new TimeSpan(0,0,49430),new TimeSpan(0,0,49640)),
                new Tuple<TimeSpan, TimeSpan>(new TimeSpan(0,0,49640),new TimeSpan(0,0,54273)),
                new Tuple<TimeSpan, TimeSpan>(new TimeSpan(0,0,54273),new TimeSpan(0,0,58671)),
                new Tuple<TimeSpan, TimeSpan>(new TimeSpan(0,0,58671),new TimeSpan(0,0,60208)),
                new Tuple<TimeSpan, TimeSpan>(new TimeSpan(0,0,60208),new TimeSpan(0,0,60338)),
                new Tuple<TimeSpan, TimeSpan>(new TimeSpan(0,0,60338),new TimeSpan(0,0,60822)),
                new Tuple<TimeSpan, TimeSpan>(new TimeSpan(0,0,60822),new TimeSpan(0,0,62980)),
                new Tuple<TimeSpan, TimeSpan>(new TimeSpan(0,0,62980),new TimeSpan(0,0,67305)),
                new Tuple<TimeSpan, TimeSpan>(new TimeSpan(0,0,67305),new TimeSpan(0,0,68317)),
                new Tuple<TimeSpan, TimeSpan>(new TimeSpan(0,0,68317),new TimeSpan(0,0,69905)),
                new Tuple<TimeSpan, TimeSpan>(new TimeSpan(0,0,69905),new TimeSpan(0,0,70492)),
                new Tuple<TimeSpan, TimeSpan>(new TimeSpan(0,0,70492),new TimeSpan(0,0,74583)),
                new Tuple<TimeSpan, TimeSpan>(new TimeSpan(0,0,74583),new TimeSpan(0,0,75425)),
                new Tuple<TimeSpan, TimeSpan>(new TimeSpan(0,0,75425),new TimeSpan(0,0,81023)),
                new Tuple<TimeSpan, TimeSpan>(new TimeSpan(0,0,81023),new TimeSpan(0,0,82800)),

            };

            //int ArrivalInterval = 480;
            int startMin = 420;
            int dayLength = 57600;

/*
            for (int i = startMin; i < (dayLength/60) + startMin; i += ArrivalInterval)
            {
                if (((double)(i + ArrivalInterval) / 60) == 24)
                {
                    intervals.Add(new Tuple<TimeSpan, TimeSpan>(new TimeSpan((int)Math.Floor((double)i / 60), i % 60, 0),
                    new TimeSpan(23, 59, 59)));
                }
                else
                {
                    intervals.Add(new Tuple<TimeSpan, TimeSpan>(new TimeSpan((int)Math.Floor((double)i / 60), i % 60, 0),
                    new TimeSpan((int)Math.Floor((double)(i + ArrivalInterval) / 60), (i + ArrivalInterval) % 60, 0)));
                }

            }

*/
            
            logger.LogDBStats("DBStats", availability.First());
            //logger.LogDBStats("DBStats", availability.First(), 600);


            int nIterations = 1;
            List<Tuple<int, int>> throughput = new List<Tuple<int, int>>();
            var iterations = Enumerable.Range(0, nIterations);
            

            int capacityInterval = 30;
            int innerInterval = 10;

            

            

            var factoryParams = new FactoryParams()
            {
                StartMin = startMin,
                //DayLength = 59400,
                DayLength = dayLength,
                Logger = logger,
                NWarmupDays = 0,
                InitialNumberInQueue = queueSizeOverTime[startMin * 60]
                //InitialNumberInQueue = 51
            };

            var breakTimes = Data.FindBreaks(availability[0], factoryParams.ArrivalAnomolyLimit);

            logger.LogDBStats("DBStats2", availability.First(), factoryParams.ProcessTimeAnomolyLimit, statsInterval, dType);

            var distParams = new DistributionSelectionParameters()
            {
                SelectedDaysForData = availability,
                ArrivalDistributionBreakpoints = intervals,
                BreakTimes = breakTimes,
                IntervalForOtherDistributions = statsInterval,
                CapacityIntervalSize = capacityInterval,
                QueueSizeData = qSizeData
            };





            var allDists = new MultiArrivalIntervalDists(factoryParams, distParams, innerInterval, dType, intervals);

            var dists = allDists.CreateNCopies(nIterations);


            Parallel.ForEach(iterations, i =>
            {

                var dist = (MultiArrivalIntervalDists)dists[i];

                
                var sim = SimulationFactory.MultiArrivalIntervalSim(factoryParams, dist, dType);

                dist.UpDateNoArrivalDists(sim);

                sim.Run();


                logger.LogResults(sim.Results, "Results_" + i, (factoryParams.NWarmupDays + 1) * factoryParams.DayLength);


                FinalResults.AddSimResults(sim.Results);

                var t = sim.Results.CalcNumOut();

                if (t.ContainsKey(ProcessType.NonPutwall))
                {
                    throughput.Add(new Tuple<int, int>(t[ProcessType.Putwall], t[ProcessType.NonPutwall]));
                }
                else
                {
                    throughput.Add(new Tuple<int, int>(t[ProcessType.Putwall], 0));
                }


                OutputTimeSeries(i, basePath, sim.Results);

                OutputTimedQueueSize(i, basePath, sim.Results);

                WriteOutTimeInQ(i, basePath, sim.Results);

                WriteOutLeftoverCapacity(i, basePath, sim.Results.LeftOverCapacity);

                logger.LogPutsPerHour("Cumulative_Arrrivals_Sim_" + i, sim.Results.OutputCumulativeArrivalsOverTime());
            });


            using (var writer = new System.IO.StreamWriter(basePath + "FINALRESULTS.txt"))
            {
                foreach (Tuple<int, int> t2 in throughput)
                {
                    writer.WriteLine(t2.Item1.ToString() + '\t' + t2.Item2.ToString());
                }

                writer.WriteLine();
                writer.WriteLine("Putwall Stats");
                writer.WriteLine("Property \t Average \t StdDev");
                writer.WriteLine("Number Created \t" + FinalResults.PutwallStatistics.NumberCreated.Average + "\t" +
                   FinalResults.PutwallStatistics.NumberCreated.StdDev);
                writer.WriteLine("Number Disposed \t" + FinalResults.PutwallStatistics.NumberDisposed.Average + "\t" +
                  FinalResults.PutwallStatistics.NumberDisposed.StdDev);
                writer.WriteLine("Time in System \t" + FinalResults.PutwallStatistics.TimeInSystem.Average + "\t" +
                    FinalResults.PutwallStatistics.TimeInSystem.StdDev);
                writer.WriteLine("Time in Queue \t" + FinalResults.PutwallStatistics.TimeInQueue.Average + "\t" +
                    FinalResults.PutwallStatistics.TimeInQueue.StdDev);
                writer.WriteLine("Time Recirculating \t" + FinalResults.PutwallStatistics.TimeRecirculating.Average + "\t" +
                    FinalResults.PutwallStatistics.TimeRecirculating.StdDev);
                writer.WriteLine("Times Recirculated \t" + FinalResults.PutwallStatistics.TimesRecirculated.Average + "\t" +
                    FinalResults.PutwallStatistics.TimesRecirculated.StdDev);
                writer.WriteLine("Time in Process \t" + FinalResults.PutwallStatistics.TimeInProcess.Average + "\t" +
                    FinalResults.PutwallStatistics.TimeInProcess.StdDev);


                writer.WriteLine();
                writer.WriteLine("NonPutwall Stats");
                writer.WriteLine("Property \t Average \t StdDev");
                writer.WriteLine("Number Created \t" + FinalResults.NonPutwallStatistics.NumberCreated.Average + "\t" +
                   FinalResults.NonPutwallStatistics.NumberCreated.StdDev);
                writer.WriteLine("Number Disposed \t" + FinalResults.NonPutwallStatistics.NumberDisposed.Average + "\t" +
                  FinalResults.NonPutwallStatistics.NumberDisposed.StdDev);
                writer.WriteLine("Time in system \t" + FinalResults.NonPutwallStatistics.TimeInSystem.Average + "\t" +
                    FinalResults.NonPutwallStatistics.TimeInSystem.StdDev);
                writer.WriteLine("Time in Queue \t" + FinalResults.NonPutwallStatistics.TimeInQueue.Average + "\t" +
                    FinalResults.NonPutwallStatistics.TimeInQueue.StdDev);
                writer.WriteLine("Time Recirculating \t" + FinalResults.NonPutwallStatistics.TimeRecirculating.Average + "\t" +
                    FinalResults.NonPutwallStatistics.TimeRecirculating.StdDev);
                writer.WriteLine("Times Recirculated \t" + FinalResults.NonPutwallStatistics.TimesRecirculated.Average + "\t" +
                    FinalResults.NonPutwallStatistics.TimesRecirculated.StdDev);
                writer.WriteLine("Time in Process \t" + FinalResults.NonPutwallStatistics.TimeInProcess.Average + "\t" +
                    FinalResults.NonPutwallStatistics.TimeInProcess.StdDev);


            }
        }
        private void MultiArrivalSim(string basePath)
        {
            var dType = WarehouseDataType.AllData;
            var Data = new WarehouseData(dType);

            var FinalResults = new MetaResults();

            var availability = new List<DateTime>()
            {
                new DateTime(2015,11,10)
            };

            var qSizeData = new List<DateTime>()
            {
                new DateTime(2015,11,9),
                new DateTime(2015,11,10),
                new DateTime(2015,11,11),
                new DateTime(2015,11,12),
                new DateTime(2015,11,13)
            };
            var statsInterval = new Tuple<TimeSpan, TimeSpan>(new TimeSpan(7, 0 , 0), new TimeSpan(23, 0, 0));

            var arrivalTimesOverTime = Data.GetInterarrivalTimesOverTime(availability[0]);
            var recircTimesOverTime = Data.GetItemsInRecircOverTime(availability[0]);
            var queueSizeOverTime = Data.GetQueueSizeOverTime(qSizeData, availability[0]);
            var qTimes = Data.GetTimeInQueue(availability[0], statsInterval);



            using (var writer = new System.IO.StreamWriter(basePath + "InterArrivals_over_time_real.txt"))
            {
                writer.WriteLine("Time \t Interarrival Time");

                foreach (var t in arrivalTimesOverTime)
                {
                    writer.WriteLine(t.Item1 + "\t" + t.Item2);
                }
            }

            using (var writer = new System.IO.StreamWriter(basePath + "ItemsInRecirc_over_time_real.txt"))
            {
                writer.WriteLine("Time of Day (s) \t Items in Reicirculation");

                for (int i = 0; i < recircTimesOverTime.Length; i++)
                {
                    writer.WriteLine(i + "\t" + recircTimesOverTime[i]);
                }
            }

            using (var writer = new System.IO.StreamWriter(basePath + "QueueSize_over_time_real.txt"))
            {
                writer.WriteLine("Time of Day (s) \t Items in Queue");

                for (int i = 0; i < queueSizeOverTime.Length; i++)
                {
                    writer.WriteLine(i + "\t" + queueSizeOverTime[i]);
                }
            }

            using (var writer = new System.IO.StreamWriter(basePath + "QTimes.txt"))
            {
                writer.WriteLine("Time in Queue");

                foreach (double time in qTimes)
                {
                    writer.WriteLine(time);
                }
            }




            var logger = new VerboseLogger(basePath);



            logger.LogDBStats("DBStats2", availability.First(), 600, statsInterval, dType);
            //logger.LogDBStats("DBStats", availability.First());
            //logger.LogDBStats("DBStats", availability.First(), 600);


            int nIterations = 100;
            List<Tuple<int, int>> throughput = new List<Tuple<int, int>>();
            var iterations = Enumerable.Range(0, nIterations);

            var intervals = new List<Tuple<TimeSpan, TimeSpan>>();

            int capacityInterval = 30;
            int innerInterval = 10;

            var operaterCount = Data.GetOperatorsPerZMins(availability[0], capacityInterval);
            logger.LogPutsPerHour("Operators_Per_" + capacityInterval + "mins_MinsFromMidnight", operaterCount);

            
            var breakTimes = Data.FindBreaks(availability[0], 600);

            int dayLength = 57600;
            var factoryParams = new FactoryParams()
            {
                StartMin = 420,
                //DayLength = 59400,
                DayLength = dayLength,
                Logger = logger,
                NWarmupDays = 0,
                InitialNumberInQueue = queueSizeOverTime[420 * 60]
                //InitialNumberInQueue = 51
            };

            var distParams = new DistributionSelectionParameters()
            {
                SelectedDaysForData = availability,
                ArrivalDistributionBreakpoints = intervals,
                BreakTimes = breakTimes,
                IntervalForOtherDistributions = statsInterval,
                CapacityIntervalSize = capacityInterval,
                QueueSizeData = qSizeData
            };

            
            var allDists = new MultiArrivalDistributions(factoryParams, distParams, innerInterval, dType);
            var dists = allDists.CreateNCopies(nIterations);

            Parallel.ForEach(iterations, i =>
            {
                var dist = dists[i];
                

                var sim = SimulationFactory.MultiArrivalSim(factoryParams, dist, dType);
                

                sim.Run();


                logger.LogResults(sim.Results, "Results_" + i, (factoryParams.NWarmupDays + 1) * factoryParams.DayLength);


                FinalResults.AddSimResults(sim.Results);

                var t = sim.Results.CalcNumOut();

                if (t.ContainsKey(ProcessType.NonPutwall))
                {
                    throughput.Add(new Tuple<int, int>(t[ProcessType.Putwall], t[ProcessType.NonPutwall]));
                }
                else
                {
                    throughput.Add(new Tuple<int, int>(t[ProcessType.Putwall], 0));
                }
                

                OutputTimeSeries(i, basePath, sim.Results);

                OutputTimedQueueSize(i, basePath, sim.Results);

                WriteOutTimeInQ(i, basePath, sim.Results);

                WriteOutLeftoverCapacity(i, basePath, sim.Results.LeftOverCapacity);
            });

            
            using (var writer = new System.IO.StreamWriter(basePath + "FINALRESULTS.txt"))
            {
                foreach (Tuple<int, int> t2 in throughput)
                {
                    writer.WriteLine(t2.Item1.ToString() + '\t' + t2.Item2.ToString());
                }

                writer.WriteLine();
                writer.WriteLine("Putwall Stats");
                writer.WriteLine("Property \t Average \t StdDev");
                writer.WriteLine("Number Created \t" + FinalResults.PutwallStatistics.NumberCreated.Average + "\t" +
                   FinalResults.PutwallStatistics.NumberCreated.StdDev);
                writer.WriteLine("Number Disposed \t" + FinalResults.PutwallStatistics.NumberDisposed.Average + "\t" +
                  FinalResults.PutwallStatistics.NumberDisposed.StdDev);
                writer.WriteLine("Time in System \t" + FinalResults.PutwallStatistics.TimeInSystem.Average + "\t" +
                    FinalResults.PutwallStatistics.TimeInSystem.StdDev);                
                writer.WriteLine("Time in Queue \t" + FinalResults.PutwallStatistics.TimeInQueue.Average + "\t" +
                    FinalResults.PutwallStatistics.TimeInQueue.StdDev);
                writer.WriteLine("Time Recirculating \t" + FinalResults.PutwallStatistics.TimeRecirculating.Average + "\t" +
                    FinalResults.PutwallStatistics.TimeRecirculating.StdDev);
                writer.WriteLine("Times Recirculated \t" + FinalResults.PutwallStatistics.TimesRecirculated.Average + "\t" +
                    FinalResults.PutwallStatistics.TimesRecirculated.StdDev);
                writer.WriteLine("Time in Process \t" + FinalResults.PutwallStatistics.TimeInProcess.Average + "\t" +
                    FinalResults.PutwallStatistics.TimeInProcess.StdDev);

                
                writer.WriteLine();
                writer.WriteLine("NonPutwall Stats");
                writer.WriteLine("Property \t Average \t StdDev");
                writer.WriteLine("Number Created \t" + FinalResults.NonPutwallStatistics.NumberCreated.Average + "\t" +
                   FinalResults.NonPutwallStatistics.NumberCreated.StdDev);
                writer.WriteLine("Number Disposed \t" + FinalResults.NonPutwallStatistics.NumberDisposed.Average + "\t" +
                  FinalResults.NonPutwallStatistics.NumberDisposed.StdDev);
                writer.WriteLine("Time in system \t" + FinalResults.NonPutwallStatistics.TimeInSystem.Average + "\t" +
                    FinalResults.NonPutwallStatistics.TimeInSystem.StdDev);                
                writer.WriteLine("Time in Queue \t" + FinalResults.NonPutwallStatistics.TimeInQueue.Average + "\t" +
                    FinalResults.NonPutwallStatistics.TimeInQueue.StdDev);
                writer.WriteLine("Time Recirculating \t" + FinalResults.NonPutwallStatistics.TimeRecirculating.Average + "\t" +
                    FinalResults.NonPutwallStatistics.TimeRecirculating.StdDev);
                writer.WriteLine("Times Recirculated \t" + FinalResults.NonPutwallStatistics.TimesRecirculated.Average + "\t" +
                    FinalResults.NonPutwallStatistics.TimesRecirculated.StdDev);
                writer.WriteLine("Time in Process \t" + FinalResults.NonPutwallStatistics.TimeInProcess.Average + "\t" +
                    FinalResults.NonPutwallStatistics.TimeInProcess.StdDev);


            }
        }
        private void SimWithConstantPOfRecirc()
        {
            var Data = new WarehouseData();

            var FinalResults = new MetaResults();

            //string basePath = @"C:\Users\Dematic\Desktop\SimLog\";
            string basePath = @"C:\Users\p2decarv\Desktop\SimLog\";
            //string basePath = @"C:\Users\Daniel\Desktop\SimLog\";

            var availability = new List<DateTime>()
            {
                new DateTime(2015,11,11)
            };

            var qSizeData = new List<DateTime>()
            {
                new DateTime(2015,11,9),
                new DateTime(2015,11,10),
                new DateTime(2015,11,11),
                new DateTime(2015,11,12),
                new DateTime(2015,11,13)
            };
            var statsInterval = new Tuple<TimeSpan, TimeSpan>(new TimeSpan(6, 0, 0), new TimeSpan(23, 59, 59));

            var arrivalTimesOverTime = Data.GetInterarrivalTimesOverTime(availability[0]);
            var recircTimesOverTime = Data.GetItemsInRecircOverTime(availability[0]);
            var queueSizeOverTime = Data.GetQueueSizeOverTime(qSizeData, availability[0]);
            var qTimes = Data.GetTimeInQueue(availability[0], statsInterval);



            using (var writer = new System.IO.StreamWriter(basePath + "InterArrivals_over_time_real.txt"))
            {
                writer.WriteLine("Time \t Interarrival Time");

                foreach (var t in arrivalTimesOverTime)
                {
                    writer.WriteLine(t.Item1 + "\t" + t.Item2);
                }
            }

            using (var writer = new System.IO.StreamWriter(basePath + "ItemsInRecirc_over_time_real.txt"))
            {
                writer.WriteLine("Time of Day (s) \t Items in Reicirculation");

                for (int i = 0; i < recircTimesOverTime.Length; i++)
                {
                    writer.WriteLine(i + "\t" + recircTimesOverTime[i]);
                }
            }

            using (var writer = new System.IO.StreamWriter(basePath + "QueueSize_over_time_real.txt"))
            {
                writer.WriteLine("Time of Day (s) \t Items in Queue");

                for (int i = 0; i < queueSizeOverTime.Length; i++)
                {
                    writer.WriteLine(i + "\t" + queueSizeOverTime[i]);
                }
            }

            using (var writer = new System.IO.StreamWriter(basePath + "QTimes.txt"))
            {
                writer.WriteLine("Time in Queue");

                foreach (double time in qTimes)
                {
                    writer.WriteLine(time);
                }
            }




            var logger = new VerboseLogger(@"C:\Users\p2decarv\Desktop\SimLog");
            //var logger = new VerboseLogger(@"C:\Users\Daniel\Desktop\SimLog");
            //var logger = new VerboseLogger(@"C:\Users\Dematic\Desktop\SimLog");



            logger.LogDBStats("DBStats2", availability.First(), 600, statsInterval);
            //logger.LogDBStats("DBStats", availability.First());
            //logger.LogDBStats("DBStats", availability.First(), 600);


            List<Tuple<int, int>> throughput = new List<Tuple<int, int>>();
            var iterations = Enumerable.Range(0, 10);

            var intervals = new List<Tuple<TimeSpan, TimeSpan>>();

            int capacityInterval = 6;
            int ArrivalInterval = 60;
            int innerInterval = 2;

            var operaterCount = Data.GetOperatorsPerZMins(availability[0], capacityInterval);
            logger.LogPutsPerHour("Operators_Per_" + capacityInterval + "mins_MinsFromMidnight", operaterCount);



            for (int i = 6 * 60; i < 24 * 60; i += ArrivalInterval)
            {
                if (((double)(i + ArrivalInterval) / 60) == 24)
                {
                    intervals.Add(new Tuple<TimeSpan, TimeSpan>(new TimeSpan((int)Math.Floor((double)i / 60), i % 60, 0),
                    new TimeSpan(23, 59, 59)));
                }
                else
                {
                    intervals.Add(new Tuple<TimeSpan, TimeSpan>(new TimeSpan((int)Math.Floor((double)i / 60), i % 60, 0),
                    new TimeSpan((int)Math.Floor((double)(i + ArrivalInterval) / 60), (i + ArrivalInterval) % 60, 0)));
                }

            }


            var breakTimes = Data.FindBreaks(availability[0], 600);

            var factoryParams = new FactoryParams()
            {
                StartMin = 360,
                //DayLength = 64800,
                DayLength = 61200,
                Logger = logger,
                NWarmupDays = 0,
                //InitialNumberInQueue = 104
                InitialNumberInQueue = 51
            };

            var distParams = new DistributionSelectionParameters()
            {
                SelectedDaysForData = availability,
                ArrivalDistributionBreakpoints = intervals,
                BreakTimes = breakTimes,
                IntervalForOtherDistributions = statsInterval,
                CapacityIntervalSize = capacityInterval
            };

            var pOfRecirc = 0.015;


            var warmupDays = new List<Tuple<DateTime, DistributionSelectionParameters>>();

            var allDists = new RequiredDistsSmoothed(factoryParams, distParams, warmupDays, innerInterval);

            var AllDists = allDists.CreateNCopiesWithOperators(10);





            Parallel.ForEach(iterations, i =>
            {
                var dists = AllDists[i];

                var sim = SimulationFactory.SimWithConstantPOfRecirc(factoryParams, dists, pOfRecirc);

                sim.Run();


                logger.LogResults(sim.Results, "Results_" + i, (factoryParams.NWarmupDays + 1) * factoryParams.DayLength);


                FinalResults.AddSimResults(sim.Results);

                var t = sim.Results.CalcNumOut();

                throughput.Add(new Tuple<int, int>(t[ProcessType.Putwall], t[ProcessType.NonPutwall]));

                OutputTimeSeries(i, basePath, sim.Results);

                OutputTimedQueueSize(i, basePath, sim.Results);

                WriteOutTimeInQ(i, basePath, sim.Results);

                WriteOutLeftoverCapacity(i, basePath, sim.Results.LeftOverCapacity);
            });


            //using (var writer = new System.IO.StreamWriter(@"C:\Users\p2decarv\Desktop\SimLog\FINALRESULTS.txt"))
            //using (var writer = new System.IO.StreamWriter(@"C:\Users\Daniel\Desktop\SimLog\FINALRESULTS.txt"))
            using (var writer = new System.IO.StreamWriter(basePath + "FINALRESULTS.txt"))
            {
                foreach (Tuple<int, int> t2 in throughput)
                {
                    writer.WriteLine(t2.Item1.ToString() + '\t' + t2.Item2.ToString());
                }

                writer.WriteLine();
                writer.WriteLine("Putwall Stats");
                writer.WriteLine("Property \t Average \t StdDev");
                writer.WriteLine("Time in system \t" + FinalResults.PutwallStatistics.TimeInSystem.Average + "\t" +
                    FinalResults.PutwallStatistics.TimeInSystem.StdDev);
                writer.WriteLine("Time in Process \t" + FinalResults.PutwallStatistics.TimeInProcess.Average + "\t" +
                    FinalResults.PutwallStatistics.TimeInProcess.StdDev);
                writer.WriteLine("Time in Queue \t" + FinalResults.PutwallStatistics.TimeInQueue.Average + "\t" +
                    FinalResults.PutwallStatistics.TimeInQueue.StdDev);
                writer.WriteLine("Time Recirculating \t" + FinalResults.PutwallStatistics.TimeRecirculating.Average + "\t" +
                    FinalResults.PutwallStatistics.TimeRecirculating.StdDev);
                writer.WriteLine("Times Recirculated \t" + FinalResults.PutwallStatistics.TimesRecirculated.Average + "\t" +
                    FinalResults.PutwallStatistics.TimesRecirculated.StdDev);
                writer.WriteLine("Number Created \t" + FinalResults.PutwallStatistics.NumberCreated.Average + "\t" +
                   FinalResults.PutwallStatistics.NumberCreated.StdDev);
                writer.WriteLine("Number Disposed \t" + FinalResults.PutwallStatistics.NumberDisposed.Average + "\t" +
                  FinalResults.PutwallStatistics.NumberDisposed.StdDev);

                writer.WriteLine();
                writer.WriteLine("NonPutwall Stats");
                writer.WriteLine("Property \t Average \t StdDev");
                writer.WriteLine("Time in system \t" + FinalResults.NonPutwallStatistics.TimeInSystem.Average + "\t" +
                    FinalResults.NonPutwallStatistics.TimeInSystem.StdDev);
                writer.WriteLine("Time in Process \t" + FinalResults.NonPutwallStatistics.TimeInProcess.Average + "\t" +
                    FinalResults.NonPutwallStatistics.TimeInProcess.StdDev);
                writer.WriteLine("Time in Queue \t" + FinalResults.NonPutwallStatistics.TimeInQueue.Average + "\t" +
                    FinalResults.NonPutwallStatistics.TimeInQueue.StdDev);
                writer.WriteLine("Time Recirculating \t" + FinalResults.NonPutwallStatistics.TimeRecirculating.Average + "\t" +
                    FinalResults.NonPutwallStatistics.TimeRecirculating.StdDev);
                writer.WriteLine("Times Recirculated \t" + FinalResults.NonPutwallStatistics.TimesRecirculated.Average + "\t" +
                    FinalResults.NonPutwallStatistics.TimesRecirculated.StdDev);
                writer.WriteLine("Number Created \t" + FinalResults.NonPutwallStatistics.NumberCreated.Average + "\t" +
                   FinalResults.NonPutwallStatistics.NumberCreated.StdDev);
                writer.WriteLine("Number Disposed \t" + FinalResults.NonPutwallStatistics.NumberDisposed.Average + "\t" +
                  FinalResults.NonPutwallStatistics.NumberDisposed.StdDev);

            }

        }
        private MetaResults IHateLife(int arrivalInterval, string thePath)
        {
            var Data = new WarehouseData();

            var FinalResults = new MetaResults();

            //string basePath = @"C:\Users\Dematic\Desktop\SimLog\";
            string basePath = thePath + arrivalInterval + @"\";
            if (!System.IO.Directory.Exists(basePath))
            {
                System.IO.Directory.CreateDirectory(basePath);
            }
            //string basePath = @"C:\Users\Daniel\Desktop\SimLog\";

            var availability = new List<DateTime>()
            {
                new DateTime(2015,11,11)
            };

            var qSizeData = new List<DateTime>()
            {
                new DateTime(2015,11,6),
                new DateTime(2015,11,7),
                new DateTime(2015, 11,8),
                new DateTime(2015,11,9),
                new DateTime(2015,11,10),
                new DateTime(2015,11,11),
                new DateTime(2015,11,12),
                new DateTime(2015,11,13)
            };
            var statsInterval = new Tuple<TimeSpan, TimeSpan>(new TimeSpan(6, 15, 0), new TimeSpan(23, 15, 0));

            var arrivalTimesOverTime = Data.GetInterarrivalTimesOverTime(availability[0]);
            var recircTimesOverTime = Data.GetItemsInRecircOverTime(qSizeData, availability[0]);
            var queueSizeOverTime = Data.GetQueueSizeOverTime(qSizeData, availability[0]);
            var qTimes = Data.GetTimeInQueue(availability[0], statsInterval);



            using (var writer = new System.IO.StreamWriter(basePath + "InterArrivals_over_time_real.txt"))
            {
                writer.WriteLine("Time \t Interarrival Time");

                foreach (var t in arrivalTimesOverTime)
                {
                    writer.WriteLine(t.Item1 + "\t" + t.Item2);
                }
            }

            using (var writer = new System.IO.StreamWriter(basePath + "ItemsInRecirc_over_time_real.txt"))
            {
                writer.WriteLine("Time of Day (s) \t Items in Reicirculation");

                for (int i = 0; i < recircTimesOverTime.Length; i++)
                {
                    writer.WriteLine(i + "\t" + recircTimesOverTime[i]);
                }
            }

            using (var writer = new System.IO.StreamWriter(basePath + "QueueSize_over_time_real.txt"))
            {
                writer.WriteLine("Time of Day (s) \t Items in Queue");

                for (int i = 0; i < queueSizeOverTime.Length; i++)
                {
                    writer.WriteLine(i + "\t" + queueSizeOverTime[i]);
                }
            }

            using (var writer = new System.IO.StreamWriter(basePath + "QTimes.txt"))
            {
                writer.WriteLine("Time in Queue");

                foreach (double time in qTimes)
                {
                    writer.WriteLine(time);
                }
            }




            var logger = new VerboseLogger(basePath);
            //var logger = new VerboseLogger(@"C:\Users\Daniel\Desktop\SimLog");
            //var logger = new VerboseLogger(@"C:\Users\Dematic\Desktop\SimLog");



            //logger.LogDBStats("DBStats2", availability.First(), 600, statsInterval);
            //logger.LogDBStats("DBStats", availability.First());
            //logger.LogDBStats("DBStats", availability.First(), 600);


            List<Tuple<int, int>> throughput = new List<Tuple<int, int>>();
            var iterations = Enumerable.Range(0, 10);

            var intervals = new List<Tuple<TimeSpan, TimeSpan>>();

            int capacityInterval = 5;
            int ArrivalInterval = arrivalInterval;
            int innerInterval = 1;

            var operaterCount = Data.GetOperatorsPerZMins(availability[0], capacityInterval);
            logger.LogPutsPerHour("Operators_Per_" + capacityInterval + "mins_MinsFromMidnight", operaterCount);



            for (int i = 6 * 60; i < 24 * 60; i += ArrivalInterval)
            {
                if (((double)(i + ArrivalInterval) / 60) == 24)
                {
                    intervals.Add(new Tuple<TimeSpan, TimeSpan>(new TimeSpan((int)Math.Floor((double)i / 60), i % 60, 0),
                    new TimeSpan(23, 59, 59)));
                }
                else
                {
                    intervals.Add(new Tuple<TimeSpan, TimeSpan>(new TimeSpan((int)Math.Floor((double)i / 60), i % 60, 0),
                    new TimeSpan((int)Math.Floor((double)(i + ArrivalInterval) / 60), (i + ArrivalInterval) % 60, 0)));
                }

            }


            var breakTimes = Data.FindBreaks(availability[0], 600);

            var factoryParams = new FactoryParams()
            {
                StartMin = 375,
                DayLength = 61200,
                //DayLength = 64800,
                Logger = logger,
                NWarmupDays = 0,
                InitialNumberInQueue = 52
                //InitialNumberInQueue = 78
            };

            var distParams = new DistributionSelectionParameters()
            {
                SelectedDaysForData = availability,
                ArrivalDistributionBreakpoints = intervals,
                BreakTimes = breakTimes,
                IntervalForOtherDistributions = statsInterval,
                CapacityIntervalSize = capacityInterval
            };

            var pOfRecirc = Data.RecirculationVSQueueSize(availability[0], qSizeData, distParams.IntervalForOtherDistributions);

            using (var writer = new System.IO.StreamWriter(basePath + "ConditionalProbDist.txt"))
            {
                writer.WriteLine("Queue Size \t Number of Recirculations Observed \t Number of Lane Entries Observed \t Total \t P(Recirc|QSize)");

                foreach (var p in pOfRecirc.OrderBy(x => x.Key))
                {
                    writer.WriteLine(p.Key + "\t" +
                                     p.Value.Item1 + "\t" +
                                     p.Value.Item2 + "\t" +
                                     (p.Value.Item1 + p.Value.Item2) + "\t" +
                                     ((double)p.Value.Item1 / ((double)p.Value.Item1 + (double)p.Value.Item2)));
                }
            }


            var warmupDays = new List<Tuple<DateTime, DistributionSelectionParameters>>();

            var allDists = new RequiredDistsSmoothed(factoryParams, distParams, warmupDays, innerInterval);

            var AllDists = allDists.CreateNCopiesWithOperators(10);





            Parallel.ForEach(iterations, i =>
            {
                var dists = AllDists[i];

                var sim = SimulationFactory.SimWithConditionalPofRecircAndNoMaxQueue(factoryParams, dists, pOfRecirc);

                sim.Run();


                logger.LogResults(sim.Results, "Results_" + i, (factoryParams.NWarmupDays + 1) * factoryParams.DayLength);


                FinalResults.AddSimResults(sim.Results);

                var t = sim.Results.CalcNumOut();

                throughput.Add(new Tuple<int, int>(t[ProcessType.Putwall], t[ProcessType.NonPutwall]));

                OutputTimeSeries(i, basePath, sim.Results);

                OutputTimedQueueSize(i, basePath, sim.Results);

                WriteOutTimeInQ(i, basePath, sim.Results);

                WriteOutLeftoverCapacity(i, basePath, sim.Results.LeftOverCapacity);
            });


            //using (var writer = new System.IO.StreamWriter(@"C:\Users\p2decarv\Desktop\SimLog\FINALRESULTS.txt"))
            //using (var writer = new System.IO.StreamWriter(@"C:\Users\Daniel\Desktop\SimLog\FINALRESULTS.txt"))
            using (var writer = new System.IO.StreamWriter(basePath + "FINALRESULTS.txt"))
            {
                foreach (Tuple<int, int> t2 in throughput)
                {
                    writer.WriteLine(t2.Item1.ToString() + '\t' + t2.Item2.ToString());
                }

                writer.WriteLine();
                writer.WriteLine("Putwall Stats");
                writer.WriteLine("Property \t Average \t StdDev");
                writer.WriteLine("Number Created \t" + FinalResults.PutwallStatistics.NumberCreated.Average + "\t" +
                   FinalResults.PutwallStatistics.NumberCreated.StdDev);
                writer.WriteLine("Number Disposed \t" + FinalResults.PutwallStatistics.NumberDisposed.Average + "\t" +
                  FinalResults.PutwallStatistics.NumberDisposed.StdDev);
                writer.WriteLine("Time in System \t" + FinalResults.PutwallStatistics.TimeInSystem.Average + "\t" +
                    FinalResults.PutwallStatistics.TimeInSystem.StdDev);
                writer.WriteLine("Time in Queue \t" + FinalResults.PutwallStatistics.TimeInQueue.Average + "\t" +
                    FinalResults.PutwallStatistics.TimeInQueue.StdDev);
                writer.WriteLine("Time Recirculating \t" + FinalResults.PutwallStatistics.TimeRecirculating.Average + "\t" +
                    FinalResults.PutwallStatistics.TimeRecirculating.StdDev);
                writer.WriteLine("Times Recirculated \t" + FinalResults.PutwallStatistics.TimesRecirculated.Average + "\t" +
                    FinalResults.PutwallStatistics.TimesRecirculated.StdDev);
                writer.WriteLine("Time in Put Process \t" + FinalResults.PutwallStatistics.TimeInProcess.Average + "\t" +
                    FinalResults.PutwallStatistics.TimeInProcess.StdDev);


                writer.WriteLine();
                writer.WriteLine("NonPutwall Stats");
                writer.WriteLine("Property \t Average \t StdDev");
                writer.WriteLine("Time in system \t" + FinalResults.NonPutwallStatistics.TimeInSystem.Average + "\t" +
                    FinalResults.NonPutwallStatistics.TimeInSystem.StdDev);
                writer.WriteLine("Time in Process \t" + FinalResults.NonPutwallStatistics.TimeInProcess.Average + "\t" +
                    FinalResults.NonPutwallStatistics.TimeInProcess.StdDev);
                writer.WriteLine("Time in Queue \t" + FinalResults.NonPutwallStatistics.TimeInQueue.Average + "\t" +
                    FinalResults.NonPutwallStatistics.TimeInQueue.StdDev);
                writer.WriteLine("Time Recirculating \t" + FinalResults.NonPutwallStatistics.TimeRecirculating.Average + "\t" +
                    FinalResults.NonPutwallStatistics.TimeRecirculating.StdDev);
                writer.WriteLine("Times Recirculated \t" + FinalResults.NonPutwallStatistics.TimesRecirculated.Average + "\t" +
                    FinalResults.NonPutwallStatistics.TimesRecirculated.StdDev);
                writer.WriteLine("Number Created \t" + FinalResults.NonPutwallStatistics.NumberCreated.Average + "\t" +
                   FinalResults.NonPutwallStatistics.NumberCreated.StdDev);
                writer.WriteLine("Number Disposed \t" + FinalResults.NonPutwallStatistics.NumberDisposed.Average + "\t" +
                  FinalResults.NonPutwallStatistics.NumberDisposed.StdDev);

            }

            return FinalResults;
        }
        private void SimWithSmoothedCapacity()
        {
            var Data = new WarehouseData();

            var FinalResults = new MetaResults();

            //string basePath = @"C:\Users\Dematic\Desktop\SimLog\";
            string basePath = @"C:\Users\p2decarv\Desktop\SimLog\";
            //string basePath = @"C:\Users\Daniel\Desktop\SimLog\";

            var availability = new List<DateTime>()
            {
                new DateTime(2015,11,9)
            };

            var qSizeData = new List<DateTime>()
            {
                new DateTime(2015,11,6),
                new DateTime(2015,11,7),
                new DateTime(2015, 11,8),
                new DateTime(2015,11,9),
                new DateTime(2015,11,10),
                new DateTime(2015,11,11),
                new DateTime(2015,11,12),
                new DateTime(2015,11,13)
            };
            var statsInterval = new Tuple<TimeSpan, TimeSpan>(new TimeSpan(6,30, 0), new TimeSpan(23, 0, 0));

            var arrivalTimesOverTime = Data.GetInterarrivalTimesOverTime(availability[0]);
            var recircTimesOverTime = Data.GetItemsInRecircOverTime(qSizeData, availability[0]);
            var queueSizeOverTime = Data.GetQueueSizeOverTime(qSizeData, availability[0]);
            var qTimes = Data.GetTimeInQueue(availability[0], statsInterval);



            using (var writer = new System.IO.StreamWriter(basePath + "InterArrivals_over_time_real.txt"))
            {
                writer.WriteLine("Time \t Interarrival Time");

                foreach (var t in arrivalTimesOverTime)
                {
                    writer.WriteLine(t.Item1 + "\t" + t.Item2);
                }
            }

            using (var writer = new System.IO.StreamWriter(basePath + "ItemsInRecirc_over_time_real.txt"))
            {
                writer.WriteLine("Time of Day (s) \t Items in Reicirculation");

                for (int i = 0; i < recircTimesOverTime.Length; i++)
                {
                    writer.WriteLine(i + "\t" + recircTimesOverTime[i]);
                }
            }

            using (var writer = new System.IO.StreamWriter(basePath + "QueueSize_over_time_real.txt"))
            {
                writer.WriteLine("Time of Day (s) \t Items in Queue");

                for (int i = 0; i < queueSizeOverTime.Length; i++)
                {
                    writer.WriteLine(i + "\t" + queueSizeOverTime[i]);
                }
            }

            using (var writer = new System.IO.StreamWriter(basePath + "QTimes.txt"))
            {
                writer.WriteLine("Time in Queue");

                foreach (double time in qTimes)
                {
                    writer.WriteLine(time);
                }
            }




            var logger = new VerboseLogger(@"C:\Users\p2decarv\Desktop\SimLog");
            //var logger = new VerboseLogger(@"C:\Users\Daniel\Desktop\SimLog");
            //var logger = new VerboseLogger(@"C:\Users\Dematic\Desktop\SimLog");



            logger.LogDBStats("DBStats2", availability.First(), 600, statsInterval);
            //logger.LogDBStats("DBStats", availability.First());
            //logger.LogDBStats("DBStats", availability.First(), 600);


            List<Tuple<int, int>> throughput = new List<Tuple<int, int>>();
            var iterations = Enumerable.Range(0, 10);

            var intervals = new List<Tuple<TimeSpan, TimeSpan>>();

            int capacityInterval = 5;
            int ArrivalInterval = 17*60;
            int innerInterval = 1;

            var operaterCount = Data.GetOperatorsPerZMins(availability[0], capacityInterval);
            logger.LogPutsPerHour("Operators_Per_" + capacityInterval + "mins_MinsFromMidnight", operaterCount);



            for (int i = 6 * 60; i < 24 * 60; i += ArrivalInterval)
            {
                if (((double)(i + ArrivalInterval) / 60) == 24)
                {
                    intervals.Add(new Tuple<TimeSpan, TimeSpan>(new TimeSpan((int)Math.Floor((double)i / 60), i % 60, 0),
                    new TimeSpan(23, 59, 59)));
                }
                else
                {
                    intervals.Add(new Tuple<TimeSpan, TimeSpan>(new TimeSpan((int)Math.Floor((double)i / 60), i % 60, 0),
                    new TimeSpan((int)Math.Floor((double)(i + ArrivalInterval) / 60), (i + ArrivalInterval) % 60, 0)));
                }

            }


            var breakTimes = Data.FindBreaks(availability[0], 600);

            var factoryParams = new FactoryParams()
            {
                StartMin = 390,
                DayLength = 61200 - 1800,
                //DayLength = 64800,
                Logger = logger,
                NWarmupDays = 0,
                //InitialNumberInQueue = 104
                InitialNumberInQueue = 132
            };

            var distParams = new DistributionSelectionParameters()
            {
                SelectedDaysForData = availability,
                ArrivalDistributionBreakpoints = intervals,
                BreakTimes = breakTimes,
                IntervalForOtherDistributions = statsInterval,
                CapacityIntervalSize = capacityInterval
            };

            var pOfRecirc = Data.RecirculationVSQueueSize(availability[0], qSizeData, distParams.IntervalForOtherDistributions);

            using (var writer = new System.IO.StreamWriter(basePath + "ConditionalProbDist.txt"))
            {
                writer.WriteLine("Queue Size \t Number of Recirculations Observed \t Number of Lane Entries Observed \t Total \t P(Recirc|QSize)");

                foreach (var p in pOfRecirc.OrderBy(x => x.Key))
                {
                    writer.WriteLine(p.Key + "\t" +
                                     p.Value.Item1 + "\t" +
                                     p.Value.Item2 + "\t" +
                                     (p.Value.Item1 + p.Value.Item2) + "\t" +
                                     ((double)p.Value.Item1 / ((double)p.Value.Item1 + (double)p.Value.Item2)));
                }
            }


            var warmupDays = new List<Tuple<DateTime, DistributionSelectionParameters>>();

            var allDists = new RequiredDistsSmoothed(factoryParams, distParams, warmupDays, innerInterval);

            var AllDists = allDists.CreateNCopiesWithOperators(10);





            Parallel.ForEach(iterations, i =>
            {
                var dists = AllDists[i];

                var sim = SimulationFactory.SimWithConditionalPofRecircAndNoMaxQueue(factoryParams, dists, pOfRecirc);

                sim.Run();


                logger.LogResults(sim.Results, "Results_" + i, (factoryParams.NWarmupDays + 1) * factoryParams.DayLength);


                FinalResults.AddSimResults(sim.Results);

                var t = sim.Results.CalcNumOut();

                throughput.Add(new Tuple<int, int>(t[ProcessType.Putwall], t[ProcessType.NonPutwall]));

                OutputTimeSeries(i, basePath, sim.Results);

                OutputTimedQueueSize(i, basePath, sim.Results);

                WriteOutTimeInQ(i, basePath, sim.Results);

                WriteOutLeftoverCapacity(i, basePath, sim.Results.LeftOverCapacity);
            });


            //using (var writer = new System.IO.StreamWriter(@"C:\Users\p2decarv\Desktop\SimLog\FINALRESULTS.txt"))
            //using (var writer = new System.IO.StreamWriter(@"C:\Users\Daniel\Desktop\SimLog\FINALRESULTS.txt"))
            using (var writer = new System.IO.StreamWriter(basePath + "FINALRESULTS.txt"))
            {
                foreach (Tuple<int, int> t2 in throughput)
                {
                    writer.WriteLine(t2.Item1.ToString() + '\t' + t2.Item2.ToString());
                }

                writer.WriteLine();
                writer.WriteLine("Putwall Stats");
                writer.WriteLine("Property \t Average \t StdDev");
                writer.WriteLine("Number Created \t" + FinalResults.PutwallStatistics.NumberCreated.Average + "\t" +
                   FinalResults.PutwallStatistics.NumberCreated.StdDev);
                writer.WriteLine("Number Disposed \t" + FinalResults.PutwallStatistics.NumberDisposed.Average + "\t" +
                  FinalResults.PutwallStatistics.NumberDisposed.StdDev);
                writer.WriteLine("Time in System \t" + FinalResults.PutwallStatistics.TimeInSystem.Average + "\t" +
                    FinalResults.PutwallStatistics.TimeInSystem.StdDev);
                writer.WriteLine("Time in Queue \t" + FinalResults.PutwallStatistics.TimeInQueue.Average + "\t" +
                    FinalResults.PutwallStatistics.TimeInQueue.StdDev);
                writer.WriteLine("Time Recirculating \t" + FinalResults.PutwallStatistics.TimeRecirculating.Average + "\t" +
                    FinalResults.PutwallStatistics.TimeRecirculating.StdDev);
                writer.WriteLine("Times Recirculated \t" + FinalResults.PutwallStatistics.TimesRecirculated.Average + "\t" +
                    FinalResults.PutwallStatistics.TimesRecirculated.StdDev);
                writer.WriteLine("Time in Put Process \t" + FinalResults.PutwallStatistics.TimeInProcess.Average + "\t" +
                    FinalResults.PutwallStatistics.TimeInProcess.StdDev);


                writer.WriteLine();
                writer.WriteLine("NonPutwall Stats");
                writer.WriteLine("Property \t Average \t StdDev");
                writer.WriteLine("Time in system \t" + FinalResults.NonPutwallStatistics.TimeInSystem.Average + "\t" +
                    FinalResults.NonPutwallStatistics.TimeInSystem.StdDev);
                writer.WriteLine("Time in Process \t" + FinalResults.NonPutwallStatistics.TimeInProcess.Average + "\t" +
                    FinalResults.NonPutwallStatistics.TimeInProcess.StdDev);
                writer.WriteLine("Time in Queue \t" + FinalResults.NonPutwallStatistics.TimeInQueue.Average + "\t" +
                    FinalResults.NonPutwallStatistics.TimeInQueue.StdDev);
                writer.WriteLine("Time Recirculating \t" + FinalResults.NonPutwallStatistics.TimeRecirculating.Average + "\t" +
                    FinalResults.NonPutwallStatistics.TimeRecirculating.StdDev);
                writer.WriteLine("Times Recirculated \t" + FinalResults.NonPutwallStatistics.TimesRecirculated.Average + "\t" +
                    FinalResults.NonPutwallStatistics.TimesRecirculated.StdDev);
                writer.WriteLine("Number Created \t" + FinalResults.NonPutwallStatistics.NumberCreated.Average + "\t" +
                   FinalResults.NonPutwallStatistics.NumberCreated.StdDev);
                writer.WriteLine("Number Disposed \t" + FinalResults.NonPutwallStatistics.NumberDisposed.Average + "\t" +
                  FinalResults.NonPutwallStatistics.NumberDisposed.StdDev);

            }

        }
        private void ConsumeAllSim()
        {
            var Data = new WarehouseData();

            var FinalResults = new MetaResults();

            string basePath = @"C:\Users\Dematic\Desktop\SimLog\";
            //string basePath = @"C:\Users\p2decarv\Desktop\SimLog\";

            var availability = new List<DateTime>()
            {
                new DateTime(2015,11,11)
            };

            var qSizeData = new List<DateTime>()
            {
                new DateTime(2015,11,9),
                new DateTime(2015,11,10),
                new DateTime(2015,11,11),
                new DateTime(2015,11,12),
                new DateTime(2015,11,13)
            };
            var statsInterval = new Tuple<TimeSpan, TimeSpan>(new TimeSpan(6, 0, 0), new TimeSpan(23, 59, 59));

            var arrivalTimesOverTime = Data.GetInterarrivalTimesOverTime(availability[0]);
            var recircTimesOverTime = Data.GetItemsInRecircOverTime(availability[0]);
            var queueSizeOverTime = Data.GetQueueSizeOverTime(qSizeData, availability[0]);
            var qTimes = Data.GetTimeInQueue(availability[0], statsInterval);



            using (var writer = new System.IO.StreamWriter(basePath + "InterArrivals_over_time_real.txt"))
            {
                writer.WriteLine("Time \t Interarrival Time");

                foreach (var t in arrivalTimesOverTime)
                {
                    writer.WriteLine(t.Item1 + "\t" + t.Item2);
                }
            }

            using (var writer = new System.IO.StreamWriter(basePath + "ItemsInRecirc_over_time_real.txt"))
            {
                writer.WriteLine("Time of Day (s) \t Items in Reicirculation");

                for (int i = 0; i < recircTimesOverTime.Length; i++)
                {
                    writer.WriteLine(i + "\t" + recircTimesOverTime[i]);
                }
            }

            using (var writer = new System.IO.StreamWriter(basePath + "QueueSize_over_time_real.txt"))
            {
                writer.WriteLine("Time of Day (s) \t Items in Queue");

                for (int i = 0; i < queueSizeOverTime.Length; i++)
                {
                    writer.WriteLine(i + "\t" + queueSizeOverTime[i]);
                }
            }

            using (var writer = new System.IO.StreamWriter(basePath + "QTimes.txt"))
            {
                writer.WriteLine("Time in Queue");

                foreach (double time in qTimes)
                {
                    writer.WriteLine(time);
                }
            }




            //var logger = new VerboseLogger(@"C:\Users\p2decarv\Desktop\SimLog");
            //var logger = new VerboseLogger(@"C:\Users\Daniel\Desktop\SimLog");
            var logger = new VerboseLogger(@"C:\Users\Dematic\Desktop\SimLog");



            logger.LogDBStats("DBStats2", availability.First(), 600, statsInterval);
            //logger.LogDBStats("DBStats", availability.First());
            //logger.LogDBStats("DBStats", availability.First(), 600);


            List<Tuple<int, int>> throughput = new List<Tuple<int, int>>();
            var iterations = Enumerable.Range(0, 10);

            var intervals = new List<Tuple<TimeSpan, TimeSpan>>();

            int incrementSize = 30;

            var operaterCount = Data.GetOperatorsPerZMins(availability[0], incrementSize);
            var ppx = Data.GetPutsPerZMins(availability[0], incrementSize);
            logger.LogPutsPerHour("Operators_Per_" + incrementSize + "mins_MinsFromMidnight", operaterCount);
            logger.LogPutsPerHour("Puts_per_" + incrementSize + "mins_MinsFromMidnight", ppx);



            for (int i = 6 * 60; i < 24 * 60; i += incrementSize)
            {
                intervals.Add(new Tuple<TimeSpan, TimeSpan>(new TimeSpan((int)Math.Floor((double)i / 60), i % 60, 0),
                    new TimeSpan((int)Math.Floor((double)(i + incrementSize) / 60), (i + incrementSize) % 60, 0)));
            }


            var breakTimes = Data.FindBreaks(availability[0], 600);

            var factoryParams = new FactoryParams()
            {
                StartMin = 360,
                // DayLength = 64800,
                DayLength = 61200,
                Logger = logger,
                NWarmupDays = 0,
                //InitialNumberInQueue = 104
                InitialNumberInQueue = 51
            };

            var distParams = new DistributionSelectionParameters()
            {
                SelectedDaysForData = availability,
                ArrivalDistributionBreakpoints = intervals,
                BreakTimes = breakTimes,
                IntervalForOtherDistributions = statsInterval,
                CapacityIntervalSize = incrementSize
            };

            var pOfRecirc = Data.RecirculationVSQueueSize(availability[0], qSizeData, distParams.IntervalForOtherDistributions);

            using (var writer = new System.IO.StreamWriter(basePath + "ConditionalProbDist.txt"))
            {
                writer.WriteLine("Queue Size \t Number of Recirculations Observed \t Number of Lane Entries Observed \t Total \t P(Recirc|QSize)");

                foreach (var p in pOfRecirc.OrderBy(x => x.Key))
                {
                    writer.WriteLine(p.Key + "\t" +
                                     p.Value.Item1 + "\t" +
                                     p.Value.Item2 + "\t" +
                                     (p.Value.Item1 + p.Value.Item2) + "\t" +
                                     ((double)p.Value.Item1 / ((double)p.Value.Item1 + (double)p.Value.Item2)));
                }
            }

            DateTime warmupDay = new DateTime(2015, 11, 9);

            var warmupParams = new DistributionSelectionParameters()
            {
                SelectedDaysForData = new List<DateTime>() { warmupDay },
                ArrivalDistributionBreakpoints = intervals,
                BreakTimes = breakTimes,
                IntervalForOtherDistributions = statsInterval,
                CapacityIntervalSize = 30
            };

            //var warmupDays = new List<Tuple<DateTime, DistributionSelectionParameters>>()
            //    {
            //        new Tuple<DateTime, DistributionSelectionParameters>(warmupDay, warmupParams)
            //    };
            var warmupDays = new List<Tuple<DateTime, DistributionSelectionParameters>>();

            var allDists = new RequiredDistsWithOperators(factoryParams, distParams, warmupDays);

            var AllDists = allDists.CreateNCopiesWithOperators(10);





            Parallel.ForEach(iterations, i =>
            {
                var dists = AllDists[i];

                var sim = SimulationFactory.ConsumeAllSim(factoryParams, dists, pOfRecirc);

                sim.Run();


                logger.LogResults(sim.Results, "Results_" + i, 3 * 64800);


                FinalResults.AddSimResults(sim.Results);

                var t = sim.Results.CalcNumOut();

                throughput.Add(new Tuple<int, int>(t[ProcessType.Putwall], t[ProcessType.NonPutwall]));

                OutputTimeSeries(i, basePath, sim.Results);

                OutputTimedQueueSize(i, basePath, sim.Results);

                WriteOutTimeInQ(i, basePath, sim.Results);

                WriteOutLeftoverCapacity(i, basePath, sim.Results.LeftOverCapacity);
            });


            //using (var writer = new System.IO.StreamWriter(@"C:\Users\p2decarv\Desktop\SimLog\FINALRESULTS.txt"))
            //using (var writer = new System.IO.StreamWriter(@"C:\Users\Daniel\Desktop\SimLog\FINALRESULTS.txt"))
            using (var writer = new System.IO.StreamWriter(basePath + "FINALRESULTS.txt"))
            {
                foreach (Tuple<int, int> t2 in throughput)
                {
                    writer.WriteLine(t2.Item1.ToString() + '\t' + t2.Item2.ToString());
                }

                writer.WriteLine();
                writer.WriteLine("Putwall Stats");
                writer.WriteLine("Property \t Average \t StdDev");
                writer.WriteLine("Time in system \t" + FinalResults.PutwallStatistics.TimeInSystem.Average + "\t" +
                    FinalResults.PutwallStatistics.TimeInSystem.StdDev);
                writer.WriteLine("Time in Process \t" + FinalResults.PutwallStatistics.TimeInProcess.Average + "\t" +
                    FinalResults.PutwallStatistics.TimeInProcess.StdDev);
                writer.WriteLine("Time in Queue \t" + FinalResults.PutwallStatistics.TimeInQueue.Average + "\t" +
                    FinalResults.PutwallStatistics.TimeInQueue.StdDev);
                writer.WriteLine("Time Recirculating \t" + FinalResults.PutwallStatistics.TimeRecirculating.Average + "\t" +
                    FinalResults.PutwallStatistics.TimeRecirculating.StdDev);
                writer.WriteLine("Times Recirculated \t" + FinalResults.PutwallStatistics.TimesRecirculated.Average + "\t" +
                    FinalResults.PutwallStatistics.TimesRecirculated.StdDev);
                writer.WriteLine("Number Created \t" + FinalResults.PutwallStatistics.NumberCreated.Average + "\t" +
                   FinalResults.PutwallStatistics.NumberCreated.StdDev);
                writer.WriteLine("Number Disposed \t" + FinalResults.PutwallStatistics.NumberDisposed.Average + "\t" +
                  FinalResults.PutwallStatistics.NumberDisposed.StdDev);

                writer.WriteLine();
                writer.WriteLine("NonPutwall Stats");
                writer.WriteLine("Property \t Average \t StdDev");
                writer.WriteLine("Time in system \t" + FinalResults.NonPutwallStatistics.TimeInSystem.Average + "\t" +
                    FinalResults.NonPutwallStatistics.TimeInSystem.StdDev);
                writer.WriteLine("Time in Process \t" + FinalResults.NonPutwallStatistics.TimeInProcess.Average + "\t" +
                    FinalResults.NonPutwallStatistics.TimeInProcess.StdDev);
                writer.WriteLine("Time in Queue \t" + FinalResults.NonPutwallStatistics.TimeInQueue.Average + "\t" +
                    FinalResults.NonPutwallStatistics.TimeInQueue.StdDev);
                writer.WriteLine("Time Recirculating \t" + FinalResults.NonPutwallStatistics.TimeRecirculating.Average + "\t" +
                    FinalResults.NonPutwallStatistics.TimeRecirculating.StdDev);
                writer.WriteLine("Times Recirculated \t" + FinalResults.NonPutwallStatistics.TimesRecirculated.Average + "\t" +
                    FinalResults.NonPutwallStatistics.TimesRecirculated.StdDev);
                writer.WriteLine("Number Created \t" + FinalResults.NonPutwallStatistics.NumberCreated.Average + "\t" +
                   FinalResults.NonPutwallStatistics.NumberCreated.StdDev);
                writer.WriteLine("Number Disposed \t" + FinalResults.NonPutwallStatistics.NumberDisposed.Average + "\t" +
                  FinalResults.NonPutwallStatistics.NumberDisposed.StdDev);

            }

        }
        private void SimWithOperatorCount()
        {
            var Data = new WarehouseData();

            var FinalResults = new MetaResults();

            //string basePath = @"C:\Users\Dematic\Desktop\SimLog\";
            //string basePath = @"C:\Users\p2decarv\Desktop\SimLog\";
            string basePath = @"C:\Users\Daniel\Desktop\SimLog\";

            var availability = new List<DateTime>()
            {
                new DateTime(2015,11,10)
            };

            var qSizeData = new List<DateTime>()
            {
                new DateTime(2015,11,9),
                new DateTime(2015,11,10),
                new DateTime(2015,11,11),
                new DateTime(2015,11,12),
                new DateTime(2015,11,13)
            };
            var statsInterval = new Tuple<TimeSpan, TimeSpan>(new TimeSpan(6, 0, 0), new TimeSpan(23, 59, 59));

            var arrivalTimesOverTime = Data.GetInterarrivalTimesOverTime(availability[0]);
            var recircTimesOverTime = Data.GetItemsInRecircOverTime(availability[0]);
            var queueSizeOverTime = Data.GetQueueSizeOverTime(qSizeData, availability[0]);
            var qTimes = Data.GetTimeInQueue(availability[0], statsInterval);

            

            using (var writer = new System.IO.StreamWriter(basePath + "InterArrivals_over_time_real.txt"))
            {
                writer.WriteLine("Time \t Interarrival Time");

                foreach (var t in arrivalTimesOverTime)
                {
                    writer.WriteLine(t.Item1 + "\t" + t.Item2);
                }
            }

            using (var writer = new System.IO.StreamWriter(basePath + "ItemsInRecirc_over_time_real.txt"))
            {
                writer.WriteLine("Time of Day (s) \t Items in Reicirculation");

                for (int i = 0; i < recircTimesOverTime.Length; i++)
                {
                    writer.WriteLine(i + "\t" + recircTimesOverTime[i]);
                }
            }

            using (var writer = new System.IO.StreamWriter(basePath + "QueueSize_over_time_real.txt"))
            {
                writer.WriteLine("Time of Day (s) \t Items in Queue");

                for (int i = 0; i < queueSizeOverTime.Length; i++)
                {
                    writer.WriteLine(i + "\t" + queueSizeOverTime[i]);
                }
            }

            using (var writer = new System.IO.StreamWriter(basePath + "QTimes.txt"))
            {
                writer.WriteLine("Time in Queue");

                foreach (double time in qTimes)
                {
                    writer.WriteLine(time);
                }
            }




            //var logger = new VerboseLogger(@"C:\Users\p2decarv\Desktop\SimLog");
            var logger = new VerboseLogger(@"C:\Users\Daniel\Desktop\SimLog");
            //var logger = new VerboseLogger(@"C:\Users\Dematic\Desktop\SimLog");



            logger.LogDBStats("DBStats2", availability.First(), 600, statsInterval);
            //logger.LogDBStats("DBStats", availability.First());
            //logger.LogDBStats("DBStats", availability.First(), 600);


            List<Tuple<int, int>> throughput = new List<Tuple<int, int>>();
            var iterations = Enumerable.Range(0, 10);

            var intervals = new List<Tuple<TimeSpan, TimeSpan>>();

            int capacityInterval = 30;
            int ArrivalInterval = 30;

            var operaterCount = Data.GetOperatorsPerZMins(availability[0], capacityInterval);
            var ppx = Data.GetPutsPerZMins(availability[0], capacityInterval);
            logger.LogPutsPerHour("Operators_Per_" + capacityInterval + "mins_MinsFromMidnight", operaterCount);
            logger.LogPutsPerHour("Puts_per_" + capacityInterval + "mins_MinsFromMidnight", ppx);



            for (int i = 6 * 60; i < 24 * 60; i += ArrivalInterval)
            {
                if(((double)(i + ArrivalInterval) / 60) == 24)
                {
                    intervals.Add(new Tuple<TimeSpan, TimeSpan>(new TimeSpan((int)Math.Floor((double)i / 60), i % 60, 0),
                    new TimeSpan(23,59,59)));
                }
                else
                {
                    intervals.Add(new Tuple<TimeSpan, TimeSpan>(new TimeSpan((int)Math.Floor((double)i / 60), i % 60, 0),
                    new TimeSpan((int)Math.Floor((double)(i + ArrivalInterval) / 60), (i + ArrivalInterval) % 60, 0)));
                }
                
            }


            var breakTimes = Data.FindBreaks(availability[0], 600);

            var factoryParams = new FactoryParams()
            {
                StartMin = 360,
                 DayLength = 64800,
                //DayLength = 61200,
                Logger = logger,
                NWarmupDays = 0,
                InitialNumberInQueue = 104
                //InitialNumberInQueue = 51
            };

            var distParams = new DistributionSelectionParameters()
            {
                SelectedDaysForData = availability,
                ArrivalDistributionBreakpoints = intervals,
                BreakTimes = breakTimes,
                IntervalForOtherDistributions = statsInterval,
                CapacityIntervalSize = capacityInterval
            };

            var pOfRecirc = Data.RecirculationVSQueueSize(availability[0], qSizeData, distParams.IntervalForOtherDistributions);

            using (var writer = new System.IO.StreamWriter(basePath + "ConditionalProbDist.txt"))
            {
                writer.WriteLine("Queue Size \t Number of Recirculations Observed \t Number of Lane Entries Observed \t Total \t P(Recirc|QSize)");

                foreach (var p in pOfRecirc.OrderBy(x => x.Key))
                {
                    writer.WriteLine(p.Key + "\t" +
                                     p.Value.Item1 + "\t" +
                                     p.Value.Item2 + "\t" +
                                     (p.Value.Item1 + p.Value.Item2) + "\t" +
                                     ((double)p.Value.Item1 / ((double)p.Value.Item1 + (double)p.Value.Item2)));
                }
            }
            

            var warmupDays = new List<Tuple<DateTime, DistributionSelectionParameters>>();

            var allDists = new RequiredDistsWithOperators(factoryParams, distParams, warmupDays);

            var AllDists = allDists.CreateNCopiesWithOperators(10);





            Parallel.ForEach(iterations, i =>
            {
                var dists = AllDists[i];

                var sim = SimulationFactory.SimWithConditionalPofRecircAndNoMaxQueueAndOperators(factoryParams, dists, pOfRecirc);

                sim.Run();


                logger.LogResults(sim.Results, "Results_" + i, (factoryParams.NWarmupDays + 1)*factoryParams.DayLength);


                FinalResults.AddSimResults(sim.Results);

                var t = sim.Results.CalcNumOut();

                throughput.Add(new Tuple<int, int>(t[ProcessType.Putwall], t[ProcessType.NonPutwall]));

                OutputTimeSeries(i, basePath, sim.Results);

                OutputTimedQueueSize(i, basePath, sim.Results);

                WriteOutTimeInQ(i, basePath, sim.Results);

                WriteOutLeftoverCapacity(i, basePath, sim.Results.LeftOverCapacity);
            });


            //using (var writer = new System.IO.StreamWriter(@"C:\Users\p2decarv\Desktop\SimLog\FINALRESULTS.txt"))
            //using (var writer = new System.IO.StreamWriter(@"C:\Users\Daniel\Desktop\SimLog\FINALRESULTS.txt"))
            using (var writer = new System.IO.StreamWriter(basePath + "FINALRESULTS.txt"))
            {
                foreach (Tuple<int, int> t2 in throughput)
                {
                    writer.WriteLine(t2.Item1.ToString() + '\t' + t2.Item2.ToString());
                }

                writer.WriteLine();
                writer.WriteLine("Putwall Stats");
                writer.WriteLine("Property \t Average \t StdDev");
                writer.WriteLine("Time in system \t" + FinalResults.PutwallStatistics.TimeInSystem.Average + "\t" +
                    FinalResults.PutwallStatistics.TimeInSystem.StdDev);
                writer.WriteLine("Time in Process \t" + FinalResults.PutwallStatistics.TimeInProcess.Average + "\t" +
                    FinalResults.PutwallStatistics.TimeInProcess.StdDev);
                writer.WriteLine("Time in Queue \t" + FinalResults.PutwallStatistics.TimeInQueue.Average + "\t" +
                    FinalResults.PutwallStatistics.TimeInQueue.StdDev);
                writer.WriteLine("Time Recirculating \t" + FinalResults.PutwallStatistics.TimeRecirculating.Average + "\t" +
                    FinalResults.PutwallStatistics.TimeRecirculating.StdDev);
                writer.WriteLine("Times Recirculated \t" + FinalResults.PutwallStatistics.TimesRecirculated.Average + "\t" +
                    FinalResults.PutwallStatistics.TimesRecirculated.StdDev);
                writer.WriteLine("Number Created \t" + FinalResults.PutwallStatistics.NumberCreated.Average + "\t" +
                   FinalResults.PutwallStatistics.NumberCreated.StdDev);
                writer.WriteLine("Number Disposed \t" + FinalResults.PutwallStatistics.NumberDisposed.Average + "\t" +
                  FinalResults.PutwallStatistics.NumberDisposed.StdDev);

                writer.WriteLine();
                writer.WriteLine("NonPutwall Stats");
                writer.WriteLine("Property \t Average \t StdDev");
                writer.WriteLine("Time in system \t" + FinalResults.NonPutwallStatistics.TimeInSystem.Average + "\t" +
                    FinalResults.NonPutwallStatistics.TimeInSystem.StdDev);
                writer.WriteLine("Time in Process \t" + FinalResults.NonPutwallStatistics.TimeInProcess.Average + "\t" +
                    FinalResults.NonPutwallStatistics.TimeInProcess.StdDev);
                writer.WriteLine("Time in Queue \t" + FinalResults.NonPutwallStatistics.TimeInQueue.Average + "\t" +
                    FinalResults.NonPutwallStatistics.TimeInQueue.StdDev);
                writer.WriteLine("Time Recirculating \t" + FinalResults.NonPutwallStatistics.TimeRecirculating.Average + "\t" +
                    FinalResults.NonPutwallStatistics.TimeRecirculating.StdDev);
                writer.WriteLine("Times Recirculated \t" + FinalResults.NonPutwallStatistics.TimesRecirculated.Average + "\t" +
                    FinalResults.NonPutwallStatistics.TimesRecirculated.StdDev);
                writer.WriteLine("Number Created \t" + FinalResults.NonPutwallStatistics.NumberCreated.Average + "\t" +
                   FinalResults.NonPutwallStatistics.NumberCreated.StdDev);
                writer.WriteLine("Number Disposed \t" + FinalResults.NonPutwallStatistics.NumberDisposed.Average + "\t" +
                  FinalResults.NonPutwallStatistics.NumberDisposed.StdDev);

            }

        }
        private void SimWithNoMaxQSize()
        {
            var Data = new WarehouseData();

            var FinalResults = new MetaResults();

            string basePath = @"C:\Users\Daniel\Desktop\SimLog\";
            //string basePath = @"C:\Users\Dematic\Desktop\SimLog\";
            //string basePath = @"C:\Users\p2decarv\Desktop\SimLog\";

            var availability = new List<DateTime>()
            {
                new DateTime(2015,11,10)
            };

            var qSizeData = new List<DateTime>()
            {
                new DateTime(2015,11,9),
                new DateTime(2015,11,10),
                new DateTime(2015,11,11),
                new DateTime(2015,11,12),
                new DateTime(2015,11,13)
            };
            var statsInterval = new Tuple<TimeSpan, TimeSpan>(new TimeSpan(6, 0, 0), new TimeSpan(23, 59, 59));

            var arrivalTimesOverTime = Data.GetInterarrivalTimesOverTime(availability[0]);
            var recircTimesOverTime = Data.GetItemsInRecircOverTime(availability[0]);
            var queueSizeOverTime = Data.GetQueueSizeOverTime(qSizeData, availability[0]);
            var qTimes = Data.GetTimeInQueue(availability[0], statsInterval);

            using (var writer = new System.IO.StreamWriter(basePath + "InterArrivals_over_time_real.txt"))
            {
                writer.WriteLine("Time \t Interarrival Time");

                foreach (var t in arrivalTimesOverTime)
                {
                    writer.WriteLine(t.Item1 + "\t" + t.Item2);
                }
            }

            using (var writer = new System.IO.StreamWriter(basePath + "ItemsInRecirc_over_time_real.txt"))
            {
                writer.WriteLine("Time of Day (s) \t Items in Reicirculation");

                for (int i = 0; i < recircTimesOverTime.Length; i++)
                {
                    writer.WriteLine(i + "\t" + recircTimesOverTime[i]);
                }
            }

            using (var writer = new System.IO.StreamWriter(basePath + "QueueSize_over_time_real.txt"))
            {
                writer.WriteLine("Time of Day (s) \t Items in Queue");

                for (int i = 0; i < queueSizeOverTime.Length; i++)
                {
                    writer.WriteLine(i + "\t" + queueSizeOverTime[i]);
                }
            }

            using (var writer = new System.IO.StreamWriter(basePath + "QTimes.txt"))
            {
                writer.WriteLine("Time in Queue");

                foreach (double time in qTimes)
                {
                    writer.WriteLine(time);
                }
            }




            //var logger = new VerboseLogger(@"C:\Users\p2decarv\Desktop\SimLog");
            var logger = new VerboseLogger(@"C:\Users\Daniel\Desktop\SimLog");
            //var logger = new VerboseLogger(@"C:\Users\Dematic\Desktop\SimLog");



            logger.LogDBStats("DBStats2", availability.First(), 600, statsInterval);
            //logger.LogDBStats("DBStats", availability.First());
            //logger.LogDBStats("DBStats", availability.First(), 600);


            List<Tuple<int, int>> throughput = new List<Tuple<int, int>>();
            var iterations = Enumerable.Range(0, 100);

            var intervals = new List<Tuple<TimeSpan, TimeSpan>>();

            int capacityInterval = 1;
            int arrivalInterval = 30;

            for (int i = 6*60; i < 24*60; i += arrivalInterval)
            {
                    intervals.Add(new Tuple<TimeSpan, TimeSpan>(new TimeSpan((int)Math.Floor((double) i / 60), i % 60, 0), 
                        new TimeSpan((int)Math.Floor((double)(i + arrivalInterval) / 60), (i + arrivalInterval) % 60, 0)));
            }
                                   

            var breakTimes = Data.FindBreaks(availability[0], 600);

            var factoryParams = new FactoryParams()
            {
                StartMin = 360,
                DayLength = 64800,
                //DayLength = 61200,
                Logger = logger,
                NWarmupDays = 0,
                InitialNumberInQueue = 104
                //InitialNumberInQueue = 51
            };

            var distParams = new DistributionSelectionParameters()
            {
                SelectedDaysForData = availability,
                ArrivalDistributionBreakpoints = intervals,
                BreakTimes = breakTimes,
                IntervalForOtherDistributions = statsInterval,
                CapacityIntervalSize = capacityInterval
            };

            var pOfRecirc = Data.RecirculationVSQueueSize(availability[0], qSizeData, distParams.IntervalForOtherDistributions);

            using (var writer = new System.IO.StreamWriter(basePath + "ConditionalProbDist.txt"))
            {
                writer.WriteLine("Queue Size \t Number of Recirculations Observed \t Number of Lane Entries Observed \t Total \t P(Recirc|QSize)");

                foreach (var p in pOfRecirc.OrderBy(x => x.Key))
                {
                    writer.WriteLine(p.Key + "\t" +
                                     p.Value.Item1 + "\t" +
                                     p.Value.Item2 + "\t" +
                                     (p.Value.Item1 + p.Value.Item2) + "\t" +
                                     ((double)p.Value.Item1 / ((double)p.Value.Item1 + (double)p.Value.Item2)));
                }
            }

            DateTime warmupDay = new DateTime(2015, 11, 9);

            var warmupParams = new DistributionSelectionParameters()
            {
                SelectedDaysForData = new List<DateTime>() { warmupDay },
                ArrivalDistributionBreakpoints = intervals,
                BreakTimes = breakTimes,
                IntervalForOtherDistributions = statsInterval,
                CapacityIntervalSize = 30
            };

            //var warmupDays = new List<Tuple<DateTime, DistributionSelectionParameters>>()
            //    {
            //        new Tuple<DateTime, DistributionSelectionParameters>(warmupDay, warmupParams)
            //    };
            var warmupDays = new List<Tuple<DateTime, DistributionSelectionParameters>>();

            var allDists = new RequiredDistributions(factoryParams, distParams, warmupDays);

            var AllDists = allDists.CreateNCopies(100);





            Parallel.ForEach(iterations, i =>
            {
                var dists = AllDists[i];

                var sim = SimulationFactory.SimWithConditionalPofRecircAndNoMaxQueue(factoryParams, dists, pOfRecirc);

                sim.Run();


                logger.LogResults(sim.Results, "Results_" + i, 3 * 64800);


                FinalResults.AddSimResults(sim.Results);

                var t = sim.Results.CalcNumOut();

                throughput.Add(new Tuple<int, int>(t[ProcessType.Putwall], t[ProcessType.NonPutwall]));

                OutputTimeSeries(i, basePath, sim.Results);

                OutputTimedQueueSize(i, basePath, sim.Results);

                WriteOutTimeInQ(i, basePath, sim.Results);

                WriteOutLeftoverCapacity(i, basePath, sim.Results.LeftOverCapacity);
            });


            //using (var writer = new System.IO.StreamWriter(@"C:\Users\p2decarv\Desktop\SimLog\FINALRESULTS.txt"))
            //using (var writer = new System.IO.StreamWriter(@"C:\Users\Daniel\Desktop\SimLog\FINALRESULTS.txt"))
            using (var writer = new System.IO.StreamWriter(basePath + "FINALRESULTS.txt"))
            {
                foreach (Tuple<int, int> t2 in throughput)
                {
                    writer.WriteLine(t2.Item1.ToString() + '\t' + t2.Item2.ToString());
                }

                writer.WriteLine();
                writer.WriteLine("Putwall Stats");
                writer.WriteLine("Property \t Average \t StdDev");
                writer.WriteLine("Time in system \t" + FinalResults.PutwallStatistics.TimeInSystem.Average + "\t" +
                    FinalResults.PutwallStatistics.TimeInSystem.StdDev);
                writer.WriteLine("Time in Process \t" + FinalResults.PutwallStatistics.TimeInProcess.Average + "\t" +
                    FinalResults.PutwallStatistics.TimeInProcess.StdDev);
                writer.WriteLine("Time in Queue \t" + FinalResults.PutwallStatistics.TimeInQueue.Average + "\t" +
                    FinalResults.PutwallStatistics.TimeInQueue.StdDev);
                writer.WriteLine("Time Recirculating \t" + FinalResults.PutwallStatistics.TimeRecirculating.Average + "\t" +
                    FinalResults.PutwallStatistics.TimeRecirculating.StdDev);
                writer.WriteLine("Times Recirculated \t" + FinalResults.PutwallStatistics.TimesRecirculated.Average + "\t" +
                    FinalResults.PutwallStatistics.TimesRecirculated.StdDev);
                writer.WriteLine("Number Created \t" + FinalResults.PutwallStatistics.NumberCreated.Average + "\t" +
                   FinalResults.PutwallStatistics.NumberCreated.StdDev);
                writer.WriteLine("Number Disposed \t" + FinalResults.PutwallStatistics.NumberDisposed.Average + "\t" +
                  FinalResults.PutwallStatistics.NumberDisposed.StdDev);

                writer.WriteLine();
                writer.WriteLine("NonPutwall Stats");
                writer.WriteLine("Property \t Average \t StdDev");
                writer.WriteLine("Time in system \t" + FinalResults.NonPutwallStatistics.TimeInSystem.Average + "\t" +
                    FinalResults.NonPutwallStatistics.TimeInSystem.StdDev);
                writer.WriteLine("Time in Process \t" + FinalResults.NonPutwallStatistics.TimeInProcess.Average + "\t" +
                    FinalResults.NonPutwallStatistics.TimeInProcess.StdDev);
                writer.WriteLine("Time in Queue \t" + FinalResults.NonPutwallStatistics.TimeInQueue.Average + "\t" +
                    FinalResults.NonPutwallStatistics.TimeInQueue.StdDev);
                writer.WriteLine("Time Recirculating \t" + FinalResults.NonPutwallStatistics.TimeRecirculating.Average + "\t" +
                    FinalResults.NonPutwallStatistics.TimeRecirculating.StdDev);
                writer.WriteLine("Times Recirculated \t" + FinalResults.NonPutwallStatistics.TimesRecirculated.Average + "\t" +
                    FinalResults.NonPutwallStatistics.TimesRecirculated.StdDev);
                writer.WriteLine("Number Created \t" + FinalResults.NonPutwallStatistics.NumberCreated.Average + "\t" +
                   FinalResults.NonPutwallStatistics.NumberCreated.StdDev);
                writer.WriteLine("Number Disposed \t" + FinalResults.NonPutwallStatistics.NumberDisposed.Average + "\t" +
                  FinalResults.NonPutwallStatistics.NumberDisposed.StdDev);

            }
        }
        private void SimWithConditionalProb()
        {
            var Data = new WarehouseData();

            var FinalResults = new MetaResults();

            string basePath = @"C:\Users\Dematic\Desktop\SimLog\";
            //string basePath = @"C:\Users\p2decarv\Desktop\SimLog\";

            var availability = new List<DateTime>()
            {
                new DateTime(2015,11,10)
            };

            var qSizeData = new List<DateTime>()
            {
                new DateTime(2015,11,9),
                new DateTime(2015,11,10),
                new DateTime(2015,11,11),
                new DateTime(2015,11,12),
                new DateTime(2015,11,13)
            };
            var statsInterval = new Tuple<TimeSpan, TimeSpan>(new TimeSpan(6, 0, 0), new TimeSpan(23, 59, 59));

            var arrivalTimesOverTime = Data.GetInterarrivalTimesOverTime(availability[0]);
            var recircTimesOverTime = Data.GetItemsInRecircOverTime(availability[0]);
            var queueSizeOverTime = Data.GetQueueSizeOverTime(qSizeData, availability[0]);
            var qTimes = Data.GetTimeInQueue(availability[0], statsInterval);

            using (var writer = new System.IO.StreamWriter(basePath + "InterArrivals_over_time_real.txt"))
            {
                writer.WriteLine("Time \t Interarrival Time");

                foreach (var t in arrivalTimesOverTime)
                {
                    writer.WriteLine(t.Item1 + "\t" + t.Item2);
                }
            }

            using (var writer = new System.IO.StreamWriter(basePath + "ItemsInRecirc_over_time_real.txt"))
            {
                writer.WriteLine("Time of Day (s) \t Items in Reicirculation");

                for (int i = 0; i < recircTimesOverTime.Length; i++)
                {
                    writer.WriteLine(i + "\t" + recircTimesOverTime[i]);
                }
            }

            using (var writer = new System.IO.StreamWriter(basePath + "QueueSize_over_time_real.txt"))
            {
                writer.WriteLine("Time of Day (s) \t Items in Queue");

                for (int i = 0; i < queueSizeOverTime.Length; i++)
                {
                    writer.WriteLine(i + "\t" + queueSizeOverTime[i]);
                }
            }

            using (var writer = new System.IO.StreamWriter(basePath + "QTimes.txt"))
            {
                writer.WriteLine("Time in Queue");

                foreach (double time in qTimes)
                {
                    writer.WriteLine(time);
                }
            }

            


            //var logger = new VerboseLogger(@"C:\Users\p2decarv\Desktop\SimLog");
            //var logger = new VerboseLogger(@"C:\Users\Daniel\Desktop\SimLog");
            var logger = new VerboseLogger(@"C:\Users\Dematic\Desktop\SimLog");



            logger.LogDBStats("DBStats2", availability.First(), 600, statsInterval);
            //logger.LogDBStats("DBStats", availability.First());
            //logger.LogDBStats("DBStats", availability.First(), 600);


            List<Tuple<int, int>> throughput = new List<Tuple<int, int>>();
            var iterations = Enumerable.Range(0, 20);

            var intervals = new List<Tuple<TimeSpan, TimeSpan>>()
                {
                    //complete distribution
                    //new Tuple<TimeSpan, TimeSpan>(new TimeSpan(6,0,0), new TimeSpan(22,0,0)),

                    //different intervals
                    //new Tuple<TimeSpan, TimeSpan>(new TimeSpan(6,0,0), new TimeSpan(6,30,0)),
                    //new Tuple<TimeSpan, TimeSpan>(new TimeSpan(6,30,0), new TimeSpan(7,0,0)),
                    //new Tuple<TimeSpan, TimeSpan>(new TimeSpan(7,0,0), new TimeSpan(9,0,0)),
                    //new Tuple<TimeSpan, TimeSpan>(new TimeSpan(9,0,0), new TimeSpan(13,30,0)),
                    //new Tuple<TimeSpan, TimeSpan>(new TimeSpan(13,30,0), new TimeSpan(15,30,0)),
                    //new Tuple<TimeSpan, TimeSpan>(new TimeSpan(15,30,0), new TimeSpan(22,0,0)),

                    //hourly intervals
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(6,0,0), new TimeSpan(7,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(7,0,0), new TimeSpan(8,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(8,0,0), new TimeSpan(9,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(9,0,0), new TimeSpan(10,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(10,0,0), new TimeSpan(11,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(11,0,0), new TimeSpan(12,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(12,0,0), new TimeSpan(13,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(13,0,0), new TimeSpan(14,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(14,0,0), new TimeSpan(15,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(15,0,0), new TimeSpan(16,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(16,0,0), new TimeSpan(17,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(17,0,0), new TimeSpan(18,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(18,0,0), new TimeSpan(19,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(19,0,0), new TimeSpan(20,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(20,0,0), new TimeSpan(21,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(21,0,0), new TimeSpan(22,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(22,0,0), new TimeSpan(23,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(23,0,0), new TimeSpan(24,0,0)),
                };

            var breakTimes = Data.FindBreaks(availability[0], 600);

            var factoryParams = new FactoryParams()
            {
                StartMin = 360,
                DayLength = 64800,
                QueueSize = 104,
                Logger = logger,
                NWarmupDays = 0,
                InitialNumberInQueue = 104
            };

            var distParams = new DistributionSelectionParameters()
            {
                SelectedDaysForData = availability,
                ArrivalDistributionBreakpoints = intervals,
                BreakTimes = breakTimes,
                IntervalForOtherDistributions = statsInterval,
                CapacityIntervalSize = 20
            };

            var pOfRecirc = Data.RecirculationVSQueueSize(availability[0], qSizeData, distParams.IntervalForOtherDistributions);

            using (var writer = new System.IO.StreamWriter(basePath + "ConditionalProbDist.txt"))
            {
                writer.WriteLine("Queue Size \t Number of Recirculations Observed \t Number of Lane Entries Observed \t Total \t P(Recirc|QSize)");

                foreach(var p in pOfRecirc.OrderBy(x => x.Key))
                {
                    writer.WriteLine(p.Key + "\t" + 
                                     p.Value.Item1 + "\t" + 
                                     p.Value.Item2 + "\t" + 
                                     (p.Value.Item1 + p.Value.Item2) + "\t" + 
                                     ((double)p.Value.Item1 / ((double)p.Value.Item1 + (double)p.Value.Item2)));
                }
            }

            DateTime warmupDay = new DateTime(2015, 11, 9);

            var warmupParams = new DistributionSelectionParameters()
            {
                SelectedDaysForData = new List<DateTime>() { warmupDay },
                ArrivalDistributionBreakpoints = intervals,
                BreakTimes = breakTimes,
                IntervalForOtherDistributions = statsInterval,
                CapacityIntervalSize = 30
            };

            //var warmupDays = new List<Tuple<DateTime, DistributionSelectionParameters>>()
            //    {
            //        new Tuple<DateTime, DistributionSelectionParameters>(warmupDay, warmupParams)
            //    };
            var warmupDays = new List<Tuple<DateTime, DistributionSelectionParameters>>();

            var allDists = new RequiredDistributions(factoryParams, distParams, warmupDays);

            var AllDists = allDists.CreateNCopies(20);

            

            

            Parallel.ForEach(iterations, i =>
            {
                var dists = AllDists[i];

                var sim = SimulationFactory.SimWithConditionalPofRecirc(factoryParams, dists, pOfRecirc);

                sim.Run();


                logger.LogResults(sim.Results, "Results_" + i, 3 * 64800);


                FinalResults.AddSimResults(sim.Results);

                var t = sim.Results.CalcNumOut();

                throughput.Add(new Tuple<int, int>(t[ProcessType.Putwall], t[ProcessType.NonPutwall]));

                OutputTimeSeries(i, basePath, sim.Results);

                OutputTimedQueueSize(i, basePath, sim.Results);

                WriteOutTimeInQ(i, basePath, sim.Results);

                WriteOutLeftoverCapacity(i, basePath, sim.Results.LeftOverCapacity);
            });


            //using (var writer = new System.IO.StreamWriter(@"C:\Users\p2decarv\Desktop\SimLog\FINALRESULTS.txt"))
            //using (var writer = new System.IO.StreamWriter(@"C:\Users\Daniel\Desktop\SimLog\FINALRESULTS.txt"))
            using (var writer = new System.IO.StreamWriter(basePath + "FINALRESULTS.txt"))
            {
                foreach (Tuple<int, int> t2 in throughput)
                {
                    writer.WriteLine(t2.Item1.ToString() + '\t' + t2.Item2.ToString());
                }

                writer.WriteLine();
                writer.WriteLine("Putwall Stats");
                writer.WriteLine("Property \t Average \t StdDev");
                writer.WriteLine("Time in system \t" + FinalResults.PutwallStatistics.TimeInSystem.Average + "\t" +
                    FinalResults.PutwallStatistics.TimeInSystem.StdDev);
                writer.WriteLine("Time in Process \t" + FinalResults.PutwallStatistics.TimeInProcess.Average + "\t" +
                    FinalResults.PutwallStatistics.TimeInProcess.StdDev);
                writer.WriteLine("Time in Queue \t" + FinalResults.PutwallStatistics.TimeInQueue.Average + "\t" +
                    FinalResults.PutwallStatistics.TimeInQueue.StdDev);
                writer.WriteLine("Time Recirculating \t" + FinalResults.PutwallStatistics.TimeRecirculating.Average + "\t" +
                    FinalResults.PutwallStatistics.TimeRecirculating.StdDev);
                writer.WriteLine("Times Recirculated \t" + FinalResults.PutwallStatistics.TimesRecirculated.Average + "\t" +
                    FinalResults.PutwallStatistics.TimesRecirculated.StdDev);
                writer.WriteLine("Number Created \t" + FinalResults.PutwallStatistics.NumberCreated.Average + "\t" +
                   FinalResults.PutwallStatistics.NumberCreated.StdDev);
                writer.WriteLine("Number Disposed \t" + FinalResults.PutwallStatistics.NumberDisposed.Average + "\t" +
                  FinalResults.PutwallStatistics.NumberDisposed.StdDev);

                writer.WriteLine();
                writer.WriteLine("NonPutwall Stats");
                writer.WriteLine("Property \t Average \t StdDev");
                writer.WriteLine("Time in system \t" + FinalResults.NonPutwallStatistics.TimeInSystem.Average + "\t" +
                    FinalResults.NonPutwallStatistics.TimeInSystem.StdDev);
                writer.WriteLine("Time in Process \t" + FinalResults.NonPutwallStatistics.TimeInProcess.Average + "\t" +
                    FinalResults.NonPutwallStatistics.TimeInProcess.StdDev);
                writer.WriteLine("Time in Queue \t" + FinalResults.NonPutwallStatistics.TimeInQueue.Average + "\t" +
                    FinalResults.NonPutwallStatistics.TimeInQueue.StdDev);
                writer.WriteLine("Time Recirculating \t" + FinalResults.NonPutwallStatistics.TimeRecirculating.Average + "\t" +
                    FinalResults.NonPutwallStatistics.TimeRecirculating.StdDev);
                writer.WriteLine("Times Recirculated \t" + FinalResults.NonPutwallStatistics.TimesRecirculated.Average + "\t" +
                    FinalResults.NonPutwallStatistics.TimesRecirculated.StdDev);
                writer.WriteLine("Number Created \t" + FinalResults.NonPutwallStatistics.NumberCreated.Average + "\t" +
                   FinalResults.NonPutwallStatistics.NumberCreated.StdDev);
                writer.WriteLine("Number Disposed \t" + FinalResults.NonPutwallStatistics.NumberDisposed.Average + "\t" +
                  FinalResults.NonPutwallStatistics.NumberDisposed.StdDev);

            }
        }
        private void NewSpecificWarmupDay()
        {
            var Data = new WarehouseData();

            var FinalResults = new MetaResultsWithWarmup();

            //string basePath = @"C:\Users\Dematic\Desktop\SimLog\";
            string basePath = @"C:\Users\p2decarv\Desktop\SimLog\";

            var availability = new List<DateTime>()
            {
                new DateTime(2015,11,10)
            };

            var qSizeData = new List<DateTime>()
            {
                new DateTime(2015,11,9),
                new DateTime(2015,11,10),
                new DateTime(2015,11,11),
                new DateTime(2015,11,12),
                new DateTime(2015,11,13)
            };
            var statsInterval = new Tuple<TimeSpan, TimeSpan>(new TimeSpan(6, 0, 0), new TimeSpan(23, 59, 59));

            var arrivalTimesOverTime = Data.GetInterarrivalTimesOverTime(availability[0]);
            var recircTimesOverTime = Data.GetItemsInRecircOverTime(availability[0]);
            var queueSizeOverTime = Data.GetQueueSizeOverTime(qSizeData, availability[0]);
            var qTimes = Data.GetTimeInQueue(availability[0], statsInterval);

            using (var writer = new System.IO.StreamWriter(basePath + "InterArrivals_over_time_real.txt"))
            {
                writer.WriteLine("Time \t Interarrival Time");

                foreach (var t in arrivalTimesOverTime)
                {
                    writer.WriteLine(t.Item1 + "\t" + t.Item2);
                }
            }

            using (var writer = new System.IO.StreamWriter(basePath + "ItemsInRecirc_over_time_real.txt"))
            {
                writer.WriteLine("Time of Day (s) \t Items in Reicirculation");

                for (int i = 0; i < recircTimesOverTime.Length; i++)
                {
                    writer.WriteLine(i + "\t" + recircTimesOverTime[i]);
                }
            }

            using (var writer = new System.IO.StreamWriter(basePath + "QueueSize_over_time_real.txt"))
            {
                writer.WriteLine("Time of Day (s) \t Items in Queue");

                for (int i = 0; i < queueSizeOverTime.Length; i++)
                {
                    writer.WriteLine(i + "\t" + queueSizeOverTime[i]);
                }
            }

            using (var writer = new System.IO.StreamWriter(basePath + "QTimes.txt"))
            {
                writer.WriteLine("Time in Queue");

                foreach (double time in qTimes)
                {
                    writer.WriteLine(time);
                }
            }




            var logger = new VerboseLogger(@"C:\Users\p2decarv\Desktop\SimLog");
            //var logger = new VerboseLogger(@"C:\Users\Daniel\Desktop\SimLog");
            //var logger = new VerboseLogger(@"C:\Users\Dematic\Desktop\SimLog");



            //logger.LogDBStats("DBStats2", availability.First(), 600, statsInterval);
            //logger.LogDBStats("DBStats", availability.First());
            //logger.LogDBStats("DBStats", availability.First(), 600);


            List<Tuple<int, int>> throughput = new List<Tuple<int, int>>();
            var iterations = Enumerable.Range(0, 20);

            var intervals = new List<Tuple<TimeSpan, TimeSpan>>()
                {
                    //complete distribution
                    //new Tuple<TimeSpan, TimeSpan>(new TimeSpan(6,0,0), new TimeSpan(22,0,0)),

                    //different intervals
                    //new Tuple<TimeSpan, TimeSpan>(new TimeSpan(6,0,0), new TimeSpan(6,30,0)),
                    //new Tuple<TimeSpan, TimeSpan>(new TimeSpan(6,30,0), new TimeSpan(7,0,0)),
                    //new Tuple<TimeSpan, TimeSpan>(new TimeSpan(7,0,0), new TimeSpan(9,0,0)),
                    //new Tuple<TimeSpan, TimeSpan>(new TimeSpan(9,0,0), new TimeSpan(13,30,0)),
                    //new Tuple<TimeSpan, TimeSpan>(new TimeSpan(13,30,0), new TimeSpan(15,30,0)),
                    //new Tuple<TimeSpan, TimeSpan>(new TimeSpan(15,30,0), new TimeSpan(22,0,0)),

                    //hourly intervals
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(6,0,0), new TimeSpan(7,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(7,0,0), new TimeSpan(8,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(8,0,0), new TimeSpan(9,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(9,0,0), new TimeSpan(10,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(10,0,0), new TimeSpan(11,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(11,0,0), new TimeSpan(12,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(12,0,0), new TimeSpan(13,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(13,0,0), new TimeSpan(14,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(14,0,0), new TimeSpan(15,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(15,0,0), new TimeSpan(16,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(16,0,0), new TimeSpan(17,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(17,0,0), new TimeSpan(18,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(18,0,0), new TimeSpan(19,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(19,0,0), new TimeSpan(20,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(20,0,0), new TimeSpan(21,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(21,0,0), new TimeSpan(22,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(22,0,0), new TimeSpan(23,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(23,0,0), new TimeSpan(24,0,0)),
                };

            var breakTimes = Data.FindBreaks(availability[0], 600);

            var factoryParams = new FactoryParams()
            {
                StartMin = 360,
                DayLength = 64800,
                QueueSize = 150,
                Logger = logger,
                NWarmupDays = 2,
                InitialNumberInQueue = 0
            };

            var distParams = new DistributionSelectionParameters()
            {
                SelectedDaysForData = availability,
                ArrivalDistributionBreakpoints = intervals,
                BreakTimes = breakTimes,
                IntervalForOtherDistributions = statsInterval,
                CapacityIntervalSize = 5
            };

            DateTime warmupDay = new DateTime(2015, 11, 10);

            var warmupParams = new DistributionSelectionParameters()
            {
                SelectedDaysForData = new List<DateTime>() { warmupDay },
                ArrivalDistributionBreakpoints = intervals,
                BreakTimes = breakTimes,
                IntervalForOtherDistributions = statsInterval,
                CapacityIntervalSize = 5
            };

            var warmupDays = new List<Tuple<DateTime, DistributionSelectionParameters>>()
                {
                    new Tuple<DateTime, DistributionSelectionParameters>(warmupDay, warmupParams)
                };
            

            var allDists = new RequiredDistributions(factoryParams, distParams, warmupDays);

            var AllDists = allDists.CreateNCopies(20);

            Parallel.ForEach(iterations, i =>
            {
                var dists = AllDists[i];

                var sim = SimulationFactory.SimWithFullQueueAndNoDoubleQueue(factoryParams, dists);

                sim.Run();


                logger.LogResults(sim.Results, "Results_" + i, 3 * 64800);


                FinalResults.AddSimResults(sim.Results);

                var t = sim.Results.CalcNumOut();

                throughput.Add(new Tuple<int, int>(t[ProcessType.Putwall], t[ProcessType.NonPutwall]));

                OutputTimeSeries(i, basePath, sim.Results);

                OutputTimedQueueSize(i, basePath, sim.Results);

                WriteOutTimeInQ(i, basePath, sim.Results);

                WriteOutLeftoverCapacity(i, basePath, sim.Results.LeftOverCapacity);
            });


            //using (var writer = new System.IO.StreamWriter(@"C:\Users\p2decarv\Desktop\SimLog\FINALRESULTS.txt"))
            //using (var writer = new System.IO.StreamWriter(@"C:\Users\Daniel\Desktop\SimLog\FINALRESULTS.txt"))
            using (var writer = new System.IO.StreamWriter(basePath + "FINALRESULTS.txt"))
            {
                foreach (Tuple<int, int> t2 in throughput)
                {
                    writer.WriteLine(t2.Item1.ToString() + '\t' + t2.Item2.ToString());
                }

                writer.WriteLine();
                writer.WriteLine("Putwall Stats");
                writer.WriteLine("Property \t Average \t StdDev");
                writer.WriteLine("Time in system \t" + FinalResults.PutwallStatistics.TimeInSystem.Average + "\t" +
                    FinalResults.PutwallStatistics.TimeInSystem.StdDev);
                writer.WriteLine("Time in Process \t" + FinalResults.PutwallStatistics.TimeInProcess.Average + "\t" +
                    FinalResults.PutwallStatistics.TimeInProcess.StdDev);
                writer.WriteLine("Time in Queue \t" + FinalResults.PutwallStatistics.TimeInQueue.Average + "\t" +
                    FinalResults.PutwallStatistics.TimeInQueue.StdDev);
                writer.WriteLine("Time Recirculating \t" + FinalResults.PutwallStatistics.TimeRecirculating.Average + "\t" +
                    FinalResults.PutwallStatistics.TimeRecirculating.StdDev);
                writer.WriteLine("Times Recirculated \t" + FinalResults.PutwallStatistics.TimesRecirculated.Average + "\t" +
                    FinalResults.PutwallStatistics.TimesRecirculated.StdDev);
                writer.WriteLine("Number Created \t" + FinalResults.PutwallStatistics.NumberCreated.Average + "\t" +
                   FinalResults.PutwallStatistics.NumberCreated.StdDev);
                writer.WriteLine("Number Disposed \t" + FinalResults.PutwallStatistics.NumberDisposed.Average + "\t" +
                  FinalResults.PutwallStatistics.NumberDisposed.StdDev);

                writer.WriteLine();
                writer.WriteLine("NonPutwall Stats");
                writer.WriteLine("Property \t Average \t StdDev");
                writer.WriteLine("Time in system \t" + FinalResults.NonPutwallStatistics.TimeInSystem.Average + "\t" +
                    FinalResults.NonPutwallStatistics.TimeInSystem.StdDev);
                writer.WriteLine("Time in Process \t" + FinalResults.NonPutwallStatistics.TimeInProcess.Average + "\t" +
                    FinalResults.NonPutwallStatistics.TimeInProcess.StdDev);
                writer.WriteLine("Time in Queue \t" + FinalResults.NonPutwallStatistics.TimeInQueue.Average + "\t" +
                    FinalResults.NonPutwallStatistics.TimeInQueue.StdDev);
                writer.WriteLine("Time Recirculating \t" + FinalResults.NonPutwallStatistics.TimeRecirculating.Average + "\t" +
                    FinalResults.NonPutwallStatistics.TimeRecirculating.StdDev);
                writer.WriteLine("Times Recirculated \t" + FinalResults.NonPutwallStatistics.TimesRecirculated.Average + "\t" +
                    FinalResults.NonPutwallStatistics.TimesRecirculated.StdDev);
                writer.WriteLine("Number Created \t" + FinalResults.NonPutwallStatistics.NumberCreated.Average + "\t" +
                   FinalResults.NonPutwallStatistics.NumberCreated.StdDev);
                writer.WriteLine("Number Disposed \t" + FinalResults.NonPutwallStatistics.NumberDisposed.Average + "\t" +
                  FinalResults.NonPutwallStatistics.NumberDisposed.StdDev);

            }
        }
        private void SpecificQueueNoQueueTime()
        {
            var Data = new WarehouseData();

            var FinalResults = new MetaResults();

            //string basePath = @"C:\Users\Dematic\Desktop\SimLog\";
            string basePath = @"C:\Users\p2decarv\Desktop\SimLog\";

            var availability = new List<DateTime>()
            {
                new DateTime(2015,11,10)
            };

            var qSizeData = new List<DateTime>()
            {
                new DateTime(2015,11,9),
                new DateTime(2015,11,10),
                new DateTime(2015,11,11),
                new DateTime(2015,11,12),
                new DateTime(2015,11,13)
            };
            var statsInterval = new Tuple<TimeSpan, TimeSpan>(new TimeSpan(6, 0, 0), new TimeSpan(23, 59, 59));

            var arrivalTimesOverTime = Data.GetInterarrivalTimesOverTime(availability[0]);
            var recircTimesOverTime = Data.GetItemsInRecircOverTime(availability[0]);
            var queueSizeOverTime = Data.GetQueueSizeOverTime(qSizeData, availability[0]);
            var qTimes = Data.GetTimeInQueue(availability[0], statsInterval);

            using (var writer = new System.IO.StreamWriter(basePath + "InterArrivals_over_time_real.txt"))
            {
                writer.WriteLine("Time \t Interarrival Time");

                foreach (var t in arrivalTimesOverTime)
                {
                    writer.WriteLine(t.Item1 + "\t" + t.Item2);
                }
            }

            using (var writer = new System.IO.StreamWriter(basePath + "ItemsInRecirc_over_time_real.txt"))
            {
                writer.WriteLine("Time of Day (s) \t Items in Reicirculation");

                for (int i = 0; i < recircTimesOverTime.Length; i++)
                {
                    writer.WriteLine(i + "\t" + recircTimesOverTime[i]);
                }
            }

            using (var writer = new System.IO.StreamWriter(basePath + "QueueSize_over_time_real.txt"))
            {
                writer.WriteLine("Time of Day (s) \t Items in Queue");

                for (int i = 0; i < queueSizeOverTime.Length; i++)
                {
                    writer.WriteLine(i + "\t" + queueSizeOverTime[i]);
                }
            }

            using (var writer = new System.IO.StreamWriter(basePath + "QTimes.txt"))
            {
                writer.WriteLine("Time in Queue");

                foreach (double time in qTimes)
                {
                    writer.WriteLine(time);
                }
            }




            var logger = new VerboseLogger(@"C:\Users\p2decarv\Desktop\SimLog");
            //var logger = new VerboseLogger(@"C:\Users\Daniel\Desktop\SimLog");
            //var logger = new VerboseLogger(@"C:\Users\Dematic\Desktop\SimLog");



            //logger.LogDBStats("DBStats2", availability.First(), 600, statsInterval);
            //logger.LogDBStats("DBStats", availability.First());
            //logger.LogDBStats("DBStats", availability.First(), 600);


            List<Tuple<int, int>> throughput = new List<Tuple<int, int>>();
            var iterations = Enumerable.Range(0, 20);

            var intervals = new List<Tuple<TimeSpan, TimeSpan>>()
                {
                    //complete distribution
                    //new Tuple<TimeSpan, TimeSpan>(new TimeSpan(6,0,0), new TimeSpan(22,0,0)),

                    //different intervals
                    //new Tuple<TimeSpan, TimeSpan>(new TimeSpan(6,0,0), new TimeSpan(6,30,0)),
                    //new Tuple<TimeSpan, TimeSpan>(new TimeSpan(6,30,0), new TimeSpan(7,0,0)),
                    //new Tuple<TimeSpan, TimeSpan>(new TimeSpan(7,0,0), new TimeSpan(9,0,0)),
                    //new Tuple<TimeSpan, TimeSpan>(new TimeSpan(9,0,0), new TimeSpan(13,30,0)),
                    //new Tuple<TimeSpan, TimeSpan>(new TimeSpan(13,30,0), new TimeSpan(15,30,0)),
                    //new Tuple<TimeSpan, TimeSpan>(new TimeSpan(15,30,0), new TimeSpan(22,0,0)),

                    //hourly intervals
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(6,0,0), new TimeSpan(7,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(7,0,0), new TimeSpan(8,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(8,0,0), new TimeSpan(9,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(9,0,0), new TimeSpan(10,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(10,0,0), new TimeSpan(11,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(11,0,0), new TimeSpan(12,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(12,0,0), new TimeSpan(13,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(13,0,0), new TimeSpan(14,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(14,0,0), new TimeSpan(15,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(15,0,0), new TimeSpan(16,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(16,0,0), new TimeSpan(17,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(17,0,0), new TimeSpan(18,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(18,0,0), new TimeSpan(19,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(19,0,0), new TimeSpan(20,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(20,0,0), new TimeSpan(21,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(21,0,0), new TimeSpan(22,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(22,0,0), new TimeSpan(23,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(23,0,0), new TimeSpan(24,0,0)),
                };

            var breakTimes = Data.FindBreaks(availability[0], 600);

            var factoryParams = new FactoryParams()
            {
                StartMin = 360,
                DayLength = 64800,
                QueueSize = 150,
                Logger = logger,
                NWarmupDays = 0,
                InitialNumberInQueue = 104
            };

            var distParams = new DistributionSelectionParameters()
            {
                SelectedDaysForData = availability,
                ArrivalDistributionBreakpoints = intervals,
                BreakTimes = breakTimes,
                IntervalForOtherDistributions = statsInterval,
                CapacityIntervalSize = 5
            };



            var warmupDays = new List<Tuple<DateTime, DistributionSelectionParameters>>();

            var allDists = new RequiredDistributions(factoryParams, distParams, warmupDays);

            var AllDists = allDists.CreateNCopies(20);

            Parallel.ForEach(iterations, i =>
            {
                var dists = AllDists[i];

                var sim = SimulationFactory.SimWithFullQueueAndNoDoubleQueue(factoryParams, dists);

                sim.Run();


                logger.LogResults(sim.Results, "Results_" + i, 3 * 64800);


                FinalResults.AddSimResults(sim.Results);

                var t = sim.Results.CalcNumOut();

                throughput.Add(new Tuple<int, int>(t[ProcessType.Putwall], t[ProcessType.NonPutwall]));

                OutputTimeSeries(i, basePath, sim.Results);

                OutputTimedQueueSize(i, basePath, sim.Results);

                WriteOutTimeInQ(i, basePath, sim.Results);

                WriteOutLeftoverCapacity(i, basePath, sim.Results.LeftOverCapacity);
            });


            //using (var writer = new System.IO.StreamWriter(@"C:\Users\p2decarv\Desktop\SimLog\FINALRESULTS.txt"))
            //using (var writer = new System.IO.StreamWriter(@"C:\Users\Daniel\Desktop\SimLog\FINALRESULTS.txt"))
            using (var writer = new System.IO.StreamWriter(basePath + "FINALRESULTS.txt"))
            {
                foreach (Tuple<int, int> t2 in throughput)
                {
                    writer.WriteLine(t2.Item1.ToString() + '\t' + t2.Item2.ToString());
                }

                writer.WriteLine();
                writer.WriteLine("Putwall Stats");
                writer.WriteLine("Property \t Average \t StdDev");
                writer.WriteLine("Time in system \t" + FinalResults.PutwallStatistics.TimeInSystem.Average + "\t" +
                    FinalResults.PutwallStatistics.TimeInSystem.StdDev);
                writer.WriteLine("Time in Process \t" + FinalResults.PutwallStatistics.TimeInProcess.Average + "\t" +
                    FinalResults.PutwallStatistics.TimeInProcess.StdDev);
                writer.WriteLine("Time in Queue \t" + FinalResults.PutwallStatistics.TimeInQueue.Average + "\t" +
                    FinalResults.PutwallStatistics.TimeInQueue.StdDev);
                writer.WriteLine("Time Recirculating \t" + FinalResults.PutwallStatistics.TimeRecirculating.Average + "\t" +
                    FinalResults.PutwallStatistics.TimeRecirculating.StdDev);
                writer.WriteLine("Times Recirculated \t" + FinalResults.PutwallStatistics.TimesRecirculated.Average + "\t" +
                    FinalResults.PutwallStatistics.TimesRecirculated.StdDev);
                writer.WriteLine("Number Created \t" + FinalResults.PutwallStatistics.NumberCreated.Average + "\t" +
                   FinalResults.PutwallStatistics.NumberCreated.StdDev);
                writer.WriteLine("Number Disposed \t" + FinalResults.PutwallStatistics.NumberDisposed.Average + "\t" +
                  FinalResults.PutwallStatistics.NumberDisposed.StdDev);

                writer.WriteLine();
                writer.WriteLine("NonPutwall Stats");
                writer.WriteLine("Property \t Average \t StdDev");
                writer.WriteLine("Time in system \t" + FinalResults.NonPutwallStatistics.TimeInSystem.Average + "\t" +
                    FinalResults.NonPutwallStatistics.TimeInSystem.StdDev);
                writer.WriteLine("Time in Process \t" + FinalResults.NonPutwallStatistics.TimeInProcess.Average + "\t" +
                    FinalResults.NonPutwallStatistics.TimeInProcess.StdDev);
                writer.WriteLine("Time in Queue \t" + FinalResults.NonPutwallStatistics.TimeInQueue.Average + "\t" +
                    FinalResults.NonPutwallStatistics.TimeInQueue.StdDev);
                writer.WriteLine("Time Recirculating \t" + FinalResults.NonPutwallStatistics.TimeRecirculating.Average + "\t" +
                    FinalResults.NonPutwallStatistics.TimeRecirculating.StdDev);
                writer.WriteLine("Times Recirculated \t" + FinalResults.NonPutwallStatistics.TimesRecirculated.Average + "\t" +
                    FinalResults.NonPutwallStatistics.TimesRecirculated.StdDev);
                writer.WriteLine("Number Created \t" + FinalResults.NonPutwallStatistics.NumberCreated.Average + "\t" +
                   FinalResults.NonPutwallStatistics.NumberCreated.StdDev);
                writer.WriteLine("Number Disposed \t" + FinalResults.NonPutwallStatistics.NumberDisposed.Average + "\t" +
                  FinalResults.NonPutwallStatistics.NumberDisposed.StdDev);

            }
        }
        private void IntervalQueue()
        {
            var Data = new WarehouseData();

            var FinalResults = new MetaResultsWithWarmup();

            //string basePath = @"C:\Users\Dematic\Desktop\SimLog\";
            string basePath = @"C:\Users\p2decarv\Desktop\SimLog\";

            var availability = new List<DateTime>()
            {
                new DateTime(2015,11,10)
            };

            var qSizeData = new List<DateTime>()
            {
                new DateTime(2015,11,9),
                new DateTime(2015,11,10),
                new DateTime(2015,11,11),
                new DateTime(2015,11,12),
                new DateTime(2015,11,13)
            };
            var statsInterval = new Tuple<TimeSpan, TimeSpan>(new TimeSpan(6, 0, 0), new TimeSpan(23, 59, 59));

            var arrivalTimesOverTime = Data.GetInterarrivalTimesOverTime(availability[0]);
            var recircTimesOverTime = Data.GetItemsInRecircOverTime(availability[0]);
            var queueSizeOverTime = Data.GetQueueSizeOverTime(qSizeData, availability[0]);
            var qTimes = Data.GetTimeInQueue(availability[0], statsInterval);

            using (var writer = new System.IO.StreamWriter(basePath + "InterArrivals_over_time_real.txt"))
            {
                writer.WriteLine("Time \t Interarrival Time");

                foreach (var t in arrivalTimesOverTime)
                {
                    writer.WriteLine(t.Item1 + "\t" + t.Item2);
                }
            }

            using (var writer = new System.IO.StreamWriter(basePath + "ItemsInRecirc_over_time_real.txt"))
            {
                writer.WriteLine("Time of Day (s) \t Items in Reicirculation");

                for (int i = 0; i < recircTimesOverTime.Length; i++)
                {
                    writer.WriteLine(i + "\t" + recircTimesOverTime[i]);
                }
            }

            using (var writer = new System.IO.StreamWriter(basePath + "QueueSize_over_time_real.txt"))
            {
                writer.WriteLine("Time of Day (s) \t Items in Queue");

                for (int i = 0; i < queueSizeOverTime.Length; i++)
                {
                    writer.WriteLine(i + "\t" + queueSizeOverTime[i]);
                }
            }

            using (var writer = new System.IO.StreamWriter(basePath + "QTimes.txt"))
            {
                writer.WriteLine("Time in Queue");

                foreach (double time in qTimes)
                {
                    writer.WriteLine(time);
                }
            }




            var logger = new VerboseLogger(@"C:\Users\p2decarv\Desktop\SimLog");
            //var logger = new VerboseLogger(@"C:\Users\Daniel\Desktop\SimLog");
            //var logger = new VerboseLogger(@"C:\Users\Dematic\Desktop\SimLog");



            //logger.LogDBStats("DBStats2", availability.First(), 600, statsInterval);
            //logger.LogDBStats("DBStats", availability.First());
            //logger.LogDBStats("DBStats", availability.First(), 600);


            List<Tuple<int, int>> throughput = new List<Tuple<int, int>>();
            var iterations = Enumerable.Range(0, 20);

            var intervals = new List<Tuple<TimeSpan, TimeSpan>>()
                {
                    //complete distribution
                    //new Tuple<TimeSpan, TimeSpan>(new TimeSpan(6,0,0), new TimeSpan(22,0,0)),

                    //different intervals
                    //new Tuple<TimeSpan, TimeSpan>(new TimeSpan(6,0,0), new TimeSpan(6,30,0)),
                    //new Tuple<TimeSpan, TimeSpan>(new TimeSpan(6,30,0), new TimeSpan(7,0,0)),
                    //new Tuple<TimeSpan, TimeSpan>(new TimeSpan(7,0,0), new TimeSpan(9,0,0)),
                    //new Tuple<TimeSpan, TimeSpan>(new TimeSpan(9,0,0), new TimeSpan(13,30,0)),
                    //new Tuple<TimeSpan, TimeSpan>(new TimeSpan(13,30,0), new TimeSpan(15,30,0)),
                    //new Tuple<TimeSpan, TimeSpan>(new TimeSpan(15,30,0), new TimeSpan(22,0,0)),

                    //hourly intervals
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(6,0,0), new TimeSpan(7,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(7,0,0), new TimeSpan(8,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(8,0,0), new TimeSpan(9,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(9,0,0), new TimeSpan(10,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(10,0,0), new TimeSpan(11,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(11,0,0), new TimeSpan(12,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(12,0,0), new TimeSpan(13,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(13,0,0), new TimeSpan(14,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(14,0,0), new TimeSpan(15,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(15,0,0), new TimeSpan(16,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(16,0,0), new TimeSpan(17,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(17,0,0), new TimeSpan(18,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(18,0,0), new TimeSpan(19,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(19,0,0), new TimeSpan(20,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(20,0,0), new TimeSpan(21,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(21,0,0), new TimeSpan(22,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(22,0,0), new TimeSpan(23,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(23,0,0), new TimeSpan(24,0,0)),
                };

            var breakTimes = Data.FindBreaks(availability[0], 600);

            var factoryParams = new FactoryParams()
            {
                StartMin = 360,
                DayLength = 64800,
                QueueSize = 150,
                Logger = logger,
                NWarmupDays = 0
            };

            var distParams = new DistributionSelectionParameters()
            {
                SelectedDaysForData = availability,
                ArrivalDistributionBreakpoints = intervals,
                BreakTimes = breakTimes,
                IntervalForOtherDistributions = statsInterval,
                CapacityIntervalSize = 5
            };

            

            var warmupDays = new List<Tuple<DateTime, DistributionSelectionParameters>>();

            var allDists = new RequiredDistributions(factoryParams, distParams, warmupDays);

            var AllDists = allDists.CreateNCopies(20);

            Parallel.ForEach(iterations, i =>
            {
                var dists = AllDists[i];

                var sim = SimulationFactory.SimWithIntervalQueueSize(factoryParams, distParams, dists, 10*60);

                sim.Run();


                logger.LogResults(sim.Results, "Results_" + i, 3 * 64800);


                FinalResults.AddSimResults(sim.Results);

                var t = sim.Results.CalcNumOut();

                throughput.Add(new Tuple<int, int>(t[ProcessType.Putwall], t[ProcessType.NonPutwall]));

                OutputTimeSeries(i, basePath, sim.Results);

                OutputTimedQueueSize(i, basePath, sim.Results);

                WriteOutTimeInQ(i, basePath, sim.Results);

                WriteOutLeftoverCapacity(i, basePath, sim.Results.LeftOverCapacity);
            });


            //using (var writer = new System.IO.StreamWriter(@"C:\Users\p2decarv\Desktop\SimLog\FINALRESULTS.txt"))
            //using (var writer = new System.IO.StreamWriter(@"C:\Users\Daniel\Desktop\SimLog\FINALRESULTS.txt"))
            using (var writer = new System.IO.StreamWriter(basePath + "FINALRESULTS.txt"))
            {
                foreach (Tuple<int, int> t2 in throughput)
                {
                    writer.WriteLine(t2.Item1.ToString() + '\t' + t2.Item2.ToString());
                }

                writer.WriteLine();
                writer.WriteLine("Putwall Stats");
                writer.WriteLine("Property \t Average \t StdDev");
                writer.WriteLine("Time in system \t" + FinalResults.PutwallStatistics.TimeInSystem.Average + "\t" +
                    FinalResults.PutwallStatistics.TimeInSystem.StdDev);
                writer.WriteLine("Time in Process \t" + FinalResults.PutwallStatistics.TimeInProcess.Average + "\t" +
                    FinalResults.PutwallStatistics.TimeInProcess.StdDev);
                writer.WriteLine("Time in Queue \t" + FinalResults.PutwallStatistics.TimeInQueue.Average + "\t" +
                    FinalResults.PutwallStatistics.TimeInQueue.StdDev);
                writer.WriteLine("Time Recirculating \t" + FinalResults.PutwallStatistics.TimeRecirculating.Average + "\t" +
                    FinalResults.PutwallStatistics.TimeRecirculating.StdDev);
                writer.WriteLine("Times Recirculated \t" + FinalResults.PutwallStatistics.TimesRecirculated.Average + "\t" +
                    FinalResults.PutwallStatistics.TimesRecirculated.StdDev);
                writer.WriteLine("Number Created \t" + FinalResults.PutwallStatistics.NumberCreated.Average + "\t" +
                   FinalResults.PutwallStatistics.NumberCreated.StdDev);
                writer.WriteLine("Number Disposed \t" + FinalResults.PutwallStatistics.NumberDisposed.Average + "\t" +
                  FinalResults.PutwallStatistics.NumberDisposed.StdDev);
                writer.WriteLine("Of Day Number Disposed \t" + FinalResults.PutwallStatistics.NumOutOfArrivals.Average + "\t" +
                  FinalResults.PutwallStatistics.NumOutOfArrivals.StdDev);

                writer.WriteLine();
                writer.WriteLine("NonPutwall Stats");
                writer.WriteLine("Property \t Average \t StdDev");
                writer.WriteLine("Time in system \t" + FinalResults.NonPutwallStatistics.TimeInSystem.Average + "\t" +
                    FinalResults.NonPutwallStatistics.TimeInSystem.StdDev);
                writer.WriteLine("Time in Process \t" + FinalResults.NonPutwallStatistics.TimeInProcess.Average + "\t" +
                    FinalResults.NonPutwallStatistics.TimeInProcess.StdDev);
                writer.WriteLine("Time in Queue \t" + FinalResults.NonPutwallStatistics.TimeInQueue.Average + "\t" +
                    FinalResults.NonPutwallStatistics.TimeInQueue.StdDev);
                writer.WriteLine("Time Recirculating \t" + FinalResults.NonPutwallStatistics.TimeRecirculating.Average + "\t" +
                    FinalResults.NonPutwallStatistics.TimeRecirculating.StdDev);
                writer.WriteLine("Times Recirculated \t" + FinalResults.NonPutwallStatistics.TimesRecirculated.Average + "\t" +
                    FinalResults.NonPutwallStatistics.TimesRecirculated.StdDev);
                writer.WriteLine("Number Created \t" + FinalResults.NonPutwallStatistics.NumberCreated.Average + "\t" +
                   FinalResults.NonPutwallStatistics.NumberCreated.StdDev);
                writer.WriteLine("Number Disposed \t" + FinalResults.NonPutwallStatistics.NumberDisposed.Average + "\t" +
                  FinalResults.NonPutwallStatistics.NumberDisposed.StdDev);
                writer.WriteLine("Of Day Number Disposed \t" + FinalResults.NonPutwallStatistics.NumOutOfArrivals.Average + "\t" +
                  FinalResults.NonPutwallStatistics.NumOutOfArrivals.StdDev);

            }
        }
        private void newSimP()
        {
            var Data = new WarehouseData();

            var FinalResults = new MetaResultsWithWarmup();

            string basePath = @"C:\Users\Dematic\Desktop\SimLog\";
            //string basePath = @"C:\Users\p2decarv\Desktop\SimLog\";

            var availability = new List<DateTime>()
            {
                new DateTime(2015,11,10)
            };
            var statsInterval = new Tuple<TimeSpan, TimeSpan>(new TimeSpan(6, 0, 0), new TimeSpan(23, 59, 59));

            var arrivalTimesOverTime = Data.GetInterarrivalTimesOverTime(availability[0]);
            var recircTimesOverTime = Data.GetItemsInRecircOverTime(availability[0]);
            var queueSizeOverTime = Data.GetQueueSizeOverTime(availability[0]);
            var qTimes = Data.GetTimeInQueue(availability[0], statsInterval);

            using (var writer = new System.IO.StreamWriter(basePath + "InterArrivals_over_time_real.txt"))
            {
                writer.WriteLine("Time \t Interarrival Time");

                foreach (var t in arrivalTimesOverTime)
                {
                    writer.WriteLine(t.Item1 + "\t" + t.Item2);
                }
            }

            using (var writer = new System.IO.StreamWriter(basePath + "ItemsInRecirc_over_time_real.txt"))
            {
                writer.WriteLine("Time of Day (s) \t Items in Reicirculation");

                for (int i = 0; i < recircTimesOverTime.Length; i++)
                {
                    writer.WriteLine(i + "\t" + recircTimesOverTime[i]);
                }
            }

            using (var writer = new System.IO.StreamWriter(basePath + "QueueSize_over_time_real.txt"))
            {
                writer.WriteLine("Time of Day (s) \t Items in Queue");

                for (int i = 0; i < queueSizeOverTime.Length; i++)
                {
                    writer.WriteLine(i + "\t" + queueSizeOverTime[i]);
                }
            }

            using (var writer = new System.IO.StreamWriter(basePath + "QTimes.txt"))
            {
                writer.WriteLine("Time in Queue");

                foreach (double time in qTimes)
                {
                    writer.WriteLine(time);
                }
            }




            //var logger = new VerboseLogger(@"C:\Users\p2decarv\Desktop\SimLog");
            //var logger = new VerboseLogger(@"C:\Users\Daniel\Desktop\SimLog");
            var logger = new VerboseLogger(@"C:\Users\Dematic\Desktop\SimLog");



            //logger.LogDBStats("DBStats2", availability.First(), 600, statsInterval);
            //logger.LogDBStats("DBStats", availability.First());
            //logger.LogDBStats("DBStats", availability.First(), 600);


            List<Tuple<int, int>> throughput = new List<Tuple<int, int>>();
            var iterations = Enumerable.Range(0, 20);

            var intervals = new List<Tuple<TimeSpan, TimeSpan>>()
                {
                    //complete distribution
                    //new Tuple<TimeSpan, TimeSpan>(new TimeSpan(6,0,0), new TimeSpan(22,0,0)),

                    //different intervals
                    //new Tuple<TimeSpan, TimeSpan>(new TimeSpan(6,0,0), new TimeSpan(6,30,0)),
                    //new Tuple<TimeSpan, TimeSpan>(new TimeSpan(6,30,0), new TimeSpan(7,0,0)),
                    //new Tuple<TimeSpan, TimeSpan>(new TimeSpan(7,0,0), new TimeSpan(9,0,0)),
                    //new Tuple<TimeSpan, TimeSpan>(new TimeSpan(9,0,0), new TimeSpan(13,30,0)),
                    //new Tuple<TimeSpan, TimeSpan>(new TimeSpan(13,30,0), new TimeSpan(15,30,0)),
                    //new Tuple<TimeSpan, TimeSpan>(new TimeSpan(15,30,0), new TimeSpan(22,0,0)),

                    //hourly intervals
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(6,0,0), new TimeSpan(7,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(7,0,0), new TimeSpan(8,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(8,0,0), new TimeSpan(9,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(9,0,0), new TimeSpan(10,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(10,0,0), new TimeSpan(11,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(11,0,0), new TimeSpan(12,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(12,0,0), new TimeSpan(13,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(13,0,0), new TimeSpan(14,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(14,0,0), new TimeSpan(15,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(15,0,0), new TimeSpan(16,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(16,0,0), new TimeSpan(17,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(17,0,0), new TimeSpan(18,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(18,0,0), new TimeSpan(19,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(19,0,0), new TimeSpan(20,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(20,0,0), new TimeSpan(21,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(21,0,0), new TimeSpan(22,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(22,0,0), new TimeSpan(23,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(23,0,0), new TimeSpan(24,0,0)),
                };

            var breakTimes = Data.FindBreaks(availability[0], 600);

            var factoryParams = new FactoryParams()
            {
                StartMin = 360,
                DayLength = 64800,
                QueueSize = 150,
                Logger = logger,
                NWarmupDays = 1
            };

            var distParams = new DistributionSelectionParameters()
            {
                SelectedDaysForData = availability,
                ArrivalDistributionBreakpoints = intervals,
                BreakTimes = breakTimes,
                IntervalForOtherDistributions = statsInterval,
                CapacityIntervalSize = 30
            };

            DateTime warmupDay = new DateTime(2015, 11, 9);

            var warmupParams = new DistributionSelectionParameters()
            {
                SelectedDaysForData = new List<DateTime>() { warmupDay },
                ArrivalDistributionBreakpoints = intervals,
                BreakTimes = breakTimes,
                IntervalForOtherDistributions = statsInterval,
                CapacityIntervalSize = 30
            };

            var warmupDays = new List<Tuple<DateTime, DistributionSelectionParameters>>()
                {
                    new Tuple<DateTime, DistributionSelectionParameters>(warmupDay, warmupParams)
                };

            var allDists = new RequiredDistributions(factoryParams, distParams, warmupDays);

            var AllDists = allDists.CreateNCopies(20);

            Parallel.ForEach(iterations, i =>
            {
                var dists = AllDists[i];

                var sim = SimulationFactory.SimWithSpecificWarmupDay(factoryParams, dists);

                sim.Run();


                logger.LogResults(sim.Results, "Results_" + i, 3 * 64800);


                FinalResults.AddSimResults(sim.Results);

                var t = sim.Results.CalcNumOut();

                throughput.Add(new Tuple<int, int>(t[ProcessType.Putwall], t[ProcessType.NonPutwall]));

                OutputTimeSeries(i, basePath, sim.Results);

                OutputTimedQueueSize(i, basePath, sim.Results);

                WriteOutTimeInQ(i, basePath, sim.Results);

                WriteOutLeftoverCapacity(i, basePath, sim.Results.LeftOverCapacity);
            });


            //using (var writer = new System.IO.StreamWriter(@"C:\Users\p2decarv\Desktop\SimLog\FINALRESULTS.txt"))
            //using (var writer = new System.IO.StreamWriter(@"C:\Users\Daniel\Desktop\SimLog\FINALRESULTS.txt"))
            using (var writer = new System.IO.StreamWriter(basePath + "FINALRESULTS.txt"))
            {
                foreach (Tuple<int, int> t2 in throughput)
                {
                    writer.WriteLine(t2.Item1.ToString() + '\t' + t2.Item2.ToString());
                }

                writer.WriteLine();
                writer.WriteLine("Putwall Stats");
                writer.WriteLine("Property \t Average \t StdDev");
                writer.WriteLine("Time in system \t" + FinalResults.PutwallStatistics.TimeInSystem.Average + "\t" +
                    FinalResults.PutwallStatistics.TimeInSystem.StdDev);
                writer.WriteLine("Time in Process \t" + FinalResults.PutwallStatistics.TimeInProcess.Average + "\t" +
                    FinalResults.PutwallStatistics.TimeInProcess.StdDev);
                writer.WriteLine("Time in Queue \t" + FinalResults.PutwallStatistics.TimeInQueue.Average + "\t" +
                    FinalResults.PutwallStatistics.TimeInQueue.StdDev);
                writer.WriteLine("Time Recirculating \t" + FinalResults.PutwallStatistics.TimeRecirculating.Average + "\t" +
                    FinalResults.PutwallStatistics.TimeRecirculating.StdDev);
                writer.WriteLine("Times Recirculated \t" + FinalResults.PutwallStatistics.TimesRecirculated.Average + "\t" +
                    FinalResults.PutwallStatistics.TimesRecirculated.StdDev);
                writer.WriteLine("Number Created \t" + FinalResults.PutwallStatistics.NumberCreated.Average + "\t" +
                   FinalResults.PutwallStatistics.NumberCreated.StdDev);
                writer.WriteLine("Number Disposed \t" + FinalResults.PutwallStatistics.NumberDisposed.Average + "\t" +
                  FinalResults.PutwallStatistics.NumberDisposed.StdDev);
                writer.WriteLine("Of Day Number Disposed \t" + FinalResults.PutwallStatistics.NumOutOfArrivals.Average + "\t" +
                  FinalResults.PutwallStatistics.NumOutOfArrivals.StdDev);

                writer.WriteLine();
                writer.WriteLine("NonPutwall Stats");
                writer.WriteLine("Property \t Average \t StdDev");
                writer.WriteLine("Time in system \t" + FinalResults.NonPutwallStatistics.TimeInSystem.Average + "\t" +
                    FinalResults.NonPutwallStatistics.TimeInSystem.StdDev);
                writer.WriteLine("Time in Process \t" + FinalResults.NonPutwallStatistics.TimeInProcess.Average + "\t" +
                    FinalResults.NonPutwallStatistics.TimeInProcess.StdDev);
                writer.WriteLine("Time in Queue \t" + FinalResults.NonPutwallStatistics.TimeInQueue.Average + "\t" +
                    FinalResults.NonPutwallStatistics.TimeInQueue.StdDev);
                writer.WriteLine("Time Recirculating \t" + FinalResults.NonPutwallStatistics.TimeRecirculating.Average + "\t" +
                    FinalResults.NonPutwallStatistics.TimeRecirculating.StdDev);
                writer.WriteLine("Times Recirculated \t" + FinalResults.NonPutwallStatistics.TimesRecirculated.Average + "\t" +
                    FinalResults.NonPutwallStatistics.TimesRecirculated.StdDev);
                writer.WriteLine("Number Created \t" + FinalResults.NonPutwallStatistics.NumberCreated.Average + "\t" +
                   FinalResults.NonPutwallStatistics.NumberCreated.StdDev);
                writer.WriteLine("Number Disposed \t" + FinalResults.NonPutwallStatistics.NumberDisposed.Average + "\t" +
                  FinalResults.NonPutwallStatistics.NumberDisposed.StdDev);
                writer.WriteLine("Of Day Number Disposed \t" + FinalResults.NonPutwallStatistics.NumOutOfArrivals.Average + "\t" +
                  FinalResults.NonPutwallStatistics.NumOutOfArrivals.StdDev);

            }
        }
        private void SimWithDifferentWarmupDay()
        {
            var Data = new WarehouseData();

            var FinalResults = new MetaResultsWithWarmup();

            //string basePath = @"C:\Users\Dematic\Desktop\SimLog\";
            string basePath = @"C:\Users\p2decarv\Desktop\SimLog\";

            var availability = new List<DateTime>()
            {
                new DateTime(2015,11,10)
            };
            var statsInterval = new Tuple<TimeSpan, TimeSpan>(new TimeSpan(6, 0, 0), new TimeSpan(23, 59, 59));

            var arrivalTimesOverTime = Data.GetInterarrivalTimesOverTime(availability[0]);
            var recircTimesOverTime = Data.GetItemsInRecircOverTime(availability[0]);
            var queueSizeOverTime = Data.GetQueueSizeOverTime(availability[0]);
            var qTimes = Data.GetTimeInQueue(availability[0], statsInterval);

            using (var writer = new System.IO.StreamWriter(basePath + "InterArrivals_over_time_real.txt"))
            {
                writer.WriteLine("Time \t Interarrival Time");

                foreach (var t in arrivalTimesOverTime)
                {
                    writer.WriteLine(t.Item1 + "\t" + t.Item2);
                }
            }

            using (var writer = new System.IO.StreamWriter(basePath + "ItemsInRecirc_over_time_real.txt"))
            {
                writer.WriteLine("Time of Day (s) \t Items in Reicirculation");

                for (int i = 0; i < recircTimesOverTime.Length; i++)
                {
                    writer.WriteLine(i + "\t" + recircTimesOverTime[i]);
                }
            }

            using (var writer = new System.IO.StreamWriter(basePath + "QueueSize_over_time_real.txt"))
            {
                writer.WriteLine("Time of Day (s) \t Items in Queue");

                for (int i = 0; i < queueSizeOverTime.Length; i++)
                {
                    writer.WriteLine(i + "\t" + queueSizeOverTime[i]);
                }
            }

            using (var writer = new System.IO.StreamWriter(basePath + "QTimes.txt"))
            {
                writer.WriteLine("Time in Queue");

                foreach (double time in qTimes)
                {
                    writer.WriteLine(time);
                }
            }




            //var logger = new VerboseLogger(@"C:\Users\p2decarv\Desktop\SimLog");
            //var logger = new VerboseLogger(@"C:\Users\Daniel\Desktop\SimLog");
            var logger = new VerboseLogger(@"C:\Users\Dematic\Desktop\SimLog");



            //logger.LogDBStats("DBStats2", availability.First(), 600, statsInterval);
            //logger.LogDBStats("DBStats", availability.First());
            //logger.LogDBStats("DBStats", availability.First(), 600);


            List<Tuple<int, int>> throughput = new List<Tuple<int, int>>();
            var iterations = Enumerable.Range(1, 20);

            var intervals = new List<Tuple<TimeSpan, TimeSpan>>()
                {
                    //complete distribution
                    //new Tuple<TimeSpan, TimeSpan>(new TimeSpan(6,0,0), new TimeSpan(22,0,0)),

                    //different intervals
                    //new Tuple<TimeSpan, TimeSpan>(new TimeSpan(6,0,0), new TimeSpan(6,30,0)),
                    //new Tuple<TimeSpan, TimeSpan>(new TimeSpan(6,30,0), new TimeSpan(7,0,0)),
                    //new Tuple<TimeSpan, TimeSpan>(new TimeSpan(7,0,0), new TimeSpan(9,0,0)),
                    //new Tuple<TimeSpan, TimeSpan>(new TimeSpan(9,0,0), new TimeSpan(13,30,0)),
                    //new Tuple<TimeSpan, TimeSpan>(new TimeSpan(13,30,0), new TimeSpan(15,30,0)),
                    //new Tuple<TimeSpan, TimeSpan>(new TimeSpan(15,30,0), new TimeSpan(22,0,0)),

                    //hourly intervals
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(6,0,0), new TimeSpan(7,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(7,0,0), new TimeSpan(8,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(8,0,0), new TimeSpan(9,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(9,0,0), new TimeSpan(10,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(10,0,0), new TimeSpan(11,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(11,0,0), new TimeSpan(12,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(12,0,0), new TimeSpan(13,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(13,0,0), new TimeSpan(14,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(14,0,0), new TimeSpan(15,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(15,0,0), new TimeSpan(16,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(16,0,0), new TimeSpan(17,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(17,0,0), new TimeSpan(18,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(18,0,0), new TimeSpan(19,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(19,0,0), new TimeSpan(20,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(20,0,0), new TimeSpan(21,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(21,0,0), new TimeSpan(22,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(22,0,0), new TimeSpan(23,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(23,0,0), new TimeSpan(24,0,0)),
                };

            var breakTimes = Data.FindBreaks(availability[0], 600);

            var factoryParams = new FactoryParams()
            {
                StartMin = 360,
                DayLength = 64800,
                QueueSize = 150,
                Logger = logger,
                NWarmupDays = 1
            };

            var distParams = new DistributionSelectionParameters()
            {
                SelectedDaysForData = availability,
                ArrivalDistributionBreakpoints = intervals,
                BreakTimes = breakTimes,
                IntervalForOtherDistributions = statsInterval,
                CapacityIntervalSize = 30
            };

            DateTime warmupDay = new DateTime(2015, 11, 9);

            var warmupParams = new DistributionSelectionParameters()
            {
                SelectedDaysForData = new List<DateTime>() { warmupDay },
                ArrivalDistributionBreakpoints = intervals,
                BreakTimes = breakTimes,
                IntervalForOtherDistributions = statsInterval,
                CapacityIntervalSize = 30
            };

            var warmupDays = new List<Tuple<DateTime, DistributionSelectionParameters>>()
                {
                    new Tuple<DateTime, DistributionSelectionParameters>(warmupDay, warmupParams)
                };

            Parallel.ForEach(iterations, i =>
            {
                

                var sim = SimulationFactory.SimWithSpecificWarmupDay(factoryParams, distParams, warmupDays);

                sim.Run();
                

                logger.LogResults(sim.Results, "Results_" + i, 3 * 64800);
                

                FinalResults.AddSimResults(sim.Results);

                var t = sim.Results.CalcNumOut();

                throughput.Add(new Tuple<int, int>(t[ProcessType.Putwall], t[ProcessType.NonPutwall]));

                OutputTimeSeries(i, basePath, sim.Results);

                OutputTimedQueueSize(i, basePath, sim.Results);

                WriteOutTimeInQ(i, basePath, sim.Results);

                WriteOutLeftoverCapacity(i, basePath, sim.Results.LeftOverCapacity);
            });


            //using (var writer = new System.IO.StreamWriter(@"C:\Users\p2decarv\Desktop\SimLog\FINALRESULTS.txt"))
            //using (var writer = new System.IO.StreamWriter(@"C:\Users\Daniel\Desktop\SimLog\FINALRESULTS.txt"))
            using (var writer = new System.IO.StreamWriter(basePath + "FINALRESULTS.txt"))
            {
                foreach (Tuple<int, int> t2 in throughput)
                {
                    writer.WriteLine(t2.Item1.ToString() + '\t' + t2.Item2.ToString());
                }

                writer.WriteLine();
                writer.WriteLine("Putwall Stats");
                writer.WriteLine("Property \t Average \t StdDev");
                writer.WriteLine("Time in system \t" + FinalResults.PutwallStatistics.TimeInSystem.Average + "\t" +
                    FinalResults.PutwallStatistics.TimeInSystem.StdDev);
                writer.WriteLine("Time in Process \t" + FinalResults.PutwallStatistics.TimeInProcess.Average + "\t" +
                    FinalResults.PutwallStatistics.TimeInProcess.StdDev);
                writer.WriteLine("Time in Queue \t" + FinalResults.PutwallStatistics.TimeInQueue.Average + "\t" +
                    FinalResults.PutwallStatistics.TimeInQueue.StdDev);
                writer.WriteLine("Time Recirculating \t" + FinalResults.PutwallStatistics.TimeRecirculating.Average + "\t" +
                    FinalResults.PutwallStatistics.TimeRecirculating.StdDev);
                writer.WriteLine("Times Recirculated \t" + FinalResults.PutwallStatistics.TimesRecirculated.Average + "\t" +
                    FinalResults.PutwallStatistics.TimesRecirculated.StdDev);
                writer.WriteLine("Number Created \t" + FinalResults.PutwallStatistics.NumberCreated.Average + "\t" +
                   FinalResults.PutwallStatistics.NumberCreated.StdDev);
                writer.WriteLine("Number Disposed \t" + FinalResults.PutwallStatistics.NumberDisposed.Average + "\t" +
                  FinalResults.PutwallStatistics.NumberDisposed.StdDev);
                writer.WriteLine("Of Day Number Disposed \t" + FinalResults.PutwallStatistics.NumOutOfArrivals.Average + "\t" +
                  FinalResults.PutwallStatistics.NumOutOfArrivals.StdDev);

                writer.WriteLine();
                writer.WriteLine("NonPutwall Stats");
                writer.WriteLine("Property \t Average \t StdDev");
                writer.WriteLine("Time in system \t" + FinalResults.NonPutwallStatistics.TimeInSystem.Average + "\t" +
                    FinalResults.NonPutwallStatistics.TimeInSystem.StdDev);
                writer.WriteLine("Time in Process \t" + FinalResults.NonPutwallStatistics.TimeInProcess.Average + "\t" +
                    FinalResults.NonPutwallStatistics.TimeInProcess.StdDev);
                writer.WriteLine("Time in Queue \t" + FinalResults.NonPutwallStatistics.TimeInQueue.Average + "\t" +
                    FinalResults.NonPutwallStatistics.TimeInQueue.StdDev);
                writer.WriteLine("Time Recirculating \t" + FinalResults.NonPutwallStatistics.TimeRecirculating.Average + "\t" +
                    FinalResults.NonPutwallStatistics.TimeRecirculating.StdDev);
                writer.WriteLine("Times Recirculated \t" + FinalResults.NonPutwallStatistics.TimesRecirculated.Average + "\t" +
                    FinalResults.NonPutwallStatistics.TimesRecirculated.StdDev);
                writer.WriteLine("Number Created \t" + FinalResults.NonPutwallStatistics.NumberCreated.Average + "\t" +
                   FinalResults.NonPutwallStatistics.NumberCreated.StdDev);
                writer.WriteLine("Number Disposed \t" + FinalResults.NonPutwallStatistics.NumberDisposed.Average + "\t" +
                  FinalResults.NonPutwallStatistics.NumberDisposed.StdDev);
                writer.WriteLine("Of Day Number Disposed \t" + FinalResults.NonPutwallStatistics.NumOutOfArrivals.Average + "\t" +
                  FinalResults.NonPutwallStatistics.NumOutOfArrivals.StdDev);

            }
        }
        private void SimWithFullQ()
        {
            var Data = new WarehouseData();

            var FinalResults = new MetaResults();

            string basePath = @"C:\Users\Dematic\Desktop\SimLog\";
            //string basePath = @"C:\Users\p2decarv\Desktop\SimLog\";

            var availability = new List<DateTime>()
            {
                new DateTime(2015,11,10)
            };
            var statsInterval = new Tuple<TimeSpan, TimeSpan>(new TimeSpan(6, 0, 0), new TimeSpan(23, 59, 59));

            var arrivalTimesOverTime = Data.GetInterarrivalTimesOverTime(availability[0]);
            var recircTimesOverTime = Data.GetItemsInRecircOverTime(availability[0]);
            var queueSizeOverTime = Data.GetQueueSizeOverTime(availability[0]);
            var qTimes = Data.GetTimeInQueue(availability[0], statsInterval);

            using (var writer = new System.IO.StreamWriter(basePath + "InterArrivals_over_time_real.txt"))
            {
                writer.WriteLine("Time \t Interarrival Time");

                foreach (var t in arrivalTimesOverTime)
                {
                    writer.WriteLine(t.Item1 + "\t" + t.Item2);
                }
            }

            using (var writer = new System.IO.StreamWriter(basePath + "ItemsInRecirc_over_time_real.txt"))
            {
                writer.WriteLine("Time of Day (s) \t Items in Reicirculation");

                for (int i = 0; i < recircTimesOverTime.Length; i++)
                {
                    writer.WriteLine(i + "\t" + recircTimesOverTime[i]);
                }
            }

            using (var writer = new System.IO.StreamWriter(basePath + "QueueSize_over_time_real.txt"))
            {
                writer.WriteLine("Time of Day (s) \t Items in Queue");

                for (int i = 0; i < queueSizeOverTime.Length; i++)
                {
                    writer.WriteLine(i + "\t" + queueSizeOverTime[i]);
                }
            }

            using (var writer = new System.IO.StreamWriter(basePath + "QTimes.txt"))
            {
                writer.WriteLine("Time in Queue");

                foreach (double time in qTimes)
                {
                    writer.WriteLine(time);
                }
            }




            //var logger = new VerboseLogger(@"C:\Users\p2decarv\Desktop\SimLog");
            //var logger = new VerboseLogger(@"C:\Users\Daniel\Desktop\SimLog");
            var logger = new VerboseLogger(@"C:\Users\Dematic\Desktop\SimLog");



            logger.LogDBStats("DBStats2", availability.First(), 600, statsInterval);
            logger.LogDBStats("DBStats", availability.First());
            logger.LogDBStats("DBStats", availability.First(), 600);


            List<Tuple<int, int>> throughput = new List<Tuple<int, int>>();

            
            for (int i = 1; i <= 20; i++)
            {
                var intervals = new List<Tuple<TimeSpan, TimeSpan>>()
                {
                    //complete distribution
                    //new Tuple<TimeSpan, TimeSpan>(new TimeSpan(6,0,0), new TimeSpan(22,0,0)),

                    //different intervals
                    //new Tuple<TimeSpan, TimeSpan>(new TimeSpan(6,0,0), new TimeSpan(6,30,0)),
                    //new Tuple<TimeSpan, TimeSpan>(new TimeSpan(6,30,0), new TimeSpan(7,0,0)),
                    //new Tuple<TimeSpan, TimeSpan>(new TimeSpan(7,0,0), new TimeSpan(9,0,0)),
                    //new Tuple<TimeSpan, TimeSpan>(new TimeSpan(9,0,0), new TimeSpan(13,30,0)),
                    //new Tuple<TimeSpan, TimeSpan>(new TimeSpan(13,30,0), new TimeSpan(15,30,0)),
                    //new Tuple<TimeSpan, TimeSpan>(new TimeSpan(15,30,0), new TimeSpan(22,0,0)),

                    //hourly intervals
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(6,0,0), new TimeSpan(7,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(7,0,0), new TimeSpan(8,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(8,0,0), new TimeSpan(9,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(9,0,0), new TimeSpan(10,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(10,0,0), new TimeSpan(11,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(11,0,0), new TimeSpan(12,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(12,0,0), new TimeSpan(13,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(13,0,0), new TimeSpan(14,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(14,0,0), new TimeSpan(15,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(15,0,0), new TimeSpan(16,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(16,0,0), new TimeSpan(17,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(17,0,0), new TimeSpan(18,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(18,0,0), new TimeSpan(19,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(19,0,0), new TimeSpan(20,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(20,0,0), new TimeSpan(21,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(21,0,0), new TimeSpan(22,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(22,0,0), new TimeSpan(23,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(23,0,0), new TimeSpan(24,0,0)),
                };

                var breakTimes = Data.FindBreaks(availability[0], 600);

                var sim = SimulationFactory.SimWithFullQ(availability, 60, 360, 64800, 150, logger, intervals, 600, breakTimes, statsInterval, 100);

                sim.Run();

                logger.PauseLogging = false;

                logger.LogResults(sim.Results, "Results_" + i, 3 * 64800);

                logger.PauseLogging = true;

                FinalResults.AddSimResults(sim.Results);

                var t = sim.Results.CalcNumOut();

                throughput.Add(new Tuple<int, int>(t[ProcessType.Putwall], t[ProcessType.NonPutwall]));

                OutputTimeSeries(i, basePath, sim.Results);

                OutputTimedQueueSize(i, basePath, sim.Results);

                WriteOutTimeInQ(i, basePath, sim.Results);

                WriteOutLeftoverCapacity(i, basePath, sim.Results.LeftOverCapacity);
            }


            //using (var writer = new System.IO.StreamWriter(@"C:\Users\p2decarv\Desktop\SimLog\FINALRESULTS.txt"))
            //using (var writer = new System.IO.StreamWriter(@"C:\Users\Daniel\Desktop\SimLog\FINALRESULTS.txt"))
            using (var writer = new System.IO.StreamWriter(basePath + "FINALRESULTS.txt"))
            {
                foreach (Tuple<int, int> t2 in throughput)
                {
                    writer.WriteLine(t2.Item1.ToString() + '\t' + t2.Item2.ToString());
                }

                writer.WriteLine();
                writer.WriteLine("Putwall Stats");
                writer.WriteLine("Property \t Average \t StdDev");
                writer.WriteLine("Time in system \t" + FinalResults.PutwallStatistics.TimeInSystem.Average + "\t" +
                    FinalResults.PutwallStatistics.TimeInSystem.StdDev);
                writer.WriteLine("Time in Process \t" + FinalResults.PutwallStatistics.TimeInProcess.Average + "\t" +
                    FinalResults.PutwallStatistics.TimeInProcess.StdDev);
                writer.WriteLine("Time in Queue \t" + FinalResults.PutwallStatistics.TimeInQueue.Average + "\t" +
                    FinalResults.PutwallStatistics.TimeInQueue.StdDev);
                writer.WriteLine("Time Recirculating \t" + FinalResults.PutwallStatistics.TimeRecirculating.Average + "\t" +
                    FinalResults.PutwallStatistics.TimeRecirculating.StdDev);
                writer.WriteLine("Times Recirculated \t" + FinalResults.PutwallStatistics.TimesRecirculated.Average + "\t" +
                    FinalResults.PutwallStatistics.TimesRecirculated.StdDev);
                writer.WriteLine("Number Created \t" + FinalResults.PutwallStatistics.NumberCreated.Average + "\t" +
                   FinalResults.PutwallStatistics.NumberCreated.StdDev);
                writer.WriteLine("Number Disposed \t" + FinalResults.PutwallStatistics.NumberDisposed.Average + "\t" +
                  FinalResults.PutwallStatistics.NumberDisposed.StdDev);

                writer.WriteLine();
                writer.WriteLine("NonPutwall Stats");
                writer.WriteLine("Property \t Average \t StdDev");
                writer.WriteLine("Time in system \t" + FinalResults.NonPutwallStatistics.TimeInSystem.Average + "\t" +
                    FinalResults.NonPutwallStatistics.TimeInSystem.StdDev);
                writer.WriteLine("Time in Process \t" + FinalResults.NonPutwallStatistics.TimeInProcess.Average + "\t" +
                    FinalResults.NonPutwallStatistics.TimeInProcess.StdDev);
                writer.WriteLine("Time in Queue \t" + FinalResults.NonPutwallStatistics.TimeInQueue.Average + "\t" +
                    FinalResults.NonPutwallStatistics.TimeInQueue.StdDev);
                writer.WriteLine("Time Recirculating \t" + FinalResults.NonPutwallStatistics.TimeRecirculating.Average + "\t" +
                    FinalResults.NonPutwallStatistics.TimeRecirculating.StdDev);
                writer.WriteLine("Times Recirculated \t" + FinalResults.NonPutwallStatistics.TimesRecirculated.Average + "\t" +
                    FinalResults.NonPutwallStatistics.TimesRecirculated.StdDev);
                writer.WriteLine("Number Created \t" + FinalResults.NonPutwallStatistics.NumberCreated.Average + "\t" +
                   FinalResults.NonPutwallStatistics.NumberCreated.StdDev);
                writer.WriteLine("Number Disposed \t" + FinalResults.NonPutwallStatistics.NumberDisposed.Average + "\t" +
                  FinalResults.NonPutwallStatistics.NumberDisposed.StdDev);

            }
        }
        private void wipSimWithBreaksAndQTime2()
        {
            var Data = new WarehouseData();

            var FinalResults = new MetaResultsWithWarmup();

            //string basePath = @"C:\Users\Dematic\Desktop\SimLog\";
            string basePath = @"C:\Users\p2decarv\Desktop\SimLog\";

            var availability = new List<DateTime>()
            {
                new DateTime(2015,11,10)
            };
            var statsInterval = new Tuple<TimeSpan, TimeSpan>(new TimeSpan(6, 0, 0), new TimeSpan(23, 59, 59));

            var arrivalTimesOverTime = Data.GetInterarrivalTimesOverTime(availability[0]);
            var recircTimesOverTime = Data.GetItemsInRecircOverTime(availability[0]);
            var queueSizeOverTime = Data.GetQueueSizeOverTime(availability[0]);
            var qTimes = Data.GetTimeInQueue(availability[0], statsInterval);

            using (var writer = new System.IO.StreamWriter(basePath + "InterArrivals_over_time_real.txt"))
            {
                writer.WriteLine("Time \t Interarrival Time");

                foreach (var t in arrivalTimesOverTime)
                {
                    writer.WriteLine(t.Item1 + "\t" + t.Item2);
                }
            }

            using (var writer = new System.IO.StreamWriter(basePath + "ItemsInRecirc_over_time_real.txt"))
            {
                writer.WriteLine("Time of Day (s) \t Items in Reicirculation");

                for (int i = 0; i < recircTimesOverTime.Length; i++)
                {
                    writer.WriteLine(i + "\t" + recircTimesOverTime[i]);
                }
            }

            using (var writer = new System.IO.StreamWriter(basePath + "QueueSize_over_time_real.txt"))
            {
                writer.WriteLine("Time of Day (s) \t Items in Queue");

                for (int i = 0; i < queueSizeOverTime.Length; i++)
                {
                    writer.WriteLine(i + "\t" + queueSizeOverTime[i]);
                }
            }

            using (var writer = new System.IO.StreamWriter(basePath + "QTimes.txt"))
            {
                writer.WriteLine("Time in Queue");

                foreach (double time in qTimes)
                {
                    writer.WriteLine(time);
                }
            }




            var logger = new VerboseLogger(@"C:\Users\p2decarv\Desktop\SimLog");
            //var logger = new VerboseLogger(@"C:\Users\Daniel\Desktop\SimLog");
            //var logger = new VerboseLogger(@"C:\Users\Dematic\Desktop\SimLog");


            
            //logger.LogDBStats("DBStats2", availability.First(), 600, statsInterval);
            //logger.LogDBStats("DBStats", availability.First());
            //logger.LogDBStats("DBStats", availability.First(), 600);
            

            List<Tuple<int, int>> throughput = new List<Tuple<int, int>>();

            for (int i = 1; i <= 20; i++)
            {
                var intervals = new List<Tuple<TimeSpan, TimeSpan>>()
                {
                    //complete distribution
                    //new Tuple<TimeSpan, TimeSpan>(new TimeSpan(6,0,0), new TimeSpan(22,0,0)),

                    //different intervals
                    //new Tuple<TimeSpan, TimeSpan>(new TimeSpan(6,0,0), new TimeSpan(6,30,0)),
                    //new Tuple<TimeSpan, TimeSpan>(new TimeSpan(6,30,0), new TimeSpan(7,0,0)),
                    //new Tuple<TimeSpan, TimeSpan>(new TimeSpan(7,0,0), new TimeSpan(9,0,0)),
                    //new Tuple<TimeSpan, TimeSpan>(new TimeSpan(9,0,0), new TimeSpan(13,30,0)),
                    //new Tuple<TimeSpan, TimeSpan>(new TimeSpan(13,30,0), new TimeSpan(15,30,0)),
                    //new Tuple<TimeSpan, TimeSpan>(new TimeSpan(15,30,0), new TimeSpan(22,0,0)),

                    //hourly intervals
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(6,0,0), new TimeSpan(7,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(7,0,0), new TimeSpan(8,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(8,0,0), new TimeSpan(9,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(9,0,0), new TimeSpan(10,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(10,0,0), new TimeSpan(11,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(11,0,0), new TimeSpan(12,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(12,0,0), new TimeSpan(13,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(13,0,0), new TimeSpan(14,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(14,0,0), new TimeSpan(15,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(15,0,0), new TimeSpan(16,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(16,0,0), new TimeSpan(17,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(17,0,0), new TimeSpan(18,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(18,0,0), new TimeSpan(19,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(19,0,0), new TimeSpan(20,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(20,0,0), new TimeSpan(21,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(21,0,0), new TimeSpan(22,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(22,0,0), new TimeSpan(23,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(23,0,0), new TimeSpan(24,0,0)),
                };

                var breakTimes = Data.FindBreaks(availability[0], 600);

                var sim = SimulationFactory.WIPSimWithBreaksAndQTime(availability, 5, 360, 64800, 150, 0, logger, intervals, 600, breakTimes, statsInterval);

                sim.Run();

                logger.PauseLogging = false;

                logger.LogResults(sim.Results, "Results_" + i, 3 * 64800);

                logger.PauseLogging = true;

                FinalResults.AddSimResults(sim.Results);

                var t = sim.Results.CalcNumOut();

                throughput.Add(new Tuple<int, int>(t[ProcessType.Putwall], t[ProcessType.NonPutwall]));

                OutputTimeSeries(i, basePath, sim.Results);

                OutputTimedQueueSize(i, basePath, sim.Results);

                WriteOutTimeInQ(i, basePath, sim.Results);

                WriteOutLeftoverCapacity(i, basePath, sim.Results.LeftOverCapacity);
            }


            //using (var writer = new System.IO.StreamWriter(@"C:\Users\p2decarv\Desktop\SimLog\FINALRESULTS.txt"))
            //using (var writer = new System.IO.StreamWriter(@"C:\Users\Daniel\Desktop\SimLog\FINALRESULTS.txt"))
            using (var writer = new System.IO.StreamWriter(basePath + "FINALRESULTS.txt"))
            {
                foreach (Tuple<int, int> t2 in throughput)
                {
                    writer.WriteLine(t2.Item1.ToString() + '\t' + t2.Item2.ToString());
                }

                writer.WriteLine();
                writer.WriteLine("Putwall Stats");
                writer.WriteLine("Property \t Average \t StdDev");
                writer.WriteLine("Time in system \t" + FinalResults.PutwallStatistics.TimeInSystem.Average + "\t" +
                    FinalResults.PutwallStatistics.TimeInSystem.StdDev);
                writer.WriteLine("Time in Process \t" + FinalResults.PutwallStatistics.TimeInProcess.Average + "\t" +
                    FinalResults.PutwallStatistics.TimeInProcess.StdDev);
                writer.WriteLine("Time in Queue \t" + FinalResults.PutwallStatistics.TimeInQueue.Average + "\t" +
                    FinalResults.PutwallStatistics.TimeInQueue.StdDev);
                writer.WriteLine("Time Recirculating \t" + FinalResults.PutwallStatistics.TimeRecirculating.Average + "\t" +
                    FinalResults.PutwallStatistics.TimeRecirculating.StdDev);
                writer.WriteLine("Times Recirculated \t" + FinalResults.PutwallStatistics.TimesRecirculated.Average + "\t" +
                    FinalResults.PutwallStatistics.TimesRecirculated.StdDev);
                writer.WriteLine("Number Created \t" + FinalResults.PutwallStatistics.NumberCreated.Average + "\t" +
                   FinalResults.PutwallStatistics.NumberCreated.StdDev);
                writer.WriteLine("Number Disposed \t" + FinalResults.PutwallStatistics.NumberDisposed.Average + "\t" +
                  FinalResults.PutwallStatistics.NumberDisposed.StdDev);
                writer.WriteLine("Of Day Number Disposed \t" + FinalResults.PutwallStatistics.NumOutOfArrivals.Average + "\t" +
                  FinalResults.PutwallStatistics.NumOutOfArrivals.StdDev);

                writer.WriteLine();
                writer.WriteLine("NonPutwall Stats");
                writer.WriteLine("Property \t Average \t StdDev");
                writer.WriteLine("Time in system \t" + FinalResults.NonPutwallStatistics.TimeInSystem.Average + "\t" +
                    FinalResults.NonPutwallStatistics.TimeInSystem.StdDev);
                writer.WriteLine("Time in Process \t" + FinalResults.NonPutwallStatistics.TimeInProcess.Average + "\t" +
                    FinalResults.NonPutwallStatistics.TimeInProcess.StdDev);
                writer.WriteLine("Time in Queue \t" + FinalResults.NonPutwallStatistics.TimeInQueue.Average + "\t" +
                    FinalResults.NonPutwallStatistics.TimeInQueue.StdDev);
                writer.WriteLine("Time Recirculating \t" + FinalResults.NonPutwallStatistics.TimeRecirculating.Average + "\t" +
                    FinalResults.NonPutwallStatistics.TimeRecirculating.StdDev);
                writer.WriteLine("Times Recirculated \t" + FinalResults.NonPutwallStatistics.TimesRecirculated.Average + "\t" +
                    FinalResults.NonPutwallStatistics.TimesRecirculated.StdDev);
                writer.WriteLine("Number Created \t" + FinalResults.NonPutwallStatistics.NumberCreated.Average + "\t" +
                   FinalResults.NonPutwallStatistics.NumberCreated.StdDev);
                writer.WriteLine("Number Disposed \t" + FinalResults.NonPutwallStatistics.NumberDisposed.Average + "\t" +
                  FinalResults.NonPutwallStatistics.NumberDisposed.StdDev);
                writer.WriteLine("Of Day Number Disposed \t" + FinalResults.NonPutwallStatistics.NumOutOfArrivals.Average + "\t" +
                  FinalResults.NonPutwallStatistics.NumOutOfArrivals.StdDev);

            }
        }
        private void WriteOutLeftoverCapacity(int index, string basePath, Dictionary<int, int> capacity)
        {
            using (var writer = new System.IO.StreamWriter(basePath + "LeftoverCapacity" + index + ".txt"))
            {
                writer.WriteLine("Time \t Capacity");

                foreach (KeyValuePair<int, int> p in capacity)
                {
                    writer.WriteLine(p.Key + "\t" + p.Value);
                }
            }
        }
        private void wipSimWithBreaksAndQTime()
        {
            var Data = new WarehouseData();

            var FinalResults = new MetaResults();

            //string basePath = @"C:\Users\Dematic\Desktop\SimLog\";
            string basePath = @"C:\Users\p2decarv\Desktop\SimLog\";

            var availability = new List<DateTime>()
            {
                new DateTime(2015,11,10)
            };

            var arrivalTimesOverTime = Data.GetInterarrivalTimesOverTime(availability[0]);
            var recircTimesOverTime = Data.GetItemsInRecircOverTime(availability[0]);
            var queueSizeOverTime = Data.GetQueueSizeOverTime(availability[0]);
            var qTimes = Data.GetTimeInQueue(availability[0]);

            using (var writer = new System.IO.StreamWriter(basePath + "InterArrivals_over_time_real.txt"))
            {
                writer.WriteLine("Time \t Interarrival Time");

                foreach (var t in arrivalTimesOverTime)
                {
                    writer.WriteLine(t.Item1 + "\t" + t.Item2);
                }
            }

            using (var writer = new System.IO.StreamWriter(basePath + "ItemsInRecirc_over_time_real.txt"))
            {
                writer.WriteLine("Time of Day (s) \t Items in Reicirculation");

                for (int i = 0; i < recircTimesOverTime.Length; i++)
                {
                    writer.WriteLine(i + "\t" + recircTimesOverTime[i]);
                }
            }

            using (var writer = new System.IO.StreamWriter(basePath + "QueueSize_over_time_real.txt"))
            {
                writer.WriteLine("Time of Day (s) \t Items in Queue");

                for (int i = 0; i < queueSizeOverTime.Length; i++)
                {
                    writer.WriteLine(i + "\t" + queueSizeOverTime[i]);
                }
            }

            using (var writer = new System.IO.StreamWriter(basePath + "QTimes.txt"))
            {
                writer.WriteLine("Time in Queue");

                foreach (double time in qTimes)
                {
                    writer.WriteLine(time);
                }
            }




            var logger = new VerboseLogger(@"C:\Users\p2decarv\Desktop\SimLog");
            //var logger = new VerboseLogger(@"C:\Users\Daniel\Desktop\SimLog");
            //var logger = new VerboseLogger(@"C:\Users\Dematic\Desktop\SimLog");



            logger.LogDBStats("DBStats", availability.First());
            logger.LogDBStats("DBStats", availability.First(), 600);

            List<Tuple<int, int>> throughput = new List<Tuple<int, int>>();

            for (int i = 1; i <= 20; i++)
            {
                var intervals = new List<Tuple<TimeSpan, TimeSpan>>()
                {
                    //complete distribution
                    //new Tuple<TimeSpan, TimeSpan>(new TimeSpan(6,0,0), new TimeSpan(22,0,0)),

                    //different intervals
                    //new Tuple<TimeSpan, TimeSpan>(new TimeSpan(6,0,0), new TimeSpan(6,30,0)),
                    //new Tuple<TimeSpan, TimeSpan>(new TimeSpan(6,30,0), new TimeSpan(7,0,0)),
                    //new Tuple<TimeSpan, TimeSpan>(new TimeSpan(7,0,0), new TimeSpan(9,0,0)),
                    //new Tuple<TimeSpan, TimeSpan>(new TimeSpan(9,0,0), new TimeSpan(13,30,0)),
                    //new Tuple<TimeSpan, TimeSpan>(new TimeSpan(13,30,0), new TimeSpan(15,30,0)),
                    //new Tuple<TimeSpan, TimeSpan>(new TimeSpan(15,30,0), new TimeSpan(22,0,0)),

                    //hourly intervals
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(6,0,0), new TimeSpan(7,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(7,0,0), new TimeSpan(8,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(8,0,0), new TimeSpan(9,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(9,0,0), new TimeSpan(10,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(10,0,0), new TimeSpan(11,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(11,0,0), new TimeSpan(12,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(12,0,0), new TimeSpan(13,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(13,0,0), new TimeSpan(14,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(14,0,0), new TimeSpan(15,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(15,0,0), new TimeSpan(16,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(16,0,0), new TimeSpan(17,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(17,0,0), new TimeSpan(18,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(18,0,0), new TimeSpan(19,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(19,0,0), new TimeSpan(20,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(20,0,0), new TimeSpan(21,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(21,0,0), new TimeSpan(22,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(22,0,0), new TimeSpan(23,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(23,0,0), new TimeSpan(24,0,0)),
                };

                var breakTimes = Data.FindBreaks(availability[0], 600);

                var sim = SimulationFactory.WIPSimWithBreaksAndQTime(availability, 30, 360, 64800, 100, 1, logger, intervals, 600, breakTimes);

                sim.Run();

                logger.PauseLogging = false;

                logger.LogResults(sim.Results, "Results_" + i, 3 * 64800);

                logger.PauseLogging = true;

                FinalResults.AddSimResults(sim.Results);

                var t = sim.Results.CalcNumOut();

                throughput.Add(new Tuple<int, int>(t[ProcessType.Putwall], t[ProcessType.NonPutwall]));

                OutputTimeSeries(i, basePath, sim.Results);

                OutputTimedQueueSize(i, basePath, sim.Results);

                WriteOutTimeInQ(i, basePath, sim.Results);
            }


            //using (var writer = new System.IO.StreamWriter(@"C:\Users\p2decarv\Desktop\SimLog\FINALRESULTS.txt"))
            //using (var writer = new System.IO.StreamWriter(@"C:\Users\Daniel\Desktop\SimLog\FINALRESULTS.txt"))
            using (var writer = new System.IO.StreamWriter(basePath + "FINALRESULTS.txt"))
            {
                foreach (Tuple<int, int> t2 in throughput)
                {
                    writer.WriteLine(t2.Item1.ToString() + '\t' + t2.Item2.ToString());
                }

                writer.WriteLine();
                writer.WriteLine("Putwall Stats");
                writer.WriteLine("Property \t Average \t StdDev");
                writer.WriteLine("Time in system \t" + FinalResults.PutwallStatistics.TimeInSystem.Average + "\t" +
                    FinalResults.PutwallStatistics.TimeInSystem.StdDev);
                writer.WriteLine("Time in Process \t" + FinalResults.PutwallStatistics.TimeInProcess.Average + "\t" +
                    FinalResults.PutwallStatistics.TimeInProcess.StdDev);
                writer.WriteLine("Time in Queue \t" + FinalResults.PutwallStatistics.TimeInQueue.Average + "\t" +
                    FinalResults.PutwallStatistics.TimeInQueue.StdDev);
                writer.WriteLine("Time Recirculating \t" + FinalResults.PutwallStatistics.TimeRecirculating.Average + "\t" +
                    FinalResults.PutwallStatistics.TimeRecirculating.StdDev);
                writer.WriteLine("Times Recirculated \t" + FinalResults.PutwallStatistics.TimesRecirculated.Average + "\t" +
                    FinalResults.PutwallStatistics.TimesRecirculated.StdDev);
                writer.WriteLine("Number Created \t" + FinalResults.PutwallStatistics.NumberCreated.Average + "\t" +
                   FinalResults.PutwallStatistics.NumberCreated.StdDev);
                writer.WriteLine("Number Disposed \t" + FinalResults.PutwallStatistics.NumberDisposed.Average + "\t" +
                  FinalResults.PutwallStatistics.NumberDisposed.StdDev);

                writer.WriteLine();
                writer.WriteLine("NonPutwall Stats");
                writer.WriteLine("Property \t Average \t StdDev");
                writer.WriteLine("Time in system \t" + FinalResults.NonPutwallStatistics.TimeInSystem.Average + "\t" +
                    FinalResults.NonPutwallStatistics.TimeInSystem.StdDev);
                writer.WriteLine("Time in Process \t" + FinalResults.NonPutwallStatistics.TimeInProcess.Average + "\t" +
                    FinalResults.NonPutwallStatistics.TimeInProcess.StdDev);
                writer.WriteLine("Time in Queue \t" + FinalResults.NonPutwallStatistics.TimeInQueue.Average + "\t" +
                    FinalResults.NonPutwallStatistics.TimeInQueue.StdDev);
                writer.WriteLine("Time Recirculating \t" + FinalResults.NonPutwallStatistics.TimeRecirculating.Average + "\t" +
                    FinalResults.NonPutwallStatistics.TimeRecirculating.StdDev);
                writer.WriteLine("Times Recirculated \t" + FinalResults.NonPutwallStatistics.TimesRecirculated.Average + "\t" +
                    FinalResults.NonPutwallStatistics.TimesRecirculated.StdDev);
                writer.WriteLine("Number Created \t" + FinalResults.NonPutwallStatistics.NumberCreated.Average + "\t" +
                   FinalResults.NonPutwallStatistics.NumberCreated.StdDev);
                writer.WriteLine("Number Disposed \t" + FinalResults.NonPutwallStatistics.NumberDisposed.Average + "\t" +
                  FinalResults.NonPutwallStatistics.NumberDisposed.StdDev);

            }
        }
        private void wipSimWithBreaksAndOperators()
        {
            var Data = new WarehouseData();

            var FinalResults = new MetaResults();

            //string basePath = @"C:\Users\Dematic\Desktop\SimLog\";
            string basePath = @"C:\Users\p2decarv\Desktop\SimLog\";

            var availability = new List<DateTime>()
            {
                new DateTime(2015,11,10)
            };

            var arrivalTimesOverTime = Data.GetInterarrivalTimesOverTime(availability[0]);
            var recircTimesOverTime = Data.GetItemsInRecircOverTime(availability[0]);
            var queueSizeOverTime = Data.GetQueueSizeOverTime(availability[0]);
            var qTimes = Data.GetTimeInQueue(availability[0]);

            using (var writer = new System.IO.StreamWriter(basePath + "InterArrivals_over_time_real.txt"))
            {
                writer.WriteLine("Time \t Interarrival Time");

                foreach (var t in arrivalTimesOverTime)
                {
                    writer.WriteLine(t.Item1 + "\t" + t.Item2);
                }
            }

            using (var writer = new System.IO.StreamWriter(basePath + "ItemsInRecirc_over_time_real.txt"))
            {
                writer.WriteLine("Time of Day (s) \t Items in Reicirculation");

                for (int i = 0; i < recircTimesOverTime.Length; i++)
                {
                    writer.WriteLine(i + "\t" + recircTimesOverTime[i]);
                }
            }

            using (var writer = new System.IO.StreamWriter(basePath + "QueueSize_over_time_real.txt"))
            {
                writer.WriteLine("Time of Day (s) \t Items in Queue");

                for (int i = 0; i < queueSizeOverTime.Length; i++)
                {
                    writer.WriteLine(i + "\t" + queueSizeOverTime[i]);
                }
            }

            using (var writer = new System.IO.StreamWriter(basePath + "QTimes.txt"))
            {
                writer.WriteLine("Time in Queue");

                foreach (double time in qTimes)
                {
                    writer.WriteLine(time);
                }
            }




            var logger = new VerboseLogger(@"C:\Users\p2decarv\Desktop\SimLog");
            //var logger = new VerboseLogger(@"C:\Users\Daniel\Desktop\SimLog");
            //var logger = new VerboseLogger(@"C:\Users\Dematic\Desktop\SimLog");



            logger.LogDBStats("DBStats", availability.First());
            logger.LogDBStats("DBStats", availability.First(), 600);

            List<Tuple<int, int>> throughput = new List<Tuple<int, int>>();

            for (int i = 1; i <= 20; i++)
            {
                var intervals = new List<Tuple<TimeSpan, TimeSpan>>()
                {
                    //complete distribution
                    //new Tuple<TimeSpan, TimeSpan>(new TimeSpan(6,0,0), new TimeSpan(22,0,0)),

                    //different intervals
                    //new Tuple<TimeSpan, TimeSpan>(new TimeSpan(6,0,0), new TimeSpan(6,30,0)),
                    //new Tuple<TimeSpan, TimeSpan>(new TimeSpan(6,30,0), new TimeSpan(7,0,0)),
                    //new Tuple<TimeSpan, TimeSpan>(new TimeSpan(7,0,0), new TimeSpan(9,0,0)),
                    //new Tuple<TimeSpan, TimeSpan>(new TimeSpan(9,0,0), new TimeSpan(13,30,0)),
                    //new Tuple<TimeSpan, TimeSpan>(new TimeSpan(13,30,0), new TimeSpan(15,30,0)),
                    //new Tuple<TimeSpan, TimeSpan>(new TimeSpan(15,30,0), new TimeSpan(22,0,0)),

                    //hourly intervals
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(6,0,0), new TimeSpan(7,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(7,0,0), new TimeSpan(8,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(8,0,0), new TimeSpan(9,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(9,0,0), new TimeSpan(10,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(10,0,0), new TimeSpan(11,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(11,0,0), new TimeSpan(12,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(12,0,0), new TimeSpan(13,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(13,0,0), new TimeSpan(14,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(14,0,0), new TimeSpan(15,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(15,0,0), new TimeSpan(16,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(16,0,0), new TimeSpan(17,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(17,0,0), new TimeSpan(18,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(18,0,0), new TimeSpan(19,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(19,0,0), new TimeSpan(20,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(20,0,0), new TimeSpan(21,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(21,0,0), new TimeSpan(22,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(22,0,0), new TimeSpan(23,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(23,0,0), new TimeSpan(24,0,0)),
                };

                var breakTimes = Data.FindBreaks(availability[0], 600);

                var sim = SimulationFactory.WIPSimWithBreaksAndOperators(availability, 30, 360, 64800, 150, 1, logger, intervals, 600, breakTimes, 2);

                sim.Run();

                logger.PauseLogging = false;

                logger.LogResults(sim.Results, "Results_" + i, 3 * 64800);

                logger.PauseLogging = true;

                FinalResults.AddSimResults(sim.Results);

                var t = sim.Results.CalcNumOut();

                throughput.Add(new Tuple<int, int>(t[ProcessType.Putwall], t[ProcessType.NonPutwall]));

                OutputTimeSeries(i, basePath, sim.Results);

                WriteOutTimeInQ(i, basePath, sim.Results);
            }


            //using (var writer = new System.IO.StreamWriter(@"C:\Users\p2decarv\Desktop\SimLog\FINALRESULTS.txt"))
            //using (var writer = new System.IO.StreamWriter(@"C:\Users\Daniel\Desktop\SimLog\FINALRESULTS.txt"))
            using (var writer = new System.IO.StreamWriter(basePath + "FINALRESULTS.txt"))
            {
                foreach (Tuple<int, int> t2 in throughput)
                {
                    writer.WriteLine(t2.Item1.ToString() + '\t' + t2.Item2.ToString());
                }

                writer.WriteLine();
                writer.WriteLine("Putwall Stats");
                writer.WriteLine("Property \t Average \t StdDev");
                writer.WriteLine("Time in system \t" + FinalResults.PutwallStatistics.TimeInSystem.Average + "\t" +
                    FinalResults.PutwallStatistics.TimeInSystem.StdDev);
                writer.WriteLine("Time in Process \t" + FinalResults.PutwallStatistics.TimeInProcess.Average + "\t" +
                    FinalResults.PutwallStatistics.TimeInProcess.StdDev);
                writer.WriteLine("Time in Queue \t" + FinalResults.PutwallStatistics.TimeInQueue.Average + "\t" +
                    FinalResults.PutwallStatistics.TimeInQueue.StdDev);
                writer.WriteLine("Time Recirculating \t" + FinalResults.PutwallStatistics.TimeRecirculating.Average + "\t" +
                    FinalResults.PutwallStatistics.TimeRecirculating.StdDev);
                writer.WriteLine("Times Recirculated \t" + FinalResults.PutwallStatistics.TimesRecirculated.Average + "\t" +
                    FinalResults.PutwallStatistics.TimesRecirculated.StdDev);
                writer.WriteLine("Number Created \t" + FinalResults.PutwallStatistics.NumberCreated.Average + "\t" +
                   FinalResults.PutwallStatistics.NumberCreated.StdDev);
                writer.WriteLine("Number Disposed \t" + FinalResults.PutwallStatistics.NumberDisposed.Average + "\t" +
                  FinalResults.PutwallStatistics.NumberDisposed.StdDev);

                writer.WriteLine();
                writer.WriteLine("NonPutwall Stats");
                writer.WriteLine("Property \t Average \t StdDev");
                writer.WriteLine("Time in system \t" + FinalResults.NonPutwallStatistics.TimeInSystem.Average + "\t" +
                    FinalResults.NonPutwallStatistics.TimeInSystem.StdDev);
                writer.WriteLine("Time in Process \t" + FinalResults.NonPutwallStatistics.TimeInProcess.Average + "\t" +
                    FinalResults.NonPutwallStatistics.TimeInProcess.StdDev);
                writer.WriteLine("Time in Queue \t" + FinalResults.NonPutwallStatistics.TimeInQueue.Average + "\t" +
                    FinalResults.NonPutwallStatistics.TimeInQueue.StdDev);
                writer.WriteLine("Time Recirculating \t" + FinalResults.NonPutwallStatistics.TimeRecirculating.Average + "\t" +
                    FinalResults.NonPutwallStatistics.TimeRecirculating.StdDev);
                writer.WriteLine("Times Recirculated \t" + FinalResults.NonPutwallStatistics.TimesRecirculated.Average + "\t" +
                    FinalResults.NonPutwallStatistics.TimesRecirculated.StdDev);
                writer.WriteLine("Number Created \t" + FinalResults.NonPutwallStatistics.NumberCreated.Average + "\t" +
                   FinalResults.NonPutwallStatistics.NumberCreated.StdDev);
                writer.WriteLine("Number Disposed \t" + FinalResults.NonPutwallStatistics.NumberDisposed.Average + "\t" +
                  FinalResults.NonPutwallStatistics.NumberDisposed.StdDev);

            }
        }
        private void wipSimWithBreaks()
        {
            var Data = new WarehouseData();

            var FinalResults = new MetaResults();

            //string basePath = @"C:\Users\Dematic\Desktop\SimLog\";
            string basePath = @"C:\Users\p2decarv\Desktop\SimLog\";

            var availability = new List<DateTime>()
            {
                new DateTime(2015,11,10)
            };

            var arrivalTimesOverTime = Data.GetInterarrivalTimesOverTime(availability[0]);
            var recircTimesOverTime = Data.GetItemsInRecircOverTime(availability[0]);
            var queueSizeOverTime = Data.GetQueueSizeOverTime(availability[0]);
            var qTimes = Data.GetTimeInQueue(availability[0]);

            using (var writer = new System.IO.StreamWriter(basePath + "InterArrivals_over_time_real.txt"))
            {
                writer.WriteLine("Time \t Interarrival Time");

                foreach (var t in arrivalTimesOverTime)
                {
                    writer.WriteLine(t.Item1 + "\t" + t.Item2);
                }
            }

            using (var writer = new System.IO.StreamWriter(basePath + "ItemsInRecirc_over_time_real.txt"))
            {
                writer.WriteLine("Time of Day (s) \t Items in Reicirculation");

                for (int i = 0; i < recircTimesOverTime.Length; i++)
                {
                    writer.WriteLine(i + "\t" + recircTimesOverTime[i]);
                }
            }

            using (var writer = new System.IO.StreamWriter(basePath + "QueueSize_over_time_real.txt"))
            {
                writer.WriteLine("Time of Day (s) \t Items in Queue");

                for (int i = 0; i < queueSizeOverTime.Length; i++)
                {
                    writer.WriteLine(i + "\t" + queueSizeOverTime[i]);
                }
            }

            using (var writer = new System.IO.StreamWriter(basePath + "QTimes.txt"))
            {
                writer.WriteLine("Time in Queue");

                foreach(double time in qTimes)
                {
                    writer.WriteLine(time);
                }
            }




            var logger = new VerboseLogger(@"C:\Users\p2decarv\Desktop\SimLog");
            //var logger = new VerboseLogger(@"C:\Users\Daniel\Desktop\SimLog");
            //var logger = new VerboseLogger(@"C:\Users\Dematic\Desktop\SimLog");



            logger.LogDBStats("DBStats", availability.First());
            logger.LogDBStats("DBStats", availability.First(), 600);

            List<Tuple<int, int>> throughput = new List<Tuple<int, int>>();

            for (int i = 1; i <= 20; i++)
            {
                var intervals = new List<Tuple<TimeSpan, TimeSpan>>()
                {
                    //complete distribution
                    //new Tuple<TimeSpan, TimeSpan>(new TimeSpan(6,0,0), new TimeSpan(22,0,0)),

                    //different intervals
                    //new Tuple<TimeSpan, TimeSpan>(new TimeSpan(6,0,0), new TimeSpan(6,30,0)),
                    //new Tuple<TimeSpan, TimeSpan>(new TimeSpan(6,30,0), new TimeSpan(7,0,0)),
                    //new Tuple<TimeSpan, TimeSpan>(new TimeSpan(7,0,0), new TimeSpan(9,0,0)),
                    //new Tuple<TimeSpan, TimeSpan>(new TimeSpan(9,0,0), new TimeSpan(13,30,0)),
                    //new Tuple<TimeSpan, TimeSpan>(new TimeSpan(13,30,0), new TimeSpan(15,30,0)),
                    //new Tuple<TimeSpan, TimeSpan>(new TimeSpan(15,30,0), new TimeSpan(22,0,0)),

                    //hourly intervals
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(6,0,0), new TimeSpan(7,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(7,0,0), new TimeSpan(8,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(8,0,0), new TimeSpan(9,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(9,0,0), new TimeSpan(10,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(10,0,0), new TimeSpan(11,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(11,0,0), new TimeSpan(12,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(12,0,0), new TimeSpan(13,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(13,0,0), new TimeSpan(14,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(14,0,0), new TimeSpan(15,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(15,0,0), new TimeSpan(16,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(16,0,0), new TimeSpan(17,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(17,0,0), new TimeSpan(18,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(18,0,0), new TimeSpan(19,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(19,0,0), new TimeSpan(20,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(20,0,0), new TimeSpan(21,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(21,0,0), new TimeSpan(22,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(22,0,0), new TimeSpan(23,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(23,0,0), new TimeSpan(24,0,0)),
                };

                var breakTimes = Data.FindBreaks(availability[0], 600);
                
                var sim = SimulationFactory.WIPSimWithBreaks(availability, 5, 360, 64800, 60, 1, logger, intervals, 600, breakTimes);

                sim.Run();

                logger.PauseLogging = false;

                logger.LogResults(sim.Results, "Results_" + i, 3 * 64800);

                logger.PauseLogging = true;

                FinalResults.AddSimResults(sim.Results);

                var t = sim.Results.CalcNumOut();

                throughput.Add(new Tuple<int, int>(t[ProcessType.Putwall], t[ProcessType.NonPutwall]));

                OutputTimeSeries(i, basePath, sim.Results);

                WriteOutTimeInQ(i, basePath, sim.Results);
            }


            //using (var writer = new System.IO.StreamWriter(@"C:\Users\p2decarv\Desktop\SimLog\FINALRESULTS.txt"))
            //using (var writer = new System.IO.StreamWriter(@"C:\Users\Daniel\Desktop\SimLog\FINALRESULTS.txt"))
            using (var writer = new System.IO.StreamWriter(basePath + "FINALRESULTS.txt"))
            {
                foreach (Tuple<int, int> t2 in throughput)
                {
                    writer.WriteLine(t2.Item1.ToString() + '\t' + t2.Item2.ToString());
                }

                writer.WriteLine();
                writer.WriteLine("Putwall Stats");
                writer.WriteLine("Property \t Average \t StdDev");
                writer.WriteLine("Time in system \t" + FinalResults.PutwallStatistics.TimeInSystem.Average + "\t" +
                    FinalResults.PutwallStatistics.TimeInSystem.StdDev);
                writer.WriteLine("Time in Process \t" + FinalResults.PutwallStatistics.TimeInProcess.Average + "\t" +
                    FinalResults.PutwallStatistics.TimeInProcess.StdDev);
                writer.WriteLine("Time in Queue \t" + FinalResults.PutwallStatistics.TimeInQueue.Average + "\t" +
                    FinalResults.PutwallStatistics.TimeInQueue.StdDev);
                writer.WriteLine("Time Recirculating \t" + FinalResults.PutwallStatistics.TimeRecirculating.Average + "\t" +
                    FinalResults.PutwallStatistics.TimeRecirculating.StdDev);
                writer.WriteLine("Times Recirculated \t" + FinalResults.PutwallStatistics.TimesRecirculated.Average + "\t" +
                    FinalResults.PutwallStatistics.TimesRecirculated.StdDev);
                writer.WriteLine("Number Created \t" + FinalResults.PutwallStatistics.NumberCreated.Average + "\t" +
                   FinalResults.PutwallStatistics.NumberCreated.StdDev);
                writer.WriteLine("Number Disposed \t" + FinalResults.PutwallStatistics.NumberDisposed.Average + "\t" +
                  FinalResults.PutwallStatistics.NumberDisposed.StdDev);

                writer.WriteLine();
                writer.WriteLine("NonPutwall Stats");
                writer.WriteLine("Property \t Average \t StdDev");
                writer.WriteLine("Time in system \t" + FinalResults.NonPutwallStatistics.TimeInSystem.Average + "\t" +
                    FinalResults.NonPutwallStatistics.TimeInSystem.StdDev);
                writer.WriteLine("Time in Process \t" + FinalResults.NonPutwallStatistics.TimeInProcess.Average + "\t" +
                    FinalResults.NonPutwallStatistics.TimeInProcess.StdDev);
                writer.WriteLine("Time in Queue \t" + FinalResults.NonPutwallStatistics.TimeInQueue.Average + "\t" +
                    FinalResults.NonPutwallStatistics.TimeInQueue.StdDev);
                writer.WriteLine("Time Recirculating \t" + FinalResults.NonPutwallStatistics.TimeRecirculating.Average + "\t" +
                    FinalResults.NonPutwallStatistics.TimeRecirculating.StdDev);
                writer.WriteLine("Times Recirculated \t" + FinalResults.NonPutwallStatistics.TimesRecirculated.Average + "\t" +
                    FinalResults.NonPutwallStatistics.TimesRecirculated.StdDev);
                writer.WriteLine("Number Created \t" + FinalResults.NonPutwallStatistics.NumberCreated.Average + "\t" +
                   FinalResults.NonPutwallStatistics.NumberCreated.StdDev);
                writer.WriteLine("Number Disposed \t" + FinalResults.NonPutwallStatistics.NumberDisposed.Average + "\t" +
                  FinalResults.NonPutwallStatistics.NumberDisposed.StdDev);

            }
        }
        private void wipSim()
        {
            var Data = new WarehouseData();

            var FinalResults = new MetaResults();

            string basePath = @"C:\Users\Dematic\Desktop\SimLog\";

            var availability = new List<DateTime>()
            {
                new DateTime(2015,11,10)
            };

            var arrivalTimesOverTime = Data.GetInterarrivalTimesOverTime(availability[0]);
            var recircTimesOverTime = Data.GetItemsInRecircOverTime(availability[0]);
            var queueSizeOverTime = Data.GetQueueSizeOverTime(availability[0]);

            using(var writer  = new System.IO.StreamWriter(basePath + "InterArrivals_over_time_real.txt"))
            {
                writer.WriteLine("Time \t Interarrival Time");

                foreach(var t in arrivalTimesOverTime)
                {
                    writer.WriteLine(t.Item1 + "\t" + t.Item2);
                }
            }

            using(var writer = new System.IO.StreamWriter(basePath + "ItemsInRecirc_over_time_real.txt"))
            {
                writer.WriteLine("Time of Day (s) \t Items in Reicirculation");

                for(int i = 0; i < recircTimesOverTime.Length; i++)
                {
                    writer.WriteLine(i + "\t" + recircTimesOverTime[i]);
                }
            }

            using (var writer = new System.IO.StreamWriter(basePath + "QueueSize_over_time_real.txt"))
            {
                writer.WriteLine("Time of Day (s) \t Items in Queue");

                for (int i = 0; i < queueSizeOverTime.Length; i++)
                {
                    writer.WriteLine(i + "\t" + queueSizeOverTime[i]);
                }
            }


            //var logger = new VerboseLogger(@"C:\Users\p2decarv\Desktop\SimLog");
            //var logger = new VerboseLogger(@"C:\Users\Daniel\Desktop\SimLog");
            var logger = new VerboseLogger(@"C:\Users\Dematic\Desktop\SimLog");

            

            logger.LogDBStats("DBStats", availability.First());
            logger.LogDBStats("DBStats", availability.First(), 600);

            List<Tuple<int, int>> throughput = new List<Tuple<int, int>>();

            for (int i = 1; i <= 20; i++)
            {
                var intervals = new List<Tuple<TimeSpan, TimeSpan>>()
                {
                    //complete distribution
                    //new Tuple<TimeSpan, TimeSpan>(new TimeSpan(6,0,0), new TimeSpan(22,0,0)),

                    //different intervals
                    //new Tuple<TimeSpan, TimeSpan>(new TimeSpan(6,0,0), new TimeSpan(6,30,0)),
                    //new Tuple<TimeSpan, TimeSpan>(new TimeSpan(6,30,0), new TimeSpan(7,0,0)),
                    //new Tuple<TimeSpan, TimeSpan>(new TimeSpan(7,0,0), new TimeSpan(9,0,0)),
                    //new Tuple<TimeSpan, TimeSpan>(new TimeSpan(9,0,0), new TimeSpan(13,30,0)),
                    //new Tuple<TimeSpan, TimeSpan>(new TimeSpan(13,30,0), new TimeSpan(15,30,0)),
                    //new Tuple<TimeSpan, TimeSpan>(new TimeSpan(15,30,0), new TimeSpan(22,0,0)),

                    //hourly intervals
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(6,0,0), new TimeSpan(7,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(7,0,0), new TimeSpan(8,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(8,0,0), new TimeSpan(9,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(9,0,0), new TimeSpan(10,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(10,0,0), new TimeSpan(11,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(11,0,0), new TimeSpan(12,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(12,0,0), new TimeSpan(13,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(13,0,0), new TimeSpan(14,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(14,0,0), new TimeSpan(15,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(15,0,0), new TimeSpan(16,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(16,0,0), new TimeSpan(17,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(17,0,0), new TimeSpan(18,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(18,0,0), new TimeSpan(19,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(19,0,0), new TimeSpan(20,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(20,0,0), new TimeSpan(21,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(21,0,0), new TimeSpan(22,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(22,0,0), new TimeSpan(23,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(23,0,0), new TimeSpan(24,0,0)),
                };

                var sim = SimulationFactory.WIPSim(availability, 5, 360, 64800, 150, 0, logger, intervals, 600);

                sim.Run();

                logger.PauseLogging = false;

                logger.LogResults(sim.Results, "Results_" + i, 3 * 64800);

                logger.PauseLogging = true;

                FinalResults.AddSimResults(sim.Results);

                var t = sim.Results.CalcNumOut();

                throughput.Add(new Tuple<int, int>(t[ProcessType.Putwall], t[ProcessType.NonPutwall]));

                OutputTimeSeries(i, basePath, sim.Results);
            }

            
            //using (var writer = new System.IO.StreamWriter(@"C:\Users\p2decarv\Desktop\SimLog\FINALRESULTS.txt"))
            //using (var writer = new System.IO.StreamWriter(@"C:\Users\Daniel\Desktop\SimLog\FINALRESULTS.txt"))
            using (var writer = new System.IO.StreamWriter(basePath + "FINALRESULTS.txt"))
            {
                foreach (Tuple<int, int> t2 in throughput)
                {
                    writer.WriteLine(t2.Item1.ToString() + '\t' + t2.Item2.ToString());
                }

                writer.WriteLine();
                writer.WriteLine("Putwall Stats");
                writer.WriteLine("Property \t Average \t StdDev");
                writer.WriteLine("Time in system \t" + FinalResults.PutwallStatistics.TimeInSystem.Average + "\t" +
                    FinalResults.PutwallStatistics.TimeInSystem.StdDev);
                writer.WriteLine("Time in Process \t" + FinalResults.PutwallStatistics.TimeInProcess.Average + "\t" +
                    FinalResults.PutwallStatistics.TimeInProcess.StdDev);
                writer.WriteLine("Time in Queue \t" + FinalResults.PutwallStatistics.TimeInQueue.Average + "\t" +
                    FinalResults.PutwallStatistics.TimeInQueue.StdDev);
                writer.WriteLine("Time Recirculating \t" + FinalResults.PutwallStatistics.TimeRecirculating.Average + "\t" +
                    FinalResults.PutwallStatistics.TimeRecirculating.StdDev);
                writer.WriteLine("Times Recirculated \t" + FinalResults.PutwallStatistics.TimesRecirculated.Average + "\t" +
                    FinalResults.PutwallStatistics.TimesRecirculated.StdDev);
                writer.WriteLine("Number Created \t" + FinalResults.PutwallStatistics.NumberCreated.Average + "\t" +
                   FinalResults.PutwallStatistics.NumberCreated.StdDev);
                writer.WriteLine("Number Disposed \t" + FinalResults.PutwallStatistics.NumberDisposed.Average + "\t" +
                  FinalResults.PutwallStatistics.NumberDisposed.StdDev);

                writer.WriteLine();
                writer.WriteLine("NonPutwall Stats");
                writer.WriteLine("Property \t Average \t StdDev");
                writer.WriteLine("Time in system \t" + FinalResults.NonPutwallStatistics.TimeInSystem.Average + "\t" +
                    FinalResults.NonPutwallStatistics.TimeInSystem.StdDev);
                writer.WriteLine("Time in Process \t" + FinalResults.NonPutwallStatistics.TimeInProcess.Average + "\t" +
                    FinalResults.NonPutwallStatistics.TimeInProcess.StdDev);
                writer.WriteLine("Time in Queue \t" + FinalResults.NonPutwallStatistics.TimeInQueue.Average + "\t" +
                    FinalResults.NonPutwallStatistics.TimeInQueue.StdDev);
                writer.WriteLine("Time Recirculating \t" + FinalResults.NonPutwallStatistics.TimeRecirculating.Average + "\t" +
                    FinalResults.NonPutwallStatistics.TimeRecirculating.StdDev);
                writer.WriteLine("Times Recirculated \t" + FinalResults.NonPutwallStatistics.TimesRecirculated.Average + "\t" +
                    FinalResults.NonPutwallStatistics.TimesRecirculated.StdDev);
                writer.WriteLine("Number Created \t" + FinalResults.NonPutwallStatistics.NumberCreated.Average + "\t" +
                   FinalResults.NonPutwallStatistics.NumberCreated.StdDev);
                writer.WriteLine("Number Disposed \t" + FinalResults.NonPutwallStatistics.NumberDisposed.Average + "\t" +
                  FinalResults.NonPutwallStatistics.NumberDisposed.StdDev);

            }
            
        }
        private void WriteOutTimeInQ(int index, string basePath, SimulationResults results)
        {
            using (var writer = new System.IO.StreamWriter(basePath + "Time_in_Q_sim_" + index + ".txt"))
            {
                var TimeInQ = results.OutputTimeInQueue();

                writer.WriteLine("Time in Queue");

                foreach (int t in TimeInQ)
                {
                    writer.WriteLine(t);
                }
            }
        }
        private void OutputTimedQueueSize(int index, string basePath, SimulationResults results)
        {
            using (var writer = new System.IO.StreamWriter(basePath + "TimedQueueSize_over_time_sim_" + index + ".txt"))
            {
                var QSizeOverTime = results.OutputTimedQueueSize();

                writer.WriteLine("Time \t Size");

                foreach (KeyValuePair<int, int> p in QSizeOverTime)
                {
                    writer.WriteLine(p.Key + "\t" + p.Value);
                }
            }
        }
        private void OutputTimeSeries(int index, string basePath, SimulationResults results)
        {
            using (var writer = new System.IO.StreamWriter(basePath + "Interarrivaltimes_over_time_sim_" + index + ".txt"))
            {
                var InterarrivalTimes = results.OutputInterarrivalTimes();

                writer.WriteLine("Time \t Duration");

                foreach(Tuple<int, int> t in InterarrivalTimes)
                {
                    writer.WriteLine(t.Item1 + "\t" + t.Item2);
                }
            }
            using (var writer = new System.IO.StreamWriter(basePath + "QueueSize_over_time_sim_" + index + ".txt"))
            {
                var QSizeOverTime = results.OutputQueueSize();

                writer.WriteLine("Time \t Size");

                foreach(KeyValuePair<int, int> p in QSizeOverTime)
                {
                    writer.WriteLine(p.Key + "\t" + p.Value);
                }
            }
            using (var writer = new System.IO.StreamWriter(basePath + "ItemsInRecirc_over_time_sim_" + index + ".txt"))
            {
                var itemsInRecirc = results.OutputItemsInRecirc();

                writer.WriteLine("Time \t Items in Recirc");

                for(int i = 0; i < itemsInRecirc.Length; i++)
                {
                    writer.WriteLine(i + "\t" + itemsInRecirc[i]);
                }

            }
        }
        private void newPPXSim()
        {
            var Data = new WarehouseData();

            var FinalResults = new MetaResults();

            var availability = new List<DateTime>()
            {
                new DateTime(2015,11,10)
            };


            //var logger = new VerboseLogger(@"C:\Users\p2decarv\Desktop\SimLog");
            //var logger = new VerboseLogger(@"C:\Users\Daniel\Desktop\SimLog");
            var logger = new VerboseLogger(@"C:\Users\Dematic\Desktop\SimLog");

            logger.LogDBStats("DBStats", availability.First());
            logger.LogDBStats("DBStats", availability.First(), 600);

            List<Tuple<int, int>> throughput = new List<Tuple<int, int>>();

            for (int i = 1; i <= 20; i++)
            {
                var intervals = new List<Tuple<TimeSpan, TimeSpan>>()
                {
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(6,0,0), new TimeSpan(6,30,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(6,30,0), new TimeSpan(7,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(7,0,0), new TimeSpan(9,0,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(9,0,0), new TimeSpan(13,30,0)),
                    new Tuple<TimeSpan, TimeSpan>(new TimeSpan(13,30,0), new TimeSpan(24,0,0)),
                };

                var sim = SimulationFactory.SimWithPPXScheduleAndInterval2(availability, 5, 360, 57600, 150, logger, intervals, 600);

                sim.Run();

                logger.PauseLogging = false;

                logger.LogResults(sim.Results, "Results_" + i, 57600);

                logger.PauseLogging = true;

                FinalResults.AddSimResults(sim.Results);

                var t = sim.Results.CalcNumOut();

                throughput.Add(new Tuple<int, int>(t[ProcessType.Putwall], t[ProcessType.NonPutwall]));
            }

            //using (var writer = new System.IO.StreamWriter(@"C:\Users\p2decarv\Desktop\SimLog\FINALRESULTS.txt"))
            //using (var writer = new System.IO.StreamWriter(@"C:\Users\Daniel\Desktop\SimLog\FINALRESULTS.txt"))
            using (var writer = new System.IO.StreamWriter(@"C:\Users\Dematic\Desktop\SimLog\FINALRESULTS.txt"))
            {
                foreach (Tuple<int, int> t2 in throughput)
                {
                    writer.WriteLine(t2.Item1.ToString() + '\t' + t2.Item2.ToString());
                }

                writer.WriteLine();
                writer.WriteLine("Putwall Stats");
                writer.WriteLine("Property \t Average \t StdDev");
                writer.WriteLine("Time in system \t" + FinalResults.PutwallStatistics.TimeInSystem.Average + "\t" +
                    FinalResults.PutwallStatistics.TimeInSystem.StdDev);
                writer.WriteLine("Time in Process \t" + FinalResults.PutwallStatistics.TimeInProcess.Average + "\t" +
                    FinalResults.PutwallStatistics.TimeInProcess.StdDev);
                writer.WriteLine("Time in Queue \t" + FinalResults.PutwallStatistics.TimeInQueue.Average + "\t" +
                    FinalResults.PutwallStatistics.TimeInQueue.StdDev);
                writer.WriteLine("Time Recirculating \t" + FinalResults.PutwallStatistics.TimeRecirculating.Average + "\t" +
                    FinalResults.PutwallStatistics.TimeRecirculating.StdDev);
                writer.WriteLine("Times Recirculated \t" + FinalResults.PutwallStatistics.TimesRecirculated.Average + "\t" +
                    FinalResults.PutwallStatistics.TimesRecirculated.StdDev);
                writer.WriteLine("Number Created \t" + FinalResults.PutwallStatistics.NumberCreated.Average + "\t" +
                   FinalResults.PutwallStatistics.NumberCreated.StdDev);
                writer.WriteLine("Number Disposed \t" + FinalResults.PutwallStatistics.NumberDisposed.Average + "\t" +
                  FinalResults.PutwallStatistics.NumberDisposed.StdDev);

                writer.WriteLine();
                writer.WriteLine("NonPutwall Stats");
                writer.WriteLine("Property \t Average \t StdDev");
                writer.WriteLine("Time in system \t" + FinalResults.NonPutwallStatistics.TimeInSystem.Average + "\t" +
                    FinalResults.NonPutwallStatistics.TimeInSystem.StdDev);
                writer.WriteLine("Time in Process \t" + FinalResults.NonPutwallStatistics.TimeInProcess.Average + "\t" +
                    FinalResults.NonPutwallStatistics.TimeInProcess.StdDev);
                writer.WriteLine("Time in Queue \t" + FinalResults.NonPutwallStatistics.TimeInQueue.Average + "\t" +
                    FinalResults.NonPutwallStatistics.TimeInQueue.StdDev);
                writer.WriteLine("Time Recirculating \t" + FinalResults.NonPutwallStatistics.TimeRecirculating.Average + "\t" +
                    FinalResults.NonPutwallStatistics.TimeRecirculating.StdDev);
                writer.WriteLine("Times Recirculated \t" + FinalResults.NonPutwallStatistics.TimesRecirculated.Average + "\t" +
                    FinalResults.NonPutwallStatistics.TimesRecirculated.StdDev);
                writer.WriteLine("Number Created \t" + FinalResults.NonPutwallStatistics.NumberCreated.Average + "\t" +
                   FinalResults.NonPutwallStatistics.NumberCreated.StdDev);
                writer.WriteLine("Number Disposed \t" + FinalResults.NonPutwallStatistics.NumberDisposed.Average + "\t" +
                  FinalResults.NonPutwallStatistics.NumberDisposed.StdDev);

            }
        }
        private void RunPPXSim()
        {
            var Data = new WarehouseData();

            var FinalResults = new MetaResults();

            var availability = new List<DateTime>()
            {
                new DateTime(2015,11,10)
            };


            //var logger = new VerboseLogger(@"C:\Users\p2decarv\Desktop\SimLog");
            //var logger = new VerboseLogger(@"C:\Users\Daniel\Desktop\SimLog");
            var logger = new VerboseLogger(@"C:\Users\Dematic\Desktop\SimLog");

            logger.LogDBStats("DBStats", availability.First());
            logger.LogDBStats("DBStats", availability.First(), 600);

            List<Tuple<int, int>> throughput = new List<Tuple<int, int>>();

            for (int i = 1; i <= 20; i++)
            {
                var sim = SimulationFactory.SimWithPPXScheduleAndInterval(availability, 10, 360, 57600, 150, logger);

                sim.Run();

                logger.PauseLogging = false;

                logger.LogResults(sim.Results, "Results_" + i, 57600);

                logger.PauseLogging = true;

                FinalResults.AddSimResults(sim.Results);

                var t = sim.Results.CalcNumOut();

                throughput.Add(new Tuple<int, int>(t[ProcessType.Putwall], t[ProcessType.NonPutwall]));
            }

            //using (var writer = new System.IO.StreamWriter(@"C:\Users\p2decarv\Desktop\SimLog\FINALRESULTS.txt"))
            //using (var writer = new System.IO.StreamWriter(@"C:\Users\Daniel\Desktop\SimLog\FINALRESULTS.txt"))
            using (var writer = new System.IO.StreamWriter(@"C:\Users\Dematic\Desktop\SimLog\FINALRESULTS.txt"))
            {
                foreach (Tuple<int, int> t2 in throughput)
                {
                    writer.WriteLine(t2.Item1.ToString() + '\t' + t2.Item2.ToString());
                }

                writer.WriteLine();
                writer.WriteLine("Putwall Stats");
                writer.WriteLine("Property \t Average \t StdDev");
                writer.WriteLine("Time in system \t" + FinalResults.PutwallStatistics.TimeInSystem.Average + "\t" + 
                    FinalResults.PutwallStatistics.TimeInSystem.StdDev);
                writer.WriteLine("Time in Process \t" + FinalResults.PutwallStatistics.TimeInProcess.Average + "\t" +
                    FinalResults.PutwallStatistics.TimeInProcess.StdDev);
                writer.WriteLine("Time in Queue \t" + FinalResults.PutwallStatistics.TimeInQueue.Average + "\t" +
                    FinalResults.PutwallStatistics.TimeInQueue.StdDev);
                writer.WriteLine("Time Recirculating \t" + FinalResults.PutwallStatistics.TimeRecirculating.Average + "\t" +
                    FinalResults.PutwallStatistics.TimeRecirculating.StdDev);
                writer.WriteLine("Times Recirculated \t" + FinalResults.PutwallStatistics.TimesRecirculated.Average + "\t" +
                    FinalResults.PutwallStatistics.TimesRecirculated.StdDev);
                writer.WriteLine("Number Created \t" + FinalResults.PutwallStatistics.NumberCreated.Average + "\t" +
                   FinalResults.PutwallStatistics.NumberCreated.StdDev);
                writer.WriteLine("Number Disposed \t" + FinalResults.PutwallStatistics.NumberDisposed.Average + "\t" +
                  FinalResults.PutwallStatistics.NumberDisposed.StdDev);

                writer.WriteLine();
                writer.WriteLine("NonPutwall Stats");
                writer.WriteLine("Property \t Average \t StdDev");
                writer.WriteLine("Time in system \t" + FinalResults.NonPutwallStatistics.TimeInSystem.Average + "\t" +
                    FinalResults.NonPutwallStatistics.TimeInSystem.StdDev);
                writer.WriteLine("Time in Process \t" + FinalResults.NonPutwallStatistics.TimeInProcess.Average + "\t" +
                    FinalResults.NonPutwallStatistics.TimeInProcess.StdDev);
                writer.WriteLine("Time in Queue \t" + FinalResults.NonPutwallStatistics.TimeInQueue.Average + "\t" +
                    FinalResults.NonPutwallStatistics.TimeInQueue.StdDev);
                writer.WriteLine("Time Recirculating \t" + FinalResults.NonPutwallStatistics.TimeRecirculating.Average + "\t" +
                    FinalResults.NonPutwallStatistics.TimeRecirculating.StdDev);
                writer.WriteLine("Times Recirculated \t" + FinalResults.NonPutwallStatistics.TimesRecirculated.Average + "\t" +
                    FinalResults.NonPutwallStatistics.TimesRecirculated.StdDev);
                writer.WriteLine("Number Created \t" + FinalResults.NonPutwallStatistics.NumberCreated.Average + "\t" +
                   FinalResults.NonPutwallStatistics.NumberCreated.StdDev);
                writer.WriteLine("Number Disposed \t" + FinalResults.NonPutwallStatistics.NumberDisposed.Average + "\t" +
                  FinalResults.NonPutwallStatistics.NumberDisposed.StdDev);

            }
        }
        private void RunPPHSim()
        {
            var Data = new WarehouseData();

            var availability = new List<DateTime>()
            {
                new DateTime(2015,11,10)
            };

            
            var logger = new VerboseLogger(@"C:\Users\p2decarv\Desktop\SimLog");
            //var logger = new VerboseLogger(@"C:\Users\Daniel\Desktop\SimLog");

            //logger.LogDBStats("DBStats", availability.First());
            //logger.LogDBStats("DBStats2", new DateTime(2015, 11, 11));

            List<Tuple<int, int>> throughput = new List<Tuple<int, int>>();

            for (int i = 1; i <= 20; i++)
            {
                var sim = SimulationFactory.SimWithPPHSchedule(availability, 6, 57600, 100, logger);

                sim.Run();

                logger.LogResults(sim.Results, "Results_" + i, 57600);

                var t = sim.Results.CalcNumOut();

                throughput.Add(new Tuple<int, int>(t[ProcessType.Putwall], t[ProcessType.NonPutwall]));
            }

            

            using (var writer = new System.IO.StreamWriter(@"C:\Users\p2decarv\Desktop\SimLog\FINALRESULTS.txt"))
            //using (var writer = new System.IO.StreamWriter(@"C:\Users\Daniel\Desktop\SimLog\FINALRESULTS.txt"))
            {
                foreach (Tuple<int, int> t2 in throughput)
                {
                    writer.WriteLine(t2.Item1.ToString() + '\t' + t2.Item2.ToString());
                }
            }
        }
        private void RunNov10Sim()
        {
            var Data = new WarehouseData();

            var availability = new List<DateTime>()
            {
                new DateTime(2015,11,10)
            };

            var operatorSchedule = new Dictionary<int, int>()
            {
                {0,5},
            {3600,6},
            {7200,6},
            {10800,6},
            {14400,7},
            {18000,8},
            {21600,8},
            {25200,8},
            {28800,9},
            {32400,7},
            {36000,4},
            {39600,5},
            {43200,5},
            {46800,5},
            {50400,5},
            {54000,6},
            {57600,6},
            {61200,7}

            };
            var logger = new VerboseLogger(@"C:\Users\p2decarv\Desktop\SimLog");
            //        var logger = new VerboseLogger(@"C:\Users\Daniel\Desktop\SimLog");

            List<Tuple<int, int>> throughput = new List<Tuple<int, int>>();

            for (int i = 1; i <= 20; i++)
            {
                var sim = SimulationFactory.SimWithOperatorSchedule(availability, 57600, operatorSchedule, logger);

                sim.Run();

                logger.LogResults(sim.Results, "Results_" + i, 57600);

                var t = sim.Results.CalcNumOut();

                throughput.Add(new Tuple<int, int>(t[ProcessType.Putwall], t[ProcessType.NonPutwall]));
            }

            using (var writer = new System.IO.StreamWriter(@"C:\Users\p2decarv\Desktop\SimLog\FINALRESULTS.txt"))
            //using (var writer = new System.IO.StreamWriter(@"C:\Users\Daniel\Desktop\SimLog\FINALRESULTS.txt"))
            {
                foreach (Tuple<int, int> t2 in throughput)
                {
                    writer.WriteLine(t2.Item1.ToString() + '\t' + t2.Item2.ToString());
                }
            }
        }
        private void RunDefaultSim()
        {
            var Data = new WarehouseData();

            var availability = Data.GetOverallAvailability();

            var logger = new VerboseLogger(@"C:\Users\Daniel\Desktop\SimLog");

            for (int i = 1; i <= 10; i++)
            {
                var sim = SimulationFactory.DefaultSimulation(availability, 57600, 6, logger);

                sim.Run();

                logger.LogResults(sim.Results, "Results_" + i, 57600);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
