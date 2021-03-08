using GMRTSClasses.ConstructionOrderDetails;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GMRTSServerCore.SimClasses.ServersideUnits
{
    /// <summary>
    /// Represents factories.
    /// </summary>
    internal class Factory : Building
    {
        /// <summary>
        /// Queue for orders this factory is executing.
        /// It's a linked list so we can remove from the middle.
        /// </summary>
        internal LinkedList<UnitConstructionOrder> ConstructionOrders;
        public Factory(Guid id, User owner) : base(id, owner)
        {
            ConstructionOrders = new LinkedList<UnitConstructionOrder>();
        }

        /// <summary>
        /// Basically updates the order first in the queue and completes it if it's done.
        /// </summary>
        /// <param name="currentMilliseconds"></param>
        /// <param name="elapsedTime"></param>
        public override void Update(ulong currentMilliseconds, float elapsedTime)
        {
            // If we have no orders, no logic to do.
            if (ConstructionOrders.Count == 0)
            {
                base.Update(currentMilliseconds, elapsedTime);
                return;
            }

            UnitConstructionOrder order = ConstructionOrders.First.Value;

            // Start it if it hasn't started already.
            if (!order.Started)
            {
                order.Started = true;
                // Record the start time so we will know how long has passed.
                order.StartMillisecond = currentMilliseconds;
            }

            // If the order should be done, complete it and move on to the next one.
            if (currentMilliseconds - order.StartMillisecond > order.Duration)
            {
                Owner.CurrentGame.SpawnUnit(order.UnitType, Position, Owner);

                // If it's done, we want the client to know so it will stop displaying the order as in-progress.
                Owner.CurrentGame.TellOwnerOrderFinished(Owner, ID, order.OrderID);

                ConstructionOrders.RemoveFirst();
            }

            base.Update(currentMilliseconds, elapsedTime);
        }

        /// <summary>
        /// Enqueues a new order.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="unitType"></param>
        /// <returns></returns>
        public bool TryEnqueue(Guid id, GMRTSClasses.Units.MobileUnitType unitType)
        {
            if (ConstructionOrders.Any(a => a.OrderID == id))
            {
                return false;
            }

            ConstructionOrders.AddLast(new UnitConstructionOrder(unitType, id, (ulong)(Prices.UnitPriceData[unitType].RequiredSeconds * 1000)));

            return true;
        }

        /// <summary>
        /// Cancels an order.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Will retrieve an order by ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
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
