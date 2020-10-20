﻿using GMRTSServer.ServersideUnits;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GMRTSServer
{
    class FlowfieldMovementCalculator : IMovementCalculator
    {
        private static Vector2[] flowfieldVels = new Vector2[] { new Vector2(-0.707f, -0.707f), new Vector2(0, -1), new Vector2(0.707f, -0.707f), new Vector2(-1, 0), new Vector2(0, 0), new Vector2(1, 0), new Vector2(-0.707f, 0.707f), new Vector2(0, 1), new Vector2(0.707f, 0.707f) };

        static ConcurrentBag<float[][]> integCostsObjPool = new ConcurrentBag<float[][]>();

        private (int x, int y) fromVec2(Vector2 vec, int tileSize) => ((int)vec.X / tileSize, (int)vec.Y / tileSize);

        public Vector2 ComputeVelocity(Game game, Unit unit, Vector2 destination)
        {
            Vector2 flowfieldVel;
            lock (game.FlowfieldLocker)
            {
                (int x, int y) tile = fromVec2(destination, game.Map.TileSize);
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
                    (int x, int y) currTile = fromVec2(unit.Position, game.Map.TileSize);
                    flowfieldVel = flowfieldVels[res[currTile.x][currTile.y]];
                }
            }

            //MANY MANY OTHER THINGS (boids)

            return flowfieldVel;
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

            queue.Enqueue(target, 0);

            while (queue.Count > 0)
            {
                (int x, int y) current = queue.Dequeue();
                //VERY WIP, as you can see
            }

            integCostsObjPool.Add(integCosts);
            throw new NotImplementedException();
        }
    }
}