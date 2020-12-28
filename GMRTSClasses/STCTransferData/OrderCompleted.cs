using System;
using System.Collections.Generic;
using System.Text;

namespace GMRTSClasses.STCTransferData
{
    public class OrderCompleted
    {
        public Guid FactoryID { get; set; }
        public Guid OrderID { get; set; }
    }
}
