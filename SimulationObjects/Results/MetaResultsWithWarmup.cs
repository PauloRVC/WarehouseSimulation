using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infrastructure;

namespace SimulationObjects.Results
{
    public class MetaResultsWithWarmup
    {
        public EntityStaticsWithWarmup PutwallStatistics { get; protected set; } = new EntityStaticsWithWarmup(ProcessType.Putwall);
        public EntityStaticsWithWarmup NonPutwallStatistics { get; protected set; } = new EntityStaticsWithWarmup(ProcessType.NonPutwall);

        public void AddSimResults(ISimResults simResults)
        {
            PutwallStatistics.AddObservations(simResults);
            NonPutwallStatistics.AddObservations(simResults);
        }
    }
}
