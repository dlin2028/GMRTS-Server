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
        /// <summary>
        /// Maps connection IDs to Users
        /// </summary>
        static Dictionary<string, User> usersFromIDs = new Dictionary<string, User>();

        /// <summary>
        /// Maps game names to games.
        /// </summary>
        static Dictionary<string, Game> games = new Dictionary<string, Game>();

        /// <summary>
        /// We'll give this to the games so that they can send messages to the clients.
        /// </summary>
        private IHubContext<GameHub> context;

        public override async Task OnConnectedAsync()
        {
            Console.WriteLine("hi");
            usersFromIDs.Add(Context.ConnectionId, new User(Context.ConnectionId));
            await base.OnConnectedAsync();
        }

        /// <summary>
        /// Method called by the client to send an Assist order or action or whatever you want to call it.
        /// This should probably return a bool success so it won't fail silently.
        /// </summary>
        /// <param name="act"></param>
        /// <returns></returns>
        public Task Assist(AssistAction act)
        {
            if (usersFromIDs[Context.ConnectionId].CurrentGame == null)
            {
                return Task.CompletedTask;
            }
            usersFromIDs[Context.ConnectionId].CurrentGame.AssistIfCan(act, usersFromIDs[Context.ConnectionId]);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Called by the client to send Attack orders. Should probably return a bool.
        /// </summary>
        /// <param name="act"></param>
        /// <returns></returns>
        public Task Attack(AttackAction act)
        {
            if (usersFromIDs[Context.ConnectionId].CurrentGame == null)
            {
                return Task.CompletedTask;
            }
            usersFromIDs[Context.ConnectionId].CurrentGame.AttackIfCan(act, usersFromIDs[Context.ConnectionId]);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Called by the client to send BuildBuilding orders. Should probably return a bool.
        /// </summary>
        /// <param name="act"></param>
        /// <returns></returns>
        public Task BuildBuilding(BuildBuildingAction act)
        {
            if (usersFromIDs[Context.ConnectionId].CurrentGame == null)
            {
                return Task.CompletedTask;
            }
            usersFromIDs[Context.ConnectionId].CurrentGame.BuildBuildingIfCan(act, usersFromIDs[Context.ConnectionId]);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Called by the client to replace an order with a Move order. Should probably return a bool.
        /// </summary>
        /// <param name="act"></param>
        /// <returns></returns>
        public Task ReplaceMoveAction(ReplaceAction<MoveAction> act)
        {
            if (usersFromIDs[Context.ConnectionId].CurrentGame == null)
            {
                return Task.CompletedTask;
            }

            usersFromIDs[Context.ConnectionId].CurrentGame.MoveIfCan(act.NewAction, usersFromIDs[Context.ConnectionId], act.TargetActionID);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Called by the client to replace an order with a BuildBuilding order. Should probably return a bool.
        /// </summary>
        /// <param name="act"></param>
        /// <returns></returns>
        public Task ReplaceBuildBuildingAction(ReplaceAction<BuildBuildingAction> act)
        {
            if (usersFromIDs[Context.ConnectionId].CurrentGame == null)
            {
                return Task.CompletedTask;
            }

            usersFromIDs[Context.ConnectionId].CurrentGame.BuildBuildingIfCan(act.NewAction, usersFromIDs[Context.ConnectionId], act.TargetActionID);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Called by the client to replace an order with an Attack order. Should probably return a bool.
        /// </summary>
        /// <param name="act"></param>
        /// <returns></returns>
        public Task ReplaceAttackAction(ReplaceAction<AttackAction> act)
        {
            if (usersFromIDs[Context.ConnectionId].CurrentGame == null)
            {
                return Task.CompletedTask;
            }

            usersFromIDs[Context.ConnectionId].CurrentGame.AttackIfCan(act.NewAction, usersFromIDs[Context.ConnectionId], act.TargetActionID);

            return Task.CompletedTask;
        }


        /// <summary>
        /// Called by the client to replace an order with an Assist order. Should probably return a bool.
        /// </summary>
        /// <param name="act"></param>
        /// <returns></returns>
        public Task ReplaceAssistAction(ReplaceAction<AssistAction> act)
        {
            if (usersFromIDs[Context.ConnectionId].CurrentGame == null)
            {
                return Task.CompletedTask;
            }

            usersFromIDs[Context.ConnectionId].CurrentGame.AssistIfCan(act.NewAction, usersFromIDs[Context.ConnectionId], act.TargetActionID);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Called by the client to delete an order. Should probably return a bool.
        /// </summary>
        /// <param name="act"></param>
        /// <returns></returns>
        public Task Delete(DeleteAction act)
        {
            if (usersFromIDs[Context.ConnectionId].CurrentGame == null)
            {
                return Task.CompletedTask;
            }

            usersFromIDs[Context.ConnectionId].CurrentGame.DeleteIfCan(act, usersFromIDs[Context.ConnectionId]);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Called by the client to send Move orders. Should probably return a bool.
        /// </summary>
        /// <param name="act"></param>
        /// <returns></returns>
        public Task Move(MoveAction act)
        {
            if (usersFromIDs[Context.ConnectionId].CurrentGame == null)
            {
                return Task.CompletedTask;
            }
            Console.WriteLine($"Enqueued action: Move {act.UnitIDs.First()} from {Context.ConnectionId} to {act.Position}");
            usersFromIDs[Context.ConnectionId].CurrentGame.MoveIfCan(act, usersFromIDs[Context.ConnectionId]);
            return Task.CompletedTask;
        }


        /// <summary>
        /// Called by the client to send construction orders to factories.
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public Task<bool> FactoryEnqueue(EnqueueBuildOrder order)
        {
            if (usersFromIDs[Context.ConnectionId].CurrentGame == null)
            {
                return Task.FromResult(false);
            }

            return Task.FromResult(usersFromIDs[Context.ConnectionId].CurrentGame.EnqueueBuildOrder(usersFromIDs[Context.ConnectionId], order));
        }

        /// <summary>
        /// Called by the client to cancel a factory order.
        /// </summary>
        /// <param name="cancel"></param>
        /// <returns></returns>
        public Task<bool> FactoryCancel(CancelBuildOrder cancel)
        {
            if (usersFromIDs[Context.ConnectionId].CurrentGame == null)
            {
                return Task.FromResult(false);
            }

            return Task.FromResult(usersFromIDs[Context.ConnectionId].CurrentGame.CancelBuildOrder(usersFromIDs[Context.ConnectionId], cancel));
        }

        /// <summary>
        /// Called by the client to schedule a start for the game.
        /// </summary>
        /// <returns></returns>
        public Task ReqStartGame()
        {
            User user = usersFromIDs[Context.ConnectionId];
            if (user.CurrentGame == null)
            {
                return Task.CompletedTask;
            }

            _ = user.CurrentGame.StartAt(DateTime.UtcNow + TimeSpan.FromSeconds(2));//.Start();

            return Task.CompletedTask;
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            RemoveUser(Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Called by the client to join a certain game.
        /// Game must be already created.
        /// Game should not have started, though come to think of it we probably don't check that.
        /// </summary>
        /// <param name="gameName"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public Task<bool> Join(string gameName, string userName)
        {
            if (!games.ContainsKey(gameName))
            {
                return Task.FromResult(false);
            }
            return Task.FromResult(JoinGame(usersFromIDs[Context.ConnectionId], games[gameName], userName));
        }

        /// <summary>
        /// Called by the client to leave whatever game they are in.
        /// </summary>
        /// <returns></returns>
        public Task Leave()
        {
            RemoveUserFromGame(usersFromIDs[Context.ConnectionId]);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Called by the client to join a game.
        /// Will create the game if it does not already exist.
        /// </summary>
        /// <param name="gameName"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public Task<bool> JoinAndMaybeCreate(string gameName, string userName)
        {
            if (!games.ContainsKey(gameName))
            {
                games.Add(gameName, new Game(context)); ;//GlobalHost.ConnectionManager.GetHubContext<GameHub>()));
            }

            if (!JoinGame(usersFromIDs[Context.ConnectionId], games[gameName], userName))
            {
                games.Remove(gameName);
                return Task.FromResult(false);
            }
            return Task.FromResult(true);
        }

        /// <summary>
        /// Joins a user into a game.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="game"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        private bool JoinGame(User user, Game game, string userName)
        {
            user.CurrentUsername = userName;
            RemoveUserFromGame(user);
            return game.AddUser(user);
        }

        /// <summary>
        /// Removes a user from their game.
        /// </summary>
        /// <param name="user"></param>
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

        /// <summary>
        /// Cleans up after a user leaves.
        /// </summary>
        /// <param name="id"></param>
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
