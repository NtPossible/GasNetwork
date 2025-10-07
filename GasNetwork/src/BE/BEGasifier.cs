using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace GasNetwork.src.BE
{
    public class BlockEntityGasifier : BlockEntity, IPipeConnectable
    {
        public PipeChannel Channels => PipeChannel.Regular;

        public bool CanAcceptPipeAt(BlockFacing face)
        {
            return true;
        }
    }
}
