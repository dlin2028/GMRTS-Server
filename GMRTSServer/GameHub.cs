using GMRTSClasses.CTSTransferData;

using Microsoft.AspNet.SignalR;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMRTSServer
{
    public class GameHub : Hub
    {
        static Dictionary<string, User> usersFromIDs = new Dictionary<string, User>();
        static List<Game> games = new List<Game>();

        public override Task OnConnected()
        {
            Console.WriteLine("hi");
            usersFromIDs.Add(Context.ConnectionId, new User(Context.ConnectionId));
            return base.OnConnected();
        }

        public async Task Assist(AssistAction act)
        {
            
        }

        public async Task Move(MoveAction act)
        {
            Console.WriteLine($"Moving {act.UnitIDs.First()} from {Context.ConnectionId} to {act.Positions.First()}");
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            RemoveUser(Context.ConnectionId);
            return base.OnDisconnected(stopCalled);
        }

        private void RemoveUser(string id)
        {
            if(usersFromIDs.ContainsKey(id))
            {
                User user = usersFromIDs[id];
                usersFromIDs.Remove(id);
                user.CurrentGame.Users.Remove(user);
            }
        }
    }
}
