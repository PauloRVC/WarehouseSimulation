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
    public class Putwall : SimBlock, IDestinationBlock
    {
        private List<Processor> Operators;
        private IDistribution<int> ProcessTimeDist;
        private IDistribution<int> RecircTimeDist;
        private IDestinationBlock NextDestination;

        public Putwall(List<Processor> operators, 
                       IDistribution<int> processTimeDist, 
                       IDistribution<int> recircTimeDist, 
                       Simulation simulation,
                       IDestinationBlock nextDestination): base(BlockType.Process, simulation)
        {
            Operators = operators;
            ProcessTimeDist = processTimeDist;
            RecircTimeDist = recircTimeDist;
            NextDestination = nextDestination;
        }
        public IEvent GetNextEvent(IEntity batch)
        {
            IEvent NextEvent;
            int Time;
            if (Operators.All(x => x.IsBusy))
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
