using SimulationObjects.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulationObjects.SimBlocks
{
    public enum BlockType
    {
        Arrival,
        Disposal,
        Process
    }
    public interface ISimBlock
    {
        BlockType BlockType { get; }
    }
}
