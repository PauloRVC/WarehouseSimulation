using Infrastructure;
using SimulationObjects.Events;
using SimulationObjects.SimBlocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulationObjects.Entities
{
    
    public interface IEntity
    {
        IDestinationBlock Destination { get; set; }
        ProcessType ProcessType { get; }
        IEvent CurrentEvent { get; set; }
    }
}
