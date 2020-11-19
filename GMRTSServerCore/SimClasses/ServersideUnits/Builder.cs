using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMRTSServerCore.SimClasses.ServersideUnits
{
    internal class Builder : Unit
    {
        public Builder(Guid id, User owner, Game game) : base(id, owner, game)
        {
            Health = 100;
        }


        public override bool TryShoot(Unit target)
        {
            throw new NotImplementedException();
        }
    }
}
