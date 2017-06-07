using SimulationObjects.SimBlocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulationObjects.Entities
{
    public enum ProcessType
    {
        Putwall,
        NonPutwall
    }
    public interface IEntity
    {
        IDestinationBlock Destination { get; set; }
        ProcessType ProcessType { get; }
    }
}
