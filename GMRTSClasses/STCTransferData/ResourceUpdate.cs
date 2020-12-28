using System;
using System.Collections.Generic;
using System.Text;

namespace GMRTSClasses.STCTransferData
{
    public class ResourceUpdate
    {
        public ResourceType ResourceType { get; set; }
        public Changing<float> Value { get; set; }
    }
}
