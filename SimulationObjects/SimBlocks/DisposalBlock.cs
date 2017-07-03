using SimulationObjects.Entities;
using SimulationObjects.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulationObjects.SimBlocks
{
    public class DisposalBlock : SimBlock, IDestinationBlock
    {
        public DisposalBlock(Simulation simulation):base(BlockType.Disposal, simulation)
        {

        }
        public IEvent GetNextEvent(IEntity batch)
        {
            
            if (Simulation.CurrentTime <= Simulation.EndTime)
            {
                Simulation.Results.ReportDisposal(batch, Simulation.CurrentTime);
            }
            return null;
        }
    }
}
