using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulationObjects
{
    public class Simulation
    {
        private List<IEvent> EventQueue;
        private ArrivalBlock ArrivalBlock;
        private List<IProcessBlock> PutwallLanes;
        private int EndTime;
        public int CurrentTime { get; private set; }
        public Simulation()
        {
            CurrentTime = 0;
        }
        public void Initialize(ArrivalBlock arrivalBlock, List<IProcessBlock> putwallLanes, int endTime, Arrival firstArrival)
        {
            ArrivalBlock = arrivalBlock;
            PutwallLanes = putwallLanes;
            EndTime = endTime;

            EventQueue = new List<IEvent>();
            EventQueue.Add(firstArrival);
        }
        public SimulationResults Run()
        {
            var results = new SimulationResults();

            int iterCount = 0;
            while (EventQueue[0].Time <= EndTime)
            {
                var newEvent = EventQueue.First();
                EventQueue.Remove(newEvent);
                CurrentTime = newEvent.Time;

                var nextEvent = newEvent.Batch.Destination.GetNextEvent(newEvent.Batch);
                if(nextEvent != null)
                    EventQueue.Add(nextEvent);
                if (newEvent.IsArrival)
                {
                    nextEvent = ArrivalBlock.GetNextEvent();

                    if (nextEvent != null)
                        EventQueue.Add(nextEvent);
                }
                newEvent.Conclude();
                EventQueue = EventQueue.OrderBy(x => x.Time).ToList();
                iterCount++;
            }
            return results;
        }
    }
}
