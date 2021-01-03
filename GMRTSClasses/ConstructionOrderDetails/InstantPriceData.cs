using System;
using System.Collections.Generic;
using System.Text;

namespace GMRTSClasses.ConstructionOrderDetails
{
    public class InstantPriceData
    {
        public readonly float RequiredMoney;
        public readonly float RequiredMineral;

        public InstantPriceData(float money, float mineral)
        {
            RequiredMoney = money;
            RequiredMineral = mineral;
        }
    }
}
