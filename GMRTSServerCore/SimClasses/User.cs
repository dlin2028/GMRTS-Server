using GMRTSServerCore.SimClasses.ServersideUnits;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GMRTSServerCore.SimClasses
{
    internal class User
    {
        public string ID { get; set; }

        public string CurrentUsername { get; set; }
        public Game CurrentGame { get; set; }
        public List<Unit> Units { get; set; } = new List<Unit>();

        public User(string id)
        {
            ID = id;
        }
    }
}
