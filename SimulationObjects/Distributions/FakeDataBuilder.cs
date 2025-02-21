﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infrastructure.Models;
using Infrastructure;
using SimulationObjects.SimBlocks;

namespace SimulationObjects.Distributions
{
    class FakeDataBuilder : IDistributionBuilder
    {
        public List<Tuple<double, IDestinationBlock>> FakeDestData { get; set; }

        public ILogger Logger { get; set; } = new NullLogger();

        public IDistribution<int> BuildArrivalDist(List<DateTime> selectedDays)
        {
            var FakeIntData = new List<Tuple<double, int>>()
            {
                new Tuple<double, int>(0.25,1),
                new Tuple<double, int>(0.25,2),
                new Tuple<double, int>(0.3, 5),
                new Tuple<double, int>(.2,7)
            };

            return new EmpiricalDist(FakeIntData);
        }
        
        public IDistribution<IDestinationBlock> BuildDestinationDist(List<DateTime> selectedDays, Dictionary<int, IDestinationBlock> processBlocks, IDestinationBlock nextDestination)
        {
            return new DestinationDist(FakeDestData);
        }

        public IDistribution<int> BuildProcessTimeDist(List<DateTime> selectedDays)
        {
            var FakeIntData = new List<Tuple<double, int>>()
            {
                new Tuple<double, int>(0.25,1),
                new Tuple<double, int>(0.25,2),
                new Tuple<double, int>(0.3, 5),
                new Tuple<double, int>(.2,7)
            };
            return new EmpiricalDist(FakeIntData);
        }
        public IDistribution<int> BuildRecircTimeDist(List<DateTime> selectedDays)
        {
            //Faking recirc time with constant 30s
            return new EmpiricalDist(new List<Tuple<double, int>>() { new Tuple<double, int>(1, 30) });
        }
    }
}
