using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimulationObjects.Entities;

namespace SimulationObjects.Events
{
    public class RecirculateEvent : IEvent
    {
        public RecirculateEvent(IEntity entity, int time, int createdTime)
        {
            Entity = entity;
            Time = time;
            IsArrival = false;
            CreatedTime = createdTime;
        }
        public int CreatedTime { get; private set; }
        public IEntity Entity { get; private set; }

        public int Time { get; private set; }

        public bool IsArrival { get; private set; }

        public void Conclude()
        {
            //Do nothing
        }
    }
}
