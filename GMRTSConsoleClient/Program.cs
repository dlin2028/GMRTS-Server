using GMRTSClasses;
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
        static double identity(int n)
        {
            return n;
        }

        static Dictionary<Guid, Unit> units = new Dictionary<Guid, Unit>();

        static async Task Main(string[] args)
        {

            SignalRClient client = new SignalRClient("http://localhost:61337/", "GameHub", a => null, TimeSpan.FromMilliseconds(400));
            bool success = await client.TryStart();
            Console.WriteLine(success ? "Success!" : "Failure");
            Console.ReadLine();
            client.OnPositionUpdate += Client_OnPositionUpdate;
            client.SpawnUnit += Client_SpawnUnit;
            await client.JoinGameByNameAndCreateIfNeeded("TestGame");
            Console.WriteLine("Connected to game!");
            Console.ReadLine();
            await client.MoveAction(new GMRTSClasses.CTSTransferData.MoveAction() { Positions = new List<Vector2>() { new Vector2(100, 200) }, UnitIDs = new List<Guid> { Guid.NewGuid() } });
            await client.MoveAction(new GMRTSClasses.CTSTransferData.MoveAction() { Positions = new List<Vector2>() { new Vector2(200, 100) }, UnitIDs = new List<Guid> { Guid.NewGuid() } });
            Console.ReadLine();
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

            Console.WriteLine($"{obj.Type} added with ID {obj.ID}");
        }

        private static void Client_OnPositionUpdate(GMRTSClasses.Units.Unit arg1, GMRTSClasses.STCTransferData.ChangingData<Vector2> arg2)
        {
            Console.WriteLine($"{arg1.GetType().Name} {arg1.ID} is at {arg2.Value} traveling {arg2.Delta} per second");
        }

        private static void Program_a()
        {
            Console.WriteLine("hi");
        }
    }
}