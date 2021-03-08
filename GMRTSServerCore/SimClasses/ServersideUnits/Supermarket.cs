using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GMRTSServerCore.SimClasses.ServersideUnits
{
    /// <summary>
    /// Represents markets.
    /// </summary>
    internal class Supermarket : Building
    {
        public const float MoneyPerSecond = 10f;

        public Supermarket(Guid id, User owner) : base(id, owner)
        {
        }

        /// <summary>
        /// Earns a little bit of money for its owner.
        /// </summary>
        /// <param name="currentMilliseconds"></param>
        /// <param name="elapsedTime"></param>
        public override void Update(ulong currentMilliseconds, float elapsedTime)
        {
            Owner.Money += MoneyPerSecond * elapsedTime;

            base.Update(currentMilliseconds, elapsedTime);
        }
    }
}
