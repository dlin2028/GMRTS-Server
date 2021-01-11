using GMRTSClasses.Units;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GMRTSServerCore.SimClasses
{
    internal class UnitConstructionOrder
    {
        internal MobileUnitType UnitType;
        internal Guid OrderID;
        internal bool Started = false;
        internal ulong StartMillisecond;
        internal ulong Duration;

        internal UnitConstructionOrder(MobileUnitType unitType, Guid id, ulong duration)
        {
            UnitType = unitType;
            OrderID = id;
            Duration = duration;
        }
    }
}
