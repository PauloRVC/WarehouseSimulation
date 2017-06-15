using SimulationObjects.Distributions;
using SimulationObjects.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimulationObjects.Entities;
using SimulationObjects.Events;

namespace SimulationObjects.SimBlocks
{
    public class PutwallWithOperatorSchedule : Putwall
    {
        private Dictionary<int, int> OperatorSchedule;

        public PutwallWithOperatorSchedule(Dictionary<int, int> operatorSchedule,
                      List<Processor> operators,
                      IDistribution<int> processTimeDist,
                      IDistribution<int> recircTimeDist,
                      Simulation simulation,
                      IDestinationBlock nextDestination): base(operators, processTimeDist, recircTimeDist, simulation, nextDestination)
        {
            OperatorSchedule = operatorSchedule;
        }

        public override IEvent GetNextEvent(IEntity batch)
        {
            IEvent NextEvent;
            int Time;
            int scheduleIndex = OperatorSchedule.Keys.Where(x => x <= Simulation.CurrentTime).Max();

            if (Operators.All(x => x.IsBusy) | Operators.Where(x => x.IsBusy).Count() >= OperatorSchedule[scheduleIndex])
            {
                // Recirculate
                Time = Simulation.CurrentTime + RecircTimeDist.DrawNext();
                batch.Destination = this;

                Simulation.Results.ReportRecirculation(batch, Simulation.CurrentTime, Time);

                NextEvent = new RecirculateEvent(batch, Time);
            }
            else
            {
                Time = Simulation.CurrentTime + ProcessTimeDist.DrawNext();
                var Operator = Operators.Where(x => !x.IsBusy).First();
                batch.Destination = NextDestination;

                Simulation.Results.ReportProcessRealization(batch, Simulation.CurrentTime, Time, new List<IResource>(1) { Operator }, this);

                NextEvent = new EndProcessEvent(Operator, batch, Time);
            }

            return NextEvent;
        }
    }
}
