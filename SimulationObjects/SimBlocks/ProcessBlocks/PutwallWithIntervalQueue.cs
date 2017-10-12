using SimulationObjects.Distributions;
using SimulationObjects.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimulationObjects.Entities;
using SimulationObjects.Events;

namespace SimulationObjects.SimBlocks
{
    public class PutwallWithIntervalQueue: PutwallWithIntervalDistributions
    {
        protected Dictionary<int, int> QueueSizeOverTime;
        public PutwallWithIntervalQueue(Dictionary<int, IDistribution<int>> processTimeDists,
                                                Dictionary<int, IDistribution<int>> recircTimeDists,
                                                Dictionary<int, IDistribution<int>> queueTimeDists,
                                                Simulation simulation,
                                                IDestinationBlock nextDestination,
                                                int qSize,
                                                Dictionary<int, int> pPXSchedule,
                                                SimulationResults results,
                                                Dictionary<int, int> queueSizeOverTime) :base(processTimeDists,
                                                                                              recircTimeDists,
                                                                                              queueTimeDists,
                                                                                              simulation,
                                                                                              nextDestination,
                                                                                              qSize,
                                                                                              pPXSchedule,
                                                                                              results)
        {
            QueueSizeOverTime = queueSizeOverTime;
        }
        public override IEvent GetNextEvent(IEntity batch)
        {
            QueueSize = QueueSizeOverTime[QueueSizeOverTime.Keys.Where(x => x <= Simulation.CurrentTime).Max()];

            return base.GetNextEvent(batch);
        }
    }
}
