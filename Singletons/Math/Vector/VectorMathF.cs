using Godot;
using System;

namespace rz_frzbn.Singletons.Math.Vector.VectorMathF{
    public class VectorMathF : Godot.Node{
        public static Vector2 fromAngleRadians(float angle){
            return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        }

        public static float CalculateVectorMagnitude(Vector2 vec, bool useRoot = true){
            if (useRoot){
                return Mathf.Sqrt(Mathf.Pow(vec.x,2) + Mathf.Pow(vec.y,2));
            }
            else {
                return Mathf.Pow(vec.x,2) + Mathf.Pow(vec.y,2);

            }
        }
    }
}