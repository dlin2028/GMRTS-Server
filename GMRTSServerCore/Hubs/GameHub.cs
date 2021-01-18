﻿using GMRTSClasses.CTSTransferData;
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

        public Task Assist(AssistAction act)
        {
            if (usersFromIDs[Context.ConnectionId].CurrentGame == null)
            {
                return Task.CompletedTask;
            }
            usersFromIDs[Context.ConnectionId].CurrentGame.AssistIfCan(act, usersFromIDs[Context.ConnectionId]);
            return Task.CompletedTask;
        }

        public Task Attack(AttackAction act)
        {
            if (usersFromIDs[Context.ConnectionId].CurrentGame == null)
            {
                return Task.CompletedTask;
            }
            usersFromIDs[Context.ConnectionId].CurrentGame.AttackIfCan(act, usersFromIDs[Context.ConnectionId]);
            return Task.CompletedTask;
        }

        public Task BuildBuilding(BuildBuildingAction act)
        {
            if (usersFromIDs[Context.ConnectionId].CurrentGame == null)
            {
                return Task.CompletedTask;
            }
            usersFromIDs[Context.ConnectionId].CurrentGame.BuildBuildingIfCan(act, usersFromIDs[Context.ConnectionId]);
            return Task.CompletedTask;
        }

        public Task ReplaceMoveAction(ReplaceAction<MoveAction> act)
        {
            if (usersFromIDs[Context.ConnectionId].CurrentGame == null)
            {
                return Task.CompletedTask;
            }

            usersFromIDs[Context.ConnectionId].CurrentGame.MoveIfCan(act.NewAction, usersFromIDs[Context.ConnectionId], act.TargetActionID);

            return Task.CompletedTask;
        }

        public Task ReplaceBuildBuildingAction(ReplaceAction<BuildBuildingAction> act)
        {
            if (usersFromIDs[Context.ConnectionId].CurrentGame == null)
            {
                return Task.CompletedTask;
            }

            usersFromIDs[Context.ConnectionId].CurrentGame.BuildBuildingIfCan(act.NewAction, usersFromIDs[Context.ConnectionId], act.TargetActionID);

            return Task.CompletedTask;
        }

        public Task ReplaceAttackAction(ReplaceAction<AttackAction> act)
        {
            if (usersFromIDs[Context.ConnectionId].CurrentGame == null)
            {
                return Task.CompletedTask;
            }

            usersFromIDs[Context.ConnectionId].CurrentGame.AttackIfCan(act.NewAction, usersFromIDs[Context.ConnectionId], act.TargetActionID);

            return Task.CompletedTask;
        }

        public Task ReplaceAssistAction(ReplaceAction<AssistAction> act)
        {
            if (usersFromIDs[Context.ConnectionId].CurrentGame == null)
            {
                return Task.CompletedTask;
            }

            usersFromIDs[Context.ConnectionId].CurrentGame.AssistIfCan(act.NewAction, usersFromIDs[Context.ConnectionId], act.TargetActionID);

            return Task.CompletedTask;
        }

        public Task Delete(DeleteAction act)
        {
            if (usersFromIDs[Context.ConnectionId].CurrentGame == null)
            {
                return Task.CompletedTask;
            }

            usersFromIDs[Context.ConnectionId].CurrentGame.DeleteIfCan(act, usersFromIDs[Context.ConnectionId]);
            return Task.CompletedTask;
        }

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

        public Task<bool> FactoryEnqueue(EnqueueBuildOrder order)
        {
            if (usersFromIDs[Context.ConnectionId].CurrentGame == null)
            {
                return Task.FromResult(false);
            }

            return Task.FromResult(usersFromIDs[Context.ConnectionId].CurrentGame.EnqueueBuildOrder(usersFromIDs[Context.ConnectionId], order));
        }

        public Task<bool> FactoryCancel(CancelBuildOrder cancel)
        {
            if (usersFromIDs[Context.ConnectionId].CurrentGame == null)
            {
                return Task.FromResult(false);
            }

            return Task.FromResult(usersFromIDs[Context.ConnectionId].CurrentGame.CancelBuildOrder(usersFromIDs[Context.ConnectionId], cancel));
        }

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

        public Task<bool> Join(string gameName, string userName)
        {
            if (!games.ContainsKey(gameName))
            {
                return Task.FromResult(false);
            }
            return Task.FromResult(JoinGame(usersFromIDs[Context.ConnectionId], games[gameName], userName));
        }

        public Task Leave()
        {
            RemoveUserFromGame(usersFromIDs[Context.ConnectionId]);
            return Task.CompletedTask;
        }

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
