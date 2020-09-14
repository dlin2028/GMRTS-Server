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

        public IUnitState Update(ulong currentMilliseconds, float elapsedTime)
        {
            if(Targets.Count == 0)
            {
                //Updates the relevant clients that |v|=0
                Unit.PositionUpdate = new GMRTSClasses.STCTransferData.ChangingData<Vector2>(currentMilliseconds, Unit.Position, Vector2.Zero);
                return new IdleState();
            }
            Vector2 currTarg = Targets.Peek();
            Vector2 diffVec = currTarg - Unit.Position;
            if(diffVec.LengthSquared() <= 20f)
            {
                Targets.Dequeue();
                return this;
            }

            Vector2 normalized = diffVec / diffVec.Length();
            Vector2 velVec = normalized * 3;
            Unit.Position += velVec * elapsedTime;
            if (velVec != lastVel)
            {
                lastVel = velVec;
                //Update clients
                Unit.PositionUpdate = new GMRTSClasses.STCTransferData.ChangingData<Vector2>(currentMilliseconds, Unit.Position, velVec);
            }
            return this;
        }
    }
}
