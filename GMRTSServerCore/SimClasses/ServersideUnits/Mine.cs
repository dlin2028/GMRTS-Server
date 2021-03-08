using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GMRTSServerCore.SimClasses.ServersideUnits
{
    /// <summary>
    /// Represents mines.
    /// </summary>
    internal class Mine : Building
    {
        public const float MineralPerSecond = 10f;

        public Mine(Guid id, User owner) : base(id, owner)
        {
        }

        /// <summary>
        /// Mines a little bit of mineral for its owner.
        /// </summary>
        /// <param name="currentMilliseconds"></param>
        /// <param name="elapsedTime"></param>
        public override void Update(ulong currentMilliseconds, float elapsedTime)
        {
            Owner.Mineral += MineralPerSecond * elapsedTime;

            base.Update(currentMilliseconds, elapsedTime);
        }
    }
}
