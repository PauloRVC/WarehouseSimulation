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
            wipSim();

        }
        private void wipSim()
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
                };

                var sim = SimulationFactory.WIPSim(availability, 5, 360, 57600, 150, 1, logger, intervals, 600);

                sim.Run();

                logger.PauseLogging = false;

                logger.LogResults(sim.Results, "Results_" + i, 3*57600);

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
