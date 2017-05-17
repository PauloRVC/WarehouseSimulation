using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulationObjects
{
    public interface IEvent
    {
        Batch Batch { get; }
        int Time { get; }
        bool IsArrival { get; }
        void Conclude();
    }
}
