using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulationObjects
{
    public class ArrivalBlock
    {
        private IDistribution<int> ArrivalTimeDist;
        private IDistribution<IProcessBlock> DestinationDist;
        private Simulation Simulation;
        public ArrivalBlock(IDistribution<int> arrivalTimeDist, IDistribution<IProcessBlock> destinationDist, Simulation simulation)
        {
            ArrivalTimeDist = arrivalTimeDist;
            DestinationDist = destinationDist;
            Simulation = simulation;
        }
        public Arrival GetNextEvent()
        {
            var Destination = DestinationDist.DrawNext();
            var Batch = new Batch(Destination);
            var Time = Simulation.CurrentTime + ArrivalTimeDist.DrawNext();
            return new Arrival(Batch, Time);
        }
    }
}
