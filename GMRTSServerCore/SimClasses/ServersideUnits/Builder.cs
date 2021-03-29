using GMRTSServerCore.SimClasses.ServersideUnits.Interfaces;

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
    internal class Builder : Unit, IMobileUnit
    {
        public Builder(Guid id, User owner) : base(id, owner)
        {
            Health = 100;
        }

        /// <summary>
        /// Very WIP system. Builders cannot shoot.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public override void Shoot(Unit target)
        {
            throw new InvalidOperationException("Builders can't attack.");
        }
    }
}
