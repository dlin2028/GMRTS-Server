using GMRTSClasses.CTSTransferData.UnitGround;

using GMRTSServer.ServersideUnits;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GMRTSServer.UnitStates
{
    internal class MoveOrder : IUnitOrder
    {
        public List<Unit> OriginalUnits { get; set; }

        public Unit Unit { get; set; }

        public Vector2 Target { get; set; }

        private Vector2 lastVel = new Vector2(0, 0);

        public float Velocity { get; }

        public float VelocitySquared { get; }
        public Guid ID { get; set; }

        public MoveOrder(float velocity)
        {
            Velocity = velocity;
            VelocitySquared = velocity * velocity;
        }

        public MoveOrder(float velocity, MoveAction act, List<Unit> originalUnits, Unit unit)
        {
            Velocity = velocity;
            VelocitySquared = velocity * velocity;

            OriginalUnits = originalUnits;
            Target = act.Position;

            Unit = unit;
            ID = act.ActionID;
        }

        public ContOrStop Update(ulong currentMilliseconds, float elapsedTime)
        {

            Vector2 diffVec = Target - Unit.Position;
            if(diffVec.LengthSquared() <= VelocitySquared)
            {
                Unit.Position = Target;

                //Updates the relevant clients that |v|=0
                Unit.PositionUpdate = new GMRTSClasses.STCTransferData.ChangingData<Vector2>(currentMilliseconds, Unit.Position, Vector2.Zero);
                Unit.UpdatePosition = true;
                return ContOrStop.Stop;
            }

            Vector2 normalized = diffVec / diffVec.Length();
            Vector2 velVec = normalized * Velocity;
            Unit.Position += velVec * elapsedTime;
            if ((velVec - lastVel).LengthSquared() >= 0.001)
            {
                lastVel = velVec;
                //Update clients
                Unit.PositionUpdate = new GMRTSClasses.STCTransferData.ChangingData<Vector2>(currentMilliseconds, Unit.Position, velVec);
                Unit.UpdatePosition = true;
            }
            return ContOrStop.Continue;
        }
    }
}
