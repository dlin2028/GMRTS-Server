using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GMRTSServerCore.SimClasses
{
    public struct CombatSettings
    {
        public readonly bool CanShoot;
        public readonly TimeSpan ShotCooldown;
        public readonly int VisionDistance;
        public readonly int VisionDistanceSquared;

        public CombatSettings(bool canShoot, TimeSpan shotCooldown, int visionDistance)
        {
            CanShoot = canShoot;
            ShotCooldown = shotCooldown;
            VisionDistance = visionDistance;
            VisionDistanceSquared = VisionDistance * VisionDistance;
        }
    }
}
