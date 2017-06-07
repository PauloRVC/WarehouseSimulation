using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Accord.Statistics.Distributions.Univariate;
using SimulationObjects.SimBlocks;

namespace SimulationObjects.Distributions
{
    public class EmpiricalDist : IDistribution<int>
    {
        private GeneralDiscreteDistribution Distribution;
        private Dictionary<int, int> Mapping;
        public EmpiricalDist(List<Tuple<double, int>> bins) 
        {
            Distribution = new GeneralDiscreteDistribution(bins.Select(x => x.Item1).ToArray());
            Mapping = new Dictionary<int, int>();
            for (int i = 0; i < bins.Count; i++)
            {
                Mapping.Add(i, bins[i].Item2);
            }
        }
        public int DrawNext()
        {
            int index = Distribution.Generate();
            return Mapping[index];
        }
    }
    public class DestinationDist : IDistribution<IDestinationBlock>
    {
        private GeneralDiscreteDistribution Distribution;
        private Dictionary<int, IDestinationBlock> Mapping;
        public DestinationDist(List<Tuple<double, IDestinationBlock>> bins)
        {
            Distribution = new GeneralDiscreteDistribution(bins.Select(x => x.Item1).ToArray());
            Mapping = new Dictionary<int, IDestinationBlock>();
            for (int i = 0; i < bins.Count; i++)
            {
                Mapping.Add(i, bins[i].Item2);
            }
        }
        public IDestinationBlock DrawNext()
        {
            int index = Distribution.Generate();
            return Mapping[index];
        }
    }
}
