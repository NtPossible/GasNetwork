using AttributeRenderingLibrary;
using GasNetwork.src.Interfaces;
using System;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;

namespace GasNetwork.src.BE
{
    public class BlockEntityGasifier : BlockEntity, IPipeConnectable
    {
        public PipeChannel Channels => PipeChannel.Regular;
        public bool CanAcceptPipeAt(BlockFacing face)
        {
            return face != BlockFacing.EAST;
        }
        readonly InventoryGeneric inventory = new(1, null, null);
        public bool HasFuel => !inventory[0].Empty;
        public bool IsDoorOpen => State.StartsWith("open");
        public bool Lit => State.EndsWith("-lit"); 

        double burnStartTotalHours;

        TreeAttribute types = new();

        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);

            inventory.LateInitialize($"gasifier-{Pos.X}/{Pos.Y}/{Pos.Z}", api);
            inventory.Pos = Pos;
            inventory.ResolveBlocksOrItems();

            RegisterGameTickListener(OnTick, 2000, 12);
        }

        public override void OnBlockPlaced(ItemStack byItemStack = null)
        {
            base.OnBlockPlaced(byItemStack);

            // Copy state from the itemstack
            ITreeAttribute itemTypes = byItemStack?.Attributes?.GetTreeAttribute("types");
            if (itemTypes != null && itemTypes.HasAttribute("state"))
            {
                types.SetString("state", itemTypes.GetString("state"));
            }

            MarkDirty(true);
        }

        public void ToggleDoor(IPlayer byPlayer)
        {
            SetState(door: IsDoorOpen ? "closed" : "open");

            Api.World.PlaySoundAt(new AssetLocation("sounds/block/metaldoor"), Pos, 0, byPlayer);
            (byPlayer as IClientPlayer)?.TriggerFpAnimation(EnumHandInteract.HeldItemInteract);
        }

        public bool TryAddFuel(ItemSlot fromSlot, IPlayer byPlayer)
        {
            if (fromSlot?.Itemstack == null)
            {
                return false;
            }

            CombustibleProperties props = fromSlot.Itemstack.Collectible.CombustibleProps;
            if (props == null || props.BurnTemperature < 1100)
            {
                return false;
            }

            int moved = fromSlot.TryPutInto(Api.World, inventory[0]);
            if (moved <= 0)
            {
                return false;
            }

            Api.World.PlaySoundAt(new AssetLocation("sounds/block/charcoal"), Pos, 0, byPlayer);
            (byPlayer as IClientPlayer)?.TriggerFpAnimation(EnumHandInteract.HeldItemInteract);

            if (!Lit)
            {
                SetState(fuel: "coal");
            }

            fromSlot.MarkDirty();
            return true;
        }

        public void OnIgnited()
        {
            if (!HasFuel || Lit)
            {
                return;
            }
            burnStartTotalHours = Api.World.Calendar.TotalHours;

            SetState(fuel: "lit");
        }

        void OnTick(float dt)
        {
            if (!Lit)
            {
                return;
            }
            double hoursPassed = Math.Min(2400, Api.World.Calendar.TotalHours - burnStartTotalHours);
            while (hoursPassed >= 12)
            {
                burnStartTotalHours += 12;
                inventory[0].TakeOut(1);
                if (inventory.Empty)
                {
                    SetState(fuel: "none");
                    break;
                }
                hoursPassed -= 12;
            }
        }

        string State
        {
            get => types.GetString("state", "closed-none");
            set
            {
                types.SetString("state", value);
                MarkDirty(true);
            }
        }

        void SetState(string door = null, string fuel = null)
        {
            string[] parts = State.Split('-');
            string curDoor = (parts.Length > 0 && parts[0].Length > 0) ? parts[0] : "closed";
            string curFuel = (parts.Length > 1 && parts[1].Length > 0) ? parts[1] : "none";

            door ??= curDoor;
            fuel ??= curFuel;

            State = $"{door}-{fuel}";
        }

        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldAccessForResolve)
        {
            base.FromTreeAttributes(tree, worldAccessForResolve);

            burnStartTotalHours = tree.GetDouble("burnStartTotalHours");

            ITreeAttribute invTree = tree.GetTreeAttribute("inventory");
            if (invTree != null)
            {
                inventory.FromTreeAttributes(invTree);
            }
            TreeAttribute treeAttr = tree.GetTreeAttribute("types") as TreeAttribute;
            types = treeAttr ?? new TreeAttribute();

            if (Api != null && Api.Side == EnumAppSide.Client)
            {
                BlockEntityBehaviorShapeTexturesFromAttributes renderer = GetBehavior<BlockEntityBehaviorShapeTexturesFromAttributes>();
                renderer?.OnBlockPlaced(null);
                Api.World.BlockAccessor.MarkBlockDirty(Pos);
            }
        }

        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);

            tree.SetDouble("burnStartTotalHours", burnStartTotalHours);

            ITreeAttribute invTree = new TreeAttribute();
            inventory.ToTreeAttributes(invTree);
            tree["inventory"] = invTree;

            tree["types"] = types;
        }

        public override void OnBlockBroken(IPlayer byPlayer = null)
        {
            if (Api.World.Side == EnumAppSide.Server)
            {
                inventory.DropAll(Pos.ToVec3d().Add(0.5, 0.5, 0.5));
            }
            base.OnBlockBroken(byPlayer);
        }

        public override void GetBlockInfo(IPlayer forPlayer, StringBuilder dsc)
        {
            base.GetBlockInfo(forPlayer, dsc);
            if (!inventory[0].Empty)
            {
                dsc.AppendLine($"{inventory[0].StackSize}x {inventory[0].GetStackName()}");
            }
        }
    }
}
