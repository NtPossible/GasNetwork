using AttributeRenderingLibrary;
using GasNetwork.src.Interfaces;
using GasNetwork.src.Utils;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;

namespace GasNetwork.src.BE
{
    public class BlockEntityPipe : BlockEntity, IPipeConnectable
    {
        private PipeChannel configuredChannels = PipeChannel.Regular;

        public PipeChannel Channels
        {
            get { return configuredChannels; }
        }

        public bool CanAcceptPipeAt(BlockFacing face)
        {
            return true;
        }

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

            if (Block != null && Block.Attributes != null)
            {
                string channels = Block.Attributes["pipe"]?["channels"].AsString("Regular");
                configuredChannels = channels == "Thin" ? PipeChannel.Thin : PipeChannel.Regular;
            }

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

            string north = PipeUtils.IsConnectableAt(Api.World, Pos.NorthCopy(), BlockFacing.SOUTH, configuredChannels) ? "1" : "0";
            string east = PipeUtils.IsConnectableAt(Api.World, Pos.EastCopy(), BlockFacing.WEST, configuredChannels) ? "1" : "0";
            string south = PipeUtils.IsConnectableAt(Api.World, Pos.SouthCopy(), BlockFacing.NORTH, configuredChannels) ? "1" : "0";
            string west = PipeUtils.IsConnectableAt(Api.World, Pos.WestCopy(), BlockFacing.EAST, configuredChannels) ? "1" : "0";
            string up = PipeUtils.IsConnectableAt(Api.World, Pos.UpCopy(), BlockFacing.DOWN, configuredChannels) ? "1" : "0";
            string down = PipeUtils.IsConnectableAt(Api.World, Pos.DownCopy(), BlockFacing.UP, configuredChannels) ? "1" : "0";

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
            connectionTypes.SetString("mask", northConnected + eastConnected + southConnected + westConnected + upConnected + downConnected);
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

        public void GetStoredConnections(out bool north, out bool east, out bool south, out bool west, out bool up, out bool down)
        {
            north = northConnected == "1";
            east = eastConnected == "1";
            south = southConnected == "1";
            west = westConnected == "1";
            up = upConnected == "1";
            down = downConnected == "1";
        }
    }
}
