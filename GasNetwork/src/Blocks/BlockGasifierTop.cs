using GasNetwork.src.Interfaces;
using System;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace GasNetwork.src.Blocks
{
    public class BlockGasifierTop : Block, IPipeConnectable
    {
        public static string GasifierKey => "gasifier";

        public PipeChannel Channels => PipeChannel.Regular;

        public bool CanAcceptPipeAt(BlockFacing face) => true;

        public override ItemStack[] GetDrops(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1f)
        {
            BlockPos bottomPos = pos.DownCopy();
            Block bottom = world.BlockAccessor.GetBlock(bottomPos);

            if (bottom != null && bottom.BlockId != 0 && bottom.Code?.Path?.StartsWith(GasifierKey) == true)
            {
                return bottom.GetDrops(world, bottomPos, byPlayer, dropQuantityMultiplier);
            }

            return Array.Empty<ItemStack>();
        }

        public override ItemStack OnPickBlock(IWorldAccessor world, BlockPos pos)
        {
            BlockPos bottomPos = pos.DownCopy();
            Block bottom = world.BlockAccessor.GetBlock(bottomPos);

            if (bottom != null && bottom.BlockId != 0 && bottom.Code?.Path?.StartsWith(GasifierKey) == true)
            {
                return bottom.OnPickBlock(world, bottomPos);
            }

            return base.OnPickBlock(world, pos);
        }

        public override void OnBlockBroken(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1f)
        {
            if (world.Side == EnumAppSide.Server)
            {
                BlockPos bottomPos = pos.DownCopy();
                Block bottom = world.BlockAccessor.GetBlock(bottomPos);

                if (bottom != null && bottom.BlockId != 0 && bottom.Code?.Path?.StartsWith(GasifierKey) == true)
                {
                    world.BlockAccessor.BreakBlock(bottomPos, byPlayer);
                }
            }

            base.OnBlockBroken(world, pos, byPlayer, dropQuantityMultiplier);
        }

        public override void OnBlockRemoved(IWorldAccessor world, BlockPos pos)
        {
            BlockPos bottomPos = pos.DownCopy();
            Block bottom = world.BlockAccessor.GetBlock(bottomPos);

            if (bottom?.Code?.Path?.StartsWith(GasifierKey) == true)
            {
                world.BlockAccessor.SetBlock(0, bottomPos);
            }

            base.OnBlockRemoved(world, pos);
        }
    }
}
