using GMRTSServerCore.SimClasses.ServersideUnits;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace GMRTSServerCore.SimClasses
{
    internal interface IUnitPositionLookup
    {
        void Update(Unit unit, Vector2 newPosition);
        Game Game { get; }
        IEnumerable<Unit> UnitsWithinManhattanTiles(int x, int y, int n);
        IEnumerable<Unit> UnitsWithinCircular(Vector2 pos, int dist);
    }
}
