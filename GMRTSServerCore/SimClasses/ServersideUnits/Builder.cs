using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMRTSServerCore.SimClasses.ServersideUnits
{
    /// <summary>
    /// The type representing a Builder unit.
    /// </summary>
    internal class Builder : Unit
    {
        public Builder(Guid id, User owner, Game game) : base(id, owner, game)
        {
            Health = 100;
        }

        /// <summary>
        /// Very WIP system. Builders cannot shoot, so this always returns false.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public override bool TryShoot(Unit target)
        {
            return false;
        }
    }
}
