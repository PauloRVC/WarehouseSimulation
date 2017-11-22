using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimulationObjects.Distributions;
using SimulationObjects.Results;
using SimulationObjects.Events;
using SimulationObjects.Entities;
using SimulationObjects.Resources;

namespace SimulationObjects.SimBlocks.ProcessBlocks
{
    public class SimplePutwall:SimBlock, IDestinationBlock
    {
        protected Dictionary<int, Tuple<int, int>> ConditionProbOfRecirc;
        protected Random rng = new Random();

        protected IDistribution<int> ProcessTimeDist;
        protected IDistribution<int> RecircTimeDist;
        protected Dictionary<int, int> PPXSchedule;
        protected IDestinationBlock NextDestination;

        protected List<IEntity> Queue = new List<IEntity>();
        protected Dictionary<int, int> OrderInQ;


        public SimplePutwall(IDistribution<int> processTimeDist,
                                     IDistribution<int> recircTimeDist,
                                     Simulation simulation,
                                     IDestinationBlock nextDestination,
                                     Dictionary<int, int> pPXSchedule,
                                     Dictionary<int, Tuple<int, int>> conditionalP) : 
                                            base(BlockType.Process, simulation)
        {
            ConditionProbOfRecirc = conditionalP;
            ProcessTimeDist = processTimeDist;
            RecircTimeDist = recircTimeDist;
            NextDestination = nextDestination;
            PPXSchedule = pPXSchedule;
            Simulation.Results.LeftOverCapacity = PPXSchedule;

            OrderInQ = new Dictionary<int, int>();
            foreach(var p in PPXSchedule)
            {
                OrderInQ.Add(p.Key, int.MinValue);
            }
        }


        public IEvent GetNextEvent(IEntity batch)
        {
            IEvent NextEvent;
            int scheduleIndex = PPXSchedule.Keys.Where(x => x <= Simulation.CurrentTime).Max();

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
            else if (ConditionProbOfRecirc.ContainsKey(Queue.Count))
            {
                int nObs = ConditionProbOfRecirc[Queue.Count].Item1 + ConditionProbOfRecirc[Queue.Count].Item2;
                double pRecirc = (double)ConditionProbOfRecirc[Queue.Count].Item1 / (double)nObs;

                if (rng.NextDouble() <= pRecirc)
                {
                    NextEvent = Recirculate(batch);
                }
                else
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
            }
            else if (ConditionProbOfRecirc.Keys.Max() < Queue.Count)
            {
                NextEvent = Recirculate(batch);
            }
            else
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

            batch.CurrentEvent = NextEvent;

            Simulation.Results.ReportQueueSize(Simulation.CurrentTime, Queue.Count);

            return NextEvent;
        }
        protected virtual EndProcessEvent Process(IEntity batch)
        {
            int Time;
            int scheduleIndex = PPXSchedule.Keys.Where(x => x <= Simulation.CurrentTime).Max();
            
            PPXSchedule[scheduleIndex]--;

            Time = Simulation.CurrentTime + ProcessTimeDist.DrawNext();
            batch.Destination = NextDestination;

            Simulation.Results.ReportProcessRealization(batch, Simulation.CurrentTime, Time, new List<IResource>(), this);

            return new EndProcessEvent(new Processor(), batch, Time, Simulation.CurrentTime);
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

            Simulation.Results.ReportQueueTime(batch, Simulation.CurrentTime, Time);

            OrderInQ[scheduleIndex]++;
            return new EndQueueEvent(DeQueue, batch, Time, OrderInQ[scheduleIndex]);
        }
        protected virtual RecirculateEvent Recirculate(IEntity batch)
        {
            int Time;
            int scheduleIndex = PPXSchedule.Keys.Where(x => x <= Simulation.CurrentTime).Max();

            Time = Simulation.CurrentTime + RecircTimeDist.DrawNext();
            batch.Destination = this;

            Simulation.Results.ReportRecirculation(batch, Simulation.CurrentTime, Time);

            return new RecirculateEvent(batch, Time, Simulation.CurrentTime);
        }
        protected void DeQueue(IEntity entity)
        {
            Queue.Remove(entity);
        }
        public List<IEvent> InitializeQueue(int initialNumberInQueue)
        {
            var deQueueEvents = new List<IEvent>();

            for (int i = 1; i <= initialNumberInQueue; i++)
            {
                var batch = new Batch(this, Infrastructure.ProcessType.Putwall);

                int time = 0;


                //Results.ReportArrival(batch, 0);

                var newEvent = new EndQueueEvent(DeQueue, batch, time, -1);

                batch.CurrentEvent = newEvent;

                Queue.Add(batch);

                deQueueEvents.Add(newEvent);
            }

            return deQueueEvents;
        }

    }
}
