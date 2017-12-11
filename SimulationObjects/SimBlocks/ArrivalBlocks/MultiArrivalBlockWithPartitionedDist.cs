using SimulationObjects.Distributions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimulationObjects.Events;

namespace SimulationObjects.SimBlocks.ArrivalBlocks
{
    public class MultiArrivalBlockWithPartitionedDist: MultiArrivalBlock
    {
        protected Dictionary<int, IDistribution<InterarrivalBatchSize>> MultiArrivalDists;
        public MultiArrivalBlockWithPartitionedDist(
                            Dictionary<int,IDistribution<InterarrivalBatchSize>> multiArrivalDists,
                            IDistribution<IDestinationBlock> destinationDist,
                            IDestinationBlock p06,
                            List<Tuple<int, int>> breaks,
                            Simulation simulation): base(multiArrivalDists[simulation.CurrentTime],
                                                          destinationDist,
                                                          p06,
                                                          breaks,
                                                          simulation)
        {
            MultiArrivalDists = multiArrivalDists;
        }
        public override IEvent GetNextEvent()
        {
            int scheduleIndex = MultiArrivalDists.Keys.Where(x => x <= Simulation.CurrentTime).Max();

            MultiArrivalDist = MultiArrivalDists[scheduleIndex];

            return base.GetNextEvent();
        }
    }
}
