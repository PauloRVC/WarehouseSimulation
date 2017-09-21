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
    public class PutwallWithPPHSchedule : Putwall
    {
        private Dictionary<int, int> PPHSchedule;

        private List<IEntity> Queue = new List<IEntity>();

        private int QueueSize;

        

        public PutwallWithPPHSchedule(int queueSize,
                      Dictionary<int, int> pPHSchedule,
                      List<Processor> operators,
                      IDistribution<int> processTimeDist,
                      IDistribution<int> recircTimeDist,
                      Simulation simulation,
                      IDestinationBlock nextDestination) : base(operators, processTimeDist, recircTimeDist, simulation, nextDestination)
        {
            PPHSchedule = pPHSchedule;
            QueueSize = queueSize;
        }

        public override IEvent GetNextEvent(IEntity batch)
        {
            IEvent NextEvent;
            int Time;
            int scheduleIndex = PPHSchedule.Keys.Where(x => x <= Simulation.CurrentTime).Max();

            if(PPHSchedule[scheduleIndex] > 0)
            {
                PPHSchedule[scheduleIndex]--;

                Time = Simulation.CurrentTime + ProcessTimeDist.DrawNext();
                batch.Destination = NextDestination;

                Simulation.Results.ReportProcessRealization(batch, Simulation.CurrentTime, Time, new List<IResource>(), this);

                NextEvent = new EndProcessEvent(Operators.First(), batch, Time);
            }
            else
            {
                if(Queue.Count < QueueSize)
                {
                    Time = PPHSchedule.Keys.Where(x => x >= Simulation.CurrentTime).Min() + 1;

                    batch.Destination = this;

                    Queue.Add(batch);

                    Simulation.Results.ReportQueueTime(batch, Simulation.CurrentTime, Time);
                    
                    NextEvent = new EndQueueEvent(DeQueue, batch, Time);
                }
                else
                {
                    // Recirculate
                    Time = Simulation.CurrentTime + RecircTimeDist.DrawNext();
                    batch.Destination = this;

                    Simulation.Results.ReportRecirculation(batch, Simulation.CurrentTime, Time);

                    NextEvent = new RecirculateEvent(batch, Time);
                }
            }

            return NextEvent;
        }
        private void DeQueue(IEntity entity)
        {
            Queue.Remove(entity);
        }
    }
}
