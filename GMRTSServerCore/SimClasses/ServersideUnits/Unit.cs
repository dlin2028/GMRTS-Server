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
    internal abstract class Unit
    {
        public Guid ID { get; set; }

        public float Health { get; set; }

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
                return;
            }

            ContOrStop keepGoing = Orders.First.Value.Update(currentMilliseconds, elapsedTime);
            
            if (keepGoing == ContOrStop.Continue)
            {
                return;
            }

            if (keepGoing == ContOrStop.Requeue)
            {
                Orders.AddLast(Orders.First.Value);
            }

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
        }

        public BoidsSettings BoidsSettings = new BoidsSettings(0.5f, 20f, 10f, 30f, 20f, 80f);
    }
}
