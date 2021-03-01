using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMRTSServerCore.SimClasses.ServersideUnits
{
    /// <summary>
    /// Represents tanks.
    /// </summary>
    internal class Tank : Unit
    {
        /// <summary>
        /// For purposes of handling shot cooldown.
        /// </summary>
        DateTime lastShot = DateTime.UnixEpoch;
        public Tank(Guid id, User owner, Game game) : base(id, owner, game)
        {

        }

        /// <summary>
        /// The cooldown time for shooting.
        /// </summary>
        static TimeSpan coolDown = TimeSpan.FromSeconds(4);

        ulong currentMillis = 0;

        static int dmg = 10;

        public override void Update(ulong currentMilliseconds, float elapsedTime)
        {
            currentMillis = currentMilliseconds;

            base.Update(currentMilliseconds, elapsedTime);
        }

        /// <summary>
        /// Still a WIP system but at least tanks can theoretically shoot.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public override void Shoot(Unit target)
        {
            target.Health -= dmg;
            target.HealthUpdate = new GMRTSClasses.STCTransferData.ChangingData<float>(currentMillis, target.Health, 0);
            target.UpdateHealth = true;
        }
    }
}
