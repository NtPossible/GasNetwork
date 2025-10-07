using GasNetwork.src.BE;
using GasNetwork.src.Utils;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;

namespace GasNetwork.src.Blocks
{
    public class BlockPipe : BlockGeneric, IPipeConnectable
    {
        private PipeChannel configuredChannels = PipeChannel.Regular;

        public PipeChannel Channels
        {
            get { return configuredChannels; }
        }

        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);
            if (Attributes != null)
            {
                string channels = Attributes["pipe"]?["channels"].AsString("Regular");
                configuredChannels = channels == "Thin" ? PipeChannel.Thin : PipeChannel.Regular;
            }
        }

        public bool CanAcceptPipeAt(BlockFacing face)
        {
            return true;
        }

        public override bool DoPlaceBlock(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, ItemStack byItemStack)
        {
            SeedInitialConnectionAttributes(byItemStack, world, blockSel.Position, configuredChannels);
            return base.DoPlaceBlock(world, byPlayer, blockSel, byItemStack);
        }

        public override void OnBlockPlaced(IWorldAccessor world, BlockPos blockPos, ItemStack byItemStack = null)
        {
            base.OnBlockPlaced(world, blockPos, byItemStack);

            if (world.BlockAccessor.GetBlockEntity(blockPos) is BlockEntityPipe be)
            {
                be.RecalculateConnections(true);
            }
        }

        public override void OnNeighbourBlockChange(IWorldAccessor world, BlockPos pos, BlockPos neibpos)
        {
            base.OnNeighbourBlockChange(world, pos, neibpos);

            BlockEntityPipe be = world.BlockAccessor.GetBlockEntity(pos) as BlockEntityPipe;
            be?.RecalculateConnections(false);
        }

        public override void OnBlockRemoved(IWorldAccessor world, BlockPos pos)
        {
            base.OnBlockRemoved(world, pos);

            foreach (BlockFacing face in BlockFacing.ALLFACES)
            {
                if (world.BlockAccessor.GetBlockEntity(pos.AddCopy(face)) is BlockEntityPipe neighborBe)
                {
                    neighborBe.RecalculateConnections(true);
                }
            }
        }

        // Used to prevent 6 stack from appearing when first placing a pipe
        private static void SeedInitialConnectionAttributes(ItemStack stack, IWorldAccessor world, BlockPos pos, PipeChannel channel)
        {
            if (stack == null)
            {
                return;
            }

            string north = PipeUtils.IsConnectableAt(world, pos.NorthCopy(), BlockFacing.SOUTH, channel) ? "1" : "0";
            string east = PipeUtils.IsConnectableAt(world, pos.EastCopy(), BlockFacing.WEST, channel) ? "1" : "0";
            string south = PipeUtils.IsConnectableAt(world, pos.SouthCopy(), BlockFacing.NORTH, channel) ? "1" : "0";
            string west = PipeUtils.IsConnectableAt(world, pos.WestCopy(), BlockFacing.EAST, channel) ? "1" : "0";
            string up = PipeUtils.IsConnectableAt(world, pos.UpCopy(), BlockFacing.DOWN, channel) ? "1" : "0";
            string down = PipeUtils.IsConnectableAt(world, pos.DownCopy(), BlockFacing.UP, channel) ? "1" : "0";

            string connectionMask = north + east + south + west + up + down;

            ITreeAttribute connectionTypes = stack.Attributes.GetOrAddTreeAttribute("types");
            connectionTypes.SetString("north", north);
            connectionTypes.SetString("east", east);
            connectionTypes.SetString("south", south);
            connectionTypes.SetString("west", west);
            connectionTypes.SetString("up", up);
            connectionTypes.SetString("down", down);
            connectionTypes.SetString("mask", connectionMask);
        }
    }
}
