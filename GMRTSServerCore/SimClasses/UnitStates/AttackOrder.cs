
using GMRTSServerCore.SimClasses.ServersideUnits;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GMRTSServerCore.SimClasses.UnitStates
{
    internal class AttackOrder : IUnitOrder
    {
        public Guid ID { get; set; }

        /// <summary>
        /// Outsource movement math.
        /// </summary>
        private IMovementCalculator movementCalculator;

        private Vector2 lastVel = new Vector2(0, 0);

        public Unit Attacker;
        public Unit Target;

        /// <summary>
        /// Move the attacker towards the target and attack if appropriate.
        /// </summary>
        /// <param name="currentMilliseconds"></param>
        /// <param name="elapsedTime"></param>
        /// <returns></returns>
        public ContOrStop Update(ulong currentMilliseconds, float elapsedTime)
        {
            Vector2 velocity = Vector2.Zero;
            Attacker.CombatTargetTracker.PreferredTarget = Target;

            // Arbitrary, hardcoded, yada yada, you know the drill.
            if ((Attacker.Position - Target.Position).LengthSquared() > 2500)
            {
                velocity = movementCalculator.ComputeVelocity(Attacker.Game, Attacker, Target.Position, currentMilliseconds);
            }
            else
            {
                velocity = Vector2.Zero;
                // TODO: Non-pathfinding computation
                // This should be a method on the movement calculator for units that are not actively pathing (maybe idle?)
                // Maybe something like this?
                // Update: Ask and you shall receive
                velocity = movementCalculator.ComputeVelocity(Attacker.Game, Attacker);

                // Mission accomplished. Next target, soldier!
                if (Target.IsDead)
                {
                    Attacker.CombatTargetTracker.PreferredTarget = null;
                    return ContOrStop.Stop;
                }
            }

            // Do I even need to say it?
            if ((velocity - lastVel).LengthSquared() >= 0.001)
            {
                lastVel = velocity;
                //Update clients
                Attacker.PositionUpdate = new GMRTSClasses.STCTransferData.ChangingData<Vector2>(currentMilliseconds, Attacker.Position, velocity);
                Attacker.UpdatePosition = true;
            }

            Attacker.Position += velocity * elapsedTime;

            return ContOrStop.Continue;
        }

        public AttackOrder(Guid id, IMovementCalculator movementCalculator)
        {
            ID = id;
            this.movementCalculator = movementCalculator;
        }
    }
}
