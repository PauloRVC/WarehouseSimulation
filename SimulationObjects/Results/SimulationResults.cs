using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimulationObjects.Entities;
using SimulationObjects.Resources;
using SimulationObjects.SimBlocks;
using Infrastructure;

namespace SimulationObjects.Results
{
    public class SimulationResults : ISimResults
    {
        //Need to keep track of:
        //      entity time in system
        //      resource utilization
        //      throughput
        //      process time
        //      recirculation time

        protected Dictionary<IEntity, int> ArrivalTimes = new Dictionary<IEntity, int>();
        private Dictionary<IEntity, int> DisposalTimes = new Dictionary<IEntity, int>();
        private Dictionary<IEntity, int> TimeInProcess = new Dictionary<IEntity, int>();
        private Dictionary<IEntity, int> TimeInRecirculation = new Dictionary<IEntity, int>();
        private Dictionary<IEntity, int> TimesRecirculated = new Dictionary<IEntity, int>();
        private Dictionary<IEntity, int> TimeInQueue = new Dictionary<IEntity, int>();

        private Dictionary<IResource, int> ConsumedTime = new Dictionary<IResource, int>();

        private Dictionary<SimBlock, List<int>> ProcessTimes = new Dictionary<SimBlock, List<int>>();

        private List<Tuple<int, int>> InterarrivalTimes = new List<Tuple<int, int>>();
        private int[] ItemsInRecirc;
        private Dictionary<int, int> QueueSizeOverTime = new Dictionary<int, int>();

        private int EndTime;

        public SimulationResults(int endTime)
        {
            EndTime = endTime;
            ItemsInRecirc = new int[EndTime];
        }
        
        public virtual void ReportArrival(IEntity entity, int arrivalTime)
        {
            if (ArrivalTimes.ContainsKey(entity))
                throw new InvalidOperationException();

            ArrivalTimes.Add(entity, arrivalTime);

            TimesRecirculated.Add(entity, 0);
            TimeInRecirculation.Add(entity, 0);
            TimeInQueue.Add(entity, 0);
            TimeInProcess.Add(entity, 0);
        }

        public virtual void ReportDisposal(IEntity entity, int disposalTime)
        {
            if (DisposalTimes.ContainsKey(entity))
                throw new InvalidOperationException();

            DisposalTimes.Add(entity, disposalTime);
        }

        public virtual void ReportProcessRealization(IEntity entity, 
                                             int startTime, 
                                             int endTime, 
                                             IEnumerable<IResource> consumedResources,
                                             SimBlock process)
        {
            endTime = Math.Min(endTime, EndTime);
            
            foreach (IResource r in consumedResources)
            {
                if (ConsumedTime.ContainsKey(r))
                {
                    ConsumedTime[r] += endTime - startTime;
                }
                else
                {
                    ConsumedTime.Add(r, endTime - startTime);
                }
            }

            if (ProcessTimes.ContainsKey(process))
            {
                ProcessTimes[process].Add(endTime - startTime);
            }
            else
            {
                ProcessTimes.Add(process, new List<int> { endTime - startTime });
            }

            if (TimeInProcess.ContainsKey(entity))
            {
                TimeInProcess[entity] += endTime - startTime;
            }
            else
            {
                TimeInProcess.Add(entity, endTime - startTime);
            }
        }

        public virtual void ReportRecirculation(IEntity entity, int startTime, int endTime)
        {
            endTime = Math.Min(endTime, EndTime);

            for(int i = startTime; i < endTime; i++)
            {
                ItemsInRecirc[i]++;
            }

            if (TimeInRecirculation.ContainsKey(entity))
            {
                TimeInRecirculation[entity] += endTime - startTime;
                TimesRecirculated[entity]++;
            }
            else
            {
                TimeInRecirculation.Add(entity, endTime - startTime);
                TimesRecirculated.Add(entity, 1);
            }
        }
        public virtual void ReportQueueTime(IEntity entity, int startTime, int endTime)
        {
            endTime = Math.Min(endTime, EndTime);

            if (TimeInQueue.ContainsKey(entity))
            {
                TimeInQueue[entity] += endTime - startTime;
            }
            else
            {
                TimeInQueue.Add(entity, endTime - startTime);
            }
        }
        public virtual void ReportInterarrivalTime(int time, int duration)
        {
            InterarrivalTimes.Add(new Tuple<int, int>(time, duration));
        }
        public virtual void ReportQueueSize(int time, int queueSize)
        {
            if (QueueSizeOverTime.ContainsKey(time))
            {
                QueueSizeOverTime[time] = queueSize;
            }
            else
            {
                QueueSizeOverTime.Add(time, queueSize);
            }
        }





        public Dictionary<ProcessType, Tuple<double, double>> CalcEntityTimeInSystemStats()
        {
            var results = new Dictionary<ProcessType, Tuple<double, double>>();

            foreach(ProcessType p in ArrivalTimes.Keys.Select(x => x.ProcessType).Distinct())
            {
                List<double> timeInSystem = new List<double>();

                foreach (IEntity e in ArrivalTimes.Keys.Where(x => x.ProcessType == p))
                {
                    if (!DisposalTimes.ContainsKey(e))
                    {
                        timeInSystem.Add(EndTime - ArrivalTimes[e]);
                    }else
                    {
                        timeInSystem.Add(DisposalTimes[e] - ArrivalTimes[e]);
                    }

                    
                }

                results.Add(p, new Tuple<double, double>(timeInSystem.Average(), timeInSystem.StandardDeviation()));
            }

            return results;
        }
        public Dictionary<IResource, int> CalcTimeConsumed()
        {
            var results = new Dictionary<IResource, int>();

            foreach(IResource r in ConsumedTime.Keys)
            {
                results.Add(r, ConsumedTime[r]);
            }

            return results;
        }
        public Dictionary<ProcessType, int> CalcNumOut()
        {
            var results = new Dictionary<ProcessType, int>();
            foreach (ProcessType p in DisposalTimes.Keys.Select(x => x.ProcessType).Distinct())
            {
                results.Add(p, DisposalTimes.Keys.Where(x => x.ProcessType == p).Count());
            }

            return results;
        }
        public Dictionary<ProcessType, int> CalcNumIn()
        {
            var results = new Dictionary<ProcessType, int>();
            foreach (ProcessType p in ArrivalTimes.Keys.Select(x => x.ProcessType).Distinct())
            {
                results.Add(p, ArrivalTimes.Keys.Where(x => x.ProcessType == p).Count());
            }

            return results;
        }
        public Dictionary<SimBlock, Tuple<double, double>> CalcProcessTimeStats()
        {
            Dictionary<SimBlock, Tuple<double, double>> results = new Dictionary<SimBlock, Tuple<double, double>>();

            foreach(SimBlock p in ProcessTimes.Keys)
            {
                results.Add(p, new Tuple<double, double>(ProcessTimes[p].Average(), ProcessTimes[p].StandardDeviation()));
            }

            return results;
        }
        public Dictionary<ProcessType, Tuple<double, double>> CalcEntityTimeInProcessStats()
        {
            var results = new Dictionary<ProcessType, Tuple<double, double>>();

            foreach (ProcessType p in TimeInProcess.Keys.Select(x => x.ProcessType).Distinct())
            {
                results.Add(p, new Tuple<double, double>(
                    TimeInProcess.Where(x => x.Key.ProcessType == p).Select(x => x.Value).Average(),
                    TimeInProcess.Where(x => x.Key.ProcessType == p).Select(x => x.Value).StandardDeviation()));
            }
            return results;
        }
        public Dictionary<ProcessType, Tuple<double, double>> CalcRecirculationTimeStats()
        {
            var results = new Dictionary<ProcessType, Tuple<double, double>>();

            foreach (ProcessType p in TimeInRecirculation.Keys.Select(x => x.ProcessType).Distinct())
            {
                results.Add(p, new Tuple<double, double>(
                    TimeInRecirculation.Where(x => x.Key.ProcessType == p).Select(x => x.Value).Average(),
                    TimeInRecirculation.Where(x => x.Key.ProcessType == p).Select(x => x.Value).StandardDeviation()));
            }
            return results;
        }
        public Dictionary<ProcessType, Tuple<double, double>> CalcTimesRecirculatedStats()
        {
            var results = new Dictionary<ProcessType, Tuple<double, double>>();

            foreach (ProcessType p in TimesRecirculated.Keys.Select(x => x.ProcessType).Distinct())
            {
                results.Add(p, new Tuple<double, double>(
                    TimesRecirculated.Where(x => x.Key.ProcessType == p).Select(x => x.Value).Average(),
                    TimesRecirculated.Where(x => x.Key.ProcessType == p).Select(x => x.Value).StandardDeviation()));
            }
            return results;
        }

        public Dictionary<ProcessType, Tuple<double, double>> CalcQueueTimes()
        {
            var results = new Dictionary<ProcessType, Tuple<double, double>>();

            foreach (ProcessType p in TimeInQueue.Keys.Select(x => x.ProcessType).Distinct())
            {
                results.Add(p, new Tuple<double, double>(
                    TimeInQueue.Where(x => x.Key.ProcessType == p).Select(x => x.Value).Average(),
                    TimeInQueue.Where(x => x.Key.ProcessType == p).Select(x => x.Value).StandardDeviation()));
            }

            return results;
        }

        public List<Tuple<int, int>> OutputInterarrivalTimes()
        {
            return InterarrivalTimes;
        }
        public int[] OutputItemsInRecirc()
        {
            return ItemsInRecirc;
        }
        public Dictionary<int, int> OutputQueueSize()
        {
            return QueueSizeOverTime;
        }

    }
}
