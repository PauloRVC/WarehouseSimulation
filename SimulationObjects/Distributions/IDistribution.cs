using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulationObjects.Distributions
{
    public interface IDistribution<T>
    {
        T DrawNext();
    }
}
