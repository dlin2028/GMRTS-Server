using GMRTSClasses.CTSTransferData.UnitGround;

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
    /// For moving to a fixed position.
    /// </summary>
    internal class MoveOrder : IUnitOrder
    {
        public List<Unit> OriginalUnits { get; set; }

        public Unit Unit { get; set; }

        public Vector2 Target { get; }

        public (int x, int y) TargetSquare { get; }

        private Vector2 lastVel = new Vector2(0, 0);

        public Guid ID { get; set; }

        /// <summary>
        /// Basically patrol or no
        /// </summary>
        public bool RequeueOnComplete { get; set; }

        // Commenting code?
        // More like commenting *out* code, amirite?

        //public MoveOrder(float velocity)
        //{
        //    Velocity = velocity;
        //    VelocitySquared = velocity * velocity;
        //}

        public MoveOrder(MoveAction act, List<Unit> originalUnits, Unit unit)
        {
            OriginalUnits = originalUnits;
            Target = act.Position;
            TargetSquare = IMovementCalculator.fromVec2(Target, unit.Owner.CurrentGame.Map.TileSize);

            Unit = unit;
            ID = act.ActionID;
            RequeueOnComplete = act.RequeueOnCompletion;
        }

        /// <summary>
        /// Moves a bit toward the destination.
        /// </summary>
        /// <param name="currentMilliseconds"></param>
        /// <param name="elapsedTime"></param>
        /// <returns></returns>
        public ContOrStop Update(ulong currentMilliseconds, float elapsedTime)
        {
            Vector2 vel = Unit.Owner.CurrentGame.movementCalculator.ComputeVelocity(Unit.Owner.CurrentGame, Unit, Target, currentMilliseconds);
            //Vector2 diffVec = Target - Unit.Position;
            //if(diffVec.LengthSquared() <= vel.LengthSquared() * 0.001f)

            // Same grid square == close enough
            if (IMovementCalculator.fromVec2(Unit.Position, Unit.Owner.CurrentGame.Map.TileSize) == TargetSquare)
            {
                //Unit.Position = Target;

                //Updates the relevant clients that |v|=0
                Unit.PositionUpdate = new GMRTSClasses.STCTransferData.ChangingData<Vector2>(currentMilliseconds, Unit.Position, Vector2.Zero);
                Unit.UpdatePosition = true;
                lastVel = Vector2.Zero;
                return RequeueOnComplete ? ContOrStop.Requeue : ContOrStop.Stop;
            }

            Unit.Position += vel * elapsedTime;

            // You know the drill.
            if ((vel - lastVel).LengthSquared() >= 0.001)
            {
                lastVel = vel;
                //Update clients
                Unit.PositionUpdate = new GMRTSClasses.STCTransferData.ChangingData<Vector2>(currentMilliseconds, Unit.Position, vel);
                Unit.UpdatePosition = true;
            }
            return ContOrStop.Continue;
        }
    }
}
