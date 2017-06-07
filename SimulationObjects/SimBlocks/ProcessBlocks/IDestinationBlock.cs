using SimulationObjects.Entities;
using SimulationObjects.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulationObjects.SimBlocks
{
    public interface IDestinationBlock: ISimBlock
    {
        IEvent GetNextEvent(IEntity batch);
    }
}
