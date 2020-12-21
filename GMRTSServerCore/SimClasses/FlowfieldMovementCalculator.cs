﻿using GMRTSServerCore.SimClasses.ServersideUnits;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GMRTSServerCore.SimClasses
{
    class FlowfieldMovementCalculator : IMovementCalculator
    {
        private static Vector2[] flowfieldVels = new Vector2[] { new Vector2(-0.707f, -0.707f), new Vector2(0, -1), new Vector2(0.707f, -0.707f), new Vector2(-1, 0), new Vector2(0, 0), new Vector2(1, 0), new Vector2(-0.707f, 0.707f), new Vector2(0, 1), new Vector2(0.707f, 0.707f) };

        static ConcurrentBag<float[][]> integCostsObjPool = new ConcurrentBag<float[][]>();

        static float RadTwo = 1.414213562373f;

        
        public Vector2 ComputeVelocity(Game game, Unit unit)
        {
            return MaxMagnitude(GetBoidsVelocity(game, unit) * unit.BoidsSettings.BoidsStrength, unit.BoidsSettings.MaximumBoidsVelocity);
        }

        static Vector2 MaxMagnitude(Vector2 vec, float maxMag)
        {
            if (vec.LengthSquared() <= maxMag * maxMag)
            {
                return vec;
            }

            return Vector2.Normalize(vec) * maxMag;
        }

        public Vector2 GetBoidsVelocity(Game game, Unit unit)
        {
            (int x, int y) = IMovementCalculator.fromVec2(unit.Position, game.Map.TileSize);

            int n = (int)unit.BoidsSettings.LargerDistance / game.Map.TileSize + 1;

            var unitsInSquare = game.unitPositionLookup.UnitsWithinManhattanTiles(x, y, n);

            Vector2 separationVelocity = Vector2.Zero;
            int sepCount = 0;

            Vector2 avgPos = Vector2.Zero;
            int neighborCount = 0;

            foreach(Unit other in unitsInSquare)
            {
                if (other == unit) continue;

                Vector2 posDiff = unit.Position - other.Position;
                float magSquare = posDiff.LengthSquared();

                if (magSquare < unit.BoidsSettings.SeparationDistanceSquared)
                {
                    separationVelocity += posDiff / Math.Max(magSquare, 0.01f) * unit.BoidsSettings.SeparationStrength;
                    sepCount++;
                }

                if (magSquare < unit.BoidsSettings.CohesionDistanceSquared)
                {
                    avgPos += other.Position;
                    neighborCount++;
                }
            }

            Vector2 totalVelocity = Vector2.Zero;

            if (sepCount > 0)
            {
                totalVelocity += MaxMagnitude(separationVelocity, unit.VelocityMagnitude);// / sepCount;
            }

            if (neighborCount > 0)
            {
                totalVelocity += (avgPos / neighborCount - unit.Position) * unit.BoidsSettings.CohesionStrength;
            }

            return totalVelocity;
        }

        public Vector2 ComputeVelocity(Game game, Unit unit, Vector2 destination)
        {
            Vector2 flowfieldVel;
            lock (game.FlowfieldLocker)
            {
                (int x, int y) tile = IMovementCalculator.fromVec2(destination, game.Map.TileSize);
                if (!game.Flowfields.ContainsKey(tile))
                {
                    game.Flowfields[tile] = ComputeFlowField(tile.x, tile.y, game.Map);
                    flowfieldVel = Vector2.Zero;
                }
                else if (!game.Flowfields[tile].IsCompleted)
                {
                    flowfieldVel = Vector2.Zero;
                }
                else
                {
                    byte[][] res = game.Flowfields[tile].Result;
                    (int x, int y) currTile = IMovementCalculator.fromVec2(unit.Position, game.Map.TileSize);
                    flowfieldVel = flowfieldVels[res[currTile.x][currTile.y]];
                }
            }

            flowfieldVel *= unit.BoidsSettings.FlowfieldStrength;

            // Boids, I think
            Vector2 boidsVel = MaxMagnitude(GetBoidsVelocity(game, unit) * unit.BoidsSettings.BoidsStrength, unit.BoidsSettings.MaximumBoidsVelocity);


            return MaxMagnitude(flowfieldVel + boidsVel, unit.VelocityMagnitude);
        }

        static async Task<byte[][]> ComputeFlowField(int x, int y, Map m)
        {
            float[][] integCosts;
            // Thread-safe object pool stuff so we aren't constantly allocating these.
            if (!integCostsObjPool.TryTake(out integCosts))
            {
                integCosts = new float[m.TilesOnSide][];
                for (int i = 0; i < integCosts.Length; i++)
                {
                    integCosts[i] = new float[m.TilesOnSide];
                }
            }


            for (int i = 0; i < integCosts.Length; i++)
            {
                for (int j = 0; j < integCosts[i].Length; j++)
                {
                    integCosts[i][j] = float.PositiveInfinity;
                }
            }

            // This is based on the code I wrote for the flowfield-experiments branch.
            // This should probably be implemented more efficiently.
            integCosts[x][y] = 0;
            var target = (x, y);

            FancyHeap<(int, int), float> queue = new FancyHeap<(int, int), float>();


            // I think this is basically Djikstra's
            queue.Enqueue(target, 0);

            while (queue.Count > 0)
            {
                (int x, int y) current = queue.Dequeue();

                int leftX = current.x > 0 ? current.x - 1 : 0;
                int topY = current.y > 0 ? current.y - 1 : 0;
                int rightX = (current.x < m.TilesOnSide - 1) ? (current.x + 1) : (m.TilesOnSide - 1);
                int bottomY = (current.y < m.TilesOnSide - 1) ? (current.y + 1) : (m.TilesOnSide - 1);

                for (int scanX = leftX; scanX <= rightX; scanX++)
                {
                    for (int scanY = topY; scanY <= bottomY; scanY++)
                    {
                        if ((scanX == current.x && scanY == current.y) || m[scanX, scanY] == ushort.MaxValue)
                        {
                            continue;
                        }

                        float costDelta = m[scanX, scanY];
                        if (scanX != current.x || scanY != current.y)
                        {
                            costDelta *= RadTwo;
                        }

                        float nextCost = integCosts[current.x][current.y] + costDelta;

                        if (nextCost < integCosts[scanX][scanY])
                        {
                            integCosts[scanX][scanY] = nextCost;

                            if (!queue.Contains((scanX, scanY)))
                            {
                                queue.Enqueue((scanX, scanY), nextCost);
                            }
                        }
                    }
                }

            }


            byte[][] vectorIndices = new byte[m.TilesOnSide][];

            // This bit is copy-pasted from the testing branch, then changed so it compiles
            // I didn't see a way to make this vastly more efficient

            //Now find the actual flowfields

            for (int x1 = 0; x1 < m.TilesOnSide; x1++)
            {
                vectorIndices[x1] = new byte[m.TilesOnSide];
                for (int y1 = 0; y1 < m.TilesOnSide; y1++)
                {
                    if (m[(x1, y1)] == ushort.MaxValue)
                    {
                        continue;
                    }

                    float min = float.PositiveInfinity;
                    byte minDX = 0;
                    byte minDY = 0;

                    for (int dx = x1 > 0 ? (-1) : 0, dxMax = x1 < m.TilesOnSide - 1 ? 1 : 0; dx <= dxMax; dx++)
                    {
                        for (int dy = y1 > 0 ? (-1) : 0, dyMax = y1 < m.TilesOnSide - 1 ? 1 : 0; dy <= dyMax; dy++)
                        {
                            if (m[(x1 + dx, y1 + dy)] == ushort.MaxValue)
                            {
                                continue;
                            }

                            if (integCosts[x1 + dx][y1 + dy] < min)
                            {
                                min = integCosts[x1 + dx][y1 + dy];
                                minDX = (byte)dx;
                                minDY = (byte)dy;
                            }

                        }
                    }

                    vectorIndices[x1][y1] = (byte)(minDX + 3 * minDY + 4);
                }
            }



            integCostsObjPool.Add(integCosts);
            return vectorIndices;
        }
    }
}
