using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace GasNetwork.src.Utils
{
    public static class PipeUtils
    {
        public static bool IsConnectableAt(IWorldAccessor world, BlockPos at, BlockFacing neighborFaceExposedToThisPipe)
        {
            Block neighborBlock = world.BlockAccessor.GetBlock(at);
            if (neighborBlock is IPipeConnectable connectableBlock)
            {
                return connectableBlock.CanAcceptPipeAt(neighborFaceExposedToThisPipe);
            }

            BlockEntity neighborBe = world.BlockAccessor.GetBlockEntity(at);
            if (neighborBe is IPipeConnectable connectableBe)
            {
                return connectableBe.CanAcceptPipeAt(neighborFaceExposedToThisPipe);
            }

            return false;
        }
    }
}
