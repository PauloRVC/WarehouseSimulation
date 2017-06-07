using SimulationObjects.Entities;
using SimulationObjects.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulationObjects.Events
{
    public interface IEvent
    {
        IEntity Entity { get; }
        int Time { get; }
        bool IsArrival { get; }
        void Conclude();
    }
}
