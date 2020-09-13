using GMRTSClasses.CTSTransferData;

using GMRTSServer.ServersideUnits;
using GMRTSServer.UnitStates;

using Microsoft.AspNet.SignalR;

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
        public List<User> Users { get; set; } = new List<User>();

        public Stopwatch Stopwatch { get; set; } = new Stopwatch();

        public IHubContext Context { get; set; }

        //public Dictionary<User, List<Unit>> Units { get; set; } = new Dictionary<User, List<Unit>>();
        public Dictionary<Guid, Unit> Units { get; set; } = new Dictionary<Guid, Unit>();
        private long currentMillis = 0;

        public void MoveIfCan(MoveAction action, User user)
        {
            foreach(Unit unit in action.UnitIDs.Select(a => Units[a]).Intersect(user.Units))
            {
                unit.State = new MoveState() { Targets = new Queue<Vector2>(action.Positions), Unit = unit };
            }
        }

        public async Task StartAt(DateTime utcStart)
        {
            TimeSpan wait = utcStart - DateTime.UtcNow;
            await Task.Delay(wait).ContinueWith(async a => await Start());
        }

        public async Task Start()
        {
            if(Stopwatch.IsRunning)
            {
                throw new Exception("Already started!");
            }
            Stopwatch.Start();
            UpdateLoop().Start();
        }

        private async Task UpdateLoop()
        {
            while(true)
            {
                long newMillis = Stopwatch.ElapsedMilliseconds;
                float deltaS = (newMillis - currentMillis) / 1000f;
                currentMillis = newMillis;
                await UpdateBody((ulong)currentMillis, deltaS);
                int passed = (int)(Stopwatch.ElapsedMilliseconds - currentMillis);
                if(passed < 16)
                {
                    await Task.Delay(16 - passed);
                }
            }
        }

        private async Task UpdateBody(ulong currentMillis, float elapsedTime)
        {
            foreach(Unit unit in Units.Values)
            {
                unit.Update(currentMillis, elapsedTime);
            }

            List<Guid> toKill = new List<Guid>();
            foreach (Unit unit in Units.Values)
            {
                if(unit.UpdateHealth)
                {
                    unit.UpdateHealth = false;
                }
            }
        }

        private IList<string> GetRelevantUserIDs(Unit unit)
        {

        }
    }
}
