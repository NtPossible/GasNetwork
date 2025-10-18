using Vintagestory.API.MathTools;

namespace GasNetwork.src.Interfaces
{
    public interface IPipeConnectable
    {
        bool CanAcceptPipeAt(BlockFacing face);
        PipeChannel Channels { get; }
    }
}
