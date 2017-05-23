using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulationObjects
{
    public class Arrival : IEvent

    {
        public Arrival(Batch batch, int time)
        {
            Batch = batch;
            Time = time;
        }
        public Batch Batch
        {
            get; private set;
        }
        
        public int Time
        {
            get; private set;
        }
        public bool IsArrival
        {
            get
            {
                return true;
            }
        }
        public void Conclude()
        {
            // do nothing, item arriving
        }
    }
}
