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
    /// Just for idling.
    /// </summary>
    internal class IdleOrder : IUnitOrder
    {
        public Guid ID { get; set; }

        public Vector2 LastVel;
        static Vector2 InfInf = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
        public Unit Unit;

        /// <summary>
        /// Literally just idle. That's it. Oh and boids too.
        /// </summary>
        /// <param name="currentMilliseconds"></param>
        /// <param name="elapsedTime"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Jesucristo there are so many better ways of doing this.
        /// </summary>
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
