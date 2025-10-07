using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
