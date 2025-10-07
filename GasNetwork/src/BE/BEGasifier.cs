using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace GasNetwork.src.BE
{
    public class BEGasifier : BlockEntity, IPipeConnectable
    {
        public bool CanAcceptPipeAt(BlockFacing face)
        {
            return true;
        }
    }
}
