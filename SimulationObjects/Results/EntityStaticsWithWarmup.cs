using Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulationObjects.Results
{
    public class EntityStaticsWithWarmup: EntityStatistics
    {
        public Statistic NumOutOfArrivals { get; private set; } = new Statistic(new List<double>());
        public EntityStaticsWithWarmup(ProcessType entityType): base(entityType) { }

        public override void AddObservations(ISimResults simResults)
        {
            base.AddObservations(simResults);

            NumOutOfArrivals.AddObservation(((ResultsWithWarmup)simResults).CalcOfDayOut()[EntityType]);
        }
    }
}
