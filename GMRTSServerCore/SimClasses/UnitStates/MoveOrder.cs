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
    internal class MoveOrder : IUnitOrder
    {
        public List<Unit> OriginalUnits { get; set; }

        public Unit Unit { get; set; }

        public Vector2 Target { get; }

        public (int x, int y) TargetSquare { get; }

        private Vector2 lastVel = new Vector2(0, 0);

        public Guid ID { get; set; }

        public bool RequeueOnComplete { get; set; }

        //public MoveOrder(float velocity)
        //{
        //    Velocity = velocity;
        //    VelocitySquared = velocity * velocity;
        //}

        public MoveOrder(MoveAction act, List<Unit> originalUnits, Unit unit)
        {
            OriginalUnits = originalUnits;
            Target = act.Position;
            TargetSquare = IMovementCalculator.fromVec2(Target, unit.Game.Map.TileSize);

            Unit = unit;
            ID = act.ActionID;
            RequeueOnComplete = act.RequeueOnCompletion;
        }

        public ContOrStop Update(ulong currentMilliseconds, float elapsedTime)
        {
            Vector2 vel = Unit.Game.movementCalculator.ComputeVelocity(Unit.Game, Unit, Target);
            //Vector2 diffVec = Target - Unit.Position;
            //if(diffVec.LengthSquared() <= vel.LengthSquared() * 0.001f)
            if (IMovementCalculator.fromVec2(Unit.Position, Unit.Game.Map.TileSize) == TargetSquare)
            {
                Unit.Position = Target;

                //Updates the relevant clients that |v|=0
                Unit.PositionUpdate = new GMRTSClasses.STCTransferData.ChangingData<Vector2>(currentMilliseconds, Unit.Position, Vector2.Zero);
                Unit.UpdatePosition = true;
                lastVel = Vector2.Zero;
                return RequeueOnComplete ? ContOrStop.Requeue : ContOrStop.Stop;
            }

            Unit.Position += vel * elapsedTime;
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
