using SimulationObjects.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulationObjects.Events
{
    public class MultiArrival : IEvent

    {
        public MultiArrival(int time, int createdTime, List<Arrival> arrivals)
        {
            Entity = null;
            Time = time;
            CreatedTime = createdTime;
            Arrivals = arrivals;
        }
        public List<Arrival> Arrivals { get; private set; }
        public IEntity Entity
        {
            get; private set;
        }

        public int Time
        {
            get; private set;
        }
        public int CreatedTime { get; private set; }
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
