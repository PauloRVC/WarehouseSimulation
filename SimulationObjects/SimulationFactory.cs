using Infrastructure;
using Infrastructure.Models;
using SimulationObjects.Distributions;
using SimulationObjects.Resources;
using SimulationObjects.SimBlocks;
using SimulationObjects.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulationObjects
{
    public enum Process
    {
        Default
    }
    public static class SimulationFactory
    {
        public static Simulation SimWithOperatorSchedule(List<DateTime> selectedDays, int endTime, Dictionary<int, int> operatorSchedule, ILogger logger)
        {
            Simulation simulation = new Simulation();

            var Operators = new List<Processor>();

            var data = new WarehouseData();

            for (int i = 1; i <= operatorSchedule.Values.Max(); i++)
            {
                Operators.Add(new Processor());
            }

            IDistributionBuilder distBuilder = new RealDistributionBuilder(simulation);

            distBuilder.Logger = logger;


            IDestinationBlock disposalBlock = new DisposalBlock(simulation);


            IDestinationBlock P06 = new PutwallWithOperatorSchedule(operatorSchedule, 
                                                                    Operators, 
                                                                    distBuilder.BuildProcessTimeDist(selectedDays, Process.Default), 
                                                                    distBuilder.BuildRecircTimeDist(selectedDays), 
                                                                    simulation, 
                                                                    disposalBlock);
            Location P06L = data.P06;

            Dictionary<int, IDestinationBlock> locationDict = new Dictionary<int, IDestinationBlock>()
            {
                { P06L.LocationID, P06 }
            };

            var ArrivalBlock = new ArrivalBlock(distBuilder.BuildArrivalDist(selectedDays), distBuilder.BuildDestinationDist(selectedDays, locationDict, disposalBlock), simulation);

            var firstArrival = ArrivalBlock.GetNextEvent();

            simulation.Initialize(ArrivalBlock, endTime, firstArrival);

            return simulation;
        }
        public static Simulation DefaultSimulation(List<DateTime> selectedDays, int endTime, int nProcessors, ILogger logger)
        {
            Simulation simulation = new Simulation();

            var Operators = new List<Processor>();

            var data = new WarehouseData();

            for(int i = 1; i <= nProcessors;i++)
            {
                Operators.Add(new Processor());
            }

            IDistributionBuilder distBuilder = new RealDistributionBuilder(simulation);

            distBuilder.Logger = logger;


            IDestinationBlock disposalBlock = new DisposalBlock(simulation);


            IDestinationBlock P06 = new Putwall(Operators, distBuilder.BuildProcessTimeDist(selectedDays, Process.Default), distBuilder.BuildRecircTimeDist(selectedDays), simulation, disposalBlock);
            Location P06L = data.P06;

            Dictionary<int, IDestinationBlock> locationDict = new Dictionary<int, IDestinationBlock>()
            {
                { P06L.LocationID, P06 }
            };
            
            var ArrivalBlock = new ArrivalBlock(distBuilder.BuildArrivalDist(selectedDays), distBuilder.BuildDestinationDist(selectedDays, locationDict, disposalBlock), simulation);

            var firstArrival = ArrivalBlock.GetNextEvent();

            simulation.Initialize(ArrivalBlock, endTime, firstArrival);

            return simulation;
        }
        public static Simulation FakeSimulation(List<DateTime> selectedDays)
        {
            Simulation simulation = new Simulation();

            int endTime = 100000;
            IDistributionBuilder distBuilder = new FakeDataBuilder();
            FakeDataBuilder destDistBuilder = new FakeDataBuilder();

            IDestinationBlock disposalBlock = new DisposalBlock(simulation);
            

            var Operators = new List<Processor>()
            {
                new Processor(),
                new Processor(),
                new Processor(),
                new Processor(),
                new Processor(),
                new Processor(),
                new Processor(),
                new Processor()
            };

            IDestinationBlock P06 = new Putwall(Operators, distBuilder.BuildProcessTimeDist(selectedDays, Process.Default), distBuilder.BuildRecircTimeDist(selectedDays), simulation, disposalBlock);


            List<Tuple<double, IDestinationBlock>> DestinationData = new List<Tuple<double, IDestinationBlock>>()
            {
                new Tuple<double, IDestinationBlock>(0.1, new NonPutwallLane(simulation, disposalBlock)),
                new Tuple<double, IDestinationBlock>(0.1, new NonPutwallLane(simulation, disposalBlock)),
                new Tuple<double, IDestinationBlock>(0.1, new NonPutwallLane(simulation, disposalBlock)),
                new Tuple<double, IDestinationBlock>(0.1, new NonPutwallLane(simulation, disposalBlock)),
                new Tuple<double, IDestinationBlock>(0.6, P06),
            };

            destDistBuilder.FakeDestData = DestinationData;

            var ArrivalBlock = new ArrivalBlock(destDistBuilder.BuildArrivalDist(selectedDays), destDistBuilder.BuildDestinationDist(selectedDays, null, disposalBlock), simulation);

            List<IDestinationBlock> putwallLanes = DestinationData.Select(x => x.Item2).ToList();
            putwallLanes.Add(P06);

            var firstArrival = ArrivalBlock.GetNextEvent();

            simulation.Initialize(ArrivalBlock, endTime, firstArrival);

            return simulation;
        }
    }
}
