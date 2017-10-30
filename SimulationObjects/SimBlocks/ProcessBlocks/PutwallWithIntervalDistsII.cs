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
    public class PutwallWithIntervalDistsII : SimBlock, IDestinationBlock
    {
        protected Dictionary<int, IDistribution<int>> ProcessTimeDists;
        protected Dictionary<int, IDistribution<int>> RecircTimeDists;

        protected Dictionary<int, int> PPXSchedule;

        protected IDestinationBlock NextDestination;
        protected List<IEntity> Queue = new List<IEntity>();
        protected int QueueSize;

        protected SimulationResults Results;

        protected List<Processor> Operators = new List<Processor>() { new Processor() };

        public PutwallWithIntervalDistsII(Dictionary<int, IDistribution<int>> processTimeDists,
                                                Dictionary<int, IDistribution<int>> recircTimeDists,
                                                Simulation simulation,
                                                IDestinationBlock nextDestination,
                                                int qSize,
                                                Dictionary<int, int> pPXSchedule,
                                                SimulationResults results) : base(BlockType.Process, simulation)
        {
            ProcessTimeDists = processTimeDists;
            RecircTimeDists = recircTimeDists;

            PPXSchedule = pPXSchedule;

            NextDestination = nextDestination;
            QueueSize = qSize;

            Results = results;
            Results.LeftOverCapacity = PPXSchedule;
        }
        public List<IEvent> InitializeQueue(int initialNumberInQueue)
        {
            var deQueueEvents = new List<IEvent>();

            for (int i = 1; i <= initialNumberInQueue; i++)
            {
                var batch = new Batch(this, Infrastructure.ProcessType.Putwall);

                int time = 0;
                

                //Results.ReportArrival(batch, 0);

                var newEvent = new EndQueueEvent(DeQueue, batch, time, Simulation.CurrentTime);

                batch.CurrentEvent = newEvent;

                Queue.Add(batch);

                deQueueEvents.Add(newEvent);
            }

            return deQueueEvents;
        }
        public virtual IEvent GetNextEvent(IEntity batch)
        {
            IEvent NextEvent;
            int Time;
            int scheduleIndex = PPXSchedule.Keys.Where(x => x <= Simulation.CurrentTime).Max();

            var ProcessTimeDist = ProcessTimeDists[ProcessTimeDists.Keys.Where(x => x <= Simulation.CurrentTime).Max()];
            var RecircTimeDist = RecircTimeDists[RecircTimeDists.Keys.Where(x => x <= Simulation.CurrentTime).Max()];
            
            if (PPXSchedule[scheduleIndex] > 0)
            {
                PPXSchedule[scheduleIndex]--;

                Time = Simulation.CurrentTime + ProcessTimeDist.DrawNext();
                batch.Destination = NextDestination;

                Results.ReportProcessRealization(batch, Simulation.CurrentTime, Time, new List<IResource>(), this);

                NextEvent = new EndProcessEvent(Operators.First(), batch, Time, Simulation.CurrentTime);
            }
            else if(Queue.Count < QueueSize)
            {
                if (PPXSchedule.Keys.Any(x => x > Simulation.CurrentTime))
                    Time = PPXSchedule.Keys.Where(x => x > Simulation.CurrentTime).Min();
                else
                    Time = Simulation.CurrentTime + 1;

                batch.Destination = this;

                Queue.Add(batch);

                Results.ReportQueueTime(batch, Simulation.CurrentTime, Time);

                NextEvent = new EndQueueEvent(DeQueue, batch, Time, Simulation.CurrentTime);
            }
            else
            {
                // Recirculate
                Time = Simulation.CurrentTime + RecircTimeDist.DrawNext();
                batch.Destination = this;

                Simulation.Results.ReportRecirculation(batch, Simulation.CurrentTime, Time);

                NextEvent = new RecirculateEvent(batch, Time, Simulation.CurrentTime);
            }

            Results.ReportQueueSize(Simulation.CurrentTime, Queue.Count);
            
            return NextEvent;
        }
        protected virtual EndProcessEvent Process(IEntity batch)
        {
            int Time;
            int scheduleIndex = PPXSchedule.Keys.Where(x => x <= Simulation.CurrentTime).Max();

            var ProcessTimeDist = ProcessTimeDists[ProcessTimeDists.Keys.Where(x => x <= Simulation.CurrentTime).Max()];

            PPXSchedule[scheduleIndex]--;

            Time = Simulation.CurrentTime + ProcessTimeDist.DrawNext();
            batch.Destination = NextDestination;

            Results.ReportProcessRealization(batch, Simulation.CurrentTime, Time, new List<IResource>(), this);

            return new EndProcessEvent(Operators.First(), batch, Time, Simulation.CurrentTime);
        }
        protected virtual EndQueueEvent Enqueue(IEntity batch)
        {
            int Time;
            int scheduleIndex = PPXSchedule.Keys.Where(x => x <= Simulation.CurrentTime).Max();

            if (PPXSchedule.Keys.Any(x => x > Simulation.CurrentTime))
                Time = PPXSchedule.Keys.Where(x => x > Simulation.CurrentTime).Min();
            else
                Time = Simulation.CurrentTime + 1;

            batch.Destination = this;

            Queue.Add(batch);

            Results.ReportQueueTime(batch, Simulation.CurrentTime, Time);

            return new EndQueueEvent(DeQueue, batch, Time, Simulation.CurrentTime);
        }
        protected virtual RecirculateEvent Recirculate(IEntity batch)
        {
            int Time;
            int scheduleIndex = PPXSchedule.Keys.Where(x => x <= Simulation.CurrentTime).Max();
            var RecircTimeDist = RecircTimeDists[RecircTimeDists.Keys.Where(x => x <= Simulation.CurrentTime).Max()];

            Time = Simulation.CurrentTime + RecircTimeDist.DrawNext();
            batch.Destination = this;

            Simulation.Results.ReportRecirculation(batch, Simulation.CurrentTime, Time);

            return new RecirculateEvent(batch, Time, Simulation.CurrentTime);
        }
        protected void DeQueue(IEntity entity)
        {
            Queue.Remove(entity);
        }
    }
}
