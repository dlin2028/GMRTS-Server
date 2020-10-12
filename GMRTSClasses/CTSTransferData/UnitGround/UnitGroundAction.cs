using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GMRTSClasses.CTSTransferData.UnitGround
{
    public abstract class UnitGroundAction : ClientAction
    {
        public Vector2 Position { get; set; }
    }
}
