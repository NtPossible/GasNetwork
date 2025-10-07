using Vintagestory.API.MathTools;

namespace GasNetwork.src
{
    public interface IPipeConnectable
    {
        bool CanAcceptPipeAt(BlockFacing face);
        PipeChannel Channels { get; }
    }
}
