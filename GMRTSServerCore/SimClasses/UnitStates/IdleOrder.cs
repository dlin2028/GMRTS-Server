using GMRTSServerCore.SimClasses.ServersideUnits;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GMRTSServerCore.SimClasses.UnitStates
{
    internal class IdleOrder : IUnitOrder
    {
        public Guid ID { get; set; }

        public Vector2 LastVel;
        static Vector2 InfInf = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
        public Unit Unit;

        public ContOrStop Update(ulong currentMilliseconds, float elapsedTime)
        {
            Vector2 velocity = Unit.Game.movementCalculator.ComputeVelocity(Unit.Game, Unit);

            Unit.Position += velocity * elapsedTime;
            if ((velocity - LastVel).LengthSquared() >= 0.001)
            {
                LastVel = velocity;
                //Update clients
                Unit.PositionUpdate = new GMRTSClasses.STCTransferData.ChangingData<Vector2>(currentMilliseconds, Unit.Position, velocity);
                Unit.UpdatePosition = true;
            }

            return ContOrStop.Continue;
        }

        internal void NoLongerIdle()
        {
            LastVel = InfInf;
        }

        internal IdleOrder(Unit unit)
        {
            LastVel = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
            Unit = unit;
        }
    }
}
