using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infrastructure.Models;

namespace SimulationObjects
{
    class NullLogger : ILogger
    {
        public void LogDistribution(string name, List<Location> observations)
        {
            //Do Nothing
        }

        public void LogDistribution(string name, List<int> observations)
        {
            //Do Nothing
        }
    }
}
