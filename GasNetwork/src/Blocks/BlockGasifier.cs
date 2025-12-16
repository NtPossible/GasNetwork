using GasNetwork.src.BE;
using GasNetwork.src.Systems;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace GasNetwork.src.Blocks
{
    public class BlockGasifier : BlockGeneric, IIgnitable
    {
        public override bool DoPlaceBlock(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, ItemStack byItemStack)
        {
            if (byItemStack != null)
            {
                ITreeAttribute types = byItemStack.Attributes.GetOrAddTreeAttribute("types");
                if (!types.HasAttribute("state"))
                {
                    types.SetString("state", "closed-none");
                }
                else
                {
                    types.SetString("state", "closed-none");
                }
            }

            BlockPos topPos = blockSel.Position.UpCopy();
            Block topExisting = world.BlockAccessor.GetBlock(topPos);
            if (topExisting != null && topExisting.Replaceable < 6000)
            {
                return false;
            }
            bool placed = base.DoPlaceBlock(world, byPlayer, blockSel, byItemStack);
            if (!placed)
            {
                return false;
            }
            Block topBlock = world.GetBlock(new AssetLocation("gasnetwork:gasifiertop"));
            if (topBlock == null || topBlock.BlockId == 0)
            {
                return false;
            }

            world.BlockAccessor.SetBlock(topBlock.BlockId, topPos);
            GasLinkRegistrySystem.RefreshNeighborPipes(world, topPos);
            GasLinkRegistrySystem.RefreshNeighborPipes(world, blockSel.Position);

            return true;
        }


        public override void OnBlockRemoved(IWorldAccessor world, BlockPos pos)
        {
            BlockPos topPos = pos.UpCopy();
            Block top = world.BlockAccessor.GetBlock(topPos);

            if (top?.Code?.Path?.StartsWith("gasifiertop") == true)
            {
                world.BlockAccessor.SetBlock(0, topPos);
                GasLinkRegistrySystem.RefreshNeighborPipes(world, pos);
                GasLinkRegistrySystem.RefreshNeighborPipes(world, topPos);
            }
            base.OnBlockRemoved(world, pos);
        }

        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            if (!world.Claims.TryAccess(byPlayer, blockSel.Position, EnumBlockAccessFlags.Use))
            {
                return false;
            }

            if (world.BlockAccessor.GetBlockEntity(blockSel.Position) is not BlockEntityGasifier beGasifier)
            {
                return false;
            }
            ItemSlot slot = byPlayer.InventoryManager.ActiveHotbarSlot;

            if (slot.Empty)
            {
                beGasifier.ToggleDoor(byPlayer);
                return true;
            }

            if (beGasifier.TryAddFuel(slot, byPlayer))
            {
                return true;
            }
            return base.OnBlockInteractStart(world, byPlayer, blockSel);
        }

        public EnumIgniteState OnTryIgniteBlock(EntityAgent byEntity, BlockPos pos, float secondsIgniting)
        {
            if (api.World.BlockAccessor.GetBlockEntity(pos) is BlockEntityGasifier beGasifier && beGasifier.HasFuel && !beGasifier.Lit && beGasifier.IsDoorOpen)
            {
                return secondsIgniting > 2 ? EnumIgniteState.IgniteNow : EnumIgniteState.Ignitable;
            }
            return EnumIgniteState.NotIgnitablePreventDefault;
        }

        public void OnTryIgniteBlockOver(EntityAgent byEntity, BlockPos pos, float secondsIgniting, ref EnumHandling handling)
        {
            handling = EnumHandling.PreventDefault;
            BlockEntityGasifier? beGasifier = api.World.BlockAccessor.GetBlockEntity(pos) as BlockEntityGasifier;
            beGasifier?.OnIgnited();
        }

        EnumIgniteState IIgnitable.OnTryIgniteStack(EntityAgent byEntity, BlockPos pos, ItemSlot slot, float secondsIgniting)
        {
            BlockEntityGasifier? beGasifier = api.World.BlockAccessor.GetBlockEntity(pos) as BlockEntityGasifier;
            if (beGasifier?.Lit == true)
            {
                return secondsIgniting > 3 ? EnumIgniteState.IgniteNow : EnumIgniteState.Ignitable;
            }
            return EnumIgniteState.NotIgnitable;
        }
    }
}
