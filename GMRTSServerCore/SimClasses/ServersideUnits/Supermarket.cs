using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GMRTSServerCore.SimClasses.ServersideUnits
{
    internal class Supermarket : Building
    {
        public Supermarket(Guid id, User owner, Game game) : base(id, owner, game)
        {
        }

        public override void Update(ulong currentMilliseconds, float elapsedTime)
        {
            throw new NotImplementedException();

            base.Update(currentMilliseconds, elapsedTime);
        }
    }
}
