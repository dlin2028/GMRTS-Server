using System;
using System.Collections.Generic;
using System.Text;

namespace GMRTSClasses.CTSTransferData.FactoryActions
{
    public class CancelBuildOrder : FactoryAction
    {
        public Guid OrderID { get; set; }
    }
}
