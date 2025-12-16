using GasNetwork.src.BE;
using GasNetwork.src.Interfaces;
using GasNetwork.src.Utils;
using System;
using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;

namespace GasNetwork.src.Blocks
{
    public class BlockPipe : BlockGeneric, IPipeConnectable
    {
        private PipeChannel configuredChannels = PipeChannel.Regular;
        public PipeChannel Channels => configuredChannels;

        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);
            if (Attributes != null)
            {
                string? channels = Attributes["pipe"]?["channels"].AsString("Regular");
                configuredChannels = channels == "Thin" ? PipeChannel.Thin : PipeChannel.Regular;
            }
        }
        
        public bool CanAcceptPipeAt(BlockFacing face)
        {
            return true;
        }

        public override void OnBlockPlaced(IWorldAccessor world, BlockPos blockPos, ItemStack? byItemStack = null)
        {
            base.OnBlockPlaced(world, blockPos, byItemStack);

            if (world.BlockAccessor.GetBlockEntity(blockPos) is BlockEntityPipe bePipe)
            {
                bePipe.RecalculateConnections(true);

                if (world.Side == EnumAppSide.Client)
                {
                    // ready the placed pipe for first frame
                    InitializeRenderStateFromMask(world, bePipe);

                    // ready the neighbours for first frame also
                    foreach (BlockFacing face in BlockFacing.ALLFACES)
                    {
                        BlockPos neighbourPos = blockPos.AddCopy(face);
                        if (world.BlockAccessor.GetBlockEntity(neighbourPos) is BlockEntityPipe neighbourBe)
                        {
                            neighbourBe.RecalculateConnections(false);
                            InitializeRenderStateFromMask(world, neighbourBe);
                        }
                    }
                }
            }
        }

        public override void OnNeighbourBlockChange(IWorldAccessor world, BlockPos pos, BlockPos neibpos)
        {
            base.OnNeighbourBlockChange(world, pos, neibpos);

            BlockEntityPipe? bePipe = world.BlockAccessor.GetBlockEntity(pos) as BlockEntityPipe;
            bePipe?.RecalculateConnections(false);
        }

        public override void OnBlockRemoved(IWorldAccessor world, BlockPos pos)
        {
            base.OnBlockRemoved(world, pos);

            foreach (BlockFacing face in BlockFacing.ALLFACES)
            {
                BlockPos neighbourPos = pos.AddCopy(face);
                if (world.BlockAccessor.GetBlockEntity(neighbourPos) is BlockEntityPipe neighbourBe)
                {
                    neighbourBe.RecalculateConnections(true);

                    if (world.Side == EnumAppSide.Client)
                    {
                        // Also ready neighbours after a removal so they don’t flash the old model
                        InitializeRenderStateFromMask(world, neighbourBe);
                    }
                }
            }
        }

        private static void InitializeRenderStateFromMask(IWorldAccessor world, BlockEntityPipe bePipe)
        {
            TreeAttribute tree = new();
            bePipe.ToTreeAttributes(tree);
            PipeUtils.PipeSides.WriteTypesTreeFromMask(tree, bePipe.ConnectedMask);
            bePipe.FromTreeAttributes(tree, world);
        }

        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            if (world == null || byPlayer == null || blockSel == null)
            {
                return false;
            }
            Item? activeItem = byPlayer.InventoryManager?.ActiveHotbarSlot?.Itemstack?.Item;
            string itemPath = activeItem?.Code?.Path ?? string.Empty;

            if (!itemPath.Contains("wrench"))
            {
                return base.OnBlockInteractStart(world, byPlayer, blockSel);
            }

            if (world.Side == EnumAppSide.Client)
            {
                return true;
            }

            if (world.BlockAccessor.GetBlockEntity(blockSel.Position) is not BlockEntityPipe bePipe)
            {
                return false;
            }

            BlockFacing targetFacing = FacingFromHit(blockSel);

            bool wasBlocked = bePipe.IsBlocked(targetFacing);
            bool newBlocked = !wasBlocked;

            bePipe.SetBlocked(targetFacing, newBlocked);

            BlockPos neighbourPos = PipeUtils.PipeSides.Offset(blockSel.Position, targetFacing);
            if (world.BlockAccessor.GetBlockEntity(neighbourPos) is BlockEntityPipe neighbourBe)
            {
                neighbourBe.SetBlocked(targetFacing.Opposite, newBlocked);
            }

            foreach (BlockFacing face in BlockFacing.ALLFACES)
            {
                BlockPos targetPos = blockSel.Position.AddCopy(face);
                BlockEntity targetEntity = world.BlockAccessor.GetBlockEntity(targetPos);
                if (targetEntity is BlockEntityPipe targetPipe)
                {
                    targetPipe.RecalculateConnections(true);
                }

                BlockPos neighbourTargetPos = neighbourPos.AddCopy(face);
                BlockEntity neighbourTargetEntity = world.BlockAccessor.GetBlockEntity(neighbourTargetPos);
                if (neighbourTargetEntity is BlockEntityPipe neighbourTargetPipe)
                {
                    neighbourTargetPipe.RecalculateConnections(true);
                }
            }

            return true;
        }

        private static BlockFacing FacingFromHit(BlockSelection sel)
        {
            float dx = (float)(sel.HitPosition.X - 0.5);
            float dy = (float)(sel.HitPosition.Y - 0.5);
            float dz = (float)(sel.HitPosition.Z - 0.5);

            float ax = Math.Abs(dx), ay = Math.Abs(dy), az = Math.Abs(dz);

            if (ax >= ay && ax >= az)
            {
                return dx >= 0 ? BlockFacing.EAST : BlockFacing.WEST;
            }
            if (az >= ax && az >= ay)
            {
                return dz >= 0 ? BlockFacing.SOUTH : BlockFacing.NORTH;
            }
            return dy >= 0 ? BlockFacing.UP : BlockFacing.DOWN;
        }

        private float GetPipeThickness()
        {
            return Attributes?["pipe"]?["thickness"].AsFloat(0.3125f) ?? 0.3125f;
        }

        public override Cuboidf[] GetCollisionBoxes(IBlockAccessor blockAccessor, BlockPos pos)
        {
            byte mask = 0;

            if (blockAccessor.GetBlockEntity(pos) is BlockEntityPipe bePipe)
            {
                mask = bePipe.ConnectedMask;
            }

            List<Cuboidf> list = GeneratePipeBoxes(mask, GetPipeThickness());
            return list.ToArray();
        }

        public override Cuboidf[] GetSelectionBoxes(IBlockAccessor blockAccessor, BlockPos pos)
        {
            return GetCollisionBoxes(blockAccessor, pos);
        }
        // Builds a core box plus arm boxes in each connected direction.

        private static List<Cuboidf> GeneratePipeBoxes(byte mask, float thickness)
        {
            // Half the total pipe thickness for symmetrical placement
            float halfThickness = thickness * 0.5f;
            // Core bounds are centered around the middle of the block
            float minBound = 0.5f - halfThickness;
            float maxBound = 0.5f + halfThickness;

            List<Cuboidf> boxes = new(7)
            {
                // Central segment of the pipe
                new Cuboidf(minBound, minBound, minBound, maxBound, maxBound, maxBound)
            };

            if (PipeUtils.PipeSides.Has(mask, BlockFacing.EAST))
            {
                boxes.Add(new Cuboidf(maxBound, minBound, minBound, 1f, maxBound, maxBound));
            }
            if (PipeUtils.PipeSides.Has(mask, BlockFacing.WEST))
            {
                boxes.Add(new Cuboidf(0f, minBound, minBound, minBound, maxBound, maxBound));
            }
            if (PipeUtils.PipeSides.Has(mask, BlockFacing.NORTH))
            {
                boxes.Add(new Cuboidf(minBound, minBound, 0f, maxBound, maxBound, minBound));
            }
            if (PipeUtils.PipeSides.Has(mask, BlockFacing.SOUTH))
            {
                boxes.Add(new Cuboidf(minBound, minBound, maxBound, maxBound, maxBound, 1f));
            }
            if (PipeUtils.PipeSides.Has(mask, BlockFacing.UP))
            {
                boxes.Add(new Cuboidf(minBound, maxBound, minBound, maxBound, 1f, maxBound));
            }
            if (PipeUtils.PipeSides.Has(mask, BlockFacing.DOWN))
            {
                boxes.Add(new Cuboidf(minBound, 0f, minBound, maxBound, minBound, maxBound));
            }
            return boxes;
        }

    }
}
