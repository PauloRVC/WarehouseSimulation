using Infrastructure;
using SimulationObjects.SimBlocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulationObjects.Entities
{
    public class Batch: IEntity
    {
        public IDestinationBlock Destination { get; set; }
        public ProcessType ProcessType { get; private set; }
        public Batch(IDestinationBlock destination)
        {
            Destination = destination;
            if (Destination.GetType() == typeof(Putwall) | Destination.GetType().IsSubclassOf(typeof(Putwall)) |
                Destination.GetType() == typeof(PutwallWithIntervalDistributions) | Destination.GetType().IsSubclassOf(typeof(PutwallWithIntervalDistributions) ))
            {
                ProcessType = ProcessType.Putwall;
            }
            else
            {
                ProcessType = ProcessType.NonPutwall;
            }
        }
        public Batch(IDestinationBlock destination, ProcessType processType)
        {
            Destination = destination;
            ProcessType = processType;
        }
    }
}
