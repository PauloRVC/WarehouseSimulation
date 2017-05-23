using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulationObjects
{
    public interface IDistributionBuilder
    {
        IDistribution<int> BuildProcessTimeDist(List<DateTime> selectedDays, Process process);
        IDistribution<int> BuildArrivalDist(List<DateTime> selectedDays);
        IDistribution<IProcessBlock> BuildDestinationDist(List<DateTime> selectedDays);
    }
}
