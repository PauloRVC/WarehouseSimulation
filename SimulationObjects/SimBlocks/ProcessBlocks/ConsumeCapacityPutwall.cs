using SimulationObjects.Distributions;
using SimulationObjects.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimulationObjects.Entities;
using SimulationObjects.Events;
using SimulationObjects.Resources;

namespace SimulationObjects.SimBlocks
{
    public class ConsumeCapacityPutwall: PutwallWithConditionalProbAndOperators
    {
        List<Processor> CheeseProcessors = new List<Processor>() { new Processor() };
        public ConsumeCapacityPutwall(Dictionary<int, IDistribution<int>> processTimeDists,
                                                      Dictionary<int, IDistribution<int>> recircTimeDists,
                                                      Simulation simulation,
                                                      IDestinationBlock nextDestination,
                                                      Dictionary<int, int> pPXSchedule,
                                                      SimulationResults results,
                                                      Dictionary<int, Tuple<int, int>> conditionalP,
                                                      Dictionary<int, int> operatorSchedul): base(processTimeDists,
                                                                                                  recircTimeDists,
                                                                                                  simulation,
                                                                                                  nextDestination,
                                                                                                  pPXSchedule,
                                                                                                  results,
                                                                                                  conditionalP,
                                                                                                  operatorSchedul)
        {

        }
        public override IEvent GetNextEvent(IEntity batch)
        {
            int scheduleIndex = PPXSchedule.Keys.Where(x => x <= Simulation.CurrentTime).Max();

            if (batch.CurrentEvent.GetType() == typeof(EndQueueEvent) & PPXSchedule.Any(x => x.Key == scheduleIndex - 1 & x.Value > 0))
            {
                var ProcessTimeDist = ProcessTimeDists[ProcessTimeDists.Keys.Where(x => x <= Simulation.CurrentTime).Max()];
                int Time;
                int newScheduleIndex = PPXSchedule.Where(x => x.Key == scheduleIndex - 1 & x.Value > 0).First().Key;

                PPXSchedule[newScheduleIndex]--;

                Time = Simulation.CurrentTime + ProcessTimeDist.DrawNext();
                batch.Destination = NextDestination;

                Results.ReportProcessRealization(batch, Simulation.CurrentTime, Time, new List<IResource>(), this);

                return new EndProcessEvent(CheeseProcessors.First(), batch, Time, Simulation.CurrentTime);
            }
            else
            {
                return base.GetNextEvent(batch);
            }
        }
    }
}
