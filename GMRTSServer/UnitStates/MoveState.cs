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
                //Update to have |v|=0
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
            }
            return this;
        }
    }
}
