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
        
        
        public static Simulation DefaultSimulation(List<DateTime> selectedDays)
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

            IProcessBlock P06 = new Putwall(Operators, distBuilder.BuildProcessTimeDist(selectedDays, Process.Default), distBuilder.BuildProcessTimeDist(selectedDays, Process.Default), simulation, disposalBlock);


            List<Tuple<double, IProcessBlock>> DestinationData = new List<Tuple<double, IProcessBlock>>()
            {
                new Tuple<double, IProcessBlock>(0.1, new NonPutwallLane()),
                new Tuple<double, IProcessBlock>(0.1, new NonPutwallLane()),
                new Tuple<double, IProcessBlock>(0.1, new NonPutwallLane()),
                new Tuple<double, IProcessBlock>(0.1, new NonPutwallLane()),
                new Tuple<double, IProcessBlock>(0.6, P06),
            };

            destDistBuilder.FakeDestData = DestinationData;

            var ArrivalBlock = new ArrivalBlock(destDistBuilder.BuildArrivalDist(selectedDays), destDistBuilder.BuildDestinationDist(selectedDays), simulation);

            List<IProcessBlock> putwallLanes = DestinationData.Select(x => x.Item2).ToList();
            putwallLanes.Add(P06);

            var firstArrival = ArrivalBlock.GetNextEvent();

            simulation.Initialize(ArrivalBlock, putwallLanes, endTime, firstArrival);

            return simulation;
        }
    }
}
