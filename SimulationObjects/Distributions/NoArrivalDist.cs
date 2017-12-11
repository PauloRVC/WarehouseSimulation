using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulationObjects.Distributions
{
    public class NoArrivalDist : IDistribution<InterarrivalBatchSize>
    {
        private int EndOfInterval;
        public Simulation Simulation { get; set; }

        public NoArrivalDist(int endOfInterval)
        {
            EndOfInterval = endOfInterval;
        }
        public InterarrivalBatchSize DrawNext()
        {
            int timeToNextInterval = EndOfInterval - Simulation.CurrentTime;
            if (timeToNextInterval < 0)
                throw new InvalidOperationException();
            return new InterarrivalBatchSize(timeToNextInterval, 0);
        }
        public NoArrivalDist Copy(MultiArrivalSimulation simulation)
        {
            var copy = new NoArrivalDist(EndOfInterval);
            copy.Simulation = simulation;
            return copy;
        }
    }
}
