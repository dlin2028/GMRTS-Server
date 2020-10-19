using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMRTSServer
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
    }
}
