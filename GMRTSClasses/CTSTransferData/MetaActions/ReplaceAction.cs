using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMRTSClasses.CTSTransferData.MetaActions
{
    public class ReplaceAction : MetaAction
    {
        public ClientAction NewAction { get; set; }
    }
}
