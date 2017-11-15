using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulationObjects.Distributions
{
    public class InterarrivalBatchSize
    {
        public int InterarrivalTime { get; private set; }
        public int BatchSize { get; private set; }

        public InterarrivalBatchSize(int interarrivalTime, int batchSize)
        {
            InterarrivalTime = interarrivalTime;
            BatchSize = batchSize;
        }
    }
    public class ConditionalInterarrivalBatchSizeDist: IDistribution<InterarrivalBatchSize>
    {
        private IDistribution<int> InterarrivalDist;
        private Dictionary<int, IDistribution<int>> BatchSizeGivenInterarrivalTime;

        public InterarrivalBatchSize DrawNext()
        {
            int interarrivalTime = InterarrivalDist.DrawNext();

            var conditionalBatchSizeDist = BatchSizeGivenInterarrivalTime[interarrivalTime];

            int batchSize = conditionalBatchSizeDist.DrawNext();

            return new InterarrivalBatchSize(interarrivalTime, batchSize);
        }
        public ConditionalInterarrivalBatchSizeDist(EmpiricalDist interarrivalDist,
                                                    Dictionary<int, IDistribution<int>> batchSizeGivenInterarrivalTime)
        {
            InterarrivalDist = interarrivalDist;
            BatchSizeGivenInterarrivalTime = batchSizeGivenInterarrivalTime;
        }
    }
    public class IndependentInterarrivalBatchSizeDist: IDistribution<InterarrivalBatchSize>
    {
        private IDistribution<int> InterarrivalDist;
        private IDistribution<int> BatchSizeDist;

        public InterarrivalBatchSize DrawNext()
        {
            int interarrivalTime = InterarrivalDist.DrawNext();
            int batchSize = BatchSizeDist.DrawNext();

            return new InterarrivalBatchSize(interarrivalTime, batchSize);
        }

        public IndependentInterarrivalBatchSizeDist(EmpiricalDist interarrivalDist,
                                                    EmpiricalDist batchSizeDist)
        {
            InterarrivalDist = interarrivalDist;
            BatchSizeDist = batchSizeDist;
        }
    }
}
