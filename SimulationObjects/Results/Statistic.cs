using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulationObjects.Results
{
    public class Statistic
    {
        public double Average { get { return Observations.Average(); } }
        public double StdDev { get { return Observations.StandardDeviation(); } }
        public List<double> Observations { get; private set; }

        public Statistic(List<double> observations)
        {
            Observations = observations;
        }
        public void AddObservation(double observation)
        {
            Observations.Add(observation);
        }
    }
}
