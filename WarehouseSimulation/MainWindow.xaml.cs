using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SimulationObjects;
using Infrastructure;
using SimulationObjects.Utils;

namespace WarehouseSimulation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            //RunDefaultSim();

            RunNov10Sim();
            
        }
        private void RunNov10Sim()
        {
            var Data = new WarehouseData();

            var availability = new List<DateTime>()
            {
                new DateTime(2015,11,10)
            };

            var operatorSchedule = new Dictionary<int, int>()
            {
                {0,5},
            {3600,6},
            {7200,6},
            {10800,6},
            {14400,7},
            {18000,8},
            {21600,8},
            {25200,8},
            {28800,9},
            {32400,7},
            {36000,4},
            {39600,5},
            {43200,5},
            {46800,5},
            {50400,5},
            {54000,6},
            {57600,6},
            {61200,7}

            };
            var logger = new VerboseLogger(@"C:\Users\Daniel\Desktop\SimLog");

            for (int i = 1; i <= 20; i++)
            {
                var sim = SimulationFactory.DefaultSimulation(availability, 57600, 6, logger);

                sim.Run();

                logger.LogResults(sim.Results, "Results_" + i, 57600);
            }
        }
        private void RunDefaultSim()
        {
            var Data = new WarehouseData();

            var availability = Data.GetOverallAvailability();

            var logger = new VerboseLogger(@"C:\Users\Daniel\Desktop\SimLog");

            for (int i = 1; i <= 10; i++)
            {
                var sim = SimulationFactory.DefaultSimulation(availability, 57600, 6, logger);

                sim.Run();

                logger.LogResults(sim.Results, "Results_" + i, 57600);
            }
        }
    }
}
