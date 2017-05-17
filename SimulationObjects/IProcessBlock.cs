using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulationObjects
{
    public interface IProcessBlock
    {
        IEvent GetNextEvent(Batch batch);
    }
}
