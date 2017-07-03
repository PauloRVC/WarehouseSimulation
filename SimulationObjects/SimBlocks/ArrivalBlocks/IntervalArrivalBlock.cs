using SimulationObjects.Distributions;
using SimulationObjects.Events;
using SimulationObjects.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulationObjects.SimBlocks
{
    public class IntervalArrivalBlock : SimBlock, IArrivalBlock
    {
        private IDistribution<IDestinationBlock> DestinationDist;

        private Dictionary<int, IDistribution<int>> InterArrivalDists;
        public IntervalArrivalBlock(Dictionary<int, IDistribution<int>> interArrivalDists,
                                    IDistribution<IDestinationBlock> destinationDist,
                                    Simulation simulation) : base(BlockType.Arrival, simulation)
        {
            InterArrivalDists = interArrivalDists;
            DestinationDist = destinationDist;
        }
        public IEvent GetNextEvent()
        {
            var Destination = DestinationDist.DrawNext();
            var Batch = new Batch(Destination);

            var interArrivalDist = InterArrivalDists[InterArrivalDists.Keys.Where(x => x <= Simulation.CurrentTime).Max()];

            int dur = interArrivalDist.DrawNext();

            var Time = Simulation.CurrentTime + dur;

            if (Time <= Simulation.EndTime)
            {
                Simulation.Results.ReportArrival(Batch, Time);
            }

            return new Arrival(Batch, Time);
        }
    }
}
