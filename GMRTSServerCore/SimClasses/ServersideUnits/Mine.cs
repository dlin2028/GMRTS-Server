using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GMRTSServerCore.SimClasses.ServersideUnits
{
    internal class Mine : Building
    {
        public const float MineralPerSecond = 10f;

        public Mine(Guid id, User owner, Game game) : base(id, owner, game)
        {
        }

        public override void Update(ulong currentMilliseconds, float elapsedTime)
        {
            Owner.Mineral += MineralPerSecond * elapsedTime;

            base.Update(currentMilliseconds, elapsedTime);
        }
    }
}
