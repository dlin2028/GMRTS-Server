using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMRTSServerCore.SimClasses
{
    class Map
    {
        private Dictionary<(int x, int y), ushort> Costs = new Dictionary<(int x, int y), ushort>();

        public int TileSize = 10;
        public int TilesOnSide = 80;

        public ushort this[int x, int y]
        {
            get => this[(x, y)];
            set => this[(x, y)] = value;
        }

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

        static Map FromImageFile(string filename)
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

                    map[(x, y)] = (ushort)((255 - bmp.GetPixel(x, y).R) * ushort.MaxValue / 255);
                }
            }

            return map;
        }
    }
}
