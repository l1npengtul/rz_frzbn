using Godot;
using System;

namespace rz_frzbn.Singletons.InvItems.Items.Weapons{
    public enum WeaponType{
        NONE,
        MAGE,
        MELEE,
        RANGED,
    }
    public class BaseWeaponClass : rz_frzbn.Singletons.InvItems.Items.BaseItemClass.BaseItemClass{
        [Export]
        protected WeaponType currentWeaponType {get; private set;} = WeaponType.NONE;

        [Export(PropertyHint.Range, "0,1000")]
        protected float weaponDamage {get; private set;} = 1.0F;

        [Export(PropertyHint.Range, "0,1000")]
        protected int weaponUseCost {get; private set;} = 1;

        [Export(PropertyHint.Range, "0,1000")]
        protected float weaponTimer {get; private set;} = 1.0F;

        protected bool canAttack = true;

        protected Timer attackTimer;
        protected Tween weaponTween;
        protected AnimatedSprite sprite;
        protected Position2D pos2d;

        public void _on_Timer_timeout(){
            canAttack = true;
        }

        public void attack(){
            // Do Nothing! This is ment as a placeholder so individual weapons can implement their own behaviour! 
            // This only exists so calls to `weapon.attack()` dont return errors!
        }
    }
}