﻿using GMRTSClasses.STCTransferData;

using GMRTSServerCore.SimClasses;
using GMRTSServerCore.SimClasses.UnitStates;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GMRTSServerCore.SimClasses.ServersideUnits
{
    internal abstract class Unit
    {
        public Guid ID { get; set; }

        public float Health { get; set; }

        public bool IsIdling { get; private set; }

        private Vector2 pos;
        public Vector2 Position
        {
            get => pos;

            set
            {
                Game.UpdateUnitLookup(this, value);
                pos = value;
            }
        }

        public float Rotation { get; set; }

        public IdleOrder IdleOrder { get; }

        public LinkedList<IUnitOrder> Orders { get; set; }

        /// <summary>
        /// Temporary!
        /// </summary>
        public float VelocityMagnitude = 20f;

        public abstract bool TryShoot(Unit target);

        public Game Game { get; set; }

        public string[] LastFrameVisibleUsers { get; set; } = new string[0];

        public virtual void Update(ulong currentMilliseconds, float elapsedTime)
        {
            if (Orders.Count == 0)
            {
                if (!IsIdling)
                {
                    IdleOrder.NoLongerIdle();
                }
                IsIdling = true;

                IdleOrder.Update(currentMilliseconds, elapsedTime);
                return;
            }

            IsIdling = false;

            ContOrStop keepGoing = Orders.First.Value.Update(currentMilliseconds, elapsedTime);
            
            if (keepGoing == ContOrStop.Continue)
            {
                return;
            }

            if (keepGoing == ContOrStop.Requeue)
            {
                Orders.AddLast(Orders.First.Value);
            }

            Game.QueueActionOver(this, Orders.First.Value.ID);

            Orders.RemoveFirst();
        }

        public ChangingData<Vector2> PositionUpdate { get; set; }
        public bool UpdatePosition { get; set; } = false;

        public ChangingData<float> HealthUpdate { get; set; }
        public bool UpdateHealth { get; set; } = false;

        public ChangingData<float> RotationUpdate { get; set; }
        public bool UpdateRotation { get; set; } = false;

        public User Owner { get; set; }

        public Unit(Guid id, User owner, Game game)
        {
            ID = id;
            Orders = new LinkedList<IUnitOrder>();
            this.Owner = owner;
            this.Game = game;
            IdleOrder = new IdleOrder(this);
        }

        public BoidsSettings BoidsSettings = new BoidsSettings(cohesionMaxMag:          30f,
                                                               cohesionStrength:        0.5f,
                                                               cohesionDistance:        20f,
                                                               separationStrength:      10f,
                                                               separationDistance:      15f,
                                                               boidsStrength:           20f,
                                                               flowfieldStrength:       80f,
                                                               maximumBoidsVelocity:    70f);
    }
}
