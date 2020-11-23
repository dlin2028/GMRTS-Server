using GMRTSClasses.CTSTransferData.MetaActions;
using GMRTSClasses.CTSTransferData.UnitGround;
using GMRTSClasses.CTSTransferData.UnitUnit;
using GMRTSClasses.STCTransferData;

using GMRTSServerCore.Hubs;
using GMRTSServerCore.SimClasses.ServersideUnits;
using GMRTSServerCore.SimClasses.UnitStates;

using Microsoft.AspNetCore.SignalR;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace GMRTSServerCore.SimClasses
{
    internal class Game
    {
        private List<User> Users { get; set; } = new List<User>();

        internal object FlowfieldLocker = new object();

        internal Dictionary<(int x, int y), Task<byte[][]>> Flowfields = new Dictionary<(int x, int y), Task<byte[][]>>();

        internal IMovementCalculator movementCalculator;

        internal Map Map = new Map();

        object locker = new object();

        public bool AddUser(User user)
        {
            if (Users.Any(a => a.CurrentUsername == user.CurrentUsername))
            {
                return false;
            }

            Users.Add(user);

            Unit unit = new Builder(Guid.NewGuid(), user, this);
            user.Units.Add(unit);
            Units.Add(unit.ID, unit);
            user.CurrentGame = this;
            return true;
        }

        public void RemoveUser(User user)
        {
            Users.Remove(user);
        }

        public Game(IHubContext<GameHub> context)
        {
            Context = context;
            movementCalculator = new FlowfieldMovementCalculator();
        }

        public int UserCount => Users.Count;

        public Stopwatch Stopwatch { get; set; } = new Stopwatch();

        public IHubContext<GameHub> Context { get; set; }

        public const float UnitSightRangeSquared = 100 * 100;

        //public Dictionary<User, List<Unit>> Units { get; set; } = new Dictionary<User, List<Unit>>();
        public Dictionary<Guid, Unit> Units { get; set; } = new Dictionary<Guid, Unit>();
        private long currentMillis = 0;

        private void Remove(Unit unit)
        {
            unit.Owner.Units.Remove(unit);
            Units.Remove(unit.ID);
        }

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

        private static void ReplaceNode(LinkedListNode<IUnitOrder> node, IUnitOrder newOrder)
        {
            node.List.AddAfter(node, newOrder);
            node.List.Remove(node);
        }

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

        public async Task StartAt(DateTime utcStart)
        {
            TimeSpan wait = utcStart - DateTime.UtcNow;
            await Task.Delay((int)wait.TotalMilliseconds);
            await Start();
        }

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
            UpdateLoop();//.Start();
        }

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

        private async Task UpdateBody(ulong currentMillis, float elapsedTime)
        {
            lock (locker)
            {
                foreach (Unit unit in Units.Values)
                {
                    unit.Update(currentMillis, elapsedTime);
                }
            }

            List<Guid> toKill = new List<Guid>();
            foreach (Unit unit in Units.Values)
            {
                IReadOnlyList<string> userIDsToUpdate = GetRelevantUserIDs(unit);
                IReadOnlyList<string> oldUserIDsToUpdate = unit.LastFrameVisibleUsers;
                string[] newlyCanSee = userIDsToUpdate.Except(oldUserIDsToUpdate).ToArray();
                string[] newlyCantSee = oldUserIDsToUpdate.Except(userIDsToUpdate).ToArray();
                var clients = Context.Clients.Clients(userIDsToUpdate);
                var newlyCanSeeClients = Context.Clients.Clients(newlyCanSee);
                var newlyCantSeeClients = Context.Clients.Clients(newlyCantSee);
                unit.LastFrameVisibleUsers = userIDsToUpdate.ToArray();

                if (unit.UpdateHealth)
                {
                    unit.UpdateHealth = false;
                    await clients.SendAsync("UpdateHealth", unit.ID, unit.HealthUpdate);
                }
                else
                {
                    await newlyCanSeeClients.SendAsync("UpdateHealth", unit.ID, unit.HealthUpdate);
                }

                if (unit.Health <= 0)
                {
                    await clients.SendAsync("KillUnit", unit.ID);
                    toKill.Add(unit.ID);
                }

                if (unit.UpdatePosition)
                {
                    unit.UpdatePosition = false;
                    await clients.SendAsync("UpdatePosition", unit.ID, unit.PositionUpdate);
                }
                else
                {
                    await newlyCanSeeClients.SendAsync("UpdatePosition", unit.ID, unit.PositionUpdate);
                }

                if (unit.UpdateRotation)
                {
                    unit.UpdateRotation = false;
                    await clients.SendAsync("UpdateRotation", unit.ID, unit.RotationUpdate);
                }
                else
                {
                    await newlyCanSeeClients.SendAsync("UpdateRotation", unit.ID, unit.RotationUpdate);
                }

                await newlyCantSeeClients.SendAsync("UpdatePosition", unit.ID, new ChangingData<Vector2>(0, new Vector2(-200, -200), new Vector2(0, 0)));
            }

            foreach (Guid id in toKill)
            {
                Remove(Units[id]);
            }
        }

        //If this function turns out to be a performance bottleneck, let me (Peter) know. I have a plan involving breaking the map into grid squares.
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
