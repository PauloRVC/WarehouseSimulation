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
    public class PutwallWithConditionalPofRecirc : PutwallWithIntervalDistsII
    {
        protected Dictionary<int, Tuple<int, int>> ConditionProbOfRecirc;
        protected Random rng = new Random();

        public PutwallWithConditionalPofRecirc(Dictionary<int, IDistribution<int>> processTimeDists,
                                                Dictionary<int, IDistribution<int>> recircTimeDists,
                                                Simulation simulation,
                                                IDestinationBlock nextDestination,
                                                int qSize,
                                                Dictionary<int, int> pPXSchedule,
                                                SimulationResults results,
                                                Dictionary<int, Tuple<int, int>> conditionalP) : base(processTimeDists,
                                                                                                      recircTimeDists,
                                                                                                      simulation,
                                                                                                      nextDestination,
                                                                                                      qSize,
                                                                                                      pPXSchedule,
                                                                                                      results)
        {
            ConditionProbOfRecirc = conditionalP;
        }
        public override IEvent GetNextEvent(IEntity batch)
        {
            IEvent NextEvent;
            int Time;
            int scheduleIndex = PPXSchedule.Keys.Where(x => x <= Simulation.CurrentTime).Max();

            var ProcessTimeDist = ProcessTimeDists[ProcessTimeDists.Keys.Where(x => x <= Simulation.CurrentTime).Max()];
            var RecircTimeDist = RecircTimeDists[RecircTimeDists.Keys.Where(x => x <= Simulation.CurrentTime).Max()];

            
            if (PPXSchedule[scheduleIndex] > 0)
            {
                if (ConditionProbOfRecirc.ContainsKey(Queue.Count))
                {
                    int nObs = ConditionProbOfRecirc[Queue.Count].Item1 + ConditionProbOfRecirc[Queue.Count].Item2;
                    double pRecirc = ConditionProbOfRecirc[Queue.Count].Item1 / nObs;

                    if(rng.NextDouble() <= pRecirc)
                    {
                        // Recirculate
                        Time = Simulation.CurrentTime + RecircTimeDist.DrawNext();
                        batch.Destination = this;

                        Simulation.Results.ReportRecirculation(batch, Simulation.CurrentTime, Time);

                        NextEvent = new RecirculateEvent(batch, Time);
                    }
                    else
                    {
                        PPXSchedule[scheduleIndex]--;

                        Time = Simulation.CurrentTime + ProcessTimeDist.DrawNext();
                        batch.Destination = NextDestination;

                        Results.ReportProcessRealization(batch, Simulation.CurrentTime, Time, new List<IResource>(), this);

                        NextEvent = new EndProcessEvent(Operators.First(), batch, Time);
                    }
                }
                else
                {
                    PPXSchedule[scheduleIndex]--;

                    Time = Simulation.CurrentTime + ProcessTimeDist.DrawNext();
                    batch.Destination = NextDestination;

                    Results.ReportProcessRealization(batch, Simulation.CurrentTime, Time, new List<IResource>(), this);

                    NextEvent = new EndProcessEvent(Operators.First(), batch, Time);
                }
            }
            else if (Queue.Count < QueueSize)
            {
                if (ConditionProbOfRecirc.ContainsKey(Queue.Count))
                {
                    int nObs = ConditionProbOfRecirc[Queue.Count].Item1 + ConditionProbOfRecirc[Queue.Count].Item2;
                    double pRecirc = ConditionProbOfRecirc[Queue.Count].Item1 / nObs;

                    if (rng.NextDouble() <= pRecirc)
                    {
                        // Recirculate
                        Time = Simulation.CurrentTime + RecircTimeDist.DrawNext();
                        batch.Destination = this;

                        Simulation.Results.ReportRecirculation(batch, Simulation.CurrentTime, Time);

                        NextEvent = new RecirculateEvent(batch, Time);
                    }
                    else
                    {
                        if (PPXSchedule.Keys.Any(x => x > Simulation.CurrentTime))
                            Time = PPXSchedule.Keys.Where(x => x > Simulation.CurrentTime).Min();
                        else
                            Time = Simulation.CurrentTime + 1;

                        batch.Destination = this;

                        Queue.Add(batch);

                        Results.ReportQueueTime(batch, Simulation.CurrentTime, Time);

                        NextEvent = new EndQueueEvent(DeQueue, batch, Time);
                    }
                }
                else
                {
                    if (PPXSchedule.Keys.Any(x => x > Simulation.CurrentTime))
                        Time = PPXSchedule.Keys.Where(x => x > Simulation.CurrentTime).Min();
                    else
                        Time = Simulation.CurrentTime + 1;

                    batch.Destination = this;

                    Queue.Add(batch);

                    Results.ReportQueueTime(batch, Simulation.CurrentTime, Time);

                    NextEvent = new EndQueueEvent(DeQueue, batch, Time);
                }
            }
            else
            {
                // Recirculate
                Time = Simulation.CurrentTime + RecircTimeDist.DrawNext();
                batch.Destination = this;

                Simulation.Results.ReportRecirculation(batch, Simulation.CurrentTime, Time);

                NextEvent = new RecirculateEvent(batch, Time);
            }

            Results.ReportQueueSize(Simulation.CurrentTime, Queue.Count);

            return NextEvent;
        }
    }
}
