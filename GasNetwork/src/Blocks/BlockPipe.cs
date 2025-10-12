using GasNetwork.src.BE;
using GasNetwork.src.Utils;
using System.Collections.Generic;
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

        public override Cuboidf[] GetCollisionBoxes(IBlockAccessor blockAccessor, BlockPos pos)
        {
            GetConnections(pos, out bool north, out bool east, out bool south, out bool west, out bool up, out bool down);

            float thickness = (configuredChannels == PipeChannel.Thin) ? 0.13f : 0.315f;

            List<Cuboidf> boxes = GeneratePipeBoxes(north, east, south, west, up, down, thickness);
            return boxes.ToArray();
        }

        public override Cuboidf[] GetSelectionBoxes(IBlockAccessor blockAccessor, BlockPos pos)
        {
            GetConnections(pos, out bool north, out bool east, out bool south, out bool west, out bool up, out bool down);

            float thickness = (configuredChannels == PipeChannel.Thin) ? 0.13f : 0.315f;

            List<Cuboidf> boxes = GeneratePipeBoxes(north, east, south, west, up, down, thickness);
            return boxes.ToArray();
        }

        // Figure out which faces this pipe connects to.
        private void GetConnections(BlockPos pos, out bool north, out bool east, out bool south, out bool west, out bool up, out bool down)
        {
            north = PipeUtils.IsConnectableAt(api.World, pos.NorthCopy(), BlockFacing.SOUTH, configuredChannels);
            east = PipeUtils.IsConnectableAt(api.World, pos.EastCopy(), BlockFacing.WEST, configuredChannels);
            south = PipeUtils.IsConnectableAt(api.World, pos.SouthCopy(), BlockFacing.NORTH, configuredChannels);
            west = PipeUtils.IsConnectableAt(api.World, pos.WestCopy(), BlockFacing.EAST, configuredChannels);
            up = PipeUtils.IsConnectableAt(api.World, pos.UpCopy(), BlockFacing.DOWN, configuredChannels);
            down = PipeUtils.IsConnectableAt(api.World, pos.DownCopy(), BlockFacing.UP, configuredChannels);
        }

        // Builds a core box plus arm boxes in each connected direction.
        private static List<Cuboidf> GeneratePipeBoxes(bool north, bool east, bool south, bool west, bool up, bool down, float thickness)
        {
            // Half the total pipe thickness for symmetrical placement
            float halfThickness = thickness * 0.5f;

            // Core bounds are centered around the middle of the block
            float minBound = 0.5f - halfThickness;
            float maxBound = 0.5f + halfThickness;

            List<Cuboidf> boxes = new();

            // Central segment of the pipe
            boxes.Add(new Cuboidf(minBound, minBound, minBound, maxBound, maxBound, maxBound));

            // Arms extending toward connected directions
            if (east)
            {
                boxes.Add(new Cuboidf(maxBound, minBound, minBound, 1f, maxBound, maxBound));
            }
            if (west)
            {
                boxes.Add(new Cuboidf(0f, minBound, minBound, minBound, maxBound, maxBound));
            }
            if (north) { 
                boxes.Add(new Cuboidf(minBound, minBound, 0f, maxBound, maxBound, minBound));
            }
            if (south)
            {
                boxes.Add(new Cuboidf(minBound, minBound, maxBound, maxBound, maxBound, 1f));
            }
            if (up)
            {
                boxes.Add(new Cuboidf(minBound, maxBound, minBound, maxBound, 1f, maxBound));
            }
            if (down)
            {
                boxes.Add(new Cuboidf(minBound, 0f, minBound, maxBound, minBound, maxBound));
            }
            return boxes;
        }

    }
}
