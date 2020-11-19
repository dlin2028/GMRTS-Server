using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMRTSServerCore.SimClasses.ServersideUnits
{
    internal class Tank : Unit
    {
        public Tank(Guid id) : base(id)
        {

        }

        public override bool TryShoot(Unit target)
        {
            throw new NotImplementedException();
        }
    }
}
