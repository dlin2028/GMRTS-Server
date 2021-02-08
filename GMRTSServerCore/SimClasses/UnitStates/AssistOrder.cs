
using GMRTSServerCore.SimClasses.ServersideUnits;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GMRTSServerCore.SimClasses.UnitStates
{
    /// <summary>
    /// Represents Assist orders. Or, actions. Depends which bit of the code you ask.
    /// </summary>
    internal class AssistOrder : IUnitOrder
    {
        public Guid ID { get; set; }

        /// <summary>
        /// Outsourcing our movement math.
        /// </summary>
        private IMovementCalculator movementCalculator;

        /// <summary>
        /// To know if we need to update the clients.
        /// </summary>
        private Vector2 lastVel = new Vector2(0, 0);

        /// <summary>
        /// Don't wanna forget this.
        /// </summary>
        public Unit Assister;

        /// <summary>
        /// Don't wanna forget this either.
        /// </summary>
        public Unit Target;

        /// <summary>
        /// Basically moves the owner of this order to the target.
        /// </summary>
        /// <param name="currentMilliseconds"></param>
        /// <param name="elapsedTime"></param>
        /// <returns></returns>
        public ContOrStop Update(ulong currentMilliseconds, float elapsedTime)
        {
            Vector2 velocity = Vector2.Zero;

            // Arbitrary hardcoded distance. pls fix.
            if ((Assister.Position - Target.Position).LengthSquared() > 2500)
            {
                velocity = movementCalculator.ComputeVelocity(Assister.Game, Assister, Target.Position);
            }
            else
            {
                velocity = Vector2.Zero;
                // TODO: Non-pathfinding computation
                // This should be a method on the movement calculator for units that are not actively pathing (maybe idle?)
                // Maybe something like this?
                // Update: Ask and you shall receive
                velocity = movementCalculator.ComputeVelocity(Assister.Game, Assister);

                // If you're too late to help in time, no purpose in going at all.
                if (Target.Health <= 0)
                {
                    return ContOrStop.Stop;
                }
            }

            // Arbitrary hardcoded velocity change threshold.
            if ((velocity - lastVel).LengthSquared() >= 0.001)
            {
                lastVel = velocity;
                //Update clients
                Assister.PositionUpdate = new GMRTSClasses.STCTransferData.ChangingData<Vector2>(currentMilliseconds, Assister.Position, velocity);
                Assister.UpdatePosition = true;
            }

            // At some point in the past I think I forgot this and, uh, that didn't work too well.
            Assister.Position += velocity * elapsedTime;

            return ContOrStop.Continue;
        }

        public AssistOrder(Guid id, IMovementCalculator movementCalculator)
        {
            ID = id;
            this.movementCalculator = movementCalculator;
        }
    }
}
