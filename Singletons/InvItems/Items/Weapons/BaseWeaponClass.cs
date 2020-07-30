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

        public void _on_Timer_timeout(){
            canAttack = true;
        }

        public void attack(){
            if(canAttack){
                switch(currentWeaponType){
                    case WeaponType.MAGE:
                        break;
                    case WeaponType.MELEE:
                        break;
                    case WeaponType.RANGED:
                        break;
                    case WeaponType.NONE:
                        break;
                }
            }
        }
    }
}