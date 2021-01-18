using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMRTSClasses.CTSTransferData.MetaActions
{
    public class ReplaceAction<T> : MetaAction where T : ClientAction
    {
        public T NewAction { get; set; }
    }
}
