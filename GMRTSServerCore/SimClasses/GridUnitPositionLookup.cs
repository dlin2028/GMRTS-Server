using GMRTSServerCore.SimClasses.ServersideUnits;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace GMRTSServerCore.SimClasses
{
    internal class GridUnitPositionLookup : IUnitPositionLookup
    {
        public Game Game { get; private set; }

        List<Unit>[][] Units;

        public IEnumerable<Unit> UnitsWithinCircular(Vector2 pos, int dist)
        {
            (int x, int y) = IMovementCalculator.fromVec2(pos, Game.Map.TileSize);

            int n = dist / Game.Map.TileSize + 1;

            int distSquared = dist * dist;

            return UnitsWithinManhattanTiles(x, y, n).Where(a => (a.Position - pos).LengthSquared() <= distSquared);
        }

        public IEnumerable<Unit> UnitsWithinManhattanTiles(int x, int y, int n)
        {
            for (int i = x - n; i <= x + n; i++)
            {
                for (int j = y - n; j <= y + n; j++)
                {
                    if (!(i >= 0 && i < Game.Map.TilesOnSide && j >= 0 && j < Game.Map.TilesOnSide))
                    {
                        continue;
                    }

                    foreach (Unit unit in Units[i][j])
                    {
                        yield return unit;
                    }
                }
            }
        }

        public void Update(Unit unit, Vector2 newPosition)
        {
            (int oldX, int oldY) = IMovementCalculator.fromVec2(unit.Position, Game.Map.TileSize);
            (int x, int y) = IMovementCalculator.fromVec2(newPosition, Game.Map.TileSize);

            if (oldX == x && oldY == y)
            {
                return;
            }

            if (oldX >= 0 && oldX < Game.Map.TilesOnSide && oldY >= 0 && oldY < Game.Map.TilesOnSide)
            {
                Units[oldX][oldY].Remove(unit);
            }


            if (x >= 0 && x < Game.Map.TilesOnSide && y >= 0 && y < Game.Map.TilesOnSide)
            {
                Units[x][y].Add(unit);
            }
        }

        public GridUnitPositionLookup(Game game)
        {
            Game = game;
            Units = new List<Unit>[game.Map.TilesOnSide][];
            for (int i = 0; i < Units.Length; i++)
            {
                Units[i] = new List<Unit>[game.Map.TilesOnSide];
                for (int j = 0; j < Units[i].Length; j++)
                {
                    Units[i][j] = new List<Unit>();
                }
            }

            Recalculate();
        }

        public void Recalculate()
        {
            for (int i = 0; i < Units.Length; i++)
            {
                for (int j = 0; j < Units[i].Length; j++)
                {
                    Units[i][j].Clear();
                }
            }

            foreach (Unit unit in Game.Units.Values)
            {
                (int x, int y) = IMovementCalculator.fromVec2(unit.Position, Game.Map.TileSize);
                Units[x][y].Add(unit);
            }
        }
    }
}
