using GMRTSServerCore.SimClasses.ServersideUnits;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GMRTSServerCore.SimClasses
{
    internal class StandardCombatTargetTracker : ICombatTargetTracker
    {
        public Unit Owner { get; }

        public Unit Target { get; private set; }

        public Unit PreferredTarget { get; set; }

        public IVisibilityChecker VisibilityChecker { get; }
        public IUnitPositionLookup UnitPositionLookup { get; }

        public void Update()
        {
            //VisibilityChecker.Update();

            // Prefer the preferred target
            if (PreferredTarget != null && !PreferredTarget.IsDead && Target != PreferredTarget && VisibilityChecker.CanSee(PreferredTarget))
            {
                Target = PreferredTarget;
            }

            // Otherwise, if new target is needed, look for new target
            else if (Target == null || Target.IsDead || !VisibilityChecker.CanSee(Target))
            {
                Target = null;
                foreach (Unit unit in UnitPositionLookup.UnitsWithinCircular(Owner.Position, Owner.CombatSettings.VisionDistance))
                {
                    if (unit.Owner == Owner.Owner)
                    {
                        continue;
                    }

                    if (VisibilityChecker.CanSee(unit))
                    {
                        Target = unit;
                        break;
                    }
                }
            }
        }

        public StandardCombatTargetTracker(Unit owner, IVisibilityChecker visibilityChecker, IUnitPositionLookup lookup)
        {
            Owner = owner;
            VisibilityChecker = visibilityChecker;
            UnitPositionLookup = lookup;
        }
    }
}
