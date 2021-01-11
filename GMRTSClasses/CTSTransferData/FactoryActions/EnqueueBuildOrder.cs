using GMRTSClasses.Units;

using System;
using System.Collections.Generic;
using System.Text;

namespace GMRTSClasses.CTSTransferData.FactoryActions
{
    public class EnqueueBuildOrder : FactoryAction
    {
        public MobileUnitType UnitType { get; set; }
        public Guid OrderID { get; set; }
    }
}
