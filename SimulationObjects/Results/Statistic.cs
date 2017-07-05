using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulationObjects.Results
{
    public class Statistic
    {
        public double Average { get; private set; }
        public double StdDev { get; private set; }
        public List<double> Observations { get; private set; }

        public Statistic(List<double> observations)
        {
            Observations = observations;
            if (observations.Count > 0)
            {
                Average = Observations.Average();
                StdDev = Observations.StandardDeviation();
            }
        }
        public void AddObservation(double observation)
        {
            Observations.Add(observation);
            Average = Observations.Average();
            StdDev = Observations.StandardDeviation();
        }
    }
}
