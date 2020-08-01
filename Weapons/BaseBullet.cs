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
        protected int BulletDamage;
        protected BulletType bulletType;
        protected int BulletVelocity;
        protected int BulletHealth;
        protected float setRot;
    }
}