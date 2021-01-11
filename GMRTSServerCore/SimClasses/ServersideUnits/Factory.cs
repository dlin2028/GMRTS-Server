using GMRTSClasses.ConstructionOrderDetails;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GMRTSServerCore.SimClasses.ServersideUnits
{
    internal class Factory : Building
    {

        internal LinkedList<UnitConstructionOrder> ConstructionOrders;
        public Factory(Guid id, User owner, Game game) : base(id, owner, game)
        {
            ConstructionOrders = new LinkedList<UnitConstructionOrder>();
        }

        public override bool TryShoot(Unit target)
        {
            return false;
        }

        public override void Update(ulong currentMilliseconds, float elapsedTime)
        {
            if (ConstructionOrders.Count == 0)
            {
                base.Update(currentMilliseconds, elapsedTime);
                return;
            }

            UnitConstructionOrder order = ConstructionOrders.First.Value;

            if (!order.Started)
            {
                order.Started = true;
                order.StartMillisecond = currentMilliseconds;
            }

            if (currentMilliseconds - order.StartMillisecond > order.Duration)
            {
                Owner.CurrentGame.SpawnUnit(order.UnitType, Position, Owner);

                Owner.CurrentGame.TellOwnerOrderFinished(Owner, ID, order.OrderID);

                ConstructionOrders.RemoveFirst();
            }

            base.Update(currentMilliseconds, elapsedTime);
        }

        public bool TryEnqueue(Guid id, GMRTSClasses.Units.MobileUnitType unitType)
        {
            if (ConstructionOrders.Any(a => a.OrderID == id))
            {
                return false;
            }

            ConstructionOrders.AddLast(new UnitConstructionOrder(unitType, id, (ulong)(Prices.UnitPriceData[unitType].RequiredSeconds * 1000)));

            return true;
        }

        public bool TryCancel(Guid id)
        {
            LinkedListNode<UnitConstructionOrder> node;
            for(node = ConstructionOrders.First; node != null; node = node.Next)
            {
                if (node.Value.OrderID == id)
                {
                    break;
                }
            }

            if (node == null)
            {
                return false;
            }

            ConstructionOrders.Remove(node);
            return true;
        }

        internal UnitConstructionOrder GetOrder(Guid id)
        {
            LinkedListNode<UnitConstructionOrder> node;
            for (node = ConstructionOrders.First; node != null; node = node.Next)
            {
                if (node.Value.OrderID == id)
                {
                    return node.Value;
                }
            }

            return null;
        }
    }
}
