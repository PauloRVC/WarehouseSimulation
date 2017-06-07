using SimulationObjects.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulationObjects.Events
{
    class GenericEvent : IEvent
    {
        public GenericEvent(IEntity batch, int time)
        {
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
            // do nothing, recirculate
        }
    }
}
