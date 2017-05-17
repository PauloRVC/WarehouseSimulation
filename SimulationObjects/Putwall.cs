using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulationObjects
{
    public class Putwall : IProcessBlock
    {
        private List<Processor> Operators;
        public IEvent GetNextEvent(Batch batch)
        {
            throw new NotImplementedException();
        }
    }
}
