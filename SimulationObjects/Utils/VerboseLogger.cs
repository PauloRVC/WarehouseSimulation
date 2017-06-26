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

                var throughput = results.CalcThroughput();
                foreach (ProcessType p in throughput.Keys)
                {
                    writer.WriteLine("Throughput " + p.ToString() + Environment.NewLine + throughput[p] + Environment.NewLine + Environment.NewLine);
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
    }
}
