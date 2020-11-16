using GMRTSClasses.CTSTransferData;
using GMRTSClasses.CTSTransferData.UnitGround;
using GMRTSClasses.STCTransferData;

using GMRTSServer.ServersideUnits;
using GMRTSServer.UnitStates;

using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GMRTSServer
{
    internal class Game
    {
        private List<User> Users { get; set; } = new List<User>();

        internal object FlowfieldLocker = new object();

        internal Dictionary<(int x, int y), Task<byte[][]>> Flowfields = new Dictionary<(int x, int y), Task<byte[][]>>();

        internal Map Map = new Map();

        object locker = new object();

        public bool AddUser(User user)
        {
            if (Users.Any(a => a.CurrentUsername == user.CurrentUsername))
            {
                return false;
            }

            Users.Add(user);

            Unit unit = new Builder(Guid.NewGuid()) { Owner = user };
            user.Units.Add(unit);
            Units.Add(unit.ID, unit);
            user.CurrentGame = this;
            return true;
        }

        public void RemoveUser(User user)
        {
            Users.Remove(user);
        }

        public Game(IHubContext context)
        {
            Context = context;
        }

        public int UserCount => Users.Count;

        public Stopwatch Stopwatch { get; set; } = new Stopwatch();

        public IHubContext Context { get; set; }

        public const float UnitSightRangeSquared = 100 * 100;

        //public Dictionary<User, List<Unit>> Units { get; set; } = new Dictionary<User, List<Unit>>();
        public Dictionary<Guid, Unit> Units { get; set; } = new Dictionary<Guid, Unit>();
        private long currentMillis = 0;

        private void Remove(Unit unit)
        {
            unit.Owner.Units.Remove(unit);
            Units.Remove(unit.ID);
        }

        public List<Unit> GetValidUnits(ClientAction act, User user)
        {
            List<Unit> units = new List<Unit>(act.UnitIDs.Count);
            foreach(Guid unitID in act.UnitIDs)
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

        public void MoveIfCan(MoveAction action, User user)
        {
            lock (locker)
            {
                List<Unit> affectedUnits = GetValidUnits(action, user);
                foreach (Unit unit in affectedUnits)
                {
                    unit.Orders.AddLast(new MoveOrder(20f, action, affectedUnits, unit));
                }
            }
        }

        public void MoveIfCan(MoveAction action, User user, Guid actionToReplace)
        {
            lock (locker)
            {
                List<Unit> affectedUnits = GetValidUnits(action, user);
                var nodes = GetOrderNodesToReplace(affectedUnits, actionToReplace);
                foreach (var node in nodes)
                {
                    ReplaceNode(node.Item1, new MoveOrder(20f, action, affectedUnits, node.Item2));
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
                Context.Clients.Client(user.ID).GameStarted(now);
            }
            foreach (User user in Users)
            {
                foreach (Unit unit in user.Units)
                {
                    foreach (User user2 in Users)
                    {
                        Context.Clients.Client(user2.ID).AddUnit(new UnitSpawnData() { ID = unit.ID, OwnerUsername = user.CurrentUsername, Type = unit.GetType().Name });
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
                IList<string> userIDsToUpdate = GetRelevantUserIDs(unit);
                IList<string> oldUserIDsToUpdate = unit.LastFrameVisibleUsers;
                string[] newlyCanSee = userIDsToUpdate.Except(oldUserIDsToUpdate).ToArray();
                string[] newlyCantSee = oldUserIDsToUpdate.Except(userIDsToUpdate).ToArray();
                var clients = Context.Clients.Clients(userIDsToUpdate);
                var newlyCanSeeClients = Context.Clients.Clients(newlyCanSee);
                var newlyCantSeeClients = Context.Clients.Clients(newlyCantSee);
                unit.LastFrameVisibleUsers = userIDsToUpdate.ToArray();

                if (unit.UpdateHealth)
                {
                    unit.UpdateHealth = false;
                    clients.UpdateHealth(unit.ID, unit.HealthUpdate);
                }
                else
                {
                    newlyCanSeeClients.UpdateHealth(unit.ID, unit.HealthUpdate);
                }

                if (unit.Health <= 0)
                {
                    clients.KillUnit(unit.ID);
                    toKill.Add(unit.ID);
                }

                if (unit.UpdatePosition)
                {
                    unit.UpdatePosition = false;
                    clients.UpdatePosition(unit.ID, unit.PositionUpdate);
                }
                else
                {
                    newlyCanSeeClients.UpdatePosition(unit.ID, unit.PositionUpdate);
                }

                if (unit.UpdateRotation)
                {
                    unit.UpdateRotation = false;
                    clients.UpdateRotation(unit.ID, unit.RotationUpdate);
                }
                else
                {
                    newlyCanSeeClients.UpdateRotation(unit.ID, unit.RotationUpdate);
                }

                newlyCantSeeClients.UpdatePosition(unit.ID, new ChangingData<Vector2>(0, new Vector2(-200, -200), new Vector2(0, 0)));
            }

            foreach (Guid id in toKill)
            {
                Remove(Units[id]);
            }
        }

        //If this function turns out to be a performance bottleneck, let me (Peter) know. I have a plan involving breaking the map into grid squares.
        private IList<string> GetRelevantUserIDs(Unit unit)
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
