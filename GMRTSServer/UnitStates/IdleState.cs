using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMRTSServer.UnitStates
{
    internal class IdleState : IUnitState
    {
        public IUnitState Update(ulong currentMilliseconds, float elapsedTime)
        {
            return this;
        }
    }
}
