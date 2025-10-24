using GasNetwork.src.Systems;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;

namespace GasNetwork.src.Items
{
    public class ItemGasLinker : Item
    {
        public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handling)
        {
            if (!firstEvent || blockSel == null)
            {
                return;
            }
            IWorldAccessor world = api.World;
            IPlayer player = (byEntity as EntityPlayer)?.Player;
            if (player != null && !world.Claims.TryAccess(player, blockSel.Position, EnumBlockAccessFlags.Use))
            {
                return;
            }
            GasLinkRegistrySystem registry = api.ModLoader.GetModSystem<GasLinkRegistrySystem>();
            BlockPos pos = blockSel.Position;
            bool nowLinked = registry.Toggle(pos);

            (player as IServerPlayer)?.SendMessage(GlobalConstants.GeneralChatGroup, nowLinked ? "Linked to gas network." : "Unlinked from gas network.", EnumChatType.Notification);

            api.World.PlaySoundAt(new AssetLocation("game:sounds/block/ingot"), pos.X, pos.Y, pos.Z, player, true, 16);

            GasLinkRegistrySystem.RefreshNeighborPipes(world, pos);
            handling = EnumHandHandling.PreventDefaultAction;
        }
    }
}
