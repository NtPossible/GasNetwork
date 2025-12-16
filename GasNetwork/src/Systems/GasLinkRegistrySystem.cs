using GasNetwork.src.BE;
using System.Collections.Generic;
using System.IO;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;

namespace GasNetwork.src.Systems
{
    public class GasLinkRegistrySystem : ModSystem
    {
        private readonly HashSet<BlockPos> linked = [];
        private ICoreServerAPI? sapi;

        const string StoreKey = "gasnetwork.linkedpositions.bin";

        public override void StartServerSide(ICoreServerAPI api)
        {
            sapi = api;
            sapi.Event.SaveGameLoaded += Load;
            sapi.Event.GameWorldSave += Save;

            sapi.Event.ChunkColumnLoaded += OnChunkColumnLoaded;
        }

        private void OnChunkColumnLoaded(Vec2i chunkCoord, IWorldChunk[] chunks)
        {
            sapi!.Event.EnqueueMainThreadTask(() =>
            {
                HashSet<BlockPos> toRecalc = [];

                foreach (IWorldChunk chunk in chunks)
                {
                    if (chunk?.BlockEntities == null)
                    {
                        continue;
                    }
                    foreach (BlockEntity? blockEntity in chunk.BlockEntities.Values)
                    {
                        if (blockEntity is not BlockEntityPipe pipe)
                        {
                            continue;
                        }
                        // this pipe
                        toRecalc.Add(pipe.Pos);

                        // + neighbors so border connections fix themselves when the other chunk appears
                        foreach (BlockFacing? face in BlockFacing.ALLFACES)
                        {
                            BlockPos? neighbourPos = pipe.Pos.AddCopy(face);
                            toRecalc.Add(neighbourPos);
                        }
                    }
                }

                IBlockAccessor blockAccessor = sapi!.World.BlockAccessor;
                foreach (BlockPos? pos in toRecalc)
                {
                    if (blockAccessor.GetBlockEntity(pos) is BlockEntityPipe pipe)
                    {
                        pipe.RecalculateConnections(true);
                    }
                }
            }, "gasnetwork:recalc-pipes-on-chunkload");
        }

        public bool IsLinked(IWorldAccessor world, BlockPos pos)
        {
            if (!linked.Contains(pos))
            {
                return false;
            }
            IWorldChunk chunk = world.BlockAccessor.GetChunkAtBlockPos(pos);
            if (chunk == null)
            {
                return true;
            }
            Block block = world.BlockAccessor.GetBlock(pos);
            if (block == null || block.BlockId == 0)
            {
                linked.Remove(pos);
                return false;
            }
            return true;
        }

        public bool Toggle(BlockPos pos)
        {
            if (!linked.Add(pos))
            {
                linked.Remove(pos);
                return false;
            }
            return true;
        }

        public static void RefreshNeighborPipes(IWorldAccessor world, BlockPos pos)
        {
            foreach (BlockFacing face in BlockFacing.ALLFACES)
            {
                BlockPos beighbourPos = pos.AddCopy(face);
                if (world.BlockAccessor.GetBlockEntity(beighbourPos) is BlockEntityPipe bePipe)
                {
                    bePipe.RecalculateConnections(false);
                }
            }
        }

        private void Load()
        {
            byte[]? data = sapi?.WorldManager.SaveGame.GetData(StoreKey);
            if (data == null || data.Length == 0)
            {
                return;
            }
            linked.Clear();

            using MemoryStream memoryStream = new(data);
            using BinaryReader binaryReader = new(memoryStream);
            int count = binaryReader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                int x = binaryReader.ReadInt32();
                int y = binaryReader.ReadInt32();
                int z = binaryReader.ReadInt32();
                linked.Add(new BlockPos(x, y, z));
            }
        }

        private void Save()
        {
            using MemoryStream memoryStream = new();
            using BinaryWriter binaryWriter = new(memoryStream);
            binaryWriter.Write(linked.Count);
            foreach (BlockPos pos in linked)
            {
                binaryWriter.Write(pos.X);
                binaryWriter.Write(pos.Y);
                binaryWriter.Write(pos.Z);
            }
            binaryWriter.Flush();
            sapi?.WorldManager.SaveGame.StoreData(StoreKey, memoryStream.ToArray());
        }
    }
}
