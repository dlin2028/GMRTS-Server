using GMRTSServer.ServersideUnits;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GMRTSServer.UnitStates
{
    internal class MoveState : IUnitState
    {
        public Queue<Vector2> Targets { get; set; }

        public Unit Unit { get; set; }

        private Vector2 lastVel = new Vector2(0, 0);

        public float Velocity { get; }

        public float VelocitySquared { get; }

        public MoveState(float velocity)
        {
            Velocity = velocity;
            VelocitySquared = velocity * velocity;
        }

        public IUnitState Update(ulong currentMilliseconds, float elapsedTime)
        {
            if(Targets.Count == 0)
            {
                //Updates the relevant clients that |v|=0
                Unit.PositionUpdate = new GMRTSClasses.STCTransferData.ChangingData<Vector2>(currentMilliseconds, Unit.Position, Vector2.Zero);
                Unit.UpdatePosition = true;
                return new IdleState();
            }
            Vector2 currTarg = Targets.Peek();
            Vector2 diffVec = currTarg - Unit.Position;
            if(diffVec.LengthSquared() <= VelocitySquared)
            {
                Unit.Position = Targets.Dequeue();
                return this;
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
            return this;
        }
    }
}
