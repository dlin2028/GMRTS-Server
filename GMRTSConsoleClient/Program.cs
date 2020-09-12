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
            await client.MoveAction(new GMRTSClasses.CTSTransferData.MoveAction() { Positions = new List<Vector2>() { new Vector2(100, 200) }, UnitIDs = new List<Guid> { Guid.NewGuid() } });
            await client.MoveAction(new GMRTSClasses.CTSTransferData.MoveAction() { Positions = new List<Vector2>() { new Vector2(200, 100) }, UnitIDs = new List<Guid> { Guid.NewGuid() } });
            Console.ReadLine();
        }

        private static void Program_a()
        {
            Console.WriteLine("hi");
        }
    }
}