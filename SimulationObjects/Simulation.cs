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

        private IArrivalBlock ArrivalBlock;

        public int EndTime { get; private set; }

        public int CurrentTime { get; private set; }
        public SimulationResults Results { get; private set; }
        public Simulation(int endTime)
        {
            CurrentTime = 0;
            EndTime = endTime;
            Results = new SimulationResults(endTime);
        }
        public Simulation(SimulationResults results, int endTime)
        {
            CurrentTime = 0;
            EndTime = endTime;
            Results = results;
        }
        public void Initialize(IArrivalBlock arrivalBlock, IEvent firstArrival)
        {
            ArrivalBlock = arrivalBlock;
            
            EventQueue = new List<IEvent>() { firstArrival };
        }
        public SimulationResults Run()
        {

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

            return Results;
        }
    }
}
