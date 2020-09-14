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
        static Dictionary<string, Game> games = new Dictionary<string, Game>();

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
            usersFromIDs[Context.ConnectionId].CurrentGame.MoveIfCan(act, usersFromIDs[Context.ConnectionId]);
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            RemoveUser(Context.ConnectionId);
            return base.OnDisconnected(stopCalled);
        }

        public async Task Join(string gameName)
        {
            if(!games.ContainsKey(gameName))
            {
                return;
            }
            JoinGame(usersFromIDs[Context.ConnectionId], games[gameName]);
        }

        public async Task Leave()
        {
            RemoveUserFromGame(usersFromIDs[Context.ConnectionId]);
        }

        public async Task JoinAndMaybeCreate(string gameName)
        {
            if(!games.ContainsKey(gameName))
            {
                games.Add(gameName, new Game());
            }

            JoinGame(usersFromIDs[Context.ConnectionId], games[gameName]);
        }

        private void JoinGame(User user, Game game)
        {
            RemoveUserFromGame(user);
            game.Users.Add(user);
            user.CurrentGame = game;
        }

        private void RemoveUserFromGame(User user)
        {
            Game game = user.CurrentGame;
            user.CurrentGame = null;
            if (game == null)
            {
                return;
            }

            user.CurrentGame.Users.Remove(user);
            if (game.Users.Count <= 0)
            {
                string name = null;
                foreach (string n in games.Keys)
                {
                    if (games[n] == game)
                    {
                        name = n;
                        break;
                    }
                }
                games.Remove(name);
            }
        }

        private void RemoveUser(string id)
        {
            if(usersFromIDs.ContainsKey(id))
            {
                User user = usersFromIDs[id];
                usersFromIDs.Remove(id);
                RemoveUserFromGame(user);
            }
        }
    }
}
