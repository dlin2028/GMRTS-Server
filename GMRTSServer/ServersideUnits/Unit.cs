using GMRTSClasses.STCTransferData;

using GMRTSServer.UnitStates;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GMRTSServer.ServersideUnits
{
    internal abstract class Unit
    {
        public Guid ID { get; set; }

        public float Health { get; set; }

        public Vector2 Position { get; set; }

        public float Rotation { get; set; }

        public IUnitState State { get; set; }

        public Game Game { get; set; }

        public string[] LastFrameVisibleUsers { get; set; } = new string[0];

        public virtual void Update(ulong currentMilliseconds, float elapsedTime)
        {
            State = State.Update(currentMilliseconds, elapsedTime);
        }

        public ChangingData<Vector2> PositionUpdate { get; set; }
        public bool UpdatePosition { get; set; } = false;

        public ChangingData<float> HealthUpdate { get; set; }
        public bool UpdateHealth { get; set; } = false;

        public ChangingData<float> RotationUpdate { get; set; }
        public bool UpdateRotation { get; set; } = false;

        public User Owner { get; set; }

        public Unit(Guid id)
        {
            ID = id;
            State = new IdleState();
        }
    }
}
