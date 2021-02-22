using GMRTSServerCore.SimClasses.ServersideUnits;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GMRTSServerCore.SimClasses
{
    internal class EuclideanDistanceLineOfSightChecker : IVisibilityChecker
    {
        public Unit Owner { get; }

        public bool CanSee(Unit target) => (target.Position - Owner.Position).LengthSquared() <= Owner.CombatSettings.VisionDistanceSquared;

        public void Update() { }

        public EuclideanDistanceLineOfSightChecker(Unit owner)
        {
            Owner = owner;
        }
    }
}
