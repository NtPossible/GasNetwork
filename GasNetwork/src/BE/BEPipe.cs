using AttributeRenderingLibrary;
using GasNetwork.src.Utils;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;

namespace GasNetwork.src.BE
{
    public class BEPipe : BlockEntity
    {
        // connection flags
        private string northConnected = "0";
        private string eastConnected = "0";
        private string southConnected = "0";
        private string westConnected = "0";
        private string upConnected = "0";
        private string downConnected = "0";

        private string lastConnectionMask;

        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);

            if (Api.Side == EnumAppSide.Server)
            {
                Api.World.RegisterCallback(_ =>
                {
                    if (Api == null || Block == null)
                    {
                        return;
                    }

                    RecalculateConnections(true);
                }, 200);
            }
        }

        public void RecalculateConnections(bool force)
        {
            if (Block == null || Api == null)
            {
                return;
            }

            string north = PipeUtils.IsConnectableAt(Api.World, Pos.NorthCopy(), BlockFacing.SOUTH) ? "1" : "0";
            string east = PipeUtils.IsConnectableAt(Api.World, Pos.EastCopy(), BlockFacing.WEST) ? "1" : "0";
            string south = PipeUtils.IsConnectableAt(Api.World, Pos.SouthCopy(), BlockFacing.NORTH) ? "1" : "0";
            string west = PipeUtils.IsConnectableAt(Api.World, Pos.WestCopy(), BlockFacing.EAST) ? "1" : "0";
            string up = PipeUtils.IsConnectableAt(Api.World, Pos.UpCopy(), BlockFacing.DOWN) ? "1" : "0";
            string down = PipeUtils.IsConnectableAt(Api.World, Pos.DownCopy(), BlockFacing.UP) ? "1" : "0";

            string newConnectionMask = north + east + south + west + up + down;

            if (!force && newConnectionMask == lastConnectionMask)
            {
                return;
            }

            northConnected = north;
            eastConnected = east;
            southConnected = south;
            westConnected = west;
            upConnected = up;
            downConnected = down;

            lastConnectionMask = newConnectionMask;

            MarkDirty(true);
        }

        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);

            ITreeAttribute connectionTypes = tree.GetOrAddTreeAttribute("types");
            connectionTypes.SetString("north", northConnected);
            connectionTypes.SetString("east", eastConnected);
            connectionTypes.SetString("south", southConnected);
            connectionTypes.SetString("west", westConnected);
            connectionTypes.SetString("up", upConnected);
            connectionTypes.SetString("down", downConnected);

            connectionTypes.SetString("mask", BuildConnectionMask());
        }

        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldAccessForResolve)
        {
            base.FromTreeAttributes(tree, worldAccessForResolve);

            ITreeAttribute connectionTypes = tree.GetOrAddTreeAttribute("types");
            northConnected = connectionTypes.GetString("north", northConnected);
            eastConnected = connectionTypes.GetString("east", eastConnected);
            southConnected = connectionTypes.GetString("south", southConnected);
            westConnected = connectionTypes.GetString("west", westConnected);
            upConnected = connectionTypes.GetString("up", upConnected);
            downConnected = connectionTypes.GetString("down", downConnected);

            if (Api != null && Api.Side == EnumAppSide.Client)
            {
                BlockEntityBehaviorShapeTexturesFromAttributes renderer = GetBehavior<BlockEntityBehaviorShapeTexturesFromAttributes>();
                renderer?.OnBlockPlaced(null);

                Api.World.BlockAccessor.MarkBlockDirty(Pos);
            }
        }

        private string BuildConnectionMask()
        {
            return northConnected + eastConnected + southConnected + westConnected + upConnected + downConnected;
        }
    }
}
