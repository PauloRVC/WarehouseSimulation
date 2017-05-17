using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulationObjects
{
    public class EndProcessEvent : IEvent
    {
        private Processor Resource;
        public EndProcessEvent(Processor resource, Batch batch, int time)
        {
            Resource = resource;
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
            Resource.IsBusy = false;
        }
    }
}
