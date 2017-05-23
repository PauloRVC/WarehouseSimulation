using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulationObjects
{
    class GenericEvent : IEvent
    {
        public GenericEvent(Batch batch, int time)
        {
            Batch = batch;
            IsArrival = false;
            Time = time;
        }
        public Batch Batch
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
