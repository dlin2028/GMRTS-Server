using GMRTSServerCore.SimClasses.ServersideUnits;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GMRTSServerCore.SimClasses
{
    /// <summary>
    /// Defines things that provide movement calculations.
    /// </summary>
    interface IMovementCalculator
    {
        Vector2 ComputeVelocity(Game game, Unit unit, Vector2 destination);
        Vector2 ComputeVelocity(Game game, Unit unit);

        internal static (int x, int y) fromVec2(Vector2 vec, int tileSize) => ((int)vec.X / tileSize, (int)vec.Y / tileSize);
    }
}
