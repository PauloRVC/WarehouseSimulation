using SimulationObjects.Distributions;
using SimulationObjects.Entities;
using SimulationObjects.Events;
using SimulationObjects.Resources;
using SimulationObjects.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulationObjects.SimBlocks
{
    public class PutwallWithConstantPofRecirc: PutwallWithIntervalDistsII
    {
        protected double POfRecirc;
        protected Random rng = new Random();

        public PutwallWithConstantPofRecirc(Dictionary<int, IDistribution<int>> processTimeDists,
                                                Dictionary<int, IDistribution<int>> recircTimeDists,
                                                Simulation simulation,
                                                IDestinationBlock nextDestination,
                                                Dictionary<int, int> pPXSchedule,
                                                SimulationResults results,
                                                double pOfRecirc) : base(processTimeDists,
                                                                                                      recircTimeDists,
                                                                                                      simulation,
                                                                                                      nextDestination,
                                                                                                      int.MaxValue,
                                                                                                      pPXSchedule,
                                                                                                      results)
        {
            if (pOfRecirc < 0 | pOfRecirc > 1)
                throw new InvalidOperationException();

            POfRecirc = pOfRecirc;
        }
        public override IEvent GetNextEvent(IEntity batch)
        {
            IEvent NextEvent;
            int Time;
            int scheduleIndex = PPXSchedule.Keys.Where(x => x <= Simulation.CurrentTime).Max();

            var ProcessTimeDist = ProcessTimeDists[ProcessTimeDists.Keys.Where(x => x <= Simulation.CurrentTime).Max()];
            var RecircTimeDist = RecircTimeDists[RecircTimeDists.Keys.Where(x => x <= Simulation.CurrentTime).Max()];

            if (batch.CurrentEvent.GetType() == typeof(EndQueueEvent))
            {
                if (PPXSchedule[scheduleIndex] > 0)
                {
                    NextEvent = Process(batch);
                }
                else
                {
                    NextEvent = Enqueue(batch);
                }
            }
            else
            {
                if (rng.NextDouble() <= POfRecirc)
                {
                    NextEvent = Recirculate(batch);
                }
                else if(PPXSchedule[scheduleIndex] > 0)
                {
                    NextEvent = Process(batch);
                }
                else
                {
                    NextEvent = Enqueue(batch);
                }
            }
                       

            Results.ReportQueueSize(Simulation.CurrentTime, Queue.Count);

            return NextEvent;
        }
    }
}
