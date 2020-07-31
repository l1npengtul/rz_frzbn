using Godot;
using System;

namespace rz_frzbn.Singletons.InvItems.Items.Weapons.Mage.IceBolt{
    public class IceBolt : rz_frzbn.Singletons.InvItems.Items.Weapons.BaseWeaponClass{
        public override void _Ready(){
            this.itemDescriptionID = "";

            this.attackTimer = GetNode<Timer>("Timer");
            this.sprite = GetNode<AnimatedSprite>("AnimatedSprite");
            this.weaponTween = GetNode<Tween>("Tween");
            this.pos2d = GetNode<Position2D>("Position2D");

        }

        new public void attack(){
            if (canAttack){
                sprite.Play("ATTACK"); // TODO: Implement Attack Anim
                float globalrot = this.GlobalRotation;
                
            }
        }
    }
}