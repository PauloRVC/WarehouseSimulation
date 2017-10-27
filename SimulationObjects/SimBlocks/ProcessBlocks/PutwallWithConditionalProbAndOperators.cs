using SimulationObjects.Distributions;
using SimulationObjects.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimulationObjects.Entities;
using SimulationObjects.Events;
using SimulationObjects.Resources;

namespace SimulationObjects.SimBlocks
{
    public class PutwallWithConditionalProbAndOperators: PutwallWithNoMaxQSize
    {
        protected Dictionary<int, int> OperatorSchedule;
        protected List<Processor> Operators;
        private int boostConstant = 2;


        public PutwallWithConditionalProbAndOperators(Dictionary<int, IDistribution<int>> processTimeDists,
                                                      Dictionary<int, IDistribution<int>> recircTimeDists,
                                                      Simulation simulation,
                                                      IDestinationBlock nextDestination,
                                                      Dictionary<int, int> pPXSchedule,
                                                      SimulationResults results,
                                                      Dictionary<int, Tuple<int, int>> conditionalP,
                                                      Dictionary<int, int> operatorSchedule) : base(processTimeDists,
                                                                                                    recircTimeDists,
                                                                                                    simulation,
                                                                                                    nextDestination,
                                                                                                    pPXSchedule,
                                                                                                    results,
                                                                                                    conditionalP)
        {
            OperatorSchedule = operatorSchedule;

            Operators = new List<Processor>();

            for(int i=1;i<= operatorSchedule.Values.Max() + boostConstant; i++)
            {
                Operators.Add(new Processor());
            }
        }

        protected override EndProcessEvent Process(IEntity batch)
        {
            int Time;
            int scheduleIndex = PPXSchedule.Keys.Where(x => x <= Simulation.CurrentTime).Max();

            var ProcessTimeDist = ProcessTimeDists[ProcessTimeDists.Keys.Where(x => x <= Simulation.CurrentTime).Max()];

            PPXSchedule[scheduleIndex]--;

            Time = Simulation.CurrentTime + ProcessTimeDist.DrawNext();
            batch.Destination = NextDestination;

            Results.ReportProcessRealization(batch, Simulation.CurrentTime, Time, new List<IResource>(), this);

            return new EndProcessEvent(Operators.Where(x => !x.IsBusy).First(), batch, Time, Simulation.CurrentTime);
        }

        public override IEvent GetNextEvent(IEntity batch)
        {
            IEvent NextEvent;
            int scheduleIndex = PPXSchedule.Keys.Where(x => x <= Simulation.CurrentTime).Max();

            if (batch.CurrentEvent.GetType() == typeof(EndQueueEvent))
            {
                if (PPXSchedule[scheduleIndex] > 0 & Operators.Where(x => x.IsBusy).Count() < OperatorSchedule[scheduleIndex] + boostConstant)
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
                    if (PPXSchedule[scheduleIndex] > 0 & Operators.Where(x => x.IsBusy).Count() < OperatorSchedule[scheduleIndex] + boostConstant)
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
                if (PPXSchedule[scheduleIndex] > 0 & Operators.Where(x => x.IsBusy).Count() < OperatorSchedule[scheduleIndex] + boostConstant)
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
