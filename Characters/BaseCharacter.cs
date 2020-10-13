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
using rz_frzbn.Environment.Mountains.Slope;



// With the release of "Complete Abandonment Announcement", my life can finally be put as a list of Nanawo Akari songs
// Discomminication Alien -> Dadadada Tenshi -> Fairy Tale Hell Asteroid -> I WANT TO BE HAPPY -> I'm not scared -> Reset Set -> Complete Abandonment Announcement 

// Note that the `Player` referred to in this script does not refer to the player, but rather the Node its controlling.

namespace rz_frzbn.Characters.basecharacter{
    

    public class BaseCharacter : KinematicBody2D{
        [Export]
        protected EntityType CurrentEntityType = EntityType.UNUSED;

        // Nodes
        protected RayCast2D InteractCast;
        protected AnimationPlayer AnimationPlayer;
        protected Tween PhysicsTween;
        protected Position2D SpawnPos;
        protected Timer timer;
        protected Timer AttackCooldownTimer;
        protected Timer DamageTimer;

        // Projectiles - Load
        protected PackedScene IceBolt = ResourceLoader.Load<PackedScene>("res://Weapons/Mage/IceBolt/IceBolt.tscn");
        protected PackedScene Sheild = ResourceLoader.Load<PackedScene>("res://Weapons/Mage/Sheild/Sheild.tscn");
        protected PackedScene Arrow = ResourceLoader.Load<PackedScene>("res://Weapons/Arch/Arrow/Arrow.tscn");

        // Hotbar
        protected virtual HotbarItems CurrentItem {get;set;}
        
        // Movement Related
        protected Vector2 MovementVector = new Vector2(0.0F,0.0F);
        protected Vector2 InputVector = new Vector2(0.0F,0.0F);
        protected const float FrictionMultiplier = 7000.0F;
        protected const float AccelerationMultiplier = 7000.0F;
        protected const float RollDuration = 0.4F;
        protected float WalkSpeedMultiplier = 1.0F;
        protected float RunSpeedMultiplier = 1.6F;
        protected float SlopeModifierX = 0.0F;
        protected float SlopeModifierY = 0.0F;
        protected int SlopeDir = -1;
        protected SlopeType SlopeDirType = SlopeType.NORTH;
        protected Vector2 SlopeVec = new Vector2(0.0F,0.0F);
        protected const int BaseSpeed = 600;
        protected const int MaxSpeed = 700;
        protected const int MaxSpeedOnSlope = 900;
        protected const int MaxSpeedOnSlopeWhileRunning = 1100;
        protected const int MaxSpeedOnBoard = 1400;
        protected const int RollSpeed = 950;
        protected const int KnockBackMultiplier = 1000;
        protected const float KnockBackDuration = 0.1F;
        protected bool IsBeingDamaged = false;
        protected float AttackCooldown = 0.15F;
        protected float DamageCooldown = 0.15F;
        protected int CurrentSpeed = 600;

        protected float PreviousAngleRadians = 0;
		protected Angles PreviousAngleAngle = Angles.NORTH;

        protected States[] ValidMoveStates = {States.IDLE, States.IDLE_LONG, States.MOVE};
        protected States[] ValidJumpStates = {States.IDLE, States.IDLE_LONG, States.MOVE, States.JUMP};
        

        protected bool AimMode = false;
        
        // FSM State Modifiers
        protected bool OnSlope = false;
        protected bool OnBoard = false;
        protected bool CanMove = true;
        

        // HP and Fighting
        
        protected virtual float HealthPoints {
            get; set;
        }
        
        protected virtual float ManaPoints {
            get; set;
        }
        [Export]
        protected int ArrowAmount = 20;
        [Export]
        protected int SecondsUntilRegenStart = 5;
        [Export]
        protected int HealPerSecond = 2;
        [Export]
        protected int ManaPerSecond = 10; 
        protected virtual float MaxHealthPoints {
            get; set;
        }
        protected virtual float MaxManaPoints {
            get; set;
        }
        [Export]
        protected float BaseMeleeDamage = 10.0F;
        [Export] 
        protected float Strength = 1.0F;
        protected long FSinceLastAtk = 0;
        protected long FSinceLastHit = 0;

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
				// Note: This is done so that melee has to finish and you cant move while attacking melee.
				case States.ATTACK_MELEE:
                    CanMove = true;
					break;
				case States.ATTACK_MAGE:
                    CanMove = true;
					break;
                case States.ATTACK_RANGED:
                    CanMove = true;
                    break;
                case States.ATTACK_SHEILD:
                    CanMove = true;
                    break;
				case States.ROLL:
                    CanMove = true;
					break;
				case States.MOVE:
					OnBoard = false;
					break;
                case States.STAGGER:
                    CanMove = true;
                    this.IsBeingDamaged = false;
                    break;
                case States.DYING:
                    QueueFree();
                    break;
			}

			// Get the new state
			switch (toState){
				case States.IDLE:
					//aniPlayer.Play("IDLE");
                    SetPhysicsProcess(true);
                    CanMove = true;
					break;
				case States.IDLE_LONG:
					// TODO: Make More Idle Long Anims if possible
					//aniPlayer.Play("IDLE_LONG");
                    SetPhysicsProcess(true);
                    CanMove = true;
					break;
				case States.MOVE:
					//aniPlayer.Play("RUN");
					break;
				case States.ROLL:
					SetPhysicsProcess(false);
					//aniPlayer.Play("ROLL");
					break;
				case States.STAGGER: // State for STUN/KNOCKBACK
					this.IsBeingDamaged = true;
                    CanMove = false;
                    AnimationPlayer.Play("STAGGER");
					break;
				case States.ATTACK_MAGE:
					// So here is the thing:
					// Emilia's sprite is broken up into many parts, allowing us to "blend" animations
					// Emilia will play the attack mage
					//aniPlayer.Play("IDLE");
					CanMove = false;
					this.Attack(AttackType.MAGE_ICEBOLT);
					break;
                case States.ATTACK_MELEE:
					CanMove = false;
					this.Attack(AttackType.MAGE_SHIELD);
					break;
                case States.ATTACK_RANGED:
					CanMove = false;
					this.Attack(AttackType.MAGE_SHIELD);
					break;
                case States.ATTACK_SHEILD:
                    CanMove = false;
                    this.Attack(AttackType.MAGE_SHIELD);
                    break;
				case States.BOARD:
					OnBoard = true;
					GD.Print("slope true");
					break;
				case States.DYING:
                    //AnimationPlayer.Play("DIE");
                    AnimationPlayer.Play("IDLE");
                    break;
				default:
					//aniPlayer.Play("IDLE");
					break;
			}
			currentState = toState;
			//GD.Print(currentState);
		}
        
        // Angle
	    protected Angles CurrentAngle = Angles.NORTH;

        public virtual void RotatePlayer(float radians, RayCast2D castRotater){
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


        public void EnterSlope(float xs, float ys, SlopeType dir){
            GD.Print("slope enter");
            SlopeModifierX = xs;
            SlopeModifierY = ys;
            SlopeVec.x = xs;
            SlopeVec.y = ys;
            SlopeDirType = dir;
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
            if (!this.IsBeingDamaged){
                this.HealthPoints += damage * -1;
                if (this.HealthPoints <= 0.0F){
                    ChangeState(States.DYING);
                }
                EmitSignal("HPChangedSignal", this.HealthPoints);
            }
            this.FSinceLastHit = 0;
        }
        public void TakeDamageWithKB(float damage, Vector2 damageLocation){
            // TODO: Take into account damage vulnarabilities
            if (!this.IsBeingDamaged){
                this.HealthPoints += damage * -1;
                if (this.HealthPoints <= 0.0F){
                    ChangeState(States.DYING);
                }
                else {
                    ChangeState(States.STAGGER);
                    float HitAngle = this.GlobalPosition.AngleTo(damageLocation);
                    this.TakeKnockback(damage*64.0F, HitAngle);
                }
                EmitSignal("HPChangedSignal", this.HealthPoints);
            }
            this.FSinceLastHit = 0;
        }
        public void TakeDamageWithStun(float damage, Vector2 damageLocation){
            // TODO: Take into account damage vulnarabilities
            if (!this.IsBeingDamaged){
                this.HealthPoints += damage * -1;
                if (this.HealthPoints <= 0.0F){
                    ChangeState(States.DYING);
                }
                else {
                    ChangeState(States.STAGGER);
                    this.Stun(damage*64.0F);
                }
                EmitSignal("HPChangedSignal", this.HealthPoints);
            }
            this.FSinceLastHit = 0;
        }

        public void HealDamage(float heal){
            this.HealthPoints += Mathf.Abs(heal);
            if (this.HealthPoints > this.MaxHealthPoints){
                this.HealthPoints = this.MaxHealthPoints;
            }
            EmitSignal("HPChangedSignal", this.HealthPoints);
        }

        protected virtual float CalculateHeals(long FramesSinceDamage, float delta){
            //GD.Print((FramesSinceDamage/(1/delta)));
            if ((FramesSinceDamage/(1/delta)) > 5){
                float Calced = HealPerSecond * Mathf.Log(FramesSinceDamage/(0.1F/delta)) / (MaxHealthPoints*2);
                if (Calced < 0){
                    Calced = 0.0F;
                }
                else if (Calced > HealPerSecond + 0.5F){
                    Calced = HealPerSecond + 0.5F;
                }
                return Calced;
            }
            return 0.0F;
        }

        protected virtual float CalculateManaRegen(long FramesSinceLastAttack, float delta){
            if ((FramesSinceLastAttack/(1/delta)) > 5){
                float Calced = ManaPerSecond * Mathf.Log(FramesSinceLastAttack/(0.1F/delta)) / (MaxManaPoints*2);
                if (Calced < 0){
                    Calced = 0.0F;
                }
                else if (Calced > ManaPerSecond + 0.5F){
                    Calced = ManaPerSecond + 0.5F;
                }
                return Calced;
            }
            return 0.0F;
        }

        protected virtual void DoRegeneration(float delta){
            HealDamage(CalculateHeals(FSinceLastHit, delta));
            RegenMana(CalculateManaRegen(FSinceLastAtk, delta));
        }

        public bool UseMana(float mana){
            if (this.ManaPoints - mana < 0.0F){
                EmitSignal(nameof(MPChangedSignal), this.ManaPoints);
                return false;
            }
            else{
                this.ManaPoints -= mana;
                EmitSignal(nameof(MPChangedSignal), this.ManaPoints);
                return true;
            }
        }

        public bool UseArrow(int arrows){
            if ((ArrowAmount - arrows) >= 0){
                ArrowAmount -= arrows;
                return true;
            }
            return false;
        }
        
        public void RegenMana(float mana){
            this.ManaPoints += mana;
            if (this.ManaPoints > MaxManaPoints){
                this.ManaPoints = MaxManaPoints;
            }
            EmitSignal(nameof(MPChangedSignal), this.ManaPoints);
        }

        protected virtual void Attack(AttackType? attackType){
            // Do Nothing! Let each class that inherits `override` and define their own behaviour!
        }

        // hypixel players be like: "what if i didnt?"
        protected void TakeKnockback(float mag, float angle){
            float angleToKB = angle + Mathf.Deg2Rad(180) + Godot.Mathf.Deg2Rad(-90.0F);
            GD.Print(angleToKB);
            Vector2 kbVector = new Vector2(Mathf.Sin(angleToKB), Mathf.Cos(angleToKB));
            kbVector.x *= mag;
            kbVector.y *= mag; 
            GD.Print(kbVector);
            PhysicsTween.InterpolateMethod(this, "move_and_slide", kbVector, kbVector, KnockBackDuration, Tween.TransitionType.Bounce, Tween.EaseType.OutIn);
            if (!PhysicsTween.IsActive()){
                PhysicsTween.Start();
            }
            ChangeState(States.IDLE);
        }

        protected void Stun(float duration){
            // this.AnimationPlayer.Play("STUN"); FIXME: Implement `STUN` animation
            
        }

        public void AssignToGroup(EntityType ent){
            switch(ent){
                case EntityType.EnemyMage:
                    this.AddToGroup("Enemy");
                    this.AddToGroup("Persist");
                    break;
                case EntityType.EnemyMelee:
                    this.AddToGroup("Enemy");
                    this.AddToGroup("Persist");
                    break;
                case EntityType.Passive:
                    this.AddToGroup("Passive");
                    this.AddToGroup("Persist");
                    break;
                case EntityType.Player:
                    this.AddToGroup("Player");
                    this.AddToGroup("Persist");
                    break;
                case EntityType.NPC:
                    this.AddToGroup("NPC");
                    this.AddToGroup("Persist");
                    break;
                default:
                    throw new InvalidOperationException("EntityType is not defined");
            }
        }
        public virtual void SetupNodes(){
            InteractCast = GetNode<RayCast2D>("InteractCast");
            AnimationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
            PhysicsTween = GetNode<Tween>("PhysicsTween");
            SpawnPos = GetNode<Position2D>("InteractCast/Spawn");
            AttackCooldownTimer = GetNode<Timer>("Timers/AttackCooldownTimer");
            timer = GetNode<Timer>("Timers/Timer");
            DamageTimer = GetNode<Timer>("Timers/HurtTimer");
        }

        protected virtual void UpdateItemHeld(HotbarItems to){
            if (this.CurrentItem == to){
                this.CurrentItem = HotbarItems.NONE;
            }
            else{
                this.CurrentItem = to;
            }
            EmitSignal(nameof(WeaponChangedSignal), this.CurrentItem);
        }

        // This function expects to be called every frame.
        public virtual void MoveActorWithInput(Vector2 input, float delta){
            if (InputVector != Vector2.Zero){
				MovementVector = MovementVector.MoveToward(InputVector * CurrentSpeed, AccelerationMultiplier * delta);
			}
			else{
				MovementVector = MovementVector.MoveToward(Vector2.Zero, FrictionMultiplier * delta);
			}
			
			if(InputVector == Vector2.Zero){
				ChangeState(States.IDLE);
			}

			if(!AimMode){
				if(!(InputVector == Vector2.Zero)){
					RotatePlayer(InputVector.Angle() + Godot.Mathf.Deg2Rad(-90.0F), InteractCast);
				}
			}
			else{
				Vector2 mousePos = GetGlobalMousePosition();
				Vector2 globalPos = this.GlobalPosition;
				RotatePlayer(Godot.Mathf.Atan2(mousePos.y - globalPos.y, mousePos.x - globalPos.x) + Godot.Mathf.Deg2Rad(-90.0F), InteractCast);
			}
			//GD.Print(movementVector);
			MovementVector.Clamped(MaxSpeed);
			MovementVector = MoveAndSlide(MovementVector);
        }

        public virtual void MoveActorOnSlope(Vector2 ToVec, float delta, bool isRunning = false){
            if (InputVector != Vector2.Zero){
				MovementVector = MovementVector.MoveToward(InputVector * MaxSpeed, AccelerationMultiplier * delta);
			}
			else{
				MovementVector = MovementVector.MoveToward(Vector2.Zero, FrictionMultiplier * delta);
			}
            if(InputVector != Vector2.Zero){
                RotatePlayer(InputVector.Normalized().Angle() + Godot.Mathf.Deg2Rad(-90.0F), InteractCast);
            }
			
			if (isRunning){
				MovementVector.Clamped(MaxSpeedOnSlope);
			}
			else {
				MovementVector.Clamped(MaxSpeedOnSlopeWhileRunning);
			}
			MoveAndSlide(MovementVector);
        }

        public virtual void RollActor(Vector2 MoveDir){
			float xDir = MoveDir.x;
			float yDir = MoveDir.y;
			

			Vector2 initialVector = new Vector2(xDir * 50,yDir * 50);
			MovementVector = Vector2.Zero;
			PhysicsTween.InterpolateMethod(this, nameof(MoveAndSlide), initialVector, initialVector, RollDuration, Tween.TransitionType.Bounce, Tween.EaseType.OutIn);
			if (!PhysicsTween.IsActive()){
				//GD.Print("start");
				PhysicsTween.Start();
			}
			//MoveAndSlide(initialVector);
			ChangeState(States.IDLE);
			//GD.Print(currentState);
		}

        public void _on_HurtTimer_timeout(){

        }

        public void _on_AttackCooldownTimer_timeout(){

        }


		public virtual void _on_AnimationPlayer_animation_finished(string AnimName){
			GD.Print("here");
			/*
            ChangeState(States.IDLE);
			if (AnimName == "ATTACK"){
				this.Rotation = PreviousAngleRadians;
				CurrentAngle = PreviousAngleAngle;
				InteractCast.RotationDegrees = ((float) CurrentAngle) * 45.0F + 180.0F;
			}
            */
            ChangeState(States.IDLE);
            switch(AnimName){
                case "ATTACK":
                    this.Rotation = PreviousAngleRadians;
                    CurrentAngle = PreviousAngleAngle;
                    InteractCast.RotationDegrees = ((float) CurrentAngle) * 45.0F + 180.0F;
                    ChangeState(States.IDLE);
                    break;
                case "STAGGER":
                    ChangeState(States.IDLE);
                    break;
            }
		}

        public void SavePlayerState(){
            
        }

        public void LoadPlayerState(){
            
        }

    }
}