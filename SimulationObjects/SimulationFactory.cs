using Infrastructure;
using Infrastructure.Models;
using SimulationObjects.Distributions;
using SimulationObjects.Resources;
using SimulationObjects.Results;
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
    public class FactoryParams
    {
        public int StartMin { get; set; }
        public int EndMin
        {
            get
            {
                return StartMin + DayLength / 60;
            }
        }
        public int DayLength { get; set; }
        public int ArrivalAnomolyLimit { get; set; } = 600;
        public int RecircAnomolyLimit { get; set; } = 600;
        public int TimeInQueueAnomolyLimit { get; set; } = int.MaxValue;
        public int ProcessTimeAnomolyLimit { get; set; } = int.MaxValue;
        public int QueueSize { get; set; }
        public int NWarmupDays { get; set; }
        public ILogger Logger { get; set; }
        public int InitialNumberInQueue { get; set; } = 0;
    }
    public class DistributionSelectionParameters
    {
        public List<DateTime> SelectedDaysForData { get; set; }
        public List<Tuple<TimeSpan, TimeSpan>> ArrivalDistributionBreakpoints { get; set; }
        public List<Tuple<DateTime, DateTime>> BreakTimes { get; set; }
        public Tuple<TimeSpan, TimeSpan> IntervalForOtherDistributions { get; set; }
        public int CapacityIntervalSize { get; set; }

    }
    public class RequiredDistsWithOperators: RequiredDistributions
    {
        public Dictionary<int, int> OperatorsPerXSchedule { get; set; }

        private RequiredDistsWithOperators()
        {

        }
        public RequiredDistsWithOperators(FactoryParams factoryParams,
                                     DistributionSelectionParameters distParams,
                                     List<Tuple<DateTime, DistributionSelectionParameters>> warmupDays): base(factoryParams,
                                                                                                              distParams,
                                                                                                              warmupDays)
        {
            int nWarmupDays = warmupDays.Count;
            int minsInShift = factoryParams.DayLength / 60;

            //var distBuilder = new NewDistBuilder();

            var data = new WarehouseData();
            

            OperatorsPerXSchedule = new Dictionary<int, int>();

            //Add ppxSchedule for warmup days
            for (int i = 0; i < nWarmupDays; i++)
            {
                var warmupDay = warmupDays[i];

                var warmupDistParams = warmupDay.Item2;

                var opSchedule = data.GetOperatorsPerZMins(warmupDistParams.SelectedDaysForData.First(), warmupDistParams.CapacityIntervalSize);

                factoryParams.Logger.LogPutsPerHour("OperatorSchedule_Mins_" + i + "_" + warmupDistParams.CapacityIntervalSize, opSchedule);

                for (int min = 0; min < minsInShift; min += warmupDistParams.CapacityIntervalSize)
                {
                    int simIntervalStart = min * 60 + factoryParams.DayLength * i;
                    OperatorsPerXSchedule.Add(simIntervalStart, opSchedule[((min + factoryParams.StartMin) / warmupDistParams.CapacityIntervalSize)]);
                }


            }

            //Add ppxSchedule for simulation
            var opsSchedule = data.GetOperatorsPerZMins(distParams.SelectedDaysForData.First(), distParams.CapacityIntervalSize);

            factoryParams.Logger.LogPutsPerHour("OperatorSchedule_Mins_Sim_" + distParams.CapacityIntervalSize, opsSchedule);

            for (int min = 0; min <= minsInShift; min += distParams.CapacityIntervalSize)
            {
                int simIntervalStart = min * 60 + factoryParams.DayLength * nWarmupDays;
                OperatorsPerXSchedule.Add(simIntervalStart, opsSchedule[((min + factoryParams.StartMin) / distParams.CapacityIntervalSize)]);
            }

            factoryParams.Logger.LogPutsPerHour("OperatorSchedule_Final", OperatorsPerXSchedule);
        }
        public List<RequiredDistsWithOperators> CreateNCopiesWithOperators(int N)
        {
            var copies = new List<RequiredDistsWithOperators>();
            for (int i = 1; i <= N; i++)
            {
                copies.Add(this.Copy());
            }
            return copies;
        }
        private RequiredDistsWithOperators Copy()
        {
            var ppxSchedule = new Dictionary<int, int>();
            foreach (KeyValuePair<int, int> p in PPXSchedule)
            {
                ppxSchedule.Add(p.Key, p.Value);
            }

            return new RequiredDistsWithOperators()
            {
                PPXSchedule = ppxSchedule,
                ProcessTimeDists = this.ProcessTimeDists,
                RecircTimeDists = this.RecircTimeDists,
                QueueTimeDists = this.QueueTimeDists,
                DestinationDists = this.DestinationDists,
                ArrivalDists = this.ArrivalDists,
                BreakTimes = this.BreakTimes,
                OperatorsPerXSchedule = this.OperatorsPerXSchedule
            };
        }
        
    }
    public class RequiredDistributions
    {
        public Dictionary<int, IDistribution<int>> ProcessTimeDists { get; set; }
        public Dictionary<int, IDistribution<int>> RecircTimeDists { get; set; }
        public Dictionary<int, IDistribution<int>> QueueTimeDists { get; set; }
        public Dictionary<int, LocationDist> DestinationDists { get; set; }
        public Dictionary<int, IDistribution<int>> ArrivalDists { get; set; }
        public Dictionary<int, int> PPXSchedule { get; set; }
        public List<Tuple<int, int>> BreakTimes { get; set; }

        public List<RequiredDistributions> CreateNCopies(int N)
        {
            var copies = new List<RequiredDistributions>();
            for(int i = 1; i <= N; i++)
            {
                copies.Add(this.Copy());
            }
            return copies;
        }
        private RequiredDistributions Copy()
        {
            var ppxSchedule = new Dictionary<int, int>();
            foreach(KeyValuePair<int, int> p in PPXSchedule)
            {
                ppxSchedule.Add(p.Key, p.Value);
            }

            return new RequiredDistributions()
            {
                PPXSchedule = ppxSchedule,
                ProcessTimeDists = this.ProcessTimeDists,
                RecircTimeDists = this.RecircTimeDists,
                QueueTimeDists = this.QueueTimeDists,
                DestinationDists = this.DestinationDists,
                ArrivalDists = this.ArrivalDists,
                BreakTimes = this.BreakTimes
            };
        }
        protected RequiredDistributions()
        {

        }
        public RequiredDistributions(FactoryParams factoryParams,
                                     DistributionSelectionParameters distParams,
                                     List<Tuple<DateTime, DistributionSelectionParameters>> warmupDays)
        {
            int nWarmupDays = warmupDays.Count;
            int minsInShift = factoryParams.DayLength / 60;

            BreakTimes = new List<Tuple<int, int>>();

            //Add warmup period breaks
            for (int i = 0; i < nWarmupDays; i++)
            {
                var warmupDay = warmupDays[i];

                foreach (var brk in warmupDay.Item2.BreakTimes)
                {
                    if (brk.Item1.TimeOfDay.TotalMinutes > factoryParams.StartMin &
                       brk.Item1.TimeOfDay.TotalMinutes < factoryParams.EndMin)
                    {
                        int t1 = ((int)brk.Item1.TimeOfDay.TotalSeconds - factoryParams.StartMin * 60);
                        int t2 = (int)brk.Item2.TimeOfDay.TotalSeconds - factoryParams.StartMin * 60;
                        t2 = Math.Min(t2, factoryParams.DayLength);

                        t1 += factoryParams.DayLength * i;
                        t2 += factoryParams.DayLength * i;

                        BreakTimes.Add(new Tuple<int, int>(t1, t2));
                    }
                }
            }

            //Add simulation breaks
            foreach (var brk in distParams.BreakTimes)
            {
                if (brk.Item1.TimeOfDay.TotalMinutes > factoryParams.StartMin &
                       brk.Item1.TimeOfDay.TotalMinutes < factoryParams.EndMin)
                {
                    int t1 = ((int)brk.Item1.TimeOfDay.TotalSeconds - factoryParams.StartMin * 60);
                    int t2 = (int)brk.Item2.TimeOfDay.TotalSeconds - factoryParams.StartMin * 60;
                    t2 = Math.Min(t2, factoryParams.DayLength);

                    t1 += factoryParams.DayLength * nWarmupDays;
                    t2 += factoryParams.DayLength * nWarmupDays;

                    BreakTimes.Add(new Tuple<int, int>(t1, t2));
                }
            }
            
            var distBuilder = new NewDistBuilder();

            distBuilder.Logger = factoryParams.Logger;

            PPXSchedule = new Dictionary<int, int>();

            //Add ppxSchedule for warmup days
            for (int i = 0; i < nWarmupDays; i++)
            {
                var warmupDay = warmupDays[i];

                var warmupDistParams = warmupDay.Item2;

                var ppxMinutes = distBuilder.GetPutsPerX(warmupDistParams.SelectedDaysForData.First(), warmupDistParams.CapacityIntervalSize);

                factoryParams.Logger.LogPutsPerHour("PPX_Mins_" + i + "_" + warmupDistParams.CapacityIntervalSize, ppxMinutes);

                for (int min = 0; min < minsInShift; min += warmupDistParams.CapacityIntervalSize)
                {
                    int simIntervalStart = min * 60 + factoryParams.DayLength * i;
                    PPXSchedule.Add(simIntervalStart, ppxMinutes[((min + factoryParams.StartMin) / warmupDistParams.CapacityIntervalSize)]);
                }


            }

            //Add ppxSchedule for simulation
            var ppxMinutesSim = distBuilder.GetPutsPerX(distParams.SelectedDaysForData.First(), distParams.CapacityIntervalSize);

            factoryParams.Logger.LogPutsPerHour("PPX_Mins_Sim_" + distParams.CapacityIntervalSize, ppxMinutesSim);

            for (int min = 0; min <= minsInShift; min += distParams.CapacityIntervalSize)
            {
                int simIntervalStart = min * 60 + factoryParams.DayLength * nWarmupDays;
                PPXSchedule.Add(simIntervalStart, ppxMinutesSim[((min + factoryParams.StartMin) / distParams.CapacityIntervalSize)]);
            }

            factoryParams.Logger.LogPutsPerHour("PPX_Schedule", PPXSchedule);

            ProcessTimeDists = new Dictionary<int, IDistribution<int>>();
            RecircTimeDists = new Dictionary<int, IDistribution<int>>();
            QueueTimeDists = new Dictionary<int, IDistribution<int>>();
            DestinationDists = new Dictionary<int, LocationDist>();
            ArrivalDists = new Dictionary<int, IDistribution<int>>();
            


            for (int i = 0; i < nWarmupDays; i++)
            {
                var warmupDay = new List<DateTime>() { warmupDays[i].Item1 };

                var distInterval = warmupDays[i].Item2.IntervalForOtherDistributions;

                //First distributions are collected in one interval
                var processTimeDist = distBuilder.BuildProcessTimeDist(warmupDay, factoryParams.ProcessTimeAnomolyLimit, distInterval);
                var recircTimeDist = distBuilder.BuildRecircTimeDist(warmupDay, factoryParams.RecircAnomolyLimit, distInterval);
                var queueTimeDist = distBuilder.BuildTimeInQueueDistribution(warmupDay, factoryParams.TimeInQueueAnomolyLimit, distInterval);
                var destinationDist = distBuilder.BuildDestinationDist(warmupDay, distInterval);

                ProcessTimeDists.Add(i * factoryParams.DayLength, processTimeDist);
                RecircTimeDists.Add(i * factoryParams.DayLength, recircTimeDist);
                QueueTimeDists.Add(i * factoryParams.DayLength, queueTimeDist);
                DestinationDists.Add(i * factoryParams.DayLength, destinationDist);

                //Arrival dist collected over several intervals
                var bps = warmupDays[i].Item2.ArrivalDistributionBreakpoints;

                var arrivalDists = distBuilder.BuildArrivalDist(warmupDay, factoryParams.ArrivalAnomolyLimit, bps);
                //var pTimeDists = distBuilder.BuildProcessTimeDists(warmupDay, factoryParams.ProcessTimeAnomolyLimit, bps);

                for (int j = 0; j < arrivalDists.Count; j++)
                {
                    var arrivalDist = arrivalDists[j];
                    //var pTimeDist = pTimeDists[j];

                    int distStart = ((int)bps[j].Item1.TotalMinutes - factoryParams.StartMin) * 60 + factoryParams.DayLength * i;

                    ArrivalDists.Add(distStart, arrivalDist);
                   // ProcessTimeDists.Add(distStart, pTimeDist);
                }
            }

            //Create distributions for sim
            //First distributions are collected in one interval
            var ptDist = distBuilder.
                         BuildProcessTimeDist(distParams.SelectedDaysForData,
                                              factoryParams.ProcessTimeAnomolyLimit,
                                              distParams.IntervalForOtherDistributions);
            var rctDist = distBuilder.
                          BuildRecircTimeDist(distParams.SelectedDaysForData,
                                              factoryParams.RecircAnomolyLimit,
                                              distParams.IntervalForOtherDistributions);
            var qtDist = distBuilder.
                         BuildTimeInQueueDistribution(distParams.SelectedDaysForData,
                                                      factoryParams.TimeInQueueAnomolyLimit,
                                                      distParams.IntervalForOtherDistributions);
            var destDist = distBuilder.
                           BuildDestinationDist(distParams.SelectedDaysForData,
                                                distParams.IntervalForOtherDistributions);

            ProcessTimeDists.Add(factoryParams.DayLength * nWarmupDays, ptDist);
            RecircTimeDists.Add(factoryParams.DayLength * nWarmupDays, rctDist);
            QueueTimeDists.Add(factoryParams.DayLength * nWarmupDays, qtDist);
            DestinationDists.Add(factoryParams.DayLength * nWarmupDays, destDist);

            //Arrival dist collected over several intervals
            var breakPoints = distParams.ArrivalDistributionBreakpoints;

            var arvlDists = distBuilder.BuildArrivalDist(distParams.SelectedDaysForData, factoryParams.ArrivalAnomolyLimit, breakPoints);
            //var pDists = distBuilder.BuildProcessTimeDists(distParams.SelectedDaysForData, factoryParams.ProcessTimeAnomolyLimit, breakPoints);

            for (int i = 0; i < arvlDists.Count; i++)
            {
                int distStart = ((int)breakPoints[i].Item1.TotalMinutes - factoryParams.StartMin) * 60 + factoryParams.DayLength * nWarmupDays;

                ArrivalDists.Add(distStart, arvlDists[i]);
                //ProcessTimeDists.Add(distStart, pDists[i]);
            }


        }
    }
    public static class SimulationFactory
    {
        public static Simulation ConsumeAllSim(FactoryParams factoryParams,
                                                             RequiredDistsWithOperators dists,
                                                             Dictionary<int, Tuple<int, int>> pOfRecirc)
        {
            var results = new SimulationResults(factoryParams.DayLength);

            Simulation simulation = new Simulation(results, factoryParams.DayLength * (factoryParams.NWarmupDays + 1));

            IDestinationBlock disposalBlock = new DisposalBlock(simulation);

            var data = new WarehouseData();

            var P06 = new ConsumeCapacityPutwall(dists.ProcessTimeDists,
                                                                   dists.RecircTimeDists,
                                                                   simulation,
                                                                   disposalBlock,
                                                                   dists.PPXSchedule,
                                                                   results,
                                                                   pOfRecirc,
                                                                   dists.OperatorsPerXSchedule);

            var deQueueEvents = P06.InitializeQueue(factoryParams.InitialNumberInQueue);




            var locationDict = new Dictionary<int, IDestinationBlock>();
            locationDict.Add(data.P06.LocationID, P06);

            var DestinationDists = new Dictionary<int, IDistribution<IDestinationBlock>>();
            foreach (KeyValuePair<int, LocationDist> p in dists.DestinationDists)
            {
                DestinationDists.Add(p.Key, new LocationWrapper(p.Value, locationDict, simulation, disposalBlock));
            }


            var ArrivalBlock = new ArrivalBlockII(dists.ArrivalDists, DestinationDists, P06, dists.BreakTimes, simulation);

            var firstArrival = ArrivalBlock.GetNextEvent();

            simulation.Initialize(ArrivalBlock, firstArrival, deQueueEvents);

            return simulation;
        }
        public static Simulation SimWithConditionalPofRecircAndNoMaxQueueAndOperators(FactoryParams factoryParams,
                                                             RequiredDistsWithOperators dists,
                                                             Dictionary<int, Tuple<int, int>> pOfRecirc)
        {
            var results = new SimulationResults(factoryParams.DayLength);

            Simulation simulation = new Simulation(results, factoryParams.DayLength * (factoryParams.NWarmupDays + 1));

            IDestinationBlock disposalBlock = new DisposalBlock(simulation);

            var data = new WarehouseData();

            var P06 = new PutwallWithConditionalProbAndOperators(dists.ProcessTimeDists,
                                                                   dists.RecircTimeDists,
                                                                   simulation,
                                                                   disposalBlock,
                                                                   dists.PPXSchedule,
                                                                   results,
                                                                   pOfRecirc,
                                                                   dists.OperatorsPerXSchedule);

            var deQueueEvents = P06.InitializeQueue(factoryParams.InitialNumberInQueue);




            var locationDict = new Dictionary<int, IDestinationBlock>();
            locationDict.Add(data.P06.LocationID, P06);

            var DestinationDists = new Dictionary<int, IDistribution<IDestinationBlock>>();
            foreach (KeyValuePair<int, LocationDist> p in dists.DestinationDists)
            {
                DestinationDists.Add(p.Key, new LocationWrapper(p.Value, locationDict, simulation, disposalBlock));
            }


            var ArrivalBlock = new ArrivalBlockII(dists.ArrivalDists, DestinationDists, P06, dists.BreakTimes, simulation);

            var firstArrival = ArrivalBlock.GetNextEvent();

            simulation.Initialize(ArrivalBlock, firstArrival, deQueueEvents);

            return simulation;
        }
        public static Simulation SimWithConditionalPofRecircAndNoMaxQueue(FactoryParams factoryParams,
                                                             RequiredDistributions dists,
                                                             Dictionary<int, Tuple<int, int>> pOfRecirc)
        {
            var results = new SimulationResults(factoryParams.DayLength);

            Simulation simulation = new Simulation(results, factoryParams.DayLength * (factoryParams.NWarmupDays + 1));

            IDestinationBlock disposalBlock = new DisposalBlock(simulation);

            var data = new WarehouseData();

            var P06 = new PutwallWithNoMaxQSize(dists.ProcessTimeDists,
                                                                   dists.RecircTimeDists,
                                                                   simulation,
                                                                   disposalBlock,
                                                                   dists.PPXSchedule,
                                                                   results,
                                                                   pOfRecirc);

            var deQueueEvents = P06.InitializeQueue(factoryParams.InitialNumberInQueue);




            var locationDict = new Dictionary<int, IDestinationBlock>();
            locationDict.Add(data.P06.LocationID, P06);

            var DestinationDists = new Dictionary<int, IDistribution<IDestinationBlock>>();
            foreach (KeyValuePair<int, LocationDist> p in dists.DestinationDists)
            {
                DestinationDists.Add(p.Key, new LocationWrapper(p.Value, locationDict, simulation, disposalBlock));
            }


            var ArrivalBlock = new ArrivalBlockII(dists.ArrivalDists, DestinationDists, P06, dists.BreakTimes, simulation);

            var firstArrival = ArrivalBlock.GetNextEvent();

            simulation.Initialize(ArrivalBlock, firstArrival, deQueueEvents);

            return simulation;
        }
        public static Simulation SimWithConditionalPofRecirc(FactoryParams factoryParams,
                                                             RequiredDistributions dists,
                                                             Dictionary<int, Tuple<int, int>> pOfRecirc)
        {
            var results = new SimulationResults(factoryParams.DayLength);

            Simulation simulation = new Simulation(results, factoryParams.DayLength * (factoryParams.NWarmupDays + 1));

            IDestinationBlock disposalBlock = new DisposalBlock(simulation);

            var data = new WarehouseData();

            var P06 = new PutwallWithConditionalPofRecirc(dists.ProcessTimeDists,
                                                                   dists.RecircTimeDists,
                                                                   simulation,
                                                                   disposalBlock,
                                                                   factoryParams.QueueSize,
                                                                   dists.PPXSchedule,
                                                                   results, 
                                                                   pOfRecirc);

            var deQueueEvents = P06.InitializeQueue(factoryParams.InitialNumberInQueue);


            

            var locationDict = new Dictionary<int, IDestinationBlock>();
            locationDict.Add(data.P06.LocationID, P06);

            var DestinationDists = new Dictionary<int, IDistribution<IDestinationBlock>>();
            foreach (KeyValuePair<int, LocationDist> p in dists.DestinationDists)
            {
                DestinationDists.Add(p.Key, new LocationWrapper(p.Value, locationDict, simulation, disposalBlock));
            }


            var ArrivalBlock = new ArrivalBlockII(dists.ArrivalDists, DestinationDists, P06, dists.BreakTimes, simulation);

            var firstArrival = ArrivalBlock.GetNextEvent();

            simulation.Initialize(ArrivalBlock, firstArrival, deQueueEvents);

            return simulation;
        }
        public static Simulation SimWithFullQueueAndNoDoubleQueue(FactoryParams factoryParams,
                                                          RequiredDistributions dists)
        {
            var results = new SimulationResults(factoryParams.DayLength);

            Simulation simulation = new Simulation(results, factoryParams.DayLength * (factoryParams.NWarmupDays + 1));

            IDestinationBlock disposalBlock = new DisposalBlock(simulation);

            PutwallWithIntervalDistsII P06 = new PutwallWithIntervalDistsII(dists.ProcessTimeDists,
                                                                   dists.RecircTimeDists,
                                                                   simulation,
                                                                   disposalBlock,
                                                                   factoryParams.QueueSize,
                                                                   dists.PPXSchedule,
                                                                   results);

            var deQueueEvents = P06.InitializeQueue(factoryParams.InitialNumberInQueue);


            var data = new WarehouseData();

            var locationDict = new Dictionary<int, IDestinationBlock>();
            locationDict.Add(data.P06.LocationID, P06);

            var DestinationDists = new Dictionary<int, IDistribution<IDestinationBlock>>();
            foreach (KeyValuePair<int, LocationDist> p in dists.DestinationDists)
            {
                DestinationDists.Add(p.Key, new LocationWrapper(p.Value, locationDict, simulation, disposalBlock));
            }


            var ArrivalBlock = new ArrivalBlockII(dists.ArrivalDists, DestinationDists, P06, dists.BreakTimes, simulation);

            var firstArrival = ArrivalBlock.GetNextEvent();

            simulation.Initialize(ArrivalBlock, firstArrival, deQueueEvents);

            return simulation;
        }
        public static Simulation SimWithIntervalQueueSize(FactoryParams factoryParams,
                                                          DistributionSelectionParameters distParams,
                                                          RequiredDistributions dists,
                                                          int QueueIntervalLength)
        {
            var db = new WarehouseData();

            var qSizeOverTime = db.GetQueueSizeOverTime(distParams.SelectedDaysForData[0]);

            var qIntervalAvg = new int[3600 * 24 / QueueIntervalLength];

            var output = new Dictionary<int, int>();
            var shiftedOutput = new Dictionary<int, int>();

            for (int i = QueueIntervalLength; i <= 3600 * 24; i += QueueIntervalLength)
            {
                var obs = new List<int>();
                for(int j = i - QueueIntervalLength; j < i; j++)
                {
                    obs.Add(qSizeOverTime[j]);
                }
                qIntervalAvg[i/QueueIntervalLength - 1] = (int)Math.Round(obs.Average());

                output.Add(i, qIntervalAvg[i / QueueIntervalLength - 1]);
            }
            
            factoryParams.Logger.LogPutsPerHour("QueueSizeAvgOverTime", output);

            if (factoryParams.StartMin * 60 % QueueIntervalLength == 0)
            {
                for (int i = 0; i <= factoryParams.DayLength; i += QueueIntervalLength)
                {
                    shiftedOutput.Add(i, output[i + factoryParams.StartMin*60]);
                }
            }
            else
            {
                throw new Exception();
            }

            factoryParams.Logger.LogPutsPerHour("QueueSizeAvgOverTime_Sim", shiftedOutput);

            var results = new ResultsWithWarmup(factoryParams.DayLength * factoryParams.NWarmupDays, factoryParams.DayLength * (factoryParams.NWarmupDays + 1));

            Simulation simulation = new Simulation(results, factoryParams.DayLength * (factoryParams.NWarmupDays + 1));

            IDestinationBlock disposalBlock = new DisposalBlock(simulation);

            IDestinationBlock P06 = new PutwallWithIntervalQueue(dists.ProcessTimeDists,
                                                                         dists.RecircTimeDists,
                                                                         dists.QueueTimeDists,
                                                                         simulation,
                                                                         disposalBlock,
                                                                         factoryParams.QueueSize,
                                                                         dists.PPXSchedule,
                                                                         results,
                                                                         shiftedOutput);
            var data = new WarehouseData();

            var locationDict = new Dictionary<int, IDestinationBlock>();
            locationDict.Add(data.P06.LocationID, P06);

            var DestinationDists = new Dictionary<int, IDistribution<IDestinationBlock>>();
            foreach (KeyValuePair<int, LocationDist> p in dists.DestinationDists)
            {
                DestinationDists.Add(p.Key, new LocationWrapper(p.Value, locationDict, simulation, disposalBlock));
            }


            var ArrivalBlock = new ArrivalBlockII(dists.ArrivalDists, DestinationDists, P06, dists.BreakTimes, simulation);

            var firstArrival = ArrivalBlock.GetNextEvent();

            simulation.Initialize(ArrivalBlock, firstArrival);

            return simulation;

        }
        public static Simulation SimWithSpecificWarmupDay(FactoryParams factoryParams,
                                                          RequiredDistributions dists)
        {
            var results = new ResultsWithWarmup(factoryParams.DayLength * factoryParams.NWarmupDays, factoryParams.DayLength * (factoryParams.NWarmupDays + 1));

            Simulation simulation = new Simulation(results, factoryParams.DayLength * (factoryParams.NWarmupDays + 1));

            IDestinationBlock disposalBlock = new DisposalBlock(simulation);
            
            IDestinationBlock P06 = new PutwallWithIntervalDistributions(dists.ProcessTimeDists,
                                                                         dists.RecircTimeDists,
                                                                         dists.QueueTimeDists,
                                                                         simulation,
                                                                         disposalBlock,
                                                                         factoryParams.QueueSize,
                                                                         dists.PPXSchedule,
                                                                         results);
            var data = new WarehouseData();

            var locationDict = new Dictionary<int, IDestinationBlock>();
            locationDict.Add(data.P06.LocationID, P06);

            var DestinationDists = new Dictionary<int, IDistribution<IDestinationBlock>>();
            foreach(KeyValuePair<int, LocationDist> p in dists.DestinationDists)
            {
                DestinationDists.Add(p.Key, new LocationWrapper(p.Value, locationDict, simulation, disposalBlock));
            }


            var ArrivalBlock = new ArrivalBlockII(dists.ArrivalDists, DestinationDists, P06, dists.BreakTimes, simulation);

            var firstArrival = ArrivalBlock.GetNextEvent();

            simulation.Initialize(ArrivalBlock, firstArrival);

            return simulation;
        }
        public static Simulation SimWithSpecificWarmupDay(FactoryParams factoryParams, 
                                                          DistributionSelectionParameters distParams, 
                                                          List<Tuple<DateTime, DistributionSelectionParameters>> warmupDays)
        {
            int nWarmupDays = warmupDays.Count;
            int minsInShift = factoryParams.DayLength / 60;
            var results = new ResultsWithWarmup(factoryParams.DayLength * nWarmupDays, factoryParams.DayLength * (nWarmupDays + 1));

            var breakTimes = new List<Tuple<int, int>>();

            //Add warmup period breaks
            for(int i = 0; i < nWarmupDays; i++)
            {
                var warmupDay = warmupDays[i];
                
                foreach(var brk in warmupDay.Item2.BreakTimes)
                {
                    if(brk.Item1.TimeOfDay.TotalMinutes > factoryParams.StartMin &
                       brk.Item1.TimeOfDay.TotalMinutes < factoryParams.EndMin)
                    {
                        int t1 = ((int)brk.Item1.TimeOfDay.TotalSeconds - factoryParams.StartMin * 60);
                        int t2 = (int)brk.Item2.TimeOfDay.TotalSeconds - factoryParams.StartMin * 60;
                        t2 = Math.Min(t2, factoryParams.DayLength);

                        t1 += factoryParams.DayLength * i;
                        t2 += factoryParams.DayLength * i;

                        breakTimes.Add(new Tuple<int, int>(t1, t2));
                    }
                }
            }

            //Add simulation breaks
            foreach(var brk in distParams.BreakTimes)
            {
                if (brk.Item1.TimeOfDay.TotalMinutes > factoryParams.StartMin &
                       brk.Item1.TimeOfDay.TotalMinutes < factoryParams.EndMin)
                {
                    int t1 = ((int)brk.Item1.TimeOfDay.TotalSeconds - factoryParams.StartMin * 60);
                    int t2 = (int)brk.Item2.TimeOfDay.TotalSeconds - factoryParams.StartMin * 60;
                    t2 = Math.Min(t2, factoryParams.DayLength);

                    t1 += factoryParams.DayLength * nWarmupDays;
                    t2 += factoryParams.DayLength * nWarmupDays;

                    breakTimes.Add(new Tuple<int, int>(t1, t2));
                }
            }

            Simulation simulation = new Simulation(results, factoryParams.DayLength * (nWarmupDays + 1));
            

            var data = new WarehouseData();
            

            RealDistributionBuilder distBuilder = new RealDistributionBuilder(simulation);

            distBuilder.Logger = factoryParams.Logger;

            var ppxSchedule = new Dictionary<int, int>();

            //Add ppxSchedule for warmup days
            for(int i = 0; i < nWarmupDays; i++)
            {
                var warmupDay = warmupDays[i];

                var warmupDistParams = warmupDay.Item2;

                var ppxMinutes = distBuilder.GetPutsPerX(warmupDistParams.SelectedDaysForData.First(), warmupDistParams.CapacityIntervalSize);

                factoryParams.Logger.LogPutsPerHour("PPX_Mins_" + i + "_" + warmupDistParams.CapacityIntervalSize, ppxMinutes);

                for (int min = 0; min < minsInShift; min += warmupDistParams.CapacityIntervalSize)
                {
                    int simIntervalStart = min * 60 + factoryParams.DayLength * i;
                    ppxSchedule.Add(simIntervalStart, ppxMinutes[((min + factoryParams.StartMin) / warmupDistParams.CapacityIntervalSize)]);
                }

                
            }

            //Add ppxSchedule for simulation
            var ppxMinutesSim = distBuilder.GetPutsPerX(distParams.SelectedDaysForData.First(), distParams.CapacityIntervalSize);

            factoryParams.Logger.LogPutsPerHour("PPX_Mins_Sim_" + distParams.CapacityIntervalSize, ppxMinutesSim);

            for (int min = 0; min <= minsInShift; min += distParams.CapacityIntervalSize)
            {
                int simIntervalStart = min * 60 + factoryParams.DayLength * nWarmupDays;
                ppxSchedule.Add(simIntervalStart, ppxMinutesSim[((min + factoryParams.StartMin) / distParams.CapacityIntervalSize)]);
            }

            factoryParams.Logger.LogPutsPerHour("PPX_Schedule", ppxSchedule);

            var ProcessTimeDists = new Dictionary<int, IDistribution<int>>();
            var RecircTimeDists = new Dictionary<int, IDistribution<int>>();
            var QueueTimeDists = new Dictionary<int, IDistribution<int>>();
            var DestinationDists = new Dictionary<int, IDistribution<IDestinationBlock>>();
            var ArrivalDists = new Dictionary<int, IDistribution<int>>();

            IDestinationBlock disposalBlock = new DisposalBlock(simulation);

            var locationDict = new Dictionary<int, IDestinationBlock>();

            IDestinationBlock P06 = new PutwallWithIntervalDistributions(ProcessTimeDists,
                                                                         RecircTimeDists,
                                                                         QueueTimeDists,
                                                                         simulation,
                                                                         disposalBlock,
                                                                         factoryParams.QueueSize,
                                                                         ppxSchedule,
                                                                         results);
            Location P06L = data.P06;

            locationDict.Add(P06L.LocationID, P06);
            
            //Create distributions for warmup days
            for(int i = 0; i < nWarmupDays; i++)
            {
                var warmupDay = new List<DateTime>() { warmupDays[i].Item1 };

                var distInterval = warmupDays[i].Item2.IntervalForOtherDistributions;

                //First distributions are collected in one interval
                var processTimeDist = distBuilder.BuildProcessTimeDist(warmupDay, factoryParams.ProcessTimeAnomolyLimit, distInterval);
                var recircTimeDist = distBuilder.BuildRecircTimeDist(warmupDay, factoryParams.RecircAnomolyLimit, distInterval);
                var queueTimeDist = distBuilder.BuildTimeInQueueDistribution(warmupDay, factoryParams.TimeInQueueAnomolyLimit, distInterval);
                var destinationDist = distBuilder.BuildDestinationDist(warmupDay, locationDict, disposalBlock, distInterval);

                ProcessTimeDists.Add(i * factoryParams.DayLength, processTimeDist);
                RecircTimeDists.Add(i * factoryParams.DayLength, recircTimeDist);
                QueueTimeDists.Add(i * factoryParams.DayLength, queueTimeDist);
                DestinationDists.Add(i * factoryParams.DayLength, destinationDist);

                //Arrival dist collected over several intervals
                var bps = warmupDays[i].Item2.ArrivalDistributionBreakpoints;

                var arrivalDists = distBuilder.BuildArrivalDist(warmupDay, factoryParams.ArrivalAnomolyLimit, bps);
                
                for(int j = 0; j < arrivalDists.Count; j++)
                {
                    var arrivalDist = arrivalDists[j];

                    int distStart = ((int)bps[j].Item1.TotalMinutes - factoryParams.StartMin) * 60 + factoryParams.DayLength * i;

                    ArrivalDists.Add(distStart, arrivalDist);
                }
            }

            //Create distributions for sim
            //First distributions are collected in one interval
            var ptDist = distBuilder.
                         BuildProcessTimeDist(distParams.SelectedDaysForData, 
                                              factoryParams.ProcessTimeAnomolyLimit, 
                                              distParams.IntervalForOtherDistributions);
            var rctDist = distBuilder.
                          BuildRecircTimeDist(distParams.SelectedDaysForData, 
                                              factoryParams.RecircAnomolyLimit, 
                                              distParams.IntervalForOtherDistributions);
            var qtDist = distBuilder.
                         BuildTimeInQueueDistribution(distParams.SelectedDaysForData, 
                                                      factoryParams.TimeInQueueAnomolyLimit, 
                                                      distParams.IntervalForOtherDistributions);
            var destDist = distBuilder.
                           BuildDestinationDist(distParams.SelectedDaysForData, 
                                                locationDict, 
                                                disposalBlock, 
                                                distParams.IntervalForOtherDistributions);

            ProcessTimeDists.Add(factoryParams.DayLength * nWarmupDays, ptDist);
            RecircTimeDists.Add(factoryParams.DayLength * nWarmupDays, rctDist);
            QueueTimeDists.Add(factoryParams.DayLength * nWarmupDays, qtDist);
            DestinationDists.Add(factoryParams.DayLength * nWarmupDays, destDist);

            //Arrival dist collected over several intervals
            var breakPoints = distParams.ArrivalDistributionBreakpoints;

            var arvlDists = distBuilder.BuildArrivalDist(distParams.SelectedDaysForData, factoryParams.ArrivalAnomolyLimit, breakPoints);

            for(int i = 0; i < arvlDists.Count; i++)
            {
                int distStart = ((int)breakPoints[i].Item1.TotalMinutes - factoryParams.StartMin) * 60 + factoryParams.DayLength *nWarmupDays;

                ArrivalDists.Add(distStart, arvlDists[i]);
            }
            
            var ArrivalBlock = new ArrivalBlockII(ArrivalDists, DestinationDists, P06, breakTimes, simulation);

            var firstArrival = ArrivalBlock.GetNextEvent();

            simulation.Initialize(ArrivalBlock, firstArrival);

            return simulation;
        }
        public static Simulation SimWithFullQ(List<DateTime> selectedDays,
                                                            int x,
                                                            int startMin,
                                                            int endTime,
                                                            int queueSize,
                                                            ILogger logger,
                                                            List<Tuple<TimeSpan, TimeSpan>> intervals,
                                                            int arrivalAnomolyLimit,
                                                            List<Tuple<DateTime, DateTime>> breaks,
                                                            Tuple<TimeSpan, TimeSpan> interval,
                                                            int numInQueue)
        {
            int minsInShift = endTime / 60;
            var results = new SimulationResults(endTime);

            var breakTimes = new List<Tuple<int, int>>();

            foreach (var brk in breaks)
            {
                if (brk.Item1.TimeOfDay.TotalMinutes > startMin)
                {
                    
                        int t1 = ((int)brk.Item1.TimeOfDay.TotalSeconds - startMin * 60);
                        int t2 = (int)brk.Item2.TimeOfDay.TotalSeconds - startMin * 60;

                        breakTimes.Add(new Tuple<int, int>(t1, t2));
                }
            }

            Simulation simulation = new Simulation(results, endTime);

            var Operators = new List<Processor>();

            var data = new WarehouseData();


            Operators.Add(new Processor());

            RealDistributionBuilder distBuilder = new RealDistributionBuilder(simulation);

            distBuilder.Logger = logger;

            var ppxMinutes = distBuilder.GetPutsPerX(selectedDays.First(), x);

            var ppxSchedule = new Dictionary<int, int>();

            for (int newX = 0; newX <= minsInShift; newX += x)
            {
                int newStartSecond = newX * 60;
                ppxSchedule.Add(newStartSecond, ppxMinutes[(((newX % minsInShift) + startMin) / x)]);
            }

            logger.LogPutsPerHour("PPX_Schedule_" + x, ppxSchedule);

            IDestinationBlock disposalBlock = new DisposalBlock(simulation);


            PutwallWithPPHScheduleAndTimeInQ P06 = new PutwallWithPPHScheduleAndTimeInQ(queueSize,
                                                               ppxSchedule,
                                                               Operators,
                                                               distBuilder.BuildProcessTimeDist(selectedDays),
                                                               distBuilder.BuildRecircTimeDist(selectedDays, 300, interval),
                                                               simulation,
                                                                disposalBlock,
                                                                distBuilder.BuildTimeInQueueDistribution(selectedDays, int.MaxValue, interval),
                                                                results);
            Location P06L = data.P06;

            var deQueueEvents = P06.InitializeQueue(numInQueue);

            Dictionary<int, IDestinationBlock> locationDict = new Dictionary<int, IDestinationBlock>()
            {
                { P06L.LocationID, P06 }
            };


            var arrivalDists = distBuilder.BuildArrivalDist(selectedDays, arrivalAnomolyLimit, intervals);

            var newArrivalDists = new Dictionary<Tuple<int, int>, IDistribution<int>>();

            for (int i = 0; i < intervals.Count; i++)
            {
                var t = new Tuple<int, int>((int)(intervals[i].Item1.TotalMinutes - startMin) * 60, (int)(intervals[i].Item2.TotalMinutes - startMin) * 60);
                newArrivalDists.Add(t, arrivalDists[i]);
            }



            var ArrivalBlock = new InterarrivalBlockWithBreaks(newArrivalDists, distBuilder.BuildDestinationDist(selectedDays, locationDict, disposalBlock), simulation, breakTimes);

            var firstArrival = ArrivalBlock.GetNextEvent();

            simulation.Initialize(ArrivalBlock, firstArrival, deQueueEvents);

            return simulation;
        }
        public static Simulation WIPSimWithBreaksAndQTime(List<DateTime> selectedDays,
                                                            int x,
                                                            int startMin,
                                                            int endTime,
                                                            int queueSize,
                                                            int warmUpDays,
                                                            ILogger logger,
                                                            List<Tuple<TimeSpan, TimeSpan>> intervals,
                                                            int arrivalAnomolyLimit,
                                                            List<Tuple<DateTime, DateTime>> breaks,
                                                            Tuple<TimeSpan, TimeSpan> interval)
        {
            int minsInShift = endTime / 60;
            var results = new ResultsWithWarmup(endTime * warmUpDays, endTime * (warmUpDays + 1));

            var breakTimes = new List<Tuple<int, int>>();

            foreach (var brk in breaks)
            {
                if (brk.Item1.TimeOfDay.TotalMinutes > startMin)
                {
                    for (int i = 0; i <= warmUpDays; i++)
                    {
                        int t1 = ((int)brk.Item1.TimeOfDay.TotalSeconds - startMin * 60) + (endTime * i);
                        int t2 = (int)brk.Item2.TimeOfDay.TotalSeconds - startMin * 60 + (endTime * i);

                        breakTimes.Add(new Tuple<int, int>(t1, t2));
                    }
                }
            }

            Simulation simulation = new Simulation(results, endTime * (warmUpDays + 1));

            var Operators = new List<Processor>();

            var data = new WarehouseData();


            Operators.Add(new Processor());

            RealDistributionBuilder distBuilder = new RealDistributionBuilder(simulation);

            distBuilder.Logger = logger;

            var ppxMinutes = distBuilder.GetPutsPerX(selectedDays.First(), x);

            var ppxSchedule = new Dictionary<int, int>();



            //for (int min = startMin; min < startMin + minsInShift * (warmUpDays + 1); min += x)
            //{
            //    int startSecond = (min - startMin) * 60;
            //    ppxSchedule.Add(startSecond, ppxMinutes[((min - startMin) % (minsInShift + startMin) + startMin) / x]);
            //}

            for (int newX = 0; newX <= minsInShift * (warmUpDays + 1); newX += x)
            {
                int newStartSecond = newX * 60;
                ppxSchedule.Add(newStartSecond, ppxMinutes[(((newX % minsInShift) + startMin) / x)]);
            }

            logger.LogPutsPerHour("PPX_Schedule_" + x, ppxSchedule);

            IDestinationBlock disposalBlock = new DisposalBlock(simulation);


            IDestinationBlock P06 = new PutwallWithPPHScheduleAndTimeInQ(queueSize,
                                                               ppxSchedule,
                                                               Operators,
                                                               distBuilder.BuildProcessTimeDist(selectedDays),
                                                               distBuilder.BuildRecircTimeDist(selectedDays, 300, interval),
                                                               simulation,
                                                                disposalBlock,
                                                                distBuilder.BuildTimeInQueueDistribution(selectedDays, int.MaxValue, interval),
                                                                results);
            Location P06L = data.P06;

            Dictionary<int, IDestinationBlock> locationDict = new Dictionary<int, IDestinationBlock>()
            {
                { P06L.LocationID, P06 }
            };


            var arrivalDists = distBuilder.BuildArrivalDist(selectedDays, arrivalAnomolyLimit, intervals);

            var newArrivalDists = new Dictionary<Tuple<int, int>, IDistribution<int>>();
            for (int j = 0; j < warmUpDays + 1; j++)
            {
                for (int i = 0; i < intervals.Count; i++)
                {

                    var t = new Tuple<int, int>((int)(intervals[i].Item1.TotalMinutes - startMin) * 60 + 60 * minsInShift * j,
                        (int)(intervals[i].Item2.TotalMinutes - startMin) * 60 + 60 * minsInShift * j);
                    newArrivalDists.Add(t, arrivalDists[i]);
                }

            }



            var ArrivalBlock = new InterarrivalBlockWithBreaks(newArrivalDists, distBuilder.BuildDestinationDist(selectedDays, locationDict, disposalBlock), simulation, breakTimes);

            var firstArrival = ArrivalBlock.GetNextEvent();

            simulation.Initialize(ArrivalBlock, firstArrival);

            return simulation;
        }
        public static Simulation WIPSimWithBreaksAndQTime(List<DateTime> selectedDays,
                                                            int x,
                                                            int startMin,
                                                            int endTime,
                                                            int queueSize,
                                                            int warmUpDays,
                                                            ILogger logger,
                                                            List<Tuple<TimeSpan, TimeSpan>> intervals,
                                                            int arrivalAnomolyLimit,
                                                            List<Tuple<DateTime, DateTime>> breaks)
        {
            int minsInShift = endTime / 60;
            var results = new ResultsWithWarmup(endTime * warmUpDays, endTime * (warmUpDays + 1));

            var breakTimes = new List<Tuple<int, int>>();

            foreach (var brk in breaks)
            {
                if (brk.Item1.TimeOfDay.TotalMinutes > startMin)
                {
                    for (int i = 0; i <= warmUpDays; i++)
                    {
                        int t1 = ((int)brk.Item1.TimeOfDay.TotalSeconds - startMin * 60) + (endTime * i);
                        int t2 = (int)brk.Item2.TimeOfDay.TotalSeconds - startMin * 60 + (endTime * i);

                        breakTimes.Add(new Tuple<int, int>(t1, t2));
                    }
                }
            }

            Simulation simulation = new Simulation(results, endTime * (warmUpDays + 1));

            var Operators = new List<Processor>();

            var data = new WarehouseData();

            
            Operators.Add(new Processor());

            RealDistributionBuilder distBuilder = new RealDistributionBuilder(simulation);

            distBuilder.Logger = logger;

            var ppxMinutes = distBuilder.GetPutsPerX(selectedDays.First(), x);

            var ppxSchedule = new Dictionary<int, int>();



            //for (int min = startMin; min < startMin + minsInShift * (warmUpDays + 1); min += x)
            //{
            //    int startSecond = (min - startMin) * 60;
            //    ppxSchedule.Add(startSecond, ppxMinutes[((min - startMin) % (minsInShift + startMin) + startMin) / x]);
            //}

            for (int newX = 0; newX <= minsInShift * (warmUpDays + 1); newX += x)
            {
                int newStartSecond = newX * 60;
                ppxSchedule.Add(newStartSecond, ppxMinutes[(((newX % minsInShift) + startMin) / x)]);
            }

            logger.LogPutsPerHour("PPX_Schedule_" + x, ppxSchedule);

            IDestinationBlock disposalBlock = new DisposalBlock(simulation);


            IDestinationBlock P06 = new PutwallWithPPHScheduleAndTimeInQ(queueSize,
                                                               ppxSchedule,
                                                               Operators,
                                                               distBuilder.BuildProcessTimeDist(selectedDays),
                                                               distBuilder.BuildRecircTimeDist(selectedDays, 300),
                                                               simulation,
                                                                disposalBlock,
                                                                distBuilder.BuildTimeInQueueDistribution(selectedDays, 40000),
                                                                results);
            Location P06L = data.P06;

            Dictionary<int, IDestinationBlock> locationDict = new Dictionary<int, IDestinationBlock>()
            {
                { P06L.LocationID, P06 }
            };


            var arrivalDists = distBuilder.BuildArrivalDist(selectedDays, arrivalAnomolyLimit, intervals);

            var newArrivalDists = new Dictionary<Tuple<int, int>, IDistribution<int>>();
            for (int j = 0; j < warmUpDays + 1; j++)
            {
                for (int i = 0; i < intervals.Count; i++)
                {

                    var t = new Tuple<int, int>((int)(intervals[i].Item1.TotalMinutes - startMin) * 60 + 60 * minsInShift * j,
                        (int)(intervals[i].Item2.TotalMinutes - startMin) * 60 + 60 * minsInShift * j);
                    newArrivalDists.Add(t, arrivalDists[i]);
                }

            }



            var ArrivalBlock = new InterarrivalBlockWithBreaks(newArrivalDists, distBuilder.BuildDestinationDist(selectedDays, locationDict, disposalBlock), simulation, breakTimes);

            var firstArrival = ArrivalBlock.GetNextEvent();

            simulation.Initialize(ArrivalBlock, firstArrival);

            return simulation;
        }
        public static Simulation WIPSimWithBreaksAndOperators(List<DateTime> selectedDays,
                                                    int x,
                                                    int startMin,
                                                    int endTime,
                                                    int queueSize,
                                                    int warmUpDays,
                                                    ILogger logger,
                                                    List<Tuple<TimeSpan, TimeSpan>> intervals,
                                                    int arrivalAnomolyLimit,
                                                    List<Tuple<DateTime, DateTime>> breaks,
                                                    int nOperators)
        {
            int minsInShift = endTime / 60;
            var results = new ResultsWithWarmup(endTime * warmUpDays, endTime * (warmUpDays + 1));

            var breakTimes = new List<Tuple<int, int>>();

            foreach (var brk in breaks)
            {
                if (brk.Item1.TimeOfDay.TotalMinutes > startMin)
                {
                    for (int i = 0; i <= warmUpDays; i++)
                    {
                        int t1 = ((int)brk.Item1.TimeOfDay.TotalSeconds - startMin * 60) + (endTime * i);
                        int t2 = (int)brk.Item2.TimeOfDay.TotalSeconds - startMin * 60 + (endTime * i);

                        breakTimes.Add(new Tuple<int, int>(t1, t2));
                    }
                }
            }

            Simulation simulation = new Simulation(results, endTime * (warmUpDays + 1));

            var Operators = new List<Processor>();

            var data = new WarehouseData();

            for(int i = 0; i < nOperators; i++)
            {
                Operators.Add(new Processor());
            }

            RealDistributionBuilder distBuilder = new RealDistributionBuilder(simulation);

            distBuilder.Logger = logger;

            var ppxMinutes = distBuilder.GetPutsPerX(selectedDays.First(), x);

            var ppxSchedule = new Dictionary<int, int>();



            //for (int min = startMin; min < startMin + minsInShift * (warmUpDays + 1); min += x)
            //{
            //    int startSecond = (min - startMin) * 60;
            //    ppxSchedule.Add(startSecond, ppxMinutes[((min - startMin) % (minsInShift + startMin) + startMin) / x]);
            //}

            for (int newX = 0; newX <= minsInShift * (warmUpDays + 1); newX += x)
            {
                int newStartSecond = newX * 60;
                ppxSchedule.Add(newStartSecond, ppxMinutes[(((newX % minsInShift) + startMin) / x)]);
            }

            logger.LogPutsPerHour("PPX_Schedule_" + x, ppxSchedule);

            IDestinationBlock disposalBlock = new DisposalBlock(simulation);


            IDestinationBlock P06 = new PutwallWithPPHSchedule (queueSize,
                                                               ppxSchedule,
                                                               Operators,
                                                               distBuilder.BuildProcessTimeDist(selectedDays),
                                                               distBuilder.BuildRecircTimeDist(selectedDays, 300),
                                                               simulation,
                                                                disposalBlock);
            Location P06L = data.P06;

            Dictionary<int, IDestinationBlock> locationDict = new Dictionary<int, IDestinationBlock>()
            {
                { P06L.LocationID, P06 }
            };


            var arrivalDists = distBuilder.BuildArrivalDist(selectedDays, arrivalAnomolyLimit, intervals);

            var newArrivalDists = new Dictionary<Tuple<int, int>, IDistribution<int>>();
            for (int j = 0; j < warmUpDays + 1; j++)
            {
                for (int i = 0; i < intervals.Count; i++)
                {

                    var t = new Tuple<int, int>((int)(intervals[i].Item1.TotalMinutes - startMin) * 60 + 60 * minsInShift * j,
                        (int)(intervals[i].Item2.TotalMinutes - startMin) * 60 + 60 * minsInShift * j);
                    newArrivalDists.Add(t, arrivalDists[i]);
                }

            }



            var ArrivalBlock = new InterarrivalBlockWithBreaks(newArrivalDists, distBuilder.BuildDestinationDist(selectedDays, locationDict, disposalBlock), simulation, breakTimes);

            var firstArrival = ArrivalBlock.GetNextEvent();

            simulation.Initialize(ArrivalBlock, firstArrival);

            return simulation;
        }
        public static Simulation WIPSimWithBreaks(List<DateTime> selectedDays,
                                                    int x,
                                                    int startMin,
                                                    int endTime,
                                                    int queueSize,
                                                    int warmUpDays,
                                                    ILogger logger,
                                                    List<Tuple<TimeSpan, TimeSpan>> intervals,
                                                    int arrivalAnomolyLimit,
                                                    List<Tuple<DateTime, DateTime>> breaks)
        {
            int minsInShift = endTime / 60;
            var results = new ResultsWithWarmup(endTime * warmUpDays, endTime * (warmUpDays + 1));

            var breakTimes = new List<Tuple<int, int>>();

            foreach (var brk in breaks)
            {
                if (brk.Item1.TimeOfDay.TotalMinutes > startMin)
                {
                    for(int i = 0; i <= warmUpDays; i++)
                    {
                        int t1 = ((int)brk.Item1.TimeOfDay.TotalSeconds - startMin * 60) + (endTime*i);
                        int t2 = (int)brk.Item2.TimeOfDay.TotalSeconds - startMin * 60 + (endTime * i);

                        breakTimes.Add(new Tuple<int, int>(t1, t2));
                    }
                }
            }

            Simulation simulation = new Simulation(results, endTime * (warmUpDays + 1));

            var Operators = new List<Processor>();

            var data = new WarehouseData();

            Operators.Add(new Processor());

            RealDistributionBuilder distBuilder = new RealDistributionBuilder(simulation);

            distBuilder.Logger = logger;

            var ppxMinutes = distBuilder.GetPutsPerX(selectedDays.First(), x);

            var ppxSchedule = new Dictionary<int, int>();



            //for (int min = startMin; min < startMin + minsInShift * (warmUpDays + 1); min += x)
            //{
            //    int startSecond = (min - startMin) * 60;
            //    ppxSchedule.Add(startSecond, ppxMinutes[((min - startMin) % (minsInShift + startMin) + startMin) / x]);
            //}

            for (int newX = 0; newX <= minsInShift * (warmUpDays + 1); newX += x)
            {
                int newStartSecond = newX * 60;
                ppxSchedule.Add(newStartSecond, ppxMinutes[(((newX % minsInShift) + startMin) / x)]);
            }

            logger.LogPutsPerHour("PPX_Schedule_" + x, ppxSchedule);

            IDestinationBlock disposalBlock = new DisposalBlock(simulation);


            IDestinationBlock P06 = new PutwallWithPPHSchedule(queueSize,
                                                               ppxSchedule,
                                                               Operators,
                                                               distBuilder.BuildProcessTimeDist(selectedDays),
                                                               distBuilder.BuildRecircTimeDist(selectedDays, 300),
                                                               simulation,
                                                                disposalBlock);
            Location P06L = data.P06;

            Dictionary<int, IDestinationBlock> locationDict = new Dictionary<int, IDestinationBlock>()
            {
                { P06L.LocationID, P06 }
            };


            var arrivalDists = distBuilder.BuildArrivalDist(selectedDays, arrivalAnomolyLimit, intervals);

            var newArrivalDists = new Dictionary<Tuple<int, int>, IDistribution<int>>();
            for (int j = 0; j < warmUpDays + 1; j++)
            {
                for (int i = 0; i < intervals.Count; i++)
                {

                    var t = new Tuple<int, int>((int)(intervals[i].Item1.TotalMinutes - startMin) * 60 + 60 * minsInShift * j,
                        (int)(intervals[i].Item2.TotalMinutes - startMin) * 60 + 60 * minsInShift * j);
                    newArrivalDists.Add(t, arrivalDists[i]);
                }

            }



            var ArrivalBlock = new InterarrivalBlockWithBreaks(newArrivalDists, distBuilder.BuildDestinationDist(selectedDays, locationDict, disposalBlock), simulation, breakTimes);

            var firstArrival = ArrivalBlock.GetNextEvent();

            simulation.Initialize(ArrivalBlock, firstArrival);

            return simulation;
        }
        public static Simulation WIPSim(List<DateTime> selectedDays,
                                                    int x,
                                                    int startMin,
                                                    int endTime,
                                                    int queueSize,
                                                    int warmUpDays,
                                                    ILogger logger,
                                                    List<Tuple<TimeSpan, TimeSpan>> intervals,
                                                    int arrivalAnomolyLimit)
        {
            int minsInShift = endTime / 60;
            var results = new ResultsWithWarmup(endTime * warmUpDays, endTime*(warmUpDays+1));

            Simulation simulation = new Simulation(results, endTime * (warmUpDays + 1));
            
            var Operators = new List<Processor>();

            var data = new WarehouseData();

            Operators.Add(new Processor());

            RealDistributionBuilder distBuilder = new RealDistributionBuilder(simulation);

            distBuilder.Logger = logger;

            var ppxMinutes = distBuilder.GetPutsPerX(selectedDays.First(), x);

            var ppxSchedule = new Dictionary<int, int>();

            

            //for (int min = startMin; min < startMin + minsInShift * (warmUpDays + 1); min += x)
            //{
            //    int startSecond = (min - startMin) * 60;
            //    ppxSchedule.Add(startSecond, ppxMinutes[((min - startMin) % (minsInShift + startMin) + startMin) / x]);
            //}

            for(int newX = 0; newX <= minsInShift * (warmUpDays + 1); newX += x)
            {
                int newStartSecond = newX * 60;
                ppxSchedule.Add(newStartSecond, ppxMinutes[(((newX % minsInShift)+startMin) / x) ]);
            }

            logger.LogPutsPerHour("PPX_Schedule_" + x, ppxSchedule);

            IDestinationBlock disposalBlock = new DisposalBlock(simulation);


            IDestinationBlock P06 = new PutwallWithPPHSchedule(queueSize,
                                                               ppxSchedule,
                                                               Operators,
                                                               distBuilder.BuildProcessTimeDist(selectedDays),
                                                               distBuilder.BuildRecircTimeDist(selectedDays, 300),
                                                               simulation,
                                                                disposalBlock);
            Location P06L = data.P06;

            Dictionary<int, IDestinationBlock> locationDict = new Dictionary<int, IDestinationBlock>()
            {
                { P06L.LocationID, P06 }
            };


            var arrivalDists = distBuilder.BuildArrivalDist(selectedDays, arrivalAnomolyLimit, intervals);

            var newArrivalDists = new Dictionary<Tuple<int, int>, IDistribution<int>>();
            for (int j = 0; j < warmUpDays + 1; j++)
            {
                for (int i = 0; i < intervals.Count; i++)
                {
                
                    var t = new Tuple<int, int>((int)(intervals[i].Item1.TotalMinutes - startMin) * 60 + 60*minsInShift*j, 
                        (int)(intervals[i].Item2.TotalMinutes - startMin) * 60 + 60*minsInShift * j) ;
                    newArrivalDists.Add(t, arrivalDists[i]);
                }
                
            }



            var ArrivalBlock = new InterarrivalBlock2(newArrivalDists, distBuilder.BuildDestinationDist(selectedDays, locationDict, disposalBlock), simulation);

            var firstArrival = ArrivalBlock.GetNextEvent();

            simulation.Initialize(ArrivalBlock, firstArrival);

            return simulation;
        }
        public static Simulation SimWithPPXScheduleAndInterval2(List<DateTime> selectedDays,
                                                    int x,
                                                    int startMin,
                                                    int endTime,
                                                    int queueSize,
                                                    ILogger logger,
                                                    List<Tuple<TimeSpan, TimeSpan>> intervals,
                                                    int arrivalAnomolyLimit)
        {
            Simulation simulation = new Simulation(endTime);



            var Operators = new List<Processor>();

            var data = new WarehouseData();

            Operators.Add(new Processor());

            RealDistributionBuilder distBuilder = new RealDistributionBuilder(simulation);

            distBuilder.Logger = logger;

            var ppxMinutes = distBuilder.GetPutsPerX(selectedDays.First(), x);

            var ppxSchedule = new Dictionary<int, int>();

            //for (int min = startMin; min < startMin + 1440; min += x)
            //{
            //    int startSecond = (min - startMin) * 60;
            //    ppxSchedule.Add(startSecond, ppxMinutes[(min % 1440) / x]);
            //}
            int minsInShift = endTime / 60;

            for (int newX = 0; newX <= minsInShift; newX += x)
            {
                int newStartSecond = newX * 60;
                ppxSchedule.Add(newStartSecond, ppxMinutes[(((newX % minsInShift) + startMin) / x)]);
            }

            logger.LogPutsPerHour("PPX_Schedule_" + x, ppxSchedule);

            IDestinationBlock disposalBlock = new DisposalBlock(simulation);


            IDestinationBlock P06 = new PutwallWithPPHSchedule(queueSize,
                                                               ppxSchedule,
                                                               Operators,
                                                               distBuilder.BuildProcessTimeDist(selectedDays),
                                                               distBuilder.BuildRecircTimeDist(selectedDays, 300),
                                                               simulation,
                                                                disposalBlock);
            Location P06L = data.P06;

            Dictionary<int, IDestinationBlock> locationDict = new Dictionary<int, IDestinationBlock>()
            {
                { P06L.LocationID, P06 }
            };


            var arrivalDists = distBuilder.BuildArrivalDist(selectedDays, arrivalAnomolyLimit, intervals);

            var newArrivalDists = new Dictionary<Tuple<int,int>, IDistribution<int>>();

            for(int i = 0; i < intervals.Count; i++)
            {
                var t = new Tuple<int, int>((int)(intervals[i].Item1.TotalMinutes - startMin) * 60, (int)(intervals[i].Item2.TotalMinutes - startMin) * 60);
                newArrivalDists.Add(t, arrivalDists[i]);
            }



            var ArrivalBlock = new InterarrivalBlock2(newArrivalDists, distBuilder.BuildDestinationDist(selectedDays, locationDict, disposalBlock), simulation);

            var firstArrival = ArrivalBlock.GetNextEvent();

            simulation.Initialize(ArrivalBlock, firstArrival);

            return simulation;
        }
        public static Simulation SimWithPPXScheduleAndInterval(List<DateTime> selectedDays,
                                                    int x,
                                                    int startMin,
                                                    int endTime,
                                                    int queueSize,
                                                    ILogger logger)
        {
            Simulation simulation = new Simulation(endTime);



            var Operators = new List<Processor>();

            var data = new WarehouseData();

            Operators.Add(new Processor());

            RealDistributionBuilder distBuilder = new RealDistributionBuilder(simulation);

            distBuilder.Logger = logger;

            var ppxMinutes = distBuilder.GetPutsPerX(selectedDays.First(), x);

            var ppxSchedule = new Dictionary<int, int>();

            for (int min = startMin; min < startMin + 1440; min += x)
            {
                int startSecond = (min - startMin) * 60;
                ppxSchedule.Add(startSecond, ppxMinutes[(min % 1440) / x]);
            }

            logger.LogPutsPerHour("PPX_Schedule_" + x, ppxSchedule);

            IDestinationBlock disposalBlock = new DisposalBlock(simulation);


            IDestinationBlock P06 = new PutwallWithPPHSchedule(queueSize,
                                                               ppxSchedule,
                                                               Operators,
                                                               distBuilder.BuildProcessTimeDist(selectedDays),
                                                               distBuilder.BuildRecircTimeDist(selectedDays, 300),
                                                               simulation,
                                                                disposalBlock);
            Location P06L = data.P06;

            Dictionary<int, IDestinationBlock> locationDict = new Dictionary<int, IDestinationBlock>()
            {
                { P06L.LocationID, P06 }
            };


            Dictionary<DateTime, int> breakpoints = new Dictionary<DateTime, int>()
            {
                { new DateTime(1,1,1,7,0,0), 0 },
                { new DateTime(1,1,1,21,0,0), 3600 },
                { new DateTime(1,1,1,22,0,0), 54000 },
            };
            

            var ArrivalBlock = new IntervalArrivalBlock(distBuilder.BuildIntervalDists(breakpoints, selectedDays, 600), distBuilder.BuildDestinationDist(selectedDays, locationDict, disposalBlock), simulation);

            var firstArrival = ArrivalBlock.GetNextEvent();

            simulation.Initialize(ArrivalBlock, firstArrival);

            return simulation;
        }
        public static Simulation SimWithPPXSchedule(List<DateTime> selectedDays,
                                                    int x,
                                                    int startMin,
                                                    int endTime,
                                                    int queueSize,
                                                    ILogger logger)
        {
            Simulation simulation = new Simulation(endTime);



            var Operators = new List<Processor>();

            var data = new WarehouseData();

            Operators.Add(new Processor());

            RealDistributionBuilder distBuilder = new RealDistributionBuilder(simulation);

            distBuilder.Logger = logger;

            var ppxMinutes = distBuilder.GetPutsPerX(selectedDays.First(), x);

            var ppxSchedule = new Dictionary<int, int>();

            for (int min = startMin; min < startMin + 1440; min+=x)
            {
                int startSecond = (min - startMin) * 60;
                ppxSchedule.Add(startSecond, ppxMinutes[(min % 1440)/x]);
            }

            logger.LogPutsPerHour("PPX_Schedule_" + x, ppxSchedule);

            IDestinationBlock disposalBlock = new DisposalBlock(simulation);


            IDestinationBlock P06 = new PutwallWithPPHSchedule(queueSize,
                                                               ppxSchedule,
                                                               Operators,
                                                               distBuilder.BuildProcessTimeDist(selectedDays),
                                                               distBuilder.BuildRecircTimeDist(selectedDays, 300),
                                                               simulation,
                                                                disposalBlock);
            Location P06L = data.P06;

            Dictionary<int, IDestinationBlock> locationDict = new Dictionary<int, IDestinationBlock>()
            {
                { P06L.LocationID, P06 }
            };

            var ArrivalBlock = new ArrivalBlock(distBuilder.BuildArrivalDist(selectedDays, 300), distBuilder.BuildDestinationDist(selectedDays, locationDict, disposalBlock), simulation);

            var firstArrival = ArrivalBlock.GetNextEvent();

            simulation.Initialize(ArrivalBlock, firstArrival);

            return simulation;
        }
        public static Simulation SimWithPPHSchedule(List<DateTime> selectedDays, 
                                                    int startHour,
                                                    int endTime, 
                                                    int queueSize,
                                                    ILogger logger)
        {
            Simulation simulation = new Simulation(endTime);

            

            var Operators = new List<Processor>();

            var data = new WarehouseData();
            
            Operators.Add(new Processor());

            RealDistributionBuilder distBuilder = new RealDistributionBuilder(simulation);

            distBuilder.Logger = logger;

            var pphHourly = distBuilder.GetPutsPerHour(selectedDays.First());

            var pphSchedule = new Dictionary<int, int>();

            for(int hour=startHour;hour<startHour + 24; hour++)
            {
                int startSecond = (hour - startHour) * 3600;
                pphSchedule.Add(startSecond, pphHourly[hour % 24]);
            }

            logger.LogPutsPerHour("PPH_Schedule", pphSchedule);

            IDestinationBlock disposalBlock = new DisposalBlock(simulation);


            IDestinationBlock P06 = new PutwallWithPPHSchedule(queueSize,
                                                               pphSchedule,
                                                               Operators,
                                                               distBuilder.BuildProcessTimeDist(selectedDays),
                                                               distBuilder.BuildRecircTimeDist(selectedDays, 300),
                                                               simulation,
                                                                disposalBlock);
            Location P06L = data.P06;

            Dictionary<int, IDestinationBlock> locationDict = new Dictionary<int, IDestinationBlock>()
            {
                { P06L.LocationID, P06 }
            };

            var ArrivalBlock = new ArrivalBlock(distBuilder.BuildArrivalDist(selectedDays, 300), distBuilder.BuildDestinationDist(selectedDays, locationDict, disposalBlock), simulation);

            var firstArrival = ArrivalBlock.GetNextEvent();

            simulation.Initialize(ArrivalBlock, firstArrival);

            return simulation;
        }
        public static Simulation SimWithOperatorSchedule(List<DateTime> selectedDays, int endTime, Dictionary<int, int> operatorSchedule, ILogger logger)
        {
            Simulation simulation = new Simulation(endTime);

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
                                                                    distBuilder.BuildProcessTimeDist(selectedDays), 
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

            simulation.Initialize(ArrivalBlock, firstArrival);

            return simulation;
        }
        public static Simulation DefaultSimulation(List<DateTime> selectedDays, int endTime, int nProcessors, ILogger logger)
        {
            Simulation simulation = new Simulation(endTime);

            var Operators = new List<Processor>();

            var data = new WarehouseData();

            for(int i = 1; i <= nProcessors;i++)
            {
                Operators.Add(new Processor());
            }

            IDistributionBuilder distBuilder = new RealDistributionBuilder(simulation);

            distBuilder.Logger = logger;


            IDestinationBlock disposalBlock = new DisposalBlock(simulation);


            IDestinationBlock P06 = new Putwall(Operators, distBuilder.BuildProcessTimeDist(selectedDays), distBuilder.BuildRecircTimeDist(selectedDays), simulation, disposalBlock);
            Location P06L = data.P06;

            Dictionary<int, IDestinationBlock> locationDict = new Dictionary<int, IDestinationBlock>()
            {
                { P06L.LocationID, P06 }
            };
            
            var ArrivalBlock = new ArrivalBlock(distBuilder.BuildArrivalDist(selectedDays), distBuilder.BuildDestinationDist(selectedDays, locationDict, disposalBlock), simulation);

            var firstArrival = ArrivalBlock.GetNextEvent();

            simulation.Initialize(ArrivalBlock, firstArrival);

            return simulation;
        }
        public static Simulation FakeSimulation(List<DateTime> selectedDays)
        {
            int endTime = 100000;
            Simulation simulation = new Simulation(endTime);

            
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

            IDestinationBlock P06 = new Putwall(Operators, distBuilder.BuildProcessTimeDist(selectedDays), distBuilder.BuildRecircTimeDist(selectedDays), simulation, disposalBlock);


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

            simulation.Initialize(ArrivalBlock, firstArrival);

            return simulation;
        }
    }
}
