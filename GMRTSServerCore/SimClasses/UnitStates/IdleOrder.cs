using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMRTSServerCore.SimClasses.UnitStates
{
    internal class IdleOrder : IUnitOrder
    {
        public Guid ID { get; set; }

        public ContOrStop Update(ulong currentMilliseconds, float elapsedTime)
        {
            return ContOrStop.Continue;
        }
    }
}
