using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMRTSServerCore.SimClasses
{
    /// <summary>
    /// Represents a game map.
    /// </summary>
    class Map
    {
        /// <summary>
        /// The costs of each tile. Only stores non-default (non-1) values, leaving an absence of a value to indicate default (1).
        /// </summary>
        private Dictionary<(int x, int y), ushort> Costs = new Dictionary<(int x, int y), ushort>();

        /// <summary>
        /// The size of a tile.
        /// </summary>
        public int TileSize = 10;

        /// <summary>
        /// Width (and height, the maps are square) of the map in tiles.
        /// </summary>
        public int TilesOnSide = 80;

        /// <summary>
        /// I'm too lazy to use a consistent notation when using the indexer.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public ushort this[int x, int y]
        {
            get => this[(x, y)];
            set => this[(x, y)] = value;
        }

        /// <summary>
        /// Lets us treat the map as a contiguous 2d array of costs, even though it's not.
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        public ushort this[(int x, int y) b]
        {
            get
            {
                if (Costs.ContainsKey(b))
                {
                    return Costs[b];
                }

                return 1;
            }
            set
            {
                // Ensures we store no default costs in the dictionary.
                if (value == 1)
                {
                    Costs.Remove(b);
                    return;
                }

                if (Costs.ContainsKey(b))
                {
                    Costs[b] = value;
                    return;
                }

                Costs.Add(b, value);
            }
        }

        /// <summary>
        /// Lets me draw a map in MS Paint.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        internal static Map FromImageFile(string filename)
        {
            Bitmap bmp = new Bitmap(filename);

            Map map = new Map();

            if (bmp.Width != bmp.Height)
            {
                throw new Exception("Only square maps supported now, sorry");
            }

            map.TilesOnSide = bmp.Width;

            for (int x = 0; x < bmp.Width; x++)
            {
                for (int y = 0; y < bmp.Height; y++)
                {
                    Color color = bmp.GetPixel(x, y);
                    if (!(color.R == color.G && color.G == color.B))
                    {
                        throw new Exception("Only grayscale images can be maps");
                    }

                    if (color.R == 255) continue;

                    map[(x, y)] = (ushort)((255 - bmp.GetPixel(x, y).R) * ushort.MaxValue / 255);
                }
            }

            return map;
        }
    }
}
