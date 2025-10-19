using GasNetwork.src.BE;
using System.Collections.Generic;
using System.IO;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;

namespace GasNetwork.src.System
{
    public class GasLinkRegistrySystem : ModSystem
    {
        private readonly HashSet<BlockPos> linked = [];
        private ICoreServerAPI sapi;

        const string StoreKey = "gasnetwork.linkedpositions.bin";

        public override void StartServerSide(ICoreServerAPI api)
        {
            sapi = api;
            sapi.Event.SaveGameLoaded += Load;
            sapi.Event.GameWorldSave += Save;
        }

        public bool IsLinked(IWorldAccessor world, BlockPos pos)
        {
            if (!linked.Contains(pos))
            {
                return false;
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

        public void RefreshNeighborPipes(IWorldAccessor world, BlockPos pos)
        {
            foreach (BlockFacing face in BlockFacing.ALLFACES)
            {
                BlockPos beighbourPos = pos.AddCopy(face);
                if (world.BlockAccessor.GetBlockEntity(beighbourPos) is BlockEntityPipe bePipe)
                {
                    bePipe.RecalculateConnections(true);
                    world.BlockAccessor.MarkBlockDirty(beighbourPos);
                }
            }
            world.BlockAccessor.MarkBlockDirty(pos);
        }

        private void Load()
        {
            byte[] data = sapi.WorldManager.SaveGame.GetData(StoreKey);
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
            sapi.WorldManager.SaveGame.StoreData(StoreKey, memoryStream.ToArray());
        }
    }
}
