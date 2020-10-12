using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMRTSClasses.CTSTransferData.UnitGround
{
    public class MoveAction : UnitGroundAction
    {
        /// <summary>
        /// True for patrol behavior, where the path repeats in a loop
        /// </summary>
        public bool RequeueOnCompletion { get; set; }
    }
}
