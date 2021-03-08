using GMRTSClasses.ConstructionOrderDetails;
using GMRTSClasses.CTSTransferData;

using GMRTSServerCore.SimClasses.ServersideUnits;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace GMRTSServerCore.SimClasses.UnitStates
{
    internal class BuildBuildingOrder : IUnitOrder
    {
        public Guid ID { get; set; }

        private IMovementCalculator movementCalculator;

        private Vector2 lastVel = new Vector2(0, 0);

        public BuildingType FinalType;
        public Vector2 TargetPosition;

        public float RequiredMoney;
        public float RequiredMineral;

        public Unit Builder;

        public ContOrStop Update(ulong currentMilliseconds, float elapsedTime)
        {
            Vector2 velocity = Vector2.Zero;

            if ((Builder.Position - TargetPosition).LengthSquared() > 2500)
            {
                velocity = movementCalculator.ComputeVelocity(Builder.Owner.CurrentGame, Builder, TargetPosition, currentMilliseconds);
            }
            else
            {
                velocity = Vector2.Zero;
                // TODO: Non-pathfinding computation
                // This should be a method on the movement calculator for units that are not actively pathing (maybe idle?)
                // Maybe something like this?
                // Update: Ask and you shall receive
                velocity = movementCalculator.ComputeVelocity(Builder.Owner.CurrentGame, Builder);

                if (Builder.Owner.Money >= RequiredMoney && Builder.Owner.Mineral >= RequiredMineral)
                {
                    Builder.Owner.CurrentGame.SpawnBuildingAndChargeUser(Builder.Owner, FinalType, TargetPosition);
                    return ContOrStop.Stop;
                }
            }

            if ((velocity - lastVel).LengthSquared() >= 0.001)
            {
                lastVel = velocity;
                //Update clients
                Builder.PositionUpdate = new GMRTSClasses.STCTransferData.ChangingData<Vector2>(currentMilliseconds, Builder.Position, velocity);
                Builder.UpdatePosition = true;
            }

            Builder.Position += velocity * elapsedTime;

            return ContOrStop.Continue;
        }

        public BuildBuildingOrder(Guid id, IMovementCalculator movementCalculator, BuildingType buildingType, Vector2 buildingPos)
        {
            ID = id;
            this.movementCalculator = movementCalculator;
            RequiredMineral = Prices.BuildingPriceData[buildingType].RequiredMineral;
            RequiredMoney = Prices.BuildingPriceData[buildingType].RequiredMoney;
            FinalType = buildingType;
            TargetPosition = buildingPos;
        }
    }
}
