using Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulationObjects.Results
{
    public class MetaResults
    {
        public EntityStatistics PutwallStatistics { get; private set; } = new EntityStatistics(ProcessType.Putwall);
        public EntityStatistics NonPutwallStatistics { get; private set; } = new EntityStatistics(ProcessType.NonPutwall);

        public void AddSimResults(ISimResults simResults)
        {
            PutwallStatistics.AddObservations(simResults);
            NonPutwallStatistics.AddObservations(simResults);
        }
    }
}
