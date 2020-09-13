using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMRTSServer.UnitStates
{
    internal interface IUnitState
    {
        IUnitState Update(ulong currentMilliseconds, float elapsedTime);
    }
}
