using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulationObjects
{
    public class Putwall : IProcessBlock
    {
        private List<Processor> Operators;
        private IDistribution<int> ProcessTimeDist;
        private IDistribution<int> RecircTimeDist;
        private Simulation Simulation;
        private IProcessBlock DisposalBlock;
        public Putwall(List<Processor> operators, IDistribution<int> processTimeDist, IDistribution<int> recircTimeDist, Simulation simulation, IProcessBlock disposalBlock)
        {
            Operators = operators;
            ProcessTimeDist = processTimeDist;
            RecircTimeDist = recircTimeDist;
            Simulation = simulation;
            DisposalBlock = disposalBlock;
        }
        public IEvent GetNextEvent(Batch batch)
        {
            IEvent NextEvent;
            int Time;
            if (Operators.All(x => x.IsBusy))
            {
                // Recirculate
                Time = Simulation.CurrentTime + RecircTimeDist.DrawNext();
                batch.Destination = this;
                NextEvent = new GenericEvent(batch, Time);
            }
            else
            {
                Time = Simulation.CurrentTime + ProcessTimeDist.DrawNext();
                var Operator = Operators.Where(x => !x.IsBusy).First();
                batch.Destination = DisposalBlock;
                NextEvent = new EndProcessEvent(Operator, batch, Time);
            }
            return NextEvent;
            
        }
    }
}
