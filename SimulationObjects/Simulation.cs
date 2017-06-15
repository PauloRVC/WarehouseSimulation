using SimulationObjects.Events;
using SimulationObjects.Results;
using SimulationObjects.SimBlocks;
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

        private int EndTime;

        public int CurrentTime { get; private set; }
        public ISimResults Results { get; private set; }
        public Simulation()
        {
            CurrentTime = 0;
            //Results = new SimulationResults();
        }
        public void Initialize(ArrivalBlock arrivalBlock, int endTime, IEvent firstArrival)
        {
            ArrivalBlock = arrivalBlock;
            EndTime = endTime;
            
            EventQueue = new List<IEvent>() { firstArrival };
        }
        public SimulationResults Run()
        {
            var results = new SimulationResults(EndTime);

            int iterCount = 0;
            while (EventQueue[0].Time <= EndTime)
            {
                var newEvent = EventQueue.First();
                EventQueue.Remove(newEvent);
                CurrentTime = newEvent.Time;
                
                var nextEvent = newEvent.Entity.Destination.GetNextEvent(newEvent.Entity);

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
