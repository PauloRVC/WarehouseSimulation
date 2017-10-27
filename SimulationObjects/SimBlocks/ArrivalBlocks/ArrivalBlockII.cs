using SimulationObjects.Distributions;
using SimulationObjects.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimulationObjects.Entities;
using Infrastructure;

namespace SimulationObjects.SimBlocks
{
    public class ArrivalBlockII : SimBlock, IArrivalBlock
    {
        protected Dictionary<int, IDistribution<int>> ArrivalDists = new Dictionary<int, IDistribution<int>>();
        protected Dictionary<int, IDistribution<IDestinationBlock>> DestinationDists;
        protected List<Tuple<int, int>> Breaks;
        protected IDestinationBlock P06;

        public ArrivalBlockII(Dictionary<int, IDistribution<int>> arrivalDists,
                              Dictionary<int, IDistribution<IDestinationBlock>> destinationDists,
                              IDestinationBlock p06,
                              List<Tuple<int, int>> breaks,
                              Simulation simulation): base(BlockType.Arrival, simulation)
        {
            ArrivalDists = arrivalDists;
            DestinationDists = destinationDists;
            P06 = p06;
            Breaks = breaks;
        }
        public virtual IEvent GetNextEvent()
        {
            var destinationDist = DestinationDists[DestinationDists.Keys.Where(x => x <= Simulation.CurrentTime).Max()];

            var Destination = destinationDist.DrawNext();

            ProcessType p;
            if (Destination == P06)
                p = ProcessType.Putwall;
            else
                p = ProcessType.NonPutwall;

           var Batch = new Batch(Destination, p);

            var interArrivalDist = ArrivalDists[ArrivalDists.Keys.Where(x => x <= Simulation.CurrentTime).Max()];

            int dur = interArrivalDist.DrawNext();

            var Time = Simulation.CurrentTime + dur;

            if (Breaks.Any(x => x.Item1 <= Time & x.Item2 >= Time))
            {
                var breakTime = Breaks.Where(x => x.Item1 <= Time & x.Item2 >= Time).First();

                Time = breakTime.Item2 + 1;

                dur = Time - Simulation.CurrentTime;
            }

            Simulation.Results.ReportInterarrivalTime(Simulation.CurrentTime, dur);

            if (Time <= Simulation.EndTime)
            {
                Simulation.Results.ReportArrival(Batch, Time);
            }

            var nextEvent = new Arrival(Batch, Time, Simulation.CurrentTime);

            Batch.CurrentEvent = nextEvent;

            return nextEvent;
        }
    }
}
