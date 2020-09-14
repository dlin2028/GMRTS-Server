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
            if(usersFromIDs[Context.ConnectionId].CurrentGame == null)
            {
                return;
            }
            Console.WriteLine($"Moving {act.UnitIDs.First()} from {Context.ConnectionId} to {act.Positions.First()}");
            usersFromIDs[Context.ConnectionId].CurrentGame.MoveIfCan(act, usersFromIDs[Context.ConnectionId]);
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            RemoveUser(Context.ConnectionId);
            return base.OnDisconnected(stopCalled);
        }

        public async Task<bool> Join(string gameName, string userName)
        {
            if(!games.ContainsKey(gameName))
            {
                return false;
            }
            return JoinGame(usersFromIDs[Context.ConnectionId], games[gameName], userName);
        }

        public async Task Leave()
        {
            RemoveUserFromGame(usersFromIDs[Context.ConnectionId]);
        }

        public async Task<bool> JoinAndMaybeCreate(string gameName, string userName)
        {
            if(!games.ContainsKey(gameName))
            {
                games.Add(gameName, new Game());
            }

            if(!JoinGame(usersFromIDs[Context.ConnectionId], games[gameName], userName))
            {
                games.Remove(gameName);
                return false;
            }
            return true;
        }

        private bool JoinGame(User user, Game game, string userName)
        {
            user.CurrentUsername = userName;
            RemoveUserFromGame(user);
            return !game.AddUser(user);
        }

        private void RemoveUserFromGame(User user)
        {
            Game game = user.CurrentGame;
            user.CurrentGame = null;
            if (game == null)
            {
                return;
            }

            user.CurrentGame.RemoveUser(user);
            if (game.UserCount <= 0)
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
