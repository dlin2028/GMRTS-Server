using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GMRTSClasses.Units
{
    public abstract class Unit
    {
        public Guid ID { get; }

        public Unit()
        {
            ID = Guid.NewGuid();
        }

        public Unit(Guid id)
        {
            ID = id;
        }

        public Changing<float> Health { get; set; }
        public Changing<Vector2> Position { get; set; }
        public Changing<float> Rotation { get; set; }

        public virtual void Update(ulong currentMilliseconds)
        {
            Health = Health.Update(currentMilliseconds);
            Position = Position.Update(currentMilliseconds);
            Rotation = Rotation.Update(currentMilliseconds);
        }
    }
}
