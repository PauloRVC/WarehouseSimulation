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
    public class PutwallWithIntervalDistributions : SimBlock, IDestinationBlock
    {
        protected Dictionary<int, IDistribution<int>> ProcessTimeDists;
        protected Dictionary<int, IDistribution<int>> RecircTimeDists;
        protected Dictionary<int, IDistribution<int>> QueueTimeDists;

        protected Dictionary<int, int> PPXSchedule;

        protected IDestinationBlock NextDestination;
        protected List<IEntity> Queue = new List<IEntity>();
        protected List<IEntity> TimedQueue = new List<IEntity>();
        protected List<IEntity> AllQueuedBatches = new List<IEntity>();
        protected int QueueSize;

        protected SimulationResults Results;

        protected List<Processor> Operators = new List<Processor>() { new Processor() };
        
        public PutwallWithIntervalDistributions(Dictionary<int, IDistribution<int>> processTimeDists,
                                                Dictionary<int, IDistribution<int>> recircTimeDists,
                                                Dictionary<int, IDistribution<int>> queueTimeDists,
                                                Simulation simulation,
                                                IDestinationBlock nextDestination,
                                                int qSize,
                                                Dictionary<int, int> pPXSchedule,
                                                SimulationResults results) : base(BlockType.Process, simulation)
        {
            ProcessTimeDists = processTimeDists;
            RecircTimeDists = recircTimeDists;
            QueueTimeDists = queueTimeDists;

            PPXSchedule = pPXSchedule;

            NextDestination = nextDestination;
            QueueSize = qSize;

            Results = results;
            Results.LeftOverCapacity = PPXSchedule;
        }
        public virtual IEvent GetNextEvent(IEntity batch)
        {
            IEvent NextEvent;
            int Time;
            int scheduleIndex = PPXSchedule.Keys.Where(x => x <= Simulation.CurrentTime).Max();

            var ProcessTimeDist = ProcessTimeDists[ProcessTimeDists.Keys.Where(x => x <= Simulation.CurrentTime).Max()];
            var RecircTimeDist = RecircTimeDists[RecircTimeDists.Keys.Where(x => x <= Simulation.CurrentTime).Max()];
            var QueueTimeDist = QueueTimeDists[QueueTimeDists.Keys.Where(x => x <= Simulation.CurrentTime).Max()];



            if (AllQueuedBatches.Contains(batch))
            {
                if (TimedQueue.Contains(batch))
                    TimedQueue.Remove(batch);

                if (PPXSchedule[scheduleIndex] > 0)
                {
                    PPXSchedule[scheduleIndex]--;

                    Time = Simulation.CurrentTime + ProcessTimeDist.DrawNext();
                    batch.Destination = NextDestination;

                    Simulation.Results.ReportProcessRealization(batch, Simulation.CurrentTime, Time, new List<IResource>(), this);

                    NextEvent = new EndProcessEvent(Operators.First(), batch, Time, Simulation.CurrentTime);
                }
                else
                {
                    Time = PPXSchedule.Keys.Where(x => x >= Simulation.CurrentTime).Min() + 1;

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
                    Time = Simulation.CurrentTime + QueueTimeDist.DrawNext();

                    batch.Destination = this;

                    TimedQueue.Add(batch);

                    AllQueuedBatches.Add(batch);

                    Simulation.Results.ReportQueueTime(batch, Simulation.CurrentTime, Time);

                    NextEvent = new EndQueueEvent(DoNothing, batch, Time, Simulation.CurrentTime);
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
        protected void DeQueue(IEntity entity)
        {
            Queue.Remove(entity);
        }
        protected void DoNothing(IEntity entity)
        {
            //do nothing
        }
    }
}
