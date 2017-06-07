using SimulationObjects.Entities;
using SimulationObjects.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulationObjects.Events
{
    public class EndProcessEvent : IEvent
    {
        private Processor Resource;
        public EndProcessEvent(Processor resource, IEntity batch, int time)
        {
            Resource = resource;
            resource.IsBusy = true;

            Entity = batch;
            IsArrival = false;
            Time = time;
        }
        public IEntity Entity
        {
            get; private set;
        }

        public bool IsArrival
        {
            get; private set;
        }

        public int Time
        {
            get; private set;
        }

        public void Conclude()
        {
            Resource.IsBusy = false;
        }
    }
}
