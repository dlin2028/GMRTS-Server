using GMRTSServerCore.SimClasses.ServersideUnits;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GMRTSServerCore.SimClasses
{
    interface ICombatTargetTracker
    {
        Unit Owner { get; }
        Unit Target { get; }

        IVisibilityChecker VisibilityChecker { get; }

        Unit PreferredTarget { get; set; }

        void Update();
    }
}
