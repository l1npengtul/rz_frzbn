using Godot;
using System;

namespace rz_frzbn.Weapons.BaseBullet{
    public enum BulletType {
        ICEBOLT,
        ARROW,
        MAGEOTHER,
        // dont bully me if i spelled it wrong
        SHEILD,
    }
    public class BaseBullet : Area2D{
        [Export]
        protected int BulletDamage = 10;
        [Export]
        protected BulletType bulletType = BulletType.MAGEOTHER;
        [Export]
        protected int BulletVelocity = 100;

        protected Vector2 moveTo = Vector2.Zero;

        public override void _Ready(){
            SetProcess(false);
        }

        // The reason that this is here instead of `_Init` is because Godot messes with constructors. 
        public void setBullet(float rot){
            this.Rotation = rot;
            moveTo = rz_frzbn.Singletons.Math.Vector.VectorMathF.VectorMathF.fromAngleRadians(rot);
            SetProcess(true);
        }

        public override void _Process(float delta){
            this.SetPosition(this.GetPosition)
        }
    }
}