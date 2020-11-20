using GMRTSClasses.CTSTransferData;
using GMRTSClasses.CTSTransferData.MetaActions;
using GMRTSClasses.CTSTransferData.UnitGround;
using GMRTSClasses.CTSTransferData.UnitUnit;
using GMRTSClasses.STCTransferData;
using GMRTSClasses.Units;

using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GMRTSClasses
{
    public class SignalRClient
    {
        private HubConnection connection;
        private Func<Guid, Unit> getUnit;
        private TimeSpan timeSinceHeartbeat;
        private TimeSpan heartbeatTimeout;

        public event Action OnHeartbeatDeath = null;

        public SignalRClient(string url, Func<Guid, Unit> getUnit, TimeSpan heartbeatTimeout)
        {
            connection = new HubConnectionBuilder().AddJsonProtocol((a) => { a.PayloadSerializerOptions.IncludeFields = true; }).WithUrl(url).Build();
            this.getUnit = getUnit;
            timeSinceHeartbeat = TimeSpan.Zero;
            this.heartbeatTimeout = heartbeatTimeout;
            OnHeartbeat += Beat;

            //These are IDisposable. I should probably make this class IDisposable, too, so it can dispose of them.
            connection.On("HeartbeatFromServer", OnHeartbeat);
            connection.On<Guid, ChangingData<Vector2>>("UpdatePosition", PosUpdate);
            connection.On<Guid, ChangingData<float>>("UpdateHealth", HealthUpdate);
            connection.On<Guid, ChangingData<float>>("UpdateRotation", RotationUpdate);
            connection.On<Guid>("KillUnit", KillUnit);
            connection.On<UnitSpawnData>("AddUnit", AddUnit);
            connection.On<DateTime>("GameStarted", GameStart);
        }

        public async Task<bool> JoinGameByName(string gameName, string userName)
        {
            return await connection.InvokeAsync<bool>("Join", gameName, userName);
        }

        public async Task<bool> JoinGameByNameAndCreateIfNeeded(string gameName, string userName)
        {
            return await connection.InvokeAsync<bool>("JoinAndMaybeCreate", gameName, userName);
        }

        public async Task RequestGameStart()
        {
            await connection.InvokeAsync("ReqStartGame");
        }

        public async Task LeaveGame()
        {
            await connection.InvokeAsync("Leave");
        }

        public async Task AssistAction(AssistAction action)
        {
            await connection.InvokeAsync("Assist", action);
        }

        public async Task AttackAction(AttackAction action)
        {
            await connection.InvokeAsync("Attack", action);
        }

        public async Task BuildFactoryAction(BuildBuildingAction action)
        {
            await connection.InvokeAsync("BuildBuilding", action);
        }

        public async Task MoveAction(MoveAction action)
        {
            await connection.InvokeAsync("Move", action);
        }

        public async Task DeleteAction(DeleteAction action)
        {
            await connection.InvokeAsync("Delete", action);
        }

        public async Task ReplaceAction(ReplaceAction action)
        {
            await connection.InvokeAsync("Replace", action);
        }
        
        public async Task ArbitraryNonmeta(ClientAction action)
        {
            await connection.InvokeAsync("Arbitrary", action);
        }

        public async Task<bool> TryStart()
        {
            bool faulted = false;
            await connection.StartAsync().ContinueWith(t => faulted = t.IsFaulted);
            return !faulted;
        }

        public void UpdateHeartbeatTimer(TimeSpan elapsedTime)
        {
            timeSinceHeartbeat += elapsedTime;
            if(timeSinceHeartbeat > heartbeatTimeout)
            {
                OnHeartbeatDeath?.Invoke();
            }
        }

        public async Task KillConnection()
        {
            await connection.StopAsync();
        }

        public event Action OnHeartbeat;
        public event Action<Unit, ChangingData<Vector2>> OnPositionUpdate;
        public event Action<Unit, ChangingData<float>> OnHealthUpdate;
        public event Action<Unit, ChangingData<float>> OnRotationUpdate;
        public event Action<Unit> TriggerUnitDeath;
        public event Action<UnitSpawnData> SpawnUnit;
        public event Action<DateTime> OnGameStart;

        private async void Beat()
        {
            timeSinceHeartbeat = TimeSpan.Zero;
            await connection.InvokeAsync("HeartbeatFromClient");
        }

        private void PosUpdate(Guid id, ChangingData<Vector2> newPos)
        {
            Unit unit = getUnit(id);
            unit.Position = new Changing<Vector2>(newPos.Value, newPos.Delta, unit.Position.Changer, newPos.StartTime);
            OnPositionUpdate?.Invoke(unit, newPos);
        }

        private void HealthUpdate(Guid id, ChangingData<float> newHealth)
        {
            Unit unit = getUnit(id);
            unit.Health = new Changing<float>(newHealth.Value, newHealth.Delta, unit.Health.Changer, newHealth.StartTime);
            OnHealthUpdate?.Invoke(unit, newHealth);
        }

        private void RotationUpdate(Guid id, ChangingData<float> newRotation)
        {
            Unit unit = getUnit(id);
            unit.Rotation = new Changing<float>(newRotation.Value, newRotation.Delta, unit.Rotation.Changer, newRotation.StartTime);
            OnRotationUpdate?.Invoke(unit, newRotation);
        }

        private void KillUnit(Guid id)
        {
            Unit unit = getUnit(id);
            TriggerUnitDeath?.Invoke(unit);
        }

        private void AddUnit(UnitSpawnData spawnInfo)
        {
            SpawnUnit?.Invoke(spawnInfo);
        }

        private void GameStart(DateTime time)
        {
            OnGameStart?.Invoke(time);
        }
    }
}
