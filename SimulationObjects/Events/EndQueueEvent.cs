using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimulationObjects.Entities;

namespace SimulationObjects.Events
{
    public class EndQueueEvent : IEvent
    {
        public IEntity Entity { get; private set; }

        public int Time { get; private set; }

        public bool IsArrival => false;

        private Action<IEntity> Conclusion;

        public EndQueueEvent(Action<IEntity> conclusion, IEntity entity, int time)
        {
            Entity = entity;
            Time = time;
            Conclusion = conclusion;
        }

        public void Conclude()
        {
            Conclusion(Entity);
        }
    }
}
