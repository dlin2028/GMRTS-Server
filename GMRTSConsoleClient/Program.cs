using GMRTSClasses;

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


        static async Task Main(string[] args)
        {

            SignalRClient client = new SignalRClient("http://localhost:61337/", "GameHub", a => null, TimeSpan.FromMilliseconds(400));
            bool success = await client.TryStart();
            ;
            ;
            Console.ReadLine();
            client.OnPositionUpdate += Client_OnPositionUpdate;
            await client.MoveAction(new GMRTSClasses.CTSTransferData.MoveAction() { Positions = new List<Vector2>() { new Vector2(100, 200) }, UnitIDs = new List<Guid> { Guid.NewGuid() } });
            await client.MoveAction(new GMRTSClasses.CTSTransferData.MoveAction() { Positions = new List<Vector2>() { new Vector2(200, 100) }, UnitIDs = new List<Guid> { Guid.NewGuid() } });
            Console.ReadLine();
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