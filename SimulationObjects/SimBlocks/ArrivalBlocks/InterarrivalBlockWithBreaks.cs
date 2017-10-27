using SimulationObjects.Distributions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimulationObjects.Events;
using SimulationObjects.Entities;

namespace SimulationObjects.SimBlocks
{
    public class InterarrivalBlockWithBreaks: InterarrivalBlock2
    {
        private List<Tuple<int, int>> Breaks;

        public InterarrivalBlockWithBreaks(Dictionary<Tuple<int, int>, IDistribution<int>> interArrivalDists,
                                            IDistribution<IDestinationBlock> destinationDist,
                                            Simulation simulation,
                                            List<Tuple<int, int>> breaks) : base(interArrivalDists,
                                                                                 destinationDist,
                                                                                 simulation)
        {
            Breaks = breaks;
        }

        public override IEvent GetNextEvent()
        {
            var Destination = DestinationDist.DrawNext();
            var Batch = new Batch(Destination);

            var interArrivalDist = InterArrivalDists.Where(x => x.Key.Item1 <= Simulation.CurrentTime && x.Key.Item2 >= Simulation.CurrentTime).First().Value;

            int dur = interArrivalDist.DrawNext();

            var Time = Simulation.CurrentTime + dur;

            if(Breaks.Any(x => x.Item1 <= Time & x.Item2 >= Time))
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

            return new Arrival(Batch, Time, Simulation.CurrentTime);
        }
    }
}
