using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace GasNetwork.src.BE
{
    internal class BEGasifierTank : BlockEntity, IPipeConnectable
    {
        public bool CanAcceptPipeAt(BlockFacing face)
        {
            return face != BlockFacing.UP;
        }
    }
}
