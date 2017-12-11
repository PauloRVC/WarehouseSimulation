using SimulationObjects.Distributions;
using SimulationObjects.Events;
using SimulationObjects.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infrastructure;

namespace SimulationObjects.SimBlocks.ArrivalBlocks
{
    public class MultiArrivalBlock: SimBlock, IArrivalBlock
    {
        protected IDistribution<InterarrivalBatchSize> MultiArrivalDist;
        protected IDistribution<IDestinationBlock> DestinationDist;
        protected List<Tuple<int, int>> Breaks;
        protected IDestinationBlock P06;

        public MultiArrivalBlock(IDistribution<InterarrivalBatchSize> multiArrivalDist,
                            IDistribution<IDestinationBlock> destinationDist,
                            IDestinationBlock p06,
                            List<Tuple<int, int>> breaks,
                            Simulation simulation): base(BlockType.Arrival, simulation)
        {
            MultiArrivalDist = multiArrivalDist;
            Breaks = breaks;
            DestinationDist = destinationDist;
            P06 = p06;
        }
        public virtual IEvent GetNextEvent()
        {
            var multiArrival = MultiArrivalDist.DrawNext();

            var arrivals = new List<Arrival>();

            int dur = multiArrival.InterarrivalTime;

            var Time = Simulation.CurrentTime + dur;

            if (Breaks.Any(x => x.Item1 <= Time & x.Item2 >= Time))
            {
                var breakTime = Breaks.Where(x => x.Item1 <= Time & x.Item2 >= Time).First();

                Time = breakTime.Item2 + 1;

                dur = Time - Simulation.CurrentTime;
            }

            Simulation.Results.ReportInterarrivalTime(Simulation.CurrentTime, dur);

            for (int i = 1; i <= multiArrival.BatchSize; i++)
            {
                var Destination = DestinationDist.DrawNext();

                ProcessType p;
                if (Destination == P06)
                    p = ProcessType.Putwall;
                else
                    p = ProcessType.NonPutwall;

                var Batch = new Batch(Destination, p);
                
                if (Time <= Simulation.EndTime)
                {
                    Simulation.Results.ReportArrival(Batch, Time);
                }

                var arv = new Arrival(Batch, Time, Simulation.CurrentTime);

                Batch.CurrentEvent = arv;

                arrivals.Add(arv);
            }

            return new MultiArrival(Time, Simulation.CurrentTime, arrivals);
        }
    }
}
