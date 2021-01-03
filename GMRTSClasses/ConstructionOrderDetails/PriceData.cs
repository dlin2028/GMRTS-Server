using System;
using System.Collections.Generic;
using System.Text;

namespace GMRTSClasses.ConstructionOrderDetails
{
    public class PriceData
    {
        public readonly float RequiredMoney;
        public readonly float RequiredMineral;
        public readonly float RequiredSeconds;

        public PriceData(float money, float mineral, float time)
        {
            RequiredMineral = mineral;
            RequiredMoney = money;
            RequiredSeconds = time;
        }
    }
}
