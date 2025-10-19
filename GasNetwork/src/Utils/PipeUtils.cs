using GasNetwork.src.Interfaces;
using GasNetwork.src.System;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace GasNetwork.src.Utils
{
    public static class PipeUtils
    {
        public static bool IsConnectableAt(IWorldAccessor world, BlockPos neighbourPos, BlockFacing neighbourFaceExposedToThisPipe, PipeChannel channel)
        {
            Block neighborBlock = world.BlockAccessor.GetBlock(neighbourPos);
            if (neighborBlock is IPipeConnectable connectableBlock && (connectableBlock.Channels & channel) != 0)
            {
                return connectableBlock.CanAcceptPipeAt(neighbourFaceExposedToThisPipe);
            }

            BlockEntity neighborBe = world.BlockAccessor.GetBlockEntity(neighbourPos);
            if (neighborBe is IPipeConnectable connectableBe && (connectableBe.Channels & channel) != 0)
            {
                return connectableBe.CanAcceptPipeAt(neighbourFaceExposedToThisPipe);
            }

            GasLinkRegistrySystem registry = world.Api.ModLoader.GetModSystem<GasLinkRegistrySystem>();
            if (registry?.IsLinked(world, neighbourPos) == true)
            {
                return true;
            }
            return false;
        }
    }
}
