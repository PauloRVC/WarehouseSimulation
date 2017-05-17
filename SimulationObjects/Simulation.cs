using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulationObjects
{
    public class Simulation
    {
        private SortedDictionary<int, IEvent> EventQueue;
        private ArrivalBlock ArrivalBlock;
        private List<IProcessBlock> PutwallLanes;
        private int EndTime;
        public SimulationResults Run()
        {
            var results = new SimulationResults();
            
            while (EventQueue.Keys.First() <= EndTime)
            {
                var newEvent = EventQueue.First();
                EventQueue.Remove(newEvent.Key);

                var nextEvent = newEvent.Value.Batch.Destination.GetNextEvent(newEvent.Value.Batch);
                if(nextEvent != null)
                    EventQueue.Add(nextEvent.Time, nextEvent);
                if (newEvent.Value.IsArrival)
                {
                    nextEvent = ArrivalBlock.GetNextEvent();
                    if (nextEvent != null)
                        EventQueue.Add(nextEvent.Time, nextEvent);
                }
                newEvent.Value.Conclude();

            }
            return results;
        }
    }
}
