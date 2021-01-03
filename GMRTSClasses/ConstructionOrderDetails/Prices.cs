using GMRTSClasses.CTSTransferData;
using GMRTSClasses.Units;

using System;
using System.Collections.Generic;
using System.Text;

namespace GMRTSClasses.ConstructionOrderDetails
{
    public static class Prices
    {
        public static readonly IReadOnlyDictionary<MobileUnitType, PriceData> UnitPriceData = new Dictionary<MobileUnitType, PriceData>()
        {
            [MobileUnitType.Builder] = new PriceData(money: 50f, mineral: 20f, time: 5f),
            [MobileUnitType.Tank] = new PriceData(money: 20f, mineral: 30f, time: 2f)
        };

        public static readonly IReadOnlyDictionary<BuildingType, InstantPriceData> BuildingPriceData = new Dictionary<BuildingType, InstantPriceData>()
        {
            [BuildingType.Factory] = new InstantPriceData(money: 30f, mineral: 30f),
            [BuildingType.Mine] = new InstantPriceData(money: 50f, mineral: 0f),
            [BuildingType.Supermarket] = new InstantPriceData(money: 0f, mineral: 30f)
        };
    }
}
