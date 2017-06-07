using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulationObjects.Resources
{
    public class Processor: IResource
    {
        public bool IsBusy { get; set; }
    }
}
