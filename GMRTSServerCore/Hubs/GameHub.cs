using GMRTSClasses.CTSTransferData;
using GMRTSClasses.CTSTransferData.FactoryActions;
using GMRTSClasses.CTSTransferData.MetaActions;
using GMRTSClasses.CTSTransferData.UnitGround;
using GMRTSClasses.CTSTransferData.UnitUnit;

using GMRTSServerCore.SimClasses;

using Microsoft.AspNetCore.SignalR;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GMRTSServerCore.Hubs
{
    public class GameHub : Hub
    {
        static Dictionary<string, User> usersFromIDs = new Dictionary<string, User>();
        static Dictionary<string, Game> games = new Dictionary<string, Game>();

        private IHubContext<GameHub> context;

        public override async Task OnConnectedAsync()
        {
            Console.WriteLine("hi");
            usersFromIDs.Add(Context.ConnectionId, new User(Context.ConnectionId));
            await base.OnConnectedAsync();
        }

        public async Task Assist(AssistAction act)
        {
            if (usersFromIDs[Context.ConnectionId].CurrentGame == null)
            {
                return;
            }
            usersFromIDs[Context.ConnectionId].CurrentGame.AssistIfCan(act, usersFromIDs[Context.ConnectionId]);
        }

        public async Task Attack(AttackAction act)
        {
            if (usersFromIDs[Context.ConnectionId].CurrentGame == null)
            {
                return;
            }
            usersFromIDs[Context.ConnectionId].CurrentGame.AttackIfCan(act, usersFromIDs[Context.ConnectionId]);
        }

        public async Task BuildBuilding(BuildBuildingAction act)
        {
            if (usersFromIDs[Context.ConnectionId].CurrentGame == null)
            {
                return;
            }
            throw new NotImplementedException();
        }

        public async Task Arbitrary(ClientAction act)
        {
            if (act is MoveAction m)
            {
                await Move(m);
            }
            else if (act is BuildBuildingAction b)
            {
                await BuildBuilding(b);
            }
            else if (act is AttackAction at)
            {
                await Attack(at);
            }
            else if (act is AssistAction @as)
            {
                await Assist(@as);
            }
            else
            {
                throw new Exception();
            }
        }

        public async Task Replace(ReplaceAction metaAct)
        {
            if (usersFromIDs[Context.ConnectionId].CurrentGame == null)
            {
                return;
            }
            ClientAction act = metaAct.NewAction;

            if (act is MoveAction m)
            {
                usersFromIDs[Context.ConnectionId].CurrentGame.MoveIfCan(m, usersFromIDs[Context.ConnectionId], metaAct.TargetActionID);
            }
            else if (act is BuildBuildingAction b)
            {
                throw new NotImplementedException();
                //await BuildBuilding(b);
            }
            else if (act is AttackAction at)
            {
                usersFromIDs[Context.ConnectionId].CurrentGame.AttackIfCan(at, usersFromIDs[Context.ConnectionId], metaAct.TargetActionID);
            }
            else if (act is AssistAction @as)
            {
                usersFromIDs[Context.ConnectionId].CurrentGame.AssistIfCan(@as, usersFromIDs[Context.ConnectionId], metaAct.TargetActionID);
            }
            else
            {
                throw new Exception();
            }
        }

        public async Task Delete(DeleteAction act)
        {
            if (usersFromIDs[Context.ConnectionId].CurrentGame == null)
            {
                return;
            }

            usersFromIDs[Context.ConnectionId].CurrentGame.DeleteIfCan(act, usersFromIDs[Context.ConnectionId]);
        }

        public async Task Move(MoveAction act)
        {
            if (usersFromIDs[Context.ConnectionId].CurrentGame == null)
            {
                return;
            }
            Console.WriteLine($"Enqueued action: Move {act.UnitIDs.First()} from {Context.ConnectionId} to {act.Position}");
            usersFromIDs[Context.ConnectionId].CurrentGame.MoveIfCan(act, usersFromIDs[Context.ConnectionId]);
        }

        public async Task<bool> FactoryAct(FactoryAction factoryAction)
        {
            if (usersFromIDs[Context.ConnectionId].CurrentGame == null)
            {
                return false;
            }

            if (factoryAction is EnqueueBuildOrder enqueue)
            {
                return usersFromIDs[Context.ConnectionId].CurrentGame.EnqueueBuildOrder(usersFromIDs[Context.ConnectionId], enqueue);
            }

            if (factoryAction is CancelBuildOrder cancel)
            {
                return usersFromIDs[Context.ConnectionId].CurrentGame.CancelBuildOrder(usersFromIDs[Context.ConnectionId], cancel);
            }

            return false;
        }

        public async Task ReqStartGame()
        {
            User user = usersFromIDs[Context.ConnectionId];
            if (user.CurrentGame == null)
            {
                return;
            }

            user.CurrentGame.StartAt(DateTime.UtcNow + TimeSpan.FromSeconds(2));//.Start();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            RemoveUser(Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }

        public async Task<bool> Join(string gameName, string userName)
        {
            if (!games.ContainsKey(gameName))
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
            if (!games.ContainsKey(gameName))
            {
                games.Add(gameName, new Game(context)); ;//GlobalHost.ConnectionManager.GetHubContext<GameHub>()));
            }

            if (!JoinGame(usersFromIDs[Context.ConnectionId], games[gameName], userName))
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
            return game.AddUser(user);
        }

        private void RemoveUserFromGame(User user)
        {
            Game game = user.CurrentGame;
            user.CurrentGame = null;
            if (game == null)
            {
                return;
            }

            game.RemoveUser(user);
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
            if (usersFromIDs.ContainsKey(id))
            {
                User user = usersFromIDs[id];
                usersFromIDs.Remove(id);
                RemoveUserFromGame(user);
            }
        }

        public GameHub(IHubContext<GameHub> context)
        {
            this.context = context;
        }
    }
}
