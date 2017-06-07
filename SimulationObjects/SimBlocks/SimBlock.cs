using SimulationObjects.Entities;
using SimulationObjects.Events;
using SimulationObjects.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulationObjects.SimBlocks
{
    public abstract class SimBlock: ISimBlock
    {
        private ISimResults Results;
        protected Simulation Simulation;
        public BlockType BlockType { get; private set; }
        public SimBlock(BlockType blockType, Simulation sim)
        {
            Results = sim.Results;
            BlockType = blockType;
            Simulation = sim;
        }
    }
}
