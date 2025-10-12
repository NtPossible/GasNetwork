using GasNetwork.src.BE;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace GasNetwork.src.BB
{
    public class BlockBehaviorGasifier : BlockBehavior
    {
        public BlockBehaviorGasifier(Block block) : base(block) { }

        WorldInteraction[] interactions;

        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);
            if (api.Side != EnumAppSide.Client)
            {
                return;
            }
            ICoreClientAPI capi = api as ICoreClientAPI;

            BlockForge forgeBlock = api.World.GetBlock(new AssetLocation("forge")) as BlockForge;

            interactions = ObjectCacheUtil.GetOrCreate(api, "gasnetwork:gasifierBlockInteractions", () =>
            {
                List<ItemStack> canIgniteStacks = BlockBehaviorCanIgnite.CanIgniteStacks(api, false);

                return new WorldInteraction[] {
                    new()
                    {
                        ActionLangCode = "gasnetwork:blockhelp-gasifier-door",
                        MouseButton = EnumMouseButton.Right,
                    },
                    new()
                    {
                        ActionLangCode = "blockhelp-coalpile-addcoal",
                        MouseButton = EnumMouseButton.Right,
                        HotKeyCode = "shift",
                        Itemstacks = forgeBlock.coalStacklist.ToArray()
                    },
                    new()
                    {
                        ActionLangCode = "blockhelp-forge-ignite",
                        HotKeyCode = "shift",
                        MouseButton = EnumMouseButton.Right,
                        Itemstacks = canIgniteStacks.ToArray(),
                        GetMatchingStacks = (worldInteraction, blockSel, entitySel) => {
                            if (api.World.BlockAccessor.GetBlockEntity(blockSel.Position) is BlockEntityGasifier beGasifier && beGasifier.HasFuel && !beGasifier.Lit)
                            {
                                return worldInteraction.Itemstacks;
                            }
                            return null;
                        }
                    }
                };
            });
        }

        public override WorldInteraction[] GetPlacedBlockInteractionHelp(IWorldAccessor world, BlockSelection selection, IPlayer forPlayer, ref EnumHandling handling)
        {
            return interactions.Append(base.GetPlacedBlockInteractionHelp(world, selection, forPlayer, ref handling));
        }
    }
}