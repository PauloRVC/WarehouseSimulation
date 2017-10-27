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
    public class PutwallWithPPHScheduleAndTimeInQ: PutwallWithPPHSchedule
    {

        protected IDistribution<int> QTimeDist;
        private List<IEntity> TimedQueue = new List<IEntity>();
        private List<IEntity> AllQueuedBatches = new List<IEntity>();
        private SimulationResults Results;
        public Dictionary<int, int> CurrentCapacity
        {
            get
            {
                return PPHSchedule;
            }
        }

        public PutwallWithPPHScheduleAndTimeInQ(int queueSize,
                                                Dictionary<int, int> pPHSchedule,
                                                List<Processor> operators,
                                                IDistribution<int> processTimeDist,
                                                IDistribution<int> recircTimeDist,
                                                Simulation simulation,
                                                IDestinationBlock nextDestination,
                                                IDistribution<int> qTimeDist,
                                                SimulationResults results) : base(queueSize, 
                                                                                        pPHSchedule, 
                                                                                        operators, 
                                                                                        processTimeDist, 
                                                                                        recircTimeDist, 
                                                                                        simulation, 
                                                                                        nextDestination)
        {
            QTimeDist = qTimeDist;
            Results = results;
            Results.LeftOverCapacity = PPHSchedule;
        }

        public List<IEvent> InitializeQueue(int initialNumberInQueue)
        {
            var deQueueEvents = new List<IEvent>();

            for(int i = 1; i <= initialNumberInQueue; i++)
            {
                var batch = new Batch(this);

                int time = 0;

                var newEvent = new EndQueueEvent(DeQueue, batch, time, Simulation.CurrentTime);

                Queue.Add(batch);

                deQueueEvents.Add(newEvent);

                AllQueuedBatches.Add(batch);
            }

            return deQueueEvents;
        }
        public override IEvent GetNextEvent(IEntity batch)
        {
            IEvent NextEvent;
            int Time;
            int scheduleIndex = PPHSchedule.Keys.Where(x => x <= Simulation.CurrentTime).Max();


            if (AllQueuedBatches.Contains(batch))
            {
                if (TimedQueue.Contains(batch))
                    TimedQueue.Remove(batch);

                if (PPHSchedule[scheduleIndex] > 0)
                {
                    PPHSchedule[scheduleIndex]--;

                    Time = Simulation.CurrentTime + ProcessTimeDist.DrawNext();
                    batch.Destination = NextDestination;

                    Simulation.Results.ReportProcessRealization(batch, Simulation.CurrentTime, Time, new List<IResource>(), this);

                    NextEvent = new EndProcessEvent(Operators.First(), batch, Time, Simulation.CurrentTime);
                }
                else
                {
                    Time = PPHSchedule.Keys.Where(x => x >= Simulation.CurrentTime).Min() + 1;

                    batch.Destination = this;

                    Queue.Add(batch);

                    Simulation.Results.ReportQueueTime(batch, Simulation.CurrentTime, Time);

                    NextEvent = new EndQueueEvent(DeQueue, batch, Time, Simulation.CurrentTime);

                }
            }
            else
            {
                if (Queue.Count + TimedQueue.Count < QueueSize)
                {
                    Time = Simulation.CurrentTime + QTimeDist.DrawNext();

                    batch.Destination = this;

                    TimedQueue.Add(batch);

                    AllQueuedBatches.Add(batch);

                    Simulation.Results.ReportQueueTime(batch, Simulation.CurrentTime, Time);

                    NextEvent = new EndQueueEvent(NewDeQueue, batch, Time, Simulation.CurrentTime);
                }
                else
                {
                    // Recirculate
                    Time = Simulation.CurrentTime + RecircTimeDist.DrawNext();
                    batch.Destination = this;

                    Simulation.Results.ReportRecirculation(batch, Simulation.CurrentTime, Time);

                    NextEvent = new RecirculateEvent(batch, Time, Simulation.CurrentTime);
                }
            }
            
            Simulation.Results.ReportQueueSize(Simulation.CurrentTime, Queue.Count);

            Simulation.Results.ReportTimedQueueSize(Simulation.CurrentTime, TimedQueue.Count);

            return NextEvent;
        }
        protected void NewDeQueue(IEntity entity)
        {
            //do nothing
        }
        
    }
}
