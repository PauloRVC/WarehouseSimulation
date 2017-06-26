using Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    public interface ILogger
    {
        void LogDistribution(string name, List<int> observations);
        void LogDistribution(string name, List<Location> observations);
        void LogBatches(string name, List<Tuple<string, DateTime>> results);
        void LogPutsPerHour(string name, Dictionary<int, int> pph);
    } 
}
