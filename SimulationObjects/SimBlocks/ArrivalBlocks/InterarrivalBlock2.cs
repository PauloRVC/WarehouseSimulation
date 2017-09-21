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
    public class InterarrivalBlock2 : SimBlock, IArrivalBlock
    {
        private IDistribution<IDestinationBlock> DestinationDist;

        private Dictionary<Tuple<int, int>, IDistribution<int>> InterArrivalDists;

        public InterarrivalBlock2(Dictionary<Tuple<int, int>, IDistribution<int>> interArrivalDists,
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

            var interArrivalDist = InterArrivalDists.Where(x => x.Key.Item1 <= Simulation.CurrentTime && x.Key.Item2 >= Simulation.CurrentTime).First().Value;

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
