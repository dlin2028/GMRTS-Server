using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMRTSClasses.CTSTransferData.MetaActions
{
    public class MetaAction
    {
        public Guid TargetActionID { get; set; }

        public List<Guid> AffectedUnits { get; set; }
    }
}
