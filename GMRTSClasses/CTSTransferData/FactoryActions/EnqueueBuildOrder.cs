using System;
using System.Collections.Generic;
using System.Text;

namespace GMRTSClasses.CTSTransferData.FactoryActions
{
    public class EnqueueBuildOrder : FactoryAction
    {
        public string UnitType { get; set; }
        public Guid OrderID { get; set; }
    }
}
