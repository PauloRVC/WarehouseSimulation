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
    public class PutwallWithNoMaxQSize: PutwallWithIntervalDistsII
    {
        protected Dictionary<int, Tuple<int, int>> ConditionProbOfRecirc;
        protected Random rng = new Random();

        public PutwallWithNoMaxQSize(Dictionary<int, IDistribution<int>> processTimeDists,
                                     Dictionary<int, IDistribution<int>> recircTimeDists,
                                     Simulation simulation,
                                     IDestinationBlock nextDestination,
                                     Dictionary<int, int> pPXSchedule,
                                     SimulationResults results,
                                     Dictionary<int, Tuple<int, int>> conditionalP) : base(processTimeDists,
                                                                                           recircTimeDists,
                                                                                           simulation,
                                                                                           nextDestination,
                                                                                           int.MaxValue,
                                                                                           pPXSchedule,
                                                                                           results)
        {
            ConditionProbOfRecirc = conditionalP;
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
        public override IEvent GetNextEvent(IEntity batch)
        {
            IEvent NextEvent;
            int scheduleIndex = PPXSchedule.Keys.Where(x => x <= Simulation.CurrentTime).Max();
            
            if(batch.CurrentEvent.GetType() == typeof(EndQueueEvent))
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
            else if(ConditionProbOfRecirc.Keys.Max() < Queue.Count)
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

            Results.ReportQueueSize(Simulation.CurrentTime, Queue.Count);

            return NextEvent;
        }

    }
}
