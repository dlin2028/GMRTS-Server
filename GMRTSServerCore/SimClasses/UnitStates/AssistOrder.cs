﻿
using GMRTSServerCore.SimClasses.ServersideUnits;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GMRTSServerCore.SimClasses.UnitStates
{
    internal class AssistOrder : IUnitOrder
    {
        public Guid ID { get; set; }

        private IMovementCalculator movementCalculator;

        private Vector2 lastVel = new Vector2(0, 0);

        public Unit Assister;
        public Unit Target;

        public ContOrStop Update(ulong currentMilliseconds, float elapsedTime)
        {
            Vector2 velocity = Vector2.Zero;

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

                if (Target.Health <= 0)
                {
                    return ContOrStop.Stop;
                }
            }

            if ((velocity - lastVel).LengthSquared() >= 0.001)
            {
                lastVel = velocity;
                //Update clients
                Assister.PositionUpdate = new GMRTSClasses.STCTransferData.ChangingData<Vector2>(currentMilliseconds, Assister.Position, velocity);
                Assister.UpdatePosition = true;
            }

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
