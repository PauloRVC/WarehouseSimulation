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
