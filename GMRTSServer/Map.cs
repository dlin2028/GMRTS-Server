using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMRTSServer
{
    class Map
    {
        public Dictionary<(int x, int y), ushort> Costs = new Dictionary<(int x, int y), ushort>();
    }
}
