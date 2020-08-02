using Godot;
using System;

namespace rz_frzbn.Singletons.Math.Vector.VectorMathF{
    public class VectorMathF : Godot.Node{
        public static Vector2 fromAngleRadians(float angle){
            return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        }
    }
}