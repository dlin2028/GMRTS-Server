using GMRTSServerCore.SimClasses.ServersideUnits;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GMRTSServerCore.SimClasses
{
    interface IMovementCalculator
    {
        Vector2 ComputeVelocity(Game game, Unit unit, Vector2 destination);
    }
}
