using Godot;
using System;
using System.Text;
using rz_frzbn.Singletons.utils;
using rz_frzbn.Weapons.basebullet;
using rz_frzbn.Weapons.Mage.shelid;
using rz_frzbn.Singletons.InvItems.Items;
using rz_frzbn.Singletons.Character.attacktype;
using rz_frzbn.Singletons.Character.states;
using rz_frzbn.Singletons.Character.entitytype;
using rz_frzbn.Singletons.Character.Angles;



// With the release of "Complete Abandonment Announcement", my life can finally be put as a list of Nanawo Akari songs
// Discomminication Alien -> Dadadada Tenshi -> Fairy Tale Hell Asteroid -> I WANT TO BE HAPPY -> I'm not scared -> Reset Set -> Complete Abandonment Announcement 

namespace rz_frzbn.Characters.basecharacter{
    

    public class BaseCharacter : KinematicBody2D{
        [Export]
        protected EntityType CurrentEntityType = EntityType.UNUSED;

        // Nodes
        protected RayCast2D InteractCast;
        protected AnimationPlayer AnimationPlayer;
        protected Tween PhysicsTween;
        protected Position2D SpawnPos;

        // Projectiles - Load
        protected PackedScene IceBolt = ResourceLoader.Load<PackedScene>("res://Weapons/Mage/IceBolt/IceBolt.tscn");
        protected PackedScene Sheild = ResourceLoader.Load<PackedScene>("res://Weapons/Mage/Sheild/Sheild.tscn");
        protected PackedScene Arrow = ResourceLoader.Load<PackedScene>("res://Weapons/Arch/Arrow/Arrow.tscn");

        // Hotbar
        protected HotbarItems CurrentItem = HotbarItems.NONE;
        
        // Movement Related
        protected Vector2 MovementVector = new Vector2(0.0F,0.0F);
        protected const float FrictionMultiplier = 7000.0F;
        protected const float AccelerationMultiplier = 7000.0F;
        protected const float RollDuration = 0.25F;
        protected float WalkSpeedMultiplier = 1.0F;
        protected float RunSpeedMultiplier = 1.6F;
        protected float SlopeModifierX = 0.0F;
        protected float SlopeModifierY = 0.0F;
        protected const int BaseSpeed = 600;
        protected const int MaxSpeed = 700;
        protected const int MaxSpeedOnSlope = 900;
        protected const int MaxSpeedOnSlopeWhileRunning = 1100;
        protected const int MaxSpeedOnBoard = 1400;
        protected const int RollSpeed = 650;
        protected int CurrentSpeed = 600;
        protected int SlopeDir = -1;
        protected Vector2 SlopeVec = new Vector2(0.0F,0.0F);

        protected bool AimMode = false;
        
        // FSM State Modifiers
        protected bool OnSlope = false;
        protected bool OnBoard = false;


        // HP and Fighting
        [Export]
        protected float HealthPoints = 100.0F;
        [Export]
        protected float ManaPoints = 100.0F;
        [Export]
        protected float MaxHealthPoints = 100.0F;
        [Export]
        protected float MaxManaPoints = 100.0F;
        [Export]
        protected float BaseMeleeDamage = 10.0F;
        [Export] 
        protected float Strength = 1.0F;

        // Stringbuilder to avoid new allocations when concatenating strings for animations
        protected StringBuilder NewAnim = new StringBuilder("", 50);

        // Signals
        [Signal]
        public delegate void HPChangedSignal(float NewHP); // Signal for HP Gain/Loss
        [Signal]
        public delegate void MPChangedSignal(float NewMP); // Signal for Mana Gain/Loss
        [Signal]
        public delegate void AmmoChangedSignal(float NewAmmoAmt); // Signal for Crossbow Ammo Gain/Loss
        [Signal]
        public delegate void StateChangedSignal(States NewState); // Signal for FSM State Change
        [Signal]
        public delegate void WeaponChangedSignal(HotbarItems NewItem); // Weapon Change Signal

        // FSM
	    protected States currentState = States.IDLE;


        protected void ChangeState(States toState){
            //GD.Print(currentState.ToString());
			// Check for any current states
			switch (currentState){
				case States.DEAD:
					QueueFree();
					break;
				// Note: This is done so that melee has to finish and you cant move while attacking melee.
				case States.ATTACK_MELEE:
					SetPhysicsProcess(true);
					break;
				case States.ATTACK_MAGE:
					SetPhysicsProcess(true);
					break;
                case States.ATTACK_SHIELD:
					SetPhysicsProcess(true);
					break;
				case States.ROLL:
					SetPhysicsProcess(true);
					break;
				case States.MOVE:
					OnBoard = false;
					break;
			}

			// Get the new state
			switch (toState){
				case States.IDLE:
					//aniPlayer.Play("IDLE");
                    SetPhysicsProcess(true);
					break;
				case States.IDLE_LONG:
					// TODO: Make More Idle Long Anims if possible
					//aniPlayer.Play("IDLE_LONG");
                    SetPhysicsProcess(true);
					break;
				case States.MOVE:
					//aniPlayer.Play("RUN");
					break;
				/* TODO: Implement Jump 
				case States.JUMP:
					aniPlayer.Play("RUN");
					break;
					*/
				case States.ROLL:
					SetPhysicsProcess(false);
					//aniPlayer.Play("ROLL");
					break;
				case States.JUMP:
					// TODO: Implement jump (Y movement UP, reduced air control by set margain)
					//aniPlayer.Play("IDLE");
					break;
				case States.STAGGER:
					// TODO: Only when hit 
					// Play knockback anim
					// Give IFrames
					//aniPlayer.Play("IDLE");
					break;
				case States.ATTACK_MAGE:
					// So here is the thing:
					// Emilia's sprite is broken up into many parts, allowing us to "blend" animations
					// Emilia will play the attack mage
					//aniPlayer.Play("IDLE");
					SetPhysicsProcess(false);
					this.Attack(AttackType.MAGE_ICEBOLT);
					break;
                case States.ATTACK_SHIELD:
					SetPhysicsProcess(false);
					this.Attack(AttackType.MAGE_SHIELD);
					break;
				case States.BOARD:
					OnBoard = true;
					GD.Print("slope true");
					break;
				// Skipping all other states until movement and roll works properly. 
				default:
					//aniPlayer.Play("IDLE");
					break;
			}
			currentState = toState;
			//GD.Print(currentState);
		}
        
        // Angle
	    protected Angles CurrentAngle = Angles.NORTH;

        public void RotatePlayer(float radians, RayCast2D castRotater){
		//GD.Print(radians);
            float degrees = (float) System.Math.Round(Godot.Mathf.Rad2Deg(radians), 1);
            //GD.Print(degrees);
            Angles toAngle = Angles.NORTH;
            
            // Godot has a dumb angle system, where North is at -180, south at 0, east at -90, and west at 90 or -270
            // YandereDev approved!
            if ((-292.5 <= degrees) && (degrees < -247.5)){
                toAngle = Angles.WEST;
            }
            else if ((degrees < -202.5)){
                toAngle = Angles.NORTHWEST;
            }
            else if((degrees < -157.5)){
                toAngle = Angles.NORTH;
            }
            else if ( (degrees < -112.5)){
                toAngle = Angles.NORTHEAST;
            }
            else if ( (degrees < -67.5)){
                toAngle = Angles.EAST;
            }
            else if ( (degrees < -22.5)){
                toAngle = Angles.SOUTHEAST;
            }
            else if ((degrees < 22.5)){
                toAngle = Angles.SOUTH;
            }
            else if ( (degrees < 67.5)){
                toAngle = Angles.SOUTHWEST;
            }
            // Note that 90 can also be -270, so we also have to check for that
            else if ((degrees < 112.5)){
                toAngle = Angles.WEST;
            }
            
            // We want only the Interact Cast (Ray Cast2D) to rotate according to the true rotation
            // Only for AimMode, which is PLAYER ONLY!!!! - Out of date
            if(castRotater != null){
                if (AimMode){
                    Vector2 mousePos = GetGlobalMousePosition();
			        Vector2 globalPos = this.GlobalPosition;
                    this.Rotation = Godot.Mathf.Atan2(mousePos.y - globalPos.y, mousePos.x - globalPos.x) + Godot.Mathf.Deg2Rad(-90.0F);
                }
                else{
                    castRotater.RotationDegrees = ((float) toAngle) * 45.0F + 180.0F;
                }
            }
		    CurrentAngle = toAngle;
	    }


        public void EnterSlope(float xs, float ys){
            GD.Print("slope enter");
            SlopeModifierX = xs;
            SlopeModifierY = ys;
            SlopeVec.x = xs;
            SlopeVec.y = ys;
            OnSlope = true;
        }
        
        public void ExitSlope(){
            GD.Print("Slope exit");
            SlopeModifierX = 0.0F;
            SlopeModifierY = 0.0F;
            SlopeVec.x = SlopeModifierX;
            SlopeVec.y = SlopeModifierY;
            SlopeDir = -1;
            OnSlope = false;
        }

        public void Roll(){
            
        }
        protected void PlayAnimation(string animationName, Angles Angles){
            NewAnim.Insert(0, animationName);
            
            switch(Angles){
                case Angles.NORTH:
                    NewAnim.Append("_NORTH");
                    break;
                case Angles.NORTHEAST:
                    NewAnim.Append("_NORTHEAST");
                    break;
                case Angles.EAST:
                    NewAnim.Append("_EAST");
                    break;
                case Angles.SOUTHEAST:
                    NewAnim.Append("_SOUTHEAST");
                    break;
                case Angles.SOUTH:
                    NewAnim.Append("_SOUTH");
                    break;
                case Angles.SOUTHWEST:
                    NewAnim.Append("_SOUTHWEST");
                    break;
                case Angles.WEST:
                    NewAnim.Append("_WEST");
                    break;
                case Angles.NORTHWEST:
                    NewAnim.Append("_NORTHWEST");
                    break;
            }
            AnimationPlayer.Play(NewAnim.ToString());
        }
        
        public void TakeDamage(float damage){
            // TODO: Take into account damage vulnarabilities
            this.HealthPoints += damage * -1;
            if (this.HealthPoints <= 0.0F){
                ChangeState(States.DYING);
            }
            EmitSignal("HPChangedSignal", this.HealthPoints);
        }

        public void HealDamage(float heal){
            this.HealthPoints += Mathf.Abs(heal);
            if (this.HealthPoints > this.MaxHealthPoints){
                this.HealthPoints = this.MaxHealthPoints;
            }
            EmitSignal("HPChangedSignal", this.HealthPoints);
        }

        public bool UseMana(float mana){
            if (this.ManaPoints - mana < 0.0F){
                return false;
            }
            else{
                this.ManaPoints -= mana;
                return true;
            }
        }

        public void RegenMana(float mana){
            this.ManaPoints += mana;
            if (this.ManaPoints > MaxManaPoints){
                this.ManaPoints = MaxManaPoints;
            }
        }

        protected virtual void Attack(AttackType? attackType){
            // Do Nothing! Let each class that inherits `override` and define their own behaviour!
        }
        public void TakeKnockback(float angle, float mag){

        }

        public void TakeKnockback(float mag){
            float angleToKB = this.MovementVector.Angle() + Mathf.Deg2Rad(180);
            // Vector2 
        }

        public void Stun(float duration){
            
        }

        public void AssignToGroup(EntityType ent){
            switch(ent){
                case EntityType.EnemyMage:
                    this.AddToGroup("Enemy");
                    break;
                case EntityType.EnemyMelee:
                    this.AddToGroup("Enemy");
                    break;
                case EntityType.Passive:
                    this.AddToGroup("Passive");
                    break;
                case EntityType.Player:
                    this.AddToGroup("Player");
                    break;
                case EntityType.NPC:
                    this.AddToGroup("NPC");
                    break;
                default:
                    throw new Exceptions.IllegalStateException("UNUSED for EntityType");
            }
        }
        public virtual void SetupNodes(){
            InteractCast = GetNode<RayCast2D>("InteractCast");
            AnimationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
            PhysicsTween = GetNode<Tween>("PhysicsTween");
            SpawnPos = GetNode<Position2D>("InteractCast/Spawn");
        }

        public virtual void UpdateItemHeld(HotbarItems to){
            if (this.CurrentItem == to){
                this.CurrentItem = HotbarItems.NONE;
            }
            else{
                this.CurrentItem = to;
            }
        }
    }
}