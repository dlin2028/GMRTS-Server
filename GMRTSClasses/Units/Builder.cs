using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMRTSClasses.Units
{
    public class Builder : Unit
    {
        public Builder() : base()
        {
        }

        public Builder(Guid id) : base(id)
        {
        }

        public override void Update(ulong currentMilliseconds)
        {
            base.Update(currentMilliseconds);
        }
    }
}
