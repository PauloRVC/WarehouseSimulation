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

            var Data = new WarehouseData();

            var availability = Data.GetOverallAvailability();

            var logger = new VerboseLogger(@"C:\Users\Daniel\Desktop\SimLog");

            for(int i = 1; i <= 10; i++)
            {
                var sim = SimulationFactory.DefaultSimulation(availability, 57600, 6, logger);

                sim.Run();

                logger.LogResults(sim.Results, "Results_" + i, 57600);
            }

            

        }
    }
}
