using GMRTSServerCore.SimClasses.ServersideUnits;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GMRTSServerCore.SimClasses
{
    internal interface IVisibilityChecker
    {
        Unit Owner { get; }
        void Update();
        bool CanSee(Unit target);
    }
}
