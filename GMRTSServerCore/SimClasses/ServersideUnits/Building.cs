using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GMRTSServerCore.SimClasses.ServersideUnits
{
    /// <summary>
    /// Base class for buildings.
    /// </summary>
    internal class Building : Unit
    {
        public Building(Guid id, User owner, Game game) : base(id, owner, game)
        {
            Health = 100;
        }


        /// <summary>
        /// WIP but Buildings cannot shoot, last I checked.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public override void Shoot(Unit target) { }
    }
}
