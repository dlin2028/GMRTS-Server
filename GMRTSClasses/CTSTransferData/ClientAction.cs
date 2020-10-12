using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMRTSClasses.CTSTransferData
{
    public abstract class ClientAction
    {
        public Guid ActionID { get; set; }
        public List<Guid> UnitIDs { get; set; }
    }
}
