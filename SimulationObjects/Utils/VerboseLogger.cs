using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infrastructure.Models;
using Infrastructure;
using SimulationObjects.Results;
using SimulationObjects.SimBlocks;
using SimulationObjects.Resources;
using SimulationObjects.Entities;

namespace SimulationObjects.Utils
{
    public class VerboseLogger : ILogger
    {
        private string FolderPath;
        public bool PauseLogging { get; set; } = false;

        public VerboseLogger(string folderPath)
        {
            FolderPath = folderPath;
            if (!System.IO.Directory.Exists(FolderPath))
            {
                System.IO.Directory.CreateDirectory(FolderPath);
            }
        }

        public void LogBatches(string name, List<Tuple<string, DateTime>> results)
        {
            if (PauseLogging)
                return;
            name += ".txt";

            while (System.IO.File.Exists(FolderPath + @"\" + name))
            {
                name = name.Substring(0, name.Length - 4);
                name += "1.txt";
            }

            using (var writer = new System.IO.StreamWriter(FolderPath + @"\" + name))
            {
                foreach (Tuple<string, DateTime> t in results)
                {
                    writer.WriteLine(t.Item1 + '\t' + t.Item2.ToString());
                }
            }
        }

        public void LogDistribution(string name, List<Location> observations)
        {
            if (PauseLogging)
                return;
            name += ".txt";

            while (System.IO.File.Exists(FolderPath + @"\" + name))
            {
                name = name.Substring(0, name.Length - 4);
                name += "1.txt";
            }

            using(var writer = new System.IO.StreamWriter(FolderPath + @"\" + name))
            {
                foreach(Location l in observations)
                {
                    writer.WriteLine(l.ScannerIndicator);
                }
            }
        }

        public void LogDistribution(string name, List<int> observations)
        {
            if (PauseLogging)
                return;
            name += ".txt";

            while (System.IO.File.Exists(FolderPath + @"\" + name))
            {
                name = name.Substring(0, name.Length - 4);
                name += "1.txt";
            }

            using (var writer = new System.IO.StreamWriter(FolderPath + @"\" + name))
            {
                foreach (int i in observations)
                {
                    writer.WriteLine(i);
                }
            }
        }

        public void LogPutsPerHour(string name, Dictionary<int, int> pph)
        {
            if (PauseLogging)
                return;
            name += ".txt";

            while (System.IO.File.Exists(FolderPath + @"\" + name))
            {
                name = name.Substring(0, name.Length - 4);
                name += "1.txt";
            }

            using (var writer = new System.IO.StreamWriter(FolderPath + @"\" + name))
            {
                foreach(var p in pph)
                {
                    writer.WriteLine(p.Key.ToString() + "\t" + p.Value.ToString());
                }
            }
        }

        public void LogResults(ISimResults results, string name, int totalTIme)
        {
            if (PauseLogging)
                return;
            name += ".txt";

            while (System.IO.File.Exists(FolderPath + @"\" + name))
            {
                name = name.Substring(0, name.Length - 4);
                name += "1.txt";
            }

            using (var writer = new System.IO.StreamWriter(FolderPath + @"\" + name))
            {
                var entityTimeInSystemStats = results.CalcEntityTimeInSystemStats();
                foreach(ProcessType p in entityTimeInSystemStats.Keys)
                {
                    writer.WriteLine("Entity Time in System for " + p.ToString() + Environment.NewLine +
                                 "Avg: " + entityTimeInSystemStats[p].Item1 + Environment.NewLine +
                                 "StdDev: " + entityTimeInSystemStats[p].Item2 + Environment.NewLine + Environment.NewLine);
                }
                

                var resourceUtilizationStats = results.CalcTimeConsumed();
                foreach (IResource r in resourceUtilizationStats.Keys)
                {
                    var stats = resourceUtilizationStats[r];
                    writer.WriteLine("Utilization: " + (double)stats/totalTIme + Environment.NewLine + Environment.NewLine);
                }

                var throughput = results.CalcNumOut();
                foreach (ProcessType p in throughput.Keys)
                {
                    writer.WriteLine("Entities Disposed " + p.ToString() + Environment.NewLine + throughput[p] + Environment.NewLine + Environment.NewLine);
                }

                var input = results.CalcNumIn();
                foreach (ProcessType p in input.Keys)
                {
                    writer.WriteLine("Entities Created " + p.ToString() + Environment.NewLine + input[p] + Environment.NewLine + Environment.NewLine);
                }

                var processTimeStats = results.CalcProcessTimeStats();
                foreach (SimBlock b in processTimeStats.Keys)
                {
                    var stats = processTimeStats[b];
                    writer.WriteLine("Process Time" + Environment.NewLine +
                                     "Avg: " + stats.Item1 + Environment.NewLine +
                                     "StdDev: " + stats.Item2 + Environment.NewLine + Environment.NewLine);
                }

                var entityTimeInProcessStats = results.CalcEntityTimeInProcessStats();
                foreach (ProcessType p in entityTimeInProcessStats.Keys)
                {
                    writer.WriteLine("Entity Time in Process for " + p.ToString() + Environment.NewLine +
                                 "Avg: " + entityTimeInProcessStats[p].Item1 + Environment.NewLine +
                                 "StdDev: " + entityTimeInProcessStats[p].Item2 + Environment.NewLine + Environment.NewLine);
                }

                var entityTimeInRecirc = results.CalcRecirculationTimeStats();
                foreach (ProcessType p in entityTimeInRecirc.Keys)
                {
                    writer.WriteLine("Entity Time in Recirculation for " + p.ToString() + Environment.NewLine +
                                 "Avg: " + entityTimeInRecirc[p].Item1 + Environment.NewLine +
                                 "StdDev: " + entityTimeInRecirc[p].Item2 + Environment.NewLine + Environment.NewLine);
                }

                var entityTimesRecirculated = results.CalcTimesRecirculatedStats();
                foreach (ProcessType p in entityTimeInRecirc.Keys)
                {
                    writer.WriteLine("Entity Times Recirculated for " + p.ToString() + Environment.NewLine +
                                 "Avg: " + entityTimesRecirculated[p].Item1 + Environment.NewLine +
                                 "StdDev: " + entityTimesRecirculated[p].Item2 + Environment.NewLine + Environment.NewLine);
                }

                var timeInQ = results.CalcQueueTimes();
                foreach (ProcessType p in timeInQ.Keys)
                {
                    writer.WriteLine("Entity Time in Queue for " + p.ToString() + Environment.NewLine +
                                 "Avg: " + timeInQ[p].Item1 + Environment.NewLine +
                                 "StdDev: " + timeInQ[p].Item2 + Environment.NewLine + Environment.NewLine);
                }

            }

        }
        public void LogDBStats(string name, DateTime day)
        {
            var dbStats = new SystemStats(day);

            WriteDBStatsToFile(name, dbStats);
        }
        public void LogDBStats(string name, DateTime day, double anomolyLimit)
        {
            var dbStats = new SystemStats(day, anomolyLimit);

            WriteDBStatsToFile(name, dbStats);
        }
        public void LogDBStats(string name, DateTime day, double anomolyLimit, Tuple<TimeSpan, TimeSpan> interval)
        {
            var dbStats = new SystemStats(day, anomolyLimit, interval);

            WriteDBStatsToFile(name, dbStats);
        }
        private void WriteDBStatsToFile(string name, SystemStats dbStats)
        {
            name += ".txt";

            while (System.IO.File.Exists(FolderPath + @"\" + name))
            {
                name = name.Substring(0, name.Length - 4);
                name += "1.txt";
            }

            using (var writer = new System.IO.StreamWriter(FolderPath + @"\" + name))
            {

                writer.WriteLine("Statistic \t Putwall \t NonPutwall");

                writer.WriteLine("Count In \t" + dbStats.ArrivalCount[ProcessType.Putwall] + "\t" + dbStats.ArrivalCount[ProcessType.NonPutwall]);

                writer.WriteLine("Count Out \t" + dbStats.ExitCount + "\t N/A");

                writer.WriteLine("Avg Time In Sys \t" + dbStats.AvgTimeInSystem + "\t N/A");

                writer.WriteLine("Avg Time In Q \t" + dbStats.AvgTimeInQueue + "\t N/A");

                writer.WriteLine("Avg Time in Recirc \t" + dbStats.AvgTimeRecirc[ProcessType.Putwall] + "\t" + dbStats.AvgTimeRecirc[ProcessType.NonPutwall]);

                writer.WriteLine("Avg Num Recirc \t" + dbStats.AvgNumTimesRecirc[ProcessType.Putwall] + "\t" + dbStats.AvgNumTimesRecirc[ProcessType.NonPutwall]);

                writer.WriteLine("Avg Time in Process \t" + dbStats.AvgTimeInProcess + "\t N/A");

            }
        }
    }
}
