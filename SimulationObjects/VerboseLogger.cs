using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infrastructure.Models;
using Infrastructure;

namespace SimulationObjects
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
    }
}
