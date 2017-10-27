using SimulationObjects.Distributions;
using SimulationObjects.Events;
using SimulationObjects.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulationObjects.SimBlocks
{
    public class ArrivalBlock: SimBlock, IArrivalBlock
    {
        private IDistribution<int> ArrivalTimeDist;
        private IDistribution<IDestinationBlock> DestinationDist;
        public ArrivalBlock(IDistribution<int> arrivalTimeDist, 
                            IDistribution<IDestinationBlock> destinationDist, 
                            Simulation simulation): base(BlockType.Arrival, simulation)
        {
            ArrivalTimeDist = arrivalTimeDist;
            DestinationDist = destinationDist;
        }
        public IEvent GetNextEvent()
        {
            var Destination = DestinationDist.DrawNext();
            var Batch = new Batch(Destination);

            int dur = ArrivalTimeDist.DrawNext();

            Simulation.Results.ReportInterarrivalTime(Simulation.CurrentTime, dur);
            
            var Time = Simulation.CurrentTime + dur;

            if(Time <= Simulation.EndTime)
            {
                Simulation.Results.ReportArrival(Batch, Time);
            }
            
            return new Arrival(Batch, Time, Simulation.CurrentTime);
        }
    }
}
