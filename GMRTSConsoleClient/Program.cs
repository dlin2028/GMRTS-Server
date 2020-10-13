using GMRTSClasses;
using GMRTSClasses.CTSTransferData.UnitGround;
using GMRTSClasses.Units;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GMRTSConsoleClient
{
    class Program
    {

        static Dictionary<Guid, Unit> units = new Dictionary<Guid, Unit>();
        static object locker = new object();
        static bool gameStarted = false;

        static async Task Main(string[] args)
        {

            SignalRClient client = new SignalRClient("http://localhost:61337/", "GameHub", a => units[a], TimeSpan.FromMilliseconds(400));
            bool success = await client.TryStart();
            Console.WriteLine(success ? "Success!" : "Failure");
            Console.ReadLine();
            client.OnPositionUpdate += Client_OnPositionUpdate;
            client.SpawnUnit += Client_SpawnUnit;
            client.OnGameStart += Client_OnGameStart;
            Console.WriteLine(await client.JoinGameByNameAndCreateIfNeeded("TestGame", "lalala") ? "Join success" : "Join failure");
            Console.ReadLine();
            await client.RequestGameStart();
            await Task.Run(async () =>
            {
                bool keepGoing = true;
                while(keepGoing)
                {
                    lock(locker)
                    {
                        keepGoing = !gameStarted;
                    }
                    await Task.Delay(1000);
                }
            });
            Console.ReadLine();
            await client.MoveAction(new MoveAction() { Position = new Vector2(100, 200), UnitIDs = new List<Guid> { units.Keys.First() } });
            await client.MoveAction(new MoveAction() { Position = new Vector2(100, 250), UnitIDs = new List<Guid> { units.Keys.First() } });
            await client.MoveAction(new MoveAction() { Position = new Vector2(0, 0), UnitIDs = new List<Guid> { units.Keys.First() } });
            Console.WriteLine("Moves requested");
            Console.ReadLine();
        }

        private static void Client_OnGameStart(DateTime obj)
        {
            Console.WriteLine($"Game started at {obj.ToLocalTime()}");
            lock(locker)
            {
                gameStarted = true;
            }
        }

        private static void Client_SpawnUnit(GMRTSClasses.STCTransferData.UnitSpawnData obj)
        {
            Unit unit = null;
            switch(obj.Type)
            {
                case "Tank":
                    unit = new Tank(obj.ID);
                    break;
                case "Builder":
                    unit = new Builder(obj.ID);
                    break;
                default:
                    throw new Exception();
            }
            units.Add(obj.ID, unit);

            Console.WriteLine($"{obj.Type} added with ID {obj.ID} to player {obj.OwnerUsername}");
        }

        private static void Client_OnPositionUpdate(GMRTSClasses.Units.Unit arg1, GMRTSClasses.STCTransferData.ChangingData<Vector2> arg2)
        {
            Console.WriteLine($"{arg1.GetType().Name} {arg1.ID} is at {arg2.Value} traveling {arg2.Delta} per second");
        }
    }
}