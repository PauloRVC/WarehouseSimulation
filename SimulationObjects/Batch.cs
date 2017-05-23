using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulationObjects
{
    public class Batch
    {
        public IProcessBlock Destination { get; set; }
        public Batch(IProcessBlock destination)
        {
            Destination = destination;
        }
    }
}
