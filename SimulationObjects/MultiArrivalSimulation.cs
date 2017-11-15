using SimulationObjects.Events;
using SimulationObjects.Results;
using SimulationObjects.SimBlocks.ArrivalBlocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulationObjects
{
    public class MultiArrivalSimulation: Simulation
    {

        private MultiArrivalBlock ArrivalBlock;
        
        public MultiArrivalSimulation(int endTime):base(endTime)
        {
        }
        public MultiArrivalSimulation(SimulationResults results, int endTime):base(results, endTime)
        {
        }
        public void Initialize(MultiArrivalBlock arrivalBlock, IEvent firstArrival)
        {
            ArrivalBlock = arrivalBlock;

            EventQueue = new List<IEvent>() { firstArrival };
        }
        public void Initialize(MultiArrivalBlock arrivalBlock, IEvent firstArrival, List<IEvent> eventQueue)
        {
            ArrivalBlock = arrivalBlock;

            EventQueue = eventQueue;

            EventQueue.Add(firstArrival);

            EventQueue = EventQueue.OrderBy(x => x.Time).ToList();
        }
        public override SimulationResults Run()
        {

            int iterCount = 0;
            while (EventQueue[0].Time <= EndTime)
            {
                var newEvent = EventQueue.First();
                EventQueue.Remove(newEvent);
                CurrentTime = newEvent.Time;

                IEvent nextEvent;

                if (newEvent.IsArrival)
                {
                    var multiArrival = (MultiArrival)newEvent;

                    foreach(Arrival a in multiArrival.Arrivals)
                    {
                        nextEvent = a.Entity.Destination.GetNextEvent(a.Entity);

                        if (nextEvent != null)
                            EventQueue.Add(nextEvent);
                    }

                    nextEvent = ArrivalBlock.GetNextEvent();

                    if (nextEvent != null)
                        EventQueue.Add(nextEvent);
                }
                else
                {
                    nextEvent = newEvent.Entity.Destination.GetNextEvent(newEvent.Entity);

                    if (nextEvent != null)
                        EventQueue.Add(nextEvent);
                }

                newEvent.Conclude();
                EventQueue = EventQueue.OrderBy(x => x.Time).ThenBy(x => x.CreatedTime).ToList();
                iterCount++;
            }

            return Results;
        }
    }
}
