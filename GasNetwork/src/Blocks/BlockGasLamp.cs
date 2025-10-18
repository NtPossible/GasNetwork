using AttributeRenderingLibrary;
using GasNetwork.src.Interfaces;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace GasNetwork.src.Blocks
{
    public class BlockGasLamp : BlockGeneric, IPipeConnectable
    {
        public PipeChannel Channels => PipeChannel.Thin;

        public bool CanAcceptPipeAt(BlockFacing face)
        {
            return true;
        }
    }
}
