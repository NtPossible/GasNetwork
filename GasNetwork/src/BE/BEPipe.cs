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

        public PipeChannel Channels => configuredChannels;

        private byte connectedMask;
        private byte blockedSidesMask;

        public byte ConnectedMask => connectedMask;

        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);

            if (Block?.Attributes != null)
            {
                string? channels = Block.Attributes["pipe"]?["channels"].AsString("Regular");
                configuredChannels = channels == "Thin" ? PipeChannel.Thin : PipeChannel.Regular;
            }

            if (Api.Side == EnumAppSide.Server)
            {
                RecalculateConnections(true);
            }
        }

        public bool CanAcceptPipeAt(BlockFacing face)
        {
            return (blockedSidesMask & PipeUtils.PipeSides.Bit(face)) == 0;
        }

        public bool IsBlocked(BlockFacing face) => (blockedSidesMask & PipeUtils.PipeSides.Bit(face)) != 0;

        public void SetBlocked(BlockFacing face, bool blocked)
        {
            byte bit = PipeUtils.PipeSides.Bit(face);
            blockedSidesMask = blocked ? (byte)(blockedSidesMask | bit) : (byte)(blockedSidesMask & ~bit);

            RecalculateConnections(true);
        }

        public void RecalculateConnections(bool force)
        {
            if (Api == null || Block == null)
            {
                return;
            }

            byte newMask = 0;
            foreach (BlockFacing face in BlockFacing.ALLFACES)
            {
                BlockPos neighbourPos = PipeUtils.PipeSides.Offset(Pos, face);
                BlockFacing opposite = face.Opposite;

                if (PipeUtils.IsConnectableAt(Api.World, neighbourPos, opposite, configuredChannels))
                {
                    newMask |= PipeUtils.PipeSides.Bit(face);
                }
            }

            // Apply wrench blocked sides
            newMask = (byte)(newMask & ~blockedSidesMask);

            if (!force && newMask == connectedMask)
            {
                return;
            }
            connectedMask = newMask;

            MarkDirty(true);
        }

        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);
            PipeUtils.PipeSides.WriteTypesTreeFromMask(tree, connectedMask);
            tree.SetInt("connectedMask", connectedMask);
            tree.SetInt("blockedSidesMask", blockedSidesMask);
        }

        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldAccessForResolve)
        {
            base.FromTreeAttributes(tree, worldAccessForResolve);
            connectedMask = (byte)tree.GetInt("connectedMask", connectedMask);
            blockedSidesMask = (byte)tree.GetInt("blockedSidesMask", blockedSidesMask);
        }
    }
}
