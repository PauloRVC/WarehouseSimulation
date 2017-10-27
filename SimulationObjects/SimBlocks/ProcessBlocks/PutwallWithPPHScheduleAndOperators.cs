using SimulationObjects.Distributions;
using SimulationObjects.Entities;
using SimulationObjects.Events;
using SimulationObjects.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulationObjects.SimBlocks
{
    public class PutwallWithPPHScheduleAndOperators: PutwallWithPPHSchedule
    {
        public PutwallWithPPHScheduleAndOperators(int queueSize,
                      Dictionary<int, int> pPHSchedule,
                      List<Processor> operators,
                      IDistribution<int> processTimeDist,
                      IDistribution<int> recircTimeDist,
                      Simulation simulation,
                      IDestinationBlock nextDestination) : base(queueSize, pPHSchedule, operators, processTimeDist, recircTimeDist, simulation, nextDestination)
        {
        }

        public override IEvent GetNextEvent(IEntity batch)
        {
            IEvent NextEvent;
            int Time;
            int scheduleIndex = PPHSchedule.Keys.Where(x => x <= Simulation.CurrentTime).Max();

            //If batch can be processed
            if (Operators.Any(x => !x.IsBusy) & PPHSchedule[scheduleIndex] > 0)
            {
                PPHSchedule[scheduleIndex]--;

                Time = Simulation.CurrentTime + ProcessTimeDist.DrawNext();
                batch.Destination = NextDestination;

                var Operator = Operators.Where(x => !x.IsBusy).First();

                Simulation.Results.ReportProcessRealization(batch, Simulation.CurrentTime, Time, new List<IResource>() { Operator }, this);

                NextEvent = new EndProcessEvent(Operator, batch, Time, Simulation.CurrentTime);
            }
            else if(Queue.Count < QueueSize) //if batch can be queued
            {
                Time = PPHSchedule.Keys.Where(x => x >= Simulation.CurrentTime).Min() + 1;

                batch.Destination = this;

                Queue.Add(batch);

                Simulation.Results.ReportQueueTime(batch, Simulation.CurrentTime, Time);

                NextEvent = new EndQueueEvent(DeQueue, batch, Time, Simulation.CurrentTime);
            }
            else //recirculate
            {
                // Recirculate
                Time = Simulation.CurrentTime + RecircTimeDist.DrawNext();
                batch.Destination = this;

                Simulation.Results.ReportRecirculation(batch, Simulation.CurrentTime, Time);

                NextEvent = new RecirculateEvent(batch, Time, Simulation.CurrentTime);
            }
            

            Simulation.Results.ReportQueueSize(Simulation.CurrentTime, Queue.Count);

            return NextEvent;
        }
    }
}
