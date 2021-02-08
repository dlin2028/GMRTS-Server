using GMRTSClasses.ConstructionOrderDetails;
using GMRTSClasses.CTSTransferData;
using GMRTSClasses.CTSTransferData.FactoryActions;
using GMRTSClasses.CTSTransferData.MetaActions;
using GMRTSClasses.CTSTransferData.UnitGround;
using GMRTSClasses.CTSTransferData.UnitUnit;
using GMRTSClasses.STCTransferData;

using GMRTSServerCore.Hubs;
using GMRTSServerCore.SimClasses.ServersideUnits;
using GMRTSServerCore.SimClasses.UnitStates;

using Microsoft.AspNetCore.SignalR;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

namespace GMRTSServerCore.SimClasses
{
    /// <summary>
    /// The actual game.
    /// </summary>
    internal class Game
    {
        /// <summary>
        /// All the users in the game.
        /// </summary>
        private List<User> Users { get; set; } = new List<User>();

        /// <summary>
        /// Pretty sure we don't need this.
        /// </summary>
        internal object FlowfieldLocker = new object();

        /// <summary>
        /// Yay flowfields. The flowfields computed by the FlowfieldMovementCalculator.
        /// </summary>
        internal Dictionary<(int x, int y), Task<byte[][]>> Flowfields = new Dictionary<(int x, int y), Task<byte[][]>>();

        /// <summary>
        /// The movement calculator. Yay for modularization!
        /// </summary>
        internal IMovementCalculator movementCalculator;

        /// <summary>
        /// A tool for looking up units by area. For faster neighbor checking.
        /// </summary>
        internal IUnitPositionLookup unitPositionLookup;

        /// <summary>
        /// The map the game is taking place on. We use this for pathfinding.
        /// </summary>
        internal Map Map;

        object locker = new object();

        /// <summary>
        /// Adds users and gives them their starting units.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public bool AddUser(User user)
        {
            if (Users.Any(a => a.CurrentUsername == user.CurrentUsername))
            {
                return false;
            }

            Users.Add(user);
            user.Money = 100;
            user.Mineral = 100;

            Unit unit = new Builder(Guid.NewGuid(), user, this);
            user.Units.Add(unit);
            Units.Add(unit.ID, unit);
            user.CurrentGame = this;
            return true;
        }

        /// <summary>
        /// Removes a user.
        /// </summary>
        /// <param name="user"></param>
        public void RemoveUser(User user)
        {
            Users.Remove(user);
        }

        public Game(IHubContext<GameHub> context)
        {
            Map = new Map();
            Context = context;
            movementCalculator = new FlowfieldMovementCalculator();
            unitPositionLookup = new GridUnitPositionLookup(this);
        }

        /// <summary>
        /// This is called by units when their position updates and it just calls the update on the actual lookup object.
        /// </summary>
        /// <param name="unit"></param>
        /// <param name="newPos"></param>
        internal void UpdateUnitLookup(Unit unit, Vector2 newPos)
        {
            unitPositionLookup.Update(unit, newPos);
        }

        public int UserCount => Users.Count;

        public Stopwatch Stopwatch { get; set; } = new Stopwatch();

        public IHubContext<GameHub> Context { get; set; }

        public const float UnitSightRangeSquared = 100 * 100;

        //public Dictionary<User, List<Unit>> Units { get; set; } = new Dictionary<User, List<Unit>>();
        public Dictionary<Guid, Unit> Units { get; set; } = new Dictionary<Guid, Unit>();
        private long currentMillis = 0;

        private ConcurrentBag<(Unit, Guid)> ActionOversToSend = new ConcurrentBag<(Unit, Guid)>();

        private List<Unit> ToSpawn = new List<Unit>();
        private ConcurrentBag<(User, OrderCompleted)> OrderCompletedsToSend = new ConcurrentBag<(User, OrderCompleted)>();

        private void Remove(Unit unit)
        {
            unit.Owner.Units.Remove(unit);
            Units.Remove(unit.ID);
        }

        /// <summary>
        /// Gets the units from the list of IDs that are owned by the specified user and exist.
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public List<Unit> GetValidUnits(List<Guid> ids, User user)
        {
            List<Unit> units = new List<Unit>(ids.Count);
            foreach (Guid unitID in ids)
            {
                if (!Units.ContainsKey(unitID))
                {
                    continue;
                }

                Unit unit = Units[unitID];
                if (unit.Owner != user)
                {
                    continue;
                }

                units.Add(unit);
            }

            return units;
        }

        /// <summary>
        /// Creates a building, schedules its information to be sent to the user at the next update, and removes the cost of it from the user's resources.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="buildingType"></param>
        /// <param name="position"></param>
        internal void SpawnBuildingAndChargeUser(User user, BuildingType buildingType, Vector2 position)
        {
            user.Money -= Prices.BuildingPriceData[buildingType].RequiredMoney;
            user.Mineral -= Prices.BuildingPriceData[buildingType].RequiredMineral;
            Building building = buildingType switch
            {
                BuildingType.Factory => new Factory(Guid.NewGuid(), user, user.CurrentGame),
                BuildingType.Mine => new Mine(Guid.NewGuid(), user, user.CurrentGame),
                BuildingType.Supermarket => new Supermarket(Guid.NewGuid(), user, user.CurrentGame),
                _ => throw new Exception(),
            };

            building.Position = position;
            building.PositionUpdate = new ChangingData<Vector2>(0, position, Vector2.Zero);
            building.UpdatePosition = true;

            ToSpawn.Add(building);
        }

        /// <summary>
        /// Finds linked list nodes for orders to be replaced or deleted.
        /// </summary>
        /// <param name="units"></param>
        /// <param name="actionToReplace"></param>
        /// <returns></returns>
        private static List<(LinkedListNode<IUnitOrder>, Unit)> GetOrderNodesToReplace(List<Unit> units, Guid actionToReplace)
        {
            List<(LinkedListNode<IUnitOrder>, Unit)> linkedListNodes = new List<(LinkedListNode<IUnitOrder>, Unit)>(units.Count);
            foreach (Unit unit in units)
            {
                LinkedListNode<IUnitOrder> nodeToReplace = null;
                var node = unit.Orders.First;
                while (node != null)
                {
                    if (node.Value.ID == actionToReplace)
                    {
                        nodeToReplace = node;
                        break;
                    }
                    node = node.Next;
                }

                if (nodeToReplace == null)
                {
                    continue;
                }

                linkedListNodes.Add((nodeToReplace, unit));
            }
            return linkedListNodes;
        }

        /// <summary>
        /// Replaces one node with another order.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="newOrder"></param>
        private static void ReplaceNode(LinkedListNode<IUnitOrder> node, IUnitOrder newOrder)
        {
            node.List.AddAfter(node, newOrder);
            node.List.Remove(node);
        }

        /// <summary>
        /// Deletes.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="user"></param>
        public void DeleteIfCan(DeleteAction action, User user)
        {
            lock (locker)
            {
                var list = GetOrderNodesToReplace(GetValidUnits(action.AffectedUnits, user), action.TargetActionID);

                foreach (var node in list)
                {
                    node.Item1.List.Remove(node.Item1);
                }
            }
        }

        /// <summary>
        /// Adds move order.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="user"></param>
        public void MoveIfCan(MoveAction action, User user)
        {
            lock (locker)
            {
                List<Unit> affectedUnits = GetValidUnits(action.UnitIDs, user);
                foreach (Unit unit in affectedUnits)
                {
                    unit.Orders.AddLast(new MoveOrder(action, affectedUnits, unit));
                }
            }
        }

        /// <summary>
        /// Replaces an order with a move order.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="user"></param>
        /// <param name="actionToReplace"></param>
        public void MoveIfCan(MoveAction action, User user, Guid actionToReplace)
        {
            lock (locker)
            {
                List<Unit> affectedUnits = GetValidUnits(action.UnitIDs, user);
                var nodes = GetOrderNodesToReplace(affectedUnits, actionToReplace);
                foreach (var node in nodes)
                {
                    ReplaceNode(node.Item1, new MoveOrder(action, affectedUnits, node.Item2));
                }
            }
        }

        /// <summary>
        /// Adds Assist order.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="user"></param>
        public void AssistIfCan(AssistAction action, User user)
        {
            lock (locker)
            {
                if (!Units.ContainsKey(action.Target))
                {
                    return;
                }

                Unit targ = Units[action.Target];

                if (targ.Owner != user)
                {
                    return;
                }

                List<Unit> affectedUnits = GetValidUnits(action.UnitIDs, user);
                foreach (Unit unit in affectedUnits)
                {
                    unit.Orders.AddLast(new AssistOrder(action.ActionID, movementCalculator) { Assister = unit, Target = targ });
                }
            }
        }

        /// <summary>
        /// Replaces an order with an assist order.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="user"></param>
        /// <param name="actionToReplace"></param>
        public void AssistIfCan(AssistAction action, User user, Guid actionToReplace)
        {
            lock (locker)
            {
                if (!Units.ContainsKey(action.Target))
                {
                    return;
                }

                Unit targ = Units[action.Target];

                if (targ.Owner != user)
                {
                    return;
                }

                List<Unit> affectedUnits = GetValidUnits(action.UnitIDs, user);
                var nodes = GetOrderNodesToReplace(affectedUnits, actionToReplace);
                foreach (var node in nodes)
                {
                    ReplaceNode(node.Item1, new AssistOrder(action.ActionID, movementCalculator) { Assister = node.Item2, Target = targ });
                }
            }
        }

        /// <summary>
        /// Adds Attack order.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="user"></param>
        public void AttackIfCan(AttackAction action, User user)
        {
            lock (locker)
            {
                if (!Units.ContainsKey(action.Target))
                {
                    return;
                }

                Unit targ = Units[action.Target];

                if (targ.Owner == user)
                {
                    return;
                }

                List<Unit> affectedUnits = GetValidUnits(action.UnitIDs, user);
                foreach (Unit unit in affectedUnits)
                {
                    unit.Orders.AddLast(new AttackOrder(action.ActionID, movementCalculator) { Attacker = unit, Target = targ });
                }
            }
        }

        /// <summary>
        /// Adds BuildBuilding order.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="user"></param>
        public void BuildBuildingIfCan(BuildBuildingAction action, User user)
        {
            lock (locker)
            {
                if (Map[IMovementCalculator.fromVec2(action.Position, Map.TileSize)] >= ushort.MaxValue / 2)
                {
                    return;
                }

                if (!action.UnitIDs.Any(a => Units.ContainsKey(a) && Units[a] is Builder && Units[a].Owner == user))
                {
                    return;
                }

                Unit targ = Units[action.UnitIDs.First(a => Units.ContainsKey(a) && Units[a] is Builder && Units[a].Owner == user)];

                List<Unit> affectedUnits = new List<Unit>() { targ };
                foreach (Unit unit in affectedUnits)
                {
                    unit.Orders.AddLast(new BuildBuildingOrder(action.ActionID, movementCalculator, action.BuildingType, action.Position) { Builder = targ });//new AssistOrder(action.ActionID, movementCalculator) { Assister = unit, Target = targ });
                }
            }
        }

        /// <summary>
        /// Replaces an order with a BuildBuilding order.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="user"></param>
        /// <param name="actionToReplace"></param>
        public void BuildBuildingIfCan(BuildBuildingAction action, User user, Guid actionToReplace)
        {
            lock (locker)
            {
                if (Map[IMovementCalculator.fromVec2(action.Position, Map.TileSize)] >= ushort.MaxValue / 2)
                {
                    return;
                }

                if (!action.UnitIDs.Any(a => Units.ContainsKey(a) && Units[a] is Builder && Units[a].Owner == user))
                {
                    return;
                }

                Unit targ = Units[action.UnitIDs.First(a => Units.ContainsKey(a) && Units[a] is Builder && Units[a].Owner == user)];

                List<Unit> affectedUnits = new List<Unit>() { targ };
                var nodes = GetOrderNodesToReplace(affectedUnits, actionToReplace);
                foreach (var node in nodes)
                {
                    ReplaceNode(node.Item1, new BuildBuildingOrder(action.ActionID, movementCalculator, action.BuildingType, action.Position) { Builder = node.Item2 });//new AssistOrder(action.ActionID, movementCalculator) { Assister = unit, Target = targ });
                }
            }
        }

        /// <summary>
        /// Enqueues a construction order at a factory.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="enq"></param>
        /// <returns></returns>
        public bool EnqueueBuildOrder(User user, EnqueueBuildOrder enq)
        {
            if (!Units.ContainsKey(enq.TargetFactory))
            {
                return false;
            }

            Unit maybeFact = Units[enq.TargetFactory];
            if (!(maybeFact is Factory factory))
            {
                return false;
            }

            if (factory.Owner != user)
            {
                return false;
            }

            float mineralPrice = Prices.UnitPriceData[enq.UnitType].RequiredMineral;
            float moneyPrice = Prices.UnitPriceData[enq.UnitType].RequiredMoney;

            if (user.Money < moneyPrice || user.Mineral < mineralPrice)
            {
                return false;
            }

            if (!factory.TryEnqueue(enq.OrderID, enq.UnitType))
            {
                return false;
            }

            user.Money -= moneyPrice;
            user.Mineral -= mineralPrice;

            return true;
        }

        /// <summary>
        /// Creates a unit and schedules its spawn information to be sent to clients next update.
        /// </summary>
        /// <param name="unitType"></param>
        /// <param name="position"></param>
        /// <param name="owner"></param>
        internal void SpawnUnit(GMRTSClasses.Units.MobileUnitType unitType, Vector2 position, User owner)
        {
            Unit unit;
            switch (unitType)
            {
                case GMRTSClasses.Units.MobileUnitType.Tank:
                    unit = new Tank(Guid.NewGuid(), owner, this);
                    break;
                case GMRTSClasses.Units.MobileUnitType.Builder:
                    unit = new Builder(Guid.NewGuid(), owner, this);
                    break;
                default:
                    throw new Exception("Spawning units of this type is unimplemented");
            }
            unit.Position = position;
            unit.PositionUpdate = new ChangingData<Vector2>(0, position, Vector2.Zero);
            unit.UpdatePosition = true;




            ToSpawn.Add(unit);
        }

        /// <summary>
        /// Schedules information to be sent to clients next update regarding the completion of a factory order.
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="factoryID"></param>
        /// <param name="orderID"></param>
        internal void TellOwnerOrderFinished(User owner, Guid factoryID, Guid orderID)
        {
            OrderCompletedsToSend.Add((owner, new OrderCompleted() { FactoryID = factoryID, OrderID = orderID }));
        }

        /// <summary>
        /// Cancels a factory construction order.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="cancel"></param>
        /// <returns></returns>
        public bool CancelBuildOrder(User user, CancelBuildOrder cancel)
        {
            if (!Units.ContainsKey(cancel.TargetFactory))
            {
                return false;
            }

            Unit maybeFact = Units[cancel.TargetFactory];
            if (!(maybeFact is Factory factory))
            {
                return false;
            }

            if (factory.Owner != user)
            {
                return false;
            }

            UnitConstructionOrder ord = factory.GetOrder(cancel.OrderID);

            if (ord == null)
            {
                return false;
            }

            if (!factory.TryCancel(cancel.OrderID))
            {
                return false;
            }

            user.Mineral += Prices.UnitPriceData[ord.UnitType].RequiredMineral;
            user.Money += Prices.UnitPriceData[ord.UnitType].RequiredMoney;

            return true;
        }

        /// <summary>
        /// Replaces an order with an attack order. I have NO idea why this isn't with the others.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="user"></param>
        /// <param name="actionToReplace"></param>
        public void AttackIfCan(AttackAction action, User user, Guid actionToReplace)
        {
            lock (locker)
            {
                if (!Units.ContainsKey(action.Target))
                {
                    return;
                }

                Unit targ = Units[action.Target];

                if (targ.Owner == user)
                {
                    return;
                }

                List<Unit> affectedUnits = GetValidUnits(action.UnitIDs, user);
                var nodes = GetOrderNodesToReplace(affectedUnits, actionToReplace);
                foreach (var node in nodes)
                {
                    ReplaceNode(node.Item1, new AttackOrder(action.ActionID, movementCalculator) { Attacker = node.Item2, Target = targ });
                }
            }
        }

        /// <summary>
        /// Schedules information about the completion of an order to be sent next update.
        /// </summary>
        /// <param name="unit"></param>
        /// <param name="actionID"></param>
        public void QueueActionOver(Unit unit, Guid actionID)
        {
            ActionOversToSend.Add((unit, actionID));
        }

        /// <summary>
        /// Schedules the start of the game.
        /// </summary>
        /// <param name="utcStart"></param>
        /// <returns></returns>
        public async Task StartAt(DateTime utcStart)
        {
            TimeSpan wait = utcStart - DateTime.UtcNow;
            await Task.Delay((int)wait.TotalMilliseconds);
            await Start();
        }

        /// <summary>
        /// Starts the game and sends relevant information to clients.
        /// </summary>
        /// <returns></returns>
        public async Task Start()
        {
            if (Stopwatch.IsRunning)
            {
                throw new Exception("Already started!");
            }
            DateTime now = DateTime.UtcNow;
            Stopwatch.Start();
            foreach (User user in Users)
            {
                await Context.Clients.Client(user.ID).SendAsync("GameStarted", now);
            }
            foreach (User user in Users)
            {
                foreach (Unit unit in user.Units)
                {
                    foreach (User user2 in Users)
                    {
                        await Context.Clients.Client(user2.ID).SendAsync("AddUnit", new UnitSpawnData() { ID = unit.ID, OwnerUsername = user.CurrentUsername, Type = unit.GetType().Name });
                    }
                }
            }
            _ = UpdateLoop().ContinueWith(t => ExceptionDispatchInfo.Capture(t.Exception.GetBaseException()).Throw(), TaskContinuationOptions.OnlyOnFaulted);//.Start();
        }

        /// <summary>
        /// Main update loop. It handles timing and calls UpdateBody.
        /// </summary>
        /// <returns></returns>
        private async Task UpdateLoop()
        {
            while (true)
            {
                long newMillis = Stopwatch.ElapsedMilliseconds;
                float deltaS = (newMillis - currentMillis) / 1000f;
                currentMillis = newMillis;
                await UpdateBody((ulong)currentMillis, deltaS);
                int passed = (int)(Stopwatch.ElapsedMilliseconds - currentMillis);
                if (passed < 16)
                {
                    await Task.Delay(16 - passed);
                }
            }
        }

        /// <summary>
        /// Main update logic.
        /// </summary>
        /// <param name="currentMillis"></param>
        /// <param name="elapsedTime"></param>
        /// <returns></returns>
        private async Task UpdateBody(ulong currentMillis, float elapsedTime)
        {
            // Updates all the units. Don't really know if we need this lock, but evidently at some point I thought we might. Better safe than sorry.
            lock (locker)
            {
                foreach (Unit unit in Units.Values)
                {
                    unit.Update(currentMillis, elapsedTime);
                }
            }

            // Sends information about the completed orders.
            foreach ((Unit unit, Guid actionID) in ActionOversToSend)
            {
                await Context.Clients.Client(unit.Owner.ID).SendAsync("ActionOver", new ActionOver() { ActionID = actionID, Units = new List<Guid>() { unit.ID } });
            }

            // Sends information about the completed orders -- the other kind of order.
            foreach ((User user, OrderCompleted comp) in OrderCompletedsToSend)
            {
                await Context.Clients.Client(user.ID).SendAsync("OrderFinished", comp);
            }

            // Sends unit spawn information.
            foreach (Unit unit in ToSpawn)
            {
                Units.Add(unit.ID, unit);
                unit.Owner.Units.Add(unit);

                foreach (User user in Users)
                {
                    await Context.Clients.Client(user.ID).SendAsync("AddUnit", new UnitSpawnData() { ID = unit.ID, OwnerUsername = unit.Owner.CurrentUsername, Type = unit.GetType().Name });
                }
            }

            // Clears the lists we just went through.
            ToSpawn.Clear();

            ActionOversToSend.Clear();
            OrderCompletedsToSend.Clear();

            // Updates the users on their resources. This is temporary! In the future, it should only send it when something uses a resource or when the rate of production changes.
            foreach (User user in Users)
            {
                await Context.Clients.Client(user.ID).SendAsync("ResourceUpdated", new ResourceUpdate() { ResourceType = ResourceType.Mineral, Value = new GMRTSClasses.Changing<float>(user.Mineral, 0, GMRTSClasses.FloatChanger.FChanger, currentMillis) });
                await Context.Clients.Client(user.ID).SendAsync("ResourceUpdated", new ResourceUpdate() { ResourceType = ResourceType.Money, Value = new GMRTSClasses.Changing<float>(user.Money, 0, GMRTSClasses.FloatChanger.FChanger, currentMillis) });
            }

            // Code does not require inspection. Move along, move along.

            // This bit basically figures out which clients need to be informed of the unit's status and then tells them about it.
            List<Guid> toKill = new List<Guid>();
            foreach (Unit unit in Units.Values)
            {
                IReadOnlyList<string> userIDsToUpdate = GetRelevantUserIDs(unit);
                IReadOnlyList<string> oldUserIDsToUpdate = unit.LastFrameVisibleUsers;

                // The differences between the sets of this update's visible users and last update's visible users tells us who
                // has to be read in and who should be told that the unit is now invisible to them.
                string[] newlyCanSee = userIDsToUpdate.Except(oldUserIDsToUpdate).ToArray();
                string[] newlyCantSee = oldUserIDsToUpdate.Except(userIDsToUpdate).ToArray();
                var clients = Context.Clients.Clients(userIDsToUpdate);
                var newlyCanSeeClients = Context.Clients.Clients(newlyCanSee);
                var newlyCantSeeClients = Context.Clients.Clients(newlyCantSee);
                unit.LastFrameVisibleUsers = userIDsToUpdate.ToArray();

                // Code that updates the health is, in the current system, responsible for figuring out if the change is sufficient to warrant a new message being sent out.
                // Not a great system, but it seems to work.
                if (unit.UpdateHealth)
                {
                    unit.UpdateHealth = false;
                    await clients.SendAsync("UpdateHealth", unit.ID, unit.HealthUpdate);
                }
                else
                {
                    await newlyCanSeeClients.SendAsync("UpdateHealth", unit.ID, unit.HealthUpdate);
                }

                // Register units for killifying
                if (unit.Health <= 0)
                {
                    await clients.SendAsync("KillUnit", unit.ID);
                    toKill.Add(unit.ID);
                }

                // Again, code that changes position/velocity should, in this system, tell us whether or not to inform the clients
                if (unit.UpdatePosition)
                {
                    unit.UpdatePosition = false;
                    await clients.SendAsync("UpdatePosition", unit.ID, unit.PositionUpdate);
                }
                else
                {
                    await newlyCanSeeClients.SendAsync("UpdatePosition", unit.ID, unit.PositionUpdate);
                }

                // Again, same for rotation
                if (unit.UpdateRotation)
                {
                    unit.UpdateRotation = false;
                    await clients.SendAsync("UpdateRotation", unit.ID, unit.RotationUpdate);
                }
                else
                {
                    await newlyCanSeeClients.SendAsync("UpdateRotation", unit.ID, unit.RotationUpdate);
                }


                // A really lazy (and originally temporary) way to make units visible when they are out of one's view
                // This really should be replaced by a separate IsVisible boolean, but whatever
                await newlyCantSeeClients.SendAsync("UpdatePosition", unit.ID, new ChangingData<Vector2>(0, new Vector2(-200, -200), new Vector2(0, 0)));
            }

            // Kill the units scheduled for killification
            foreach (Guid id in toKill)
            {
                Remove(Units[id]);
            }
        }

        /// <summary>
        /// Gets the users that can see a unit, ignoring things like mountains that obstruct vision.
        /// Might end up being a performance bottleneck. If it is, switch to using the UnitPositionLookup thing.
        /// The only reason it doesn't already use that is because this was written before that existed.
        /// </summary>
        /// <param name="unit"></param>
        /// <returns></returns>
        private IReadOnlyList<string> GetRelevantUserIDs(Unit unit)
        {
            List<string> users = new List<string>(Users.Count);
            users.Add(unit.Owner.ID);

            foreach (User user in Users)
            {
                if (user == unit.Owner)
                {
                    continue;
                }

                foreach (Unit myUnit in user.Units)
                {
                    if ((myUnit.Position - unit.Position).LengthSquared() <= UnitSightRangeSquared)
                    {
                        users.Add(user.ID);
                        break;
                    }
                }
            }

            return users;
        }
    }
}
