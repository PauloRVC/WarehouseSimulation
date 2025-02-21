﻿using Infrastructure;
using SimulationObjects.Entities;
using SimulationObjects.Resources;
using SimulationObjects.SimBlocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulationObjects.Results
{
    public class ResultsWithWarmup : SimulationResults
    {
        private int CollectionStart;

        

        public ResultsWithWarmup(int collectionStart, int endTime): base(endTime)
        {
            CollectionStart = collectionStart;
        }
        public override void ReportArrival(IEntity entity, int arrivalTime)
        {
            if (arrivalTime >= CollectionStart)
                base.ReportArrival(entity, arrivalTime);
        }
        public override void ReportDisposal(IEntity entity, int disposalTime)
        {
            if (disposalTime >= CollectionStart
                )
                //& base.ArrivalTimes.Keys.Contains(entity))
                base.ReportDisposal(entity, disposalTime);
        }
        public override void ReportProcessRealization(IEntity entity,
                                             int startTime,
                                             int endTime,
                                             IEnumerable<IResource> consumedResources,
                                             SimBlock process)
        {
            if (startTime >= CollectionStart)
                base.ReportProcessRealization(entity, startTime, endTime, consumedResources, process);
        }
        public override void ReportRecirculation(IEntity entity, int startTime, int endTime)
        {
            if (startTime >= CollectionStart)
                base.ReportRecirculation(entity, startTime, endTime);
        }
        public override void ReportQueueTime(IEntity entity, int startTime, int endTime)
        {
            if (startTime >= CollectionStart)
                base.ReportQueueTime(entity, startTime, endTime);
        }
        public override void ReportInterarrivalTime(int time, int duration)
        {
            if(time >= CollectionStart)
                base.ReportInterarrivalTime(time, duration);
        }
        public override void ReportQueueSize(int time, int queueSize)
        {
            if (time >= CollectionStart)
                base.ReportQueueSize(time, queueSize);
        }
        public override void ReportTimedQueueSize(int time, int queueSize)
        {
            if (time >= CollectionStart)
                base.ReportTimedQueueSize(time, queueSize);
        }

        public Dictionary<ProcessType, int> CalcOfDayOut()
        {
            var results = new Dictionary<ProcessType, int>();

            foreach (ProcessType p in DisposalTimes.Keys.Select(x => x.ProcessType).Distinct())
            {
                results.Add(p, DisposalTimes.Keys.Where(x => x.ProcessType == p & ArrivalTimes.ContainsKey(x)).Count());
            }

            return results;
        }
    }
}
