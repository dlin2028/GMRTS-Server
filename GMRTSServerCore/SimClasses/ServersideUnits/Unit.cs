using GMRTSClasses.STCTransferData;

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
    /// <summary>
    /// Base class for server-side units.
    /// </summary>
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
                // Update the unit lookup by position.
                Owner.CurrentGame.UpdateUnitLookup(this, value);
                pos = value;
            }
        }

        public float Rotation { get; set; }

        /// <summary>
        /// The order to run when we aren't doing anything.
        /// </summary>
        public IdleOrder IdleOrder { get; }

        /// <summary>
        /// Orders to do.
        /// </summary>
        public LinkedList<IUnitOrder> Orders { get; set; }

        /// <summary>
        /// This comment used to say:
        /// Temporary!
        /// Now, I saw this and I was gonna say that aged like milk.
        /// However! I don't think it's in use anymore. I would delete it but
        /// if there is any chance of breaking something I don't wanna do it
        /// right before a code review.
        /// </summary>
        public float VelocityMagnitude = 20f;

        public abstract void Shoot(Unit target);

        /// <summary>
        /// Oh god this is awful. Just, like, don't look and pretend it's not here.
        /// Well, I mean, I guess it's not too bad. For some reason I don't like it. Just seems weird to me.
        /// It's for keeping track of which users do not need to be updated on the unit's status if nothing has changed.
        /// As in, they already know it's got X health and is traveling at Y velocity. If the status changes, they must be updated anyway.
        /// Hopefully keeps network traffic down.
        /// </summary>
        public string[] LastFrameVisibleUsers { get; set; } = new string[0];

        /// <summary>
        /// Whee!
        /// </summary>
        /// <param name="currentMilliseconds"></param>
        /// <param name="elapsedTime"></param>
        public virtual void Update(ulong currentMilliseconds, float elapsedTime)
        {
            VisibilityChecker.Update();
            CombatTargetTracker.Update();
            if (CombatSettings.CanShoot && CombatTargetTracker.Target != null)
            {
                if (currentMilliseconds >= LastShotTime + CombatSettings.ShotCooldown.TotalMilliseconds)
                {
                    LastShotTime = currentMilliseconds;
                    Shoot(CombatTargetTracker.Target);
                }
            }

            // If we have no orders, idle.
            if (Orders.Count == 0)
            {
                if (!IsIdling)
                {
                    // Oh lmao this is hacky, I forgot about this.
                    // It tells us our last velocity was infinite so that when we check to see if velocity has changed we say yes, yes it has, now please tell the clients that.
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

            // Exists only for patrolling, where the move actions get moved to the back of the queue on completion instead of just disappearing.
            if (keepGoing == ContOrStop.Requeue)
            {
                Orders.AddLast(Orders.First.Value);
            }

            // Tell the client.
            Owner.CurrentGame.QueueActionOver(this, Orders.First.Value.ID);

            Orders.RemoveFirst();
        }

        /// <summary>
        /// Just, don't worry about it.
        /// </summary>
        public ChangingData<Vector2> PositionUpdate { get; set; }
        public bool UpdatePosition { get; set; } = false;

        public ChangingData<float> HealthUpdate { get; set; }
        public bool UpdateHealth { get; set; } = false;

        public ChangingData<float> RotationUpdate { get; set; }
        public bool UpdateRotation { get; set; } = false;

        public bool IsDead = false;

        public IVisibilityChecker VisibilityChecker;
        public ICombatTargetTracker CombatTargetTracker;

        /// <summary>
        /// There are many occassions where we want to be able to know a unit's owner.
        /// Like, all the time.
        /// </summary>
        public User Owner { get; set; }

        public Unit(Guid id, User owner)
        {
            ID = id;
            Orders = new LinkedList<IUnitOrder>();
            this.Owner = owner;
            IdleOrder = new IdleOrder(this);

            // Bad ugly *bonk*
            VisibilityChecker = new EuclideanDistanceLineOfSightChecker(this);
            CombatTargetTracker = new StandardCombatTargetTracker(this, VisibilityChecker, Owner.CurrentGame.unitPositionLookup);
        }

        /// <summary>
        /// For boids.
        /// </summary>
        public BoidsSettings BoidsSettings = new BoidsSettings(cohesionMaxMag:          30f,
                                                               cohesionStrength:        0.5f,
                                                               cohesionDistance:        20f,
                                                               separationStrength:      10f,
                                                               separationDistance:      15f,
                                                               boidsStrength:           20f,
                                                               flowfieldStrength:       80f,
                                                               maximumBoidsVelocity:    70f);

        public CombatSettings CombatSettings = new CombatSettings(canShoot:             true,
                                                                  shotCooldown:         TimeSpan.FromSeconds(3),
                                                                  visionDistance:       60);

        public ulong LastShotTime { get; private set; }
    }
}
