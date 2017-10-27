using SimulationObjects.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulationObjects.Events
{
    public class Arrival : IEvent

    {
        public Arrival(IEntity batch, int time, int createdTime)
        {
            Entity = batch;
            Time = time;
            CreatedTime = createdTime;
        }
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
