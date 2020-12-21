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

        public string OwnerUsername { get; set; }

        public Unit()
        {
            ID = Guid.NewGuid();

            Position = new GMRTSClasses.Changing<System.Numerics.Vector2>(System.Numerics.Vector2.Zero, System.Numerics.Vector2.Zero, Vector2Changer.VectorChanger, 0);
            Rotation = new GMRTSClasses.Changing<float>(0, 0, FloatChanger.FChanger, 0);
            Health = new GMRTSClasses.Changing<float>(100, 0, FloatChanger.FChanger, 0);
        }

        public Unit(Guid id)
        {
            ID = id;

            Position = new GMRTSClasses.Changing<System.Numerics.Vector2>(System.Numerics.Vector2.Zero, System.Numerics.Vector2.Zero, Vector2Changer.VectorChanger, 0);
            Rotation = new GMRTSClasses.Changing<float>(0, 0, FloatChanger.FChanger, 0);
            Health = new GMRTSClasses.Changing<float>(100, 0, FloatChanger.FChanger, 0);
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
