using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace GasNetwork.src.Utils
{
    public static class PipeUtils
    {
        public static bool IsConnectableAt(IWorldAccessor world, BlockPos at, BlockFacing neighborFaceExposedToThisPipe, PipeChannel channel)
        {
            Block neighborBlock = world.BlockAccessor.GetBlock(at);
            if (neighborBlock is IPipeConnectable connectableBlock && (connectableBlock.Channels & channel) != 0)
            {
                return connectableBlock.CanAcceptPipeAt(neighborFaceExposedToThisPipe);
            }

            BlockEntity neighborBe = world.BlockAccessor.GetBlockEntity(at);
            if (neighborBe is IPipeConnectable connectableBe && (connectableBe.Channels & channel) != 0)
            {
                return connectableBe.CanAcceptPipeAt(neighborFaceExposedToThisPipe);
            }

            return false;
        }
    }
}
