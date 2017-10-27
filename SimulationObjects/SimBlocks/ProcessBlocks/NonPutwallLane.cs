using SimulationObjects.Entities;
using SimulationObjects.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulationObjects.SimBlocks
{
    public class NonPutwallLane : SimBlock, IDestinationBlock
    {
        private IDestinationBlock NextDestination;
        public NonPutwallLane(Simulation simulation, IDestinationBlock nextDestination): base(BlockType.Process, simulation)
        {
            NextDestination = nextDestination;
        }
        public IEvent GetNextEvent(IEntity batch)
        {
            batch.Destination = NextDestination;
            return new GenericEvent(batch, Simulation.CurrentTime, Simulation.CurrentTime);
        }
    }
}
