using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GMRTSServerCore.SimClasses.ServersideUnits
{
    internal class Factory : Building
    {
        public Factory(Guid id, User owner, Game game) : base(id, owner, game)
        {
        }

        public override bool TryShoot(Unit target)
        {
            return false;
        }

        public override void Update(ulong currentMilliseconds, float elapsedTime)
        {
            throw new NotImplementedException();

            base.Update(currentMilliseconds, elapsedTime);
        }
    }
}
