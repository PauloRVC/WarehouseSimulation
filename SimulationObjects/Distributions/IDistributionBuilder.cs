using Infrastructure;
using Infrastructure.Models;
using SimulationObjects.SimBlocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulationObjects.Distributions
{
    public interface IDistributionBuilder
    {
        ILogger Logger { get; set; }
        IDistribution<int> BuildProcessTimeDist(List<DateTime> selectedDays);
        IDistribution<int> BuildArrivalDist(List<DateTime> selectedDays);
        IDistribution<int> BuildRecircTimeDist(List<DateTime> selectedDays);
        IDistribution<IDestinationBlock> BuildDestinationDist(List<DateTime> selectedDays, Dictionary<int, IDestinationBlock> processBlocks, IDestinationBlock nextDestination);
    }
}
