using Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    public class Statistic
    {
        public double Average { get { return Observations.Count> 0 ? Observations.Average(): 0; } }
        public double StdDev { get { return Observations.Count > 0 ? Observations.StandardDeviation() : 0; } }
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
