using GasNetwork.src.Interfaces;
using GasNetwork.src.Systems;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;

namespace GasNetwork.src.Utils
{
    public static class PipeUtils
    {
        public static bool IsConnectableAt(IWorldAccessor world, BlockPos neighbourPos, BlockFacing neighbourFaceExposedToThisPipe, PipeChannel channel)
        {
            BlockEntity neighborBe = world.BlockAccessor.GetBlockEntity(neighbourPos);
            if (neighborBe is IPipeConnectable connectableBe && (connectableBe.Channels & channel) != 0)
            {
                return connectableBe.CanAcceptPipeAt(neighbourFaceExposedToThisPipe);
            }

            Block neighborBlock = world.BlockAccessor.GetBlock(neighbourPos);
            if (neighborBlock is IPipeConnectable connectableBlock && (connectableBlock.Channels & channel) != 0)
            {
                return connectableBlock.CanAcceptPipeAt(neighbourFaceExposedToThisPipe);
            }

            GasLinkRegistrySystem registry = world.Api.ModLoader.GetModSystem<GasLinkRegistrySystem>();
            return registry?.IsLinked(world, neighbourPos) == true;
        }


        public static class PipeSides
        {
            private static readonly byte[] faceBits = [
                1 << 0, // NORTH
                1 << 1, // EAST
                1 << 2, // SOUTH
                1 << 3, // WEST
                1 << 4, // UP
                1 << 5  // DOWN
            ];

            public static byte Bit(BlockFacing face) => faceBits[face.Index];

            public static bool Has(byte mask, BlockFacing face) => (mask & Bit(face)) != 0;

            public static BlockPos Offset(BlockPos pos, BlockFacing face)
            {
                return new BlockPos(pos.X + face.Normali.X, pos.Y + face.Normali.Y, pos.Z + face.Normali.Z);
            }

            // Converts the mask to a string
            public static string ToMaskString(byte mask)
            {
                char north = Has(mask, BlockFacing.NORTH) ? '1' : '0';
                char east = Has(mask, BlockFacing.EAST) ? '1' : '0';
                char south = Has(mask, BlockFacing.SOUTH) ? '1' : '0';
                char west = Has(mask, BlockFacing.WEST) ? '1' : '0';
                char up = Has(mask, BlockFacing.UP) ? '1' : '0';
                char down = Has(mask, BlockFacing.DOWN) ? '1' : '0';

                return new string([north, east, south, west, up, down]);
            }

            public static void WriteTypesTreeFromMask(ITreeAttribute tree, byte mask)
            {
                ITreeAttribute t = tree.GetOrAddTreeAttribute("types");
                t.SetString("mask", ToMaskString(mask));
            }
        }
    }
}
