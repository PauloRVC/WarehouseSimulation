using Infrastructure;
using Infrastructure.Models;
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
        
        public static Simulation DefaultSimulation(List<DateTime> selectedDays, int endTime, int nProcessors)
        {
            var Operators = new List<Processor>();

            var data = new WarehouseData();

            for(int i = 1; i <= nProcessors;i++)
            {
                Operators.Add(new Processor());
            }

            IDistributionBuilder distBuilder = new RealDistributionBuilder();

            distBuilder.Logger = new VerboseLogger(@"C:\Users\p2decarv\Desktop\SimLog");


            IProcessBlock disposalBlock = new DisposalBlock();

            Simulation simulation = new Simulation();

            IProcessBlock P06 = new Putwall(Operators, distBuilder.BuildProcessTimeDist(selectedDays, Process.Default), distBuilder.BuildRecircTimeDist(selectedDays), simulation, disposalBlock);

            Dictionary<Location, IProcessBlock> locationDict = new Dictionary<Location, IProcessBlock>()
            {
                { data.P06, P06 }
            };
            
            var ArrivalBlock = new ArrivalBlock(distBuilder.BuildArrivalDist(selectedDays), distBuilder.BuildDestinationDist(selectedDays, locationDict), simulation);

            var firstArrival = ArrivalBlock.GetNextEvent();

            simulation.Initialize(ArrivalBlock, locationDict.Values.ToList(), endTime, firstArrival);

            return simulation;
        }
        public static Simulation FakeSimulation(List<DateTime> selectedDays)
        {
            int endTime = 100000;
            IDistributionBuilder distBuilder = new FakeDataBuilder();
            FakeDataBuilder destDistBuilder = new FakeDataBuilder();

            IProcessBlock disposalBlock = new DisposalBlock();

            Simulation simulation = new Simulation();

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

            IProcessBlock P06 = new Putwall(Operators, distBuilder.BuildProcessTimeDist(selectedDays, Process.Default), distBuilder.BuildRecircTimeDist(selectedDays), simulation, disposalBlock);


            List<Tuple<double, IProcessBlock>> DestinationData = new List<Tuple<double, IProcessBlock>>()
            {
                new Tuple<double, IProcessBlock>(0.1, new NonPutwallLane()),
                new Tuple<double, IProcessBlock>(0.1, new NonPutwallLane()),
                new Tuple<double, IProcessBlock>(0.1, new NonPutwallLane()),
                new Tuple<double, IProcessBlock>(0.1, new NonPutwallLane()),
                new Tuple<double, IProcessBlock>(0.6, P06),
            };

            destDistBuilder.FakeDestData = DestinationData;

            var ArrivalBlock = new ArrivalBlock(destDistBuilder.BuildArrivalDist(selectedDays), destDistBuilder.BuildDestinationDist(selectedDays, null), simulation);

            List<IProcessBlock> putwallLanes = DestinationData.Select(x => x.Item2).ToList();
            putwallLanes.Add(P06);

            var firstArrival = ArrivalBlock.GetNextEvent();

            simulation.Initialize(ArrivalBlock, putwallLanes, endTime, firstArrival);

            return simulation;
        }
    }
}
