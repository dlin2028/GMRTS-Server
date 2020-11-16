using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GMRTSClasses
{
    public interface IChanger<T>
    {
        T Scale(float scalar, T right);
        T Add(T left, T right);
    }

    public class Vector2Changer : IChanger<Vector2>
    {
        public Vector2 Scale(float scalar, Vector2 right)
        {
            return Vector2.Multiply(scalar, right);
        }

        public Vector2 Add(Vector2 left, Vector2 right)
        {
            return Vector2.Add(left, right);
        }

        private Vector2Changer()
        {

        }

        private static Vector2Changer vecChanger = null;
        public static Vector2Changer VectorChanger => vecChanger ?? (vecChanger = new Vector2Changer());
    }

    public class FloatChanger : IChanger<float>
    {
        public float Scale(float scalar, float right)
        {
            return scalar * right;
        }

        public float Add(float left, float right)
        {
            return left + right;
        }

        private FloatChanger()
        {

        }

        private static FloatChanger fChanger = null;
        public static FloatChanger FChanger => fChanger ?? (fChanger = new FloatChanger());
    }
}
