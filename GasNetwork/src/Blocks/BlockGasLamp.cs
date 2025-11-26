using GasNetwork.src.Interfaces;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace GasNetwork.src.Blocks
{
    public class BlockGasLamp : BlockGeneric, IPipeConnectable
    {
        public PipeChannel Channels => PipeChannel.Thin;

        public bool CanAcceptPipeAt(BlockFacing face)
        {
            string mount = Variant?["mount"] ?? "floor";
            string sideCode = Variant?["side"] ?? "north";
            BlockFacing facing = BlockFacing.FromCode(sideCode);

            return mount switch
            {
                "ceiling" => face == BlockFacing.UP,
                "floor" => face == BlockFacing.DOWN,
                "wall" => face == facing,
                _ => true
            };
        }

        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            if (blockSel == null || byPlayer == null)
            {
                return false;
            }
            if (!world.Claims.TryAccess(byPlayer, blockSel.Position, EnumBlockAccessFlags.Use))
            {
                return false;
            }

            if (world.Side == EnumAppSide.Client)
            {
                return true;
            }

            BlockPos pos = blockSel.Position;
            Block curBlock = world.BlockAccessor.GetBlock(pos);

            string curState = curBlock.Variant.TryGetValue("state", out string state) ? state : "off";
            string newState = curState == "on" ? "off" : "on";

            AssetLocation newCode = curBlock.CodeWithVariant("state", newState);
            Block newBlock = world.GetBlock(newCode);
            if (newBlock == null || newBlock.BlockId == 0)
            {
                return false;
            }
            world.BlockAccessor.SetBlock(newBlock.BlockId, pos);
            return true;
        }
    }
}
