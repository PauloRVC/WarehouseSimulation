using Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulationObjects.Results
{
    public class EntityStatistics
    {
        public Statistic TimeInSystem { get; private set; } = new Statistic(new List<double>());
        public Statistic TimeInProcess { get; private set; } = new Statistic(new List<double>());
        public Statistic TimeRecirculating { get; private set; } = new Statistic(new List<double>());
        public Statistic TimesRecirculated { get; private set; } = new Statistic(new List<double>());
        public Statistic TimeInQueue { get; private set; } = new Statistic(new List<double>());
        public Statistic NumberCreated { get; private set; } = new Statistic(new List<double>());
        public Statistic NumberDisposed { get; private set; } = new Statistic(new List<double>());
        private ProcessType EntityType;

        public EntityStatistics(ProcessType entityType)
        {
            EntityType = entityType;
        }
        
        public void AddObservations(ISimResults simResults)
        {
            TimeInSystem.AddObservation(simResults.CalcEntityTimeInSystemStats()[EntityType].Item1);
            TimeInProcess.AddObservation(simResults.CalcEntityTimeInProcessStats()[EntityType].Item1);
            TimeRecirculating.AddObservation(simResults.CalcRecirculationTimeStats()[EntityType].Item1);
            TimesRecirculated.AddObservation(simResults.CalcTimesRecirculatedStats()[EntityType].Item1);
            TimeInQueue.AddObservation(simResults.CalcQueueTimes()[EntityType].Item1);
            NumberCreated.AddObservation(simResults.CalcNumIn()[EntityType]);
            NumberDisposed.AddObservation(simResults.CalcNumOut()[EntityType]);
        }
        
    }
}
