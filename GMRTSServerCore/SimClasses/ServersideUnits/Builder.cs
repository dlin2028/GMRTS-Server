using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMRTSServerCore.SimClasses.ServersideUnits
{
    internal class Builder : Unit
    {
        public Builder(Guid id) : base(id)
        {
            Health = 100;
        }


        public override bool TryShoot(Unit target)
        {
            throw new NotImplementedException();
        }
    }
}
