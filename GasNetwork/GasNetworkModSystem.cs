using GasNetwork.src.BB;
using GasNetwork.src.BE;
using GasNetwork.src.Blocks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;

namespace GasNetwork
{
    public class GasNetworkModSystem : ModSystem
    {

        public override void Start(ICoreAPI api)
        {
            api.RegisterBlockClass($"{Mod.Info.ModID}:BlockPipe", typeof(BlockPipe));
            api.RegisterBlockEntityClass($"{Mod.Info.ModID}:BEPipe", typeof(BlockEntityPipe));

            api.RegisterBlockClass($"{Mod.Info.ModID}:BlockGasifier", typeof(BlockGasifier));
            api.RegisterBlockEntityClass($"{Mod.Info.ModID}:BEGasifier", typeof(BlockEntityGasifier));
            api.RegisterBlockBehaviorClass($"{Mod.Info.ModID}:BBGasifier", typeof(BlockBehaviorGasifier));

            api.RegisterBlockClass($"{Mod.Info.ModID}:BlockGasifierTank", typeof(BlockGasifierTank));
            api.RegisterBlockEntityClass($"{Mod.Info.ModID}:BEGasifierTank", typeof(BlockEntityGasifierTank));

            api.RegisterBlockClass($"{Mod.Info.ModID}:BlockGasLamp", typeof(BlockGasLamp));
        }
    }
}
