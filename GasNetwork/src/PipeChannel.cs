using System;

namespace GasNetwork.src
{
    [Flags]
    public enum PipeChannel
    {
        None = 0,
        Regular = 1,
        Thin = 2,
        All = Regular | Thin
    }
}
