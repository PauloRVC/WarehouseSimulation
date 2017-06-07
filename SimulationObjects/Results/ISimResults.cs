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
    public interface ISimResults
    {
        void ReportArrival(IEntity entity, int arrivalTime);
        void ReportDisposal(IEntity entity, int disposalTime);
        void ReportProcessRealization(IEntity entity, int startTime, int endTime, IEnumerable<IResource> consumedResources, SimBlock process);
        void ReportRecirculation(IEntity entity, int startTime, int endTime);

        Dictionary<ProcessType, Tuple<double, double>> CalcEntityTimeInSystemStats();
        Dictionary<IResource, int> CalcTimeConsumed();
        Dictionary<ProcessType, int> CalcThroughput();
        Dictionary<SimBlock, Tuple<double, double>> CalcProcessTimeStats();
        Dictionary<ProcessType, Tuple<double, double>> CalcEntityTimeInProcessStats();
        Dictionary<ProcessType, Tuple<double, double>> CalcRecirculationTimeStats();
        Dictionary<ProcessType, Tuple<double, double>> CalcTimesRecirculatedStats();
    }
}
