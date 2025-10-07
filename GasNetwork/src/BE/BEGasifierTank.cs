using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace GasNetwork.src.BE
{
    internal class BlockEntityGasifierTank : BlockEntity, IPipeConnectable
    {
        public PipeChannel Channels => PipeChannel.All;

        public bool CanAcceptPipeAt(BlockFacing face)
        {
            return face != BlockFacing.UP;
        }
    }
}
