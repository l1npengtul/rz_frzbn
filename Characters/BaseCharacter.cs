using Godot;
using System;
using System.Text;
using rz_frzbn.Singletons.utils;
using rz_frzbn.Singletons.InvItems.Items;

namespace rz_frzbn.Characters.BaseCharacter{
    
    public enum EntityType{
        Player,
        EnemyMage,
        EnemyMelee,
        Passive,
        NPC,
        UNUSED,
    }


    public class BaseCharacter : KinematicBody2D{
        [Export]
        protected EntityType entityType = EntityType.UNUSED;

        // Nodes
        protected RayCast2D interactCast;
        protected AnimationPlayer animationPlayer;
        protected Tween physicsTween;
        protected Position2D spawnPos;

        // Hotbar
        protected HotbarItems currentItem = HotbarItems.NONE;
        
        // Movement Related
        protected Vector2 movementVector = new Vector2(0.0F,0.0F);
        protected const float frictionMultiplier = 7000.0F;
        protected const float accelerationMultiplier = 7000.0F;
        protected const float rollDuration = 0.25F;
        protected float walkSpeedMultiplier = 1.0F;
        protected float runSpeedMultiplier = 1.6F;
        protected float xSlopeModifier = 0.0F;
        protected float ySlopeModifier = 0.0F;
        protected const int baseSpeed = 600;
        protected const int maxSpeed = 700;
        protected const int rollSpeed = 650;
        protected int currentSpeed = 600;
        protected int slopeDir = -1;
        protected Vector2 slopeVec = new Vector2(0.0F,0.0F);

        protected bool aimMode = false;
        
        // FSM State Modifiers
        protected bool onSlope = false;
        protected bool onBoard = false;


        // HP and Fighting
        [Export]
        protected float healthPoints = 100.0F;
        [Export]
        protected float manaPoints = 100.0F;
        [Export]
        protected float baseMeleeDamage = 10.0F;
        [Export] 
        protected float strength = 1.0F;

        protected StringBuilder newAnim = new StringBuilder("", 50);


       // protected 

        public enum AttackType{
			MAGE_TRIBOLT,
			MAGE_SHIELD,
			RANGED_CROSSBOW,
			MELEE_KICK,
			MELEE_PUNCH,
		}

        // FSM
        protected enum STATES {
            IDLE,
            IDLE_LONG,
            MOVE,
            JUMP,
			BOARD,
			ROLL,
            STAGGER,
            ATTACK_MAGE,
            ATTACK_SHIELD,
            ATTACK_RANGED,
            ATTACK_MELEE,
            //TALK,
            //TALK_SHOP,
            DYING,
            DEAD,
        }
	    protected STATES currentState = STATES.IDLE;

        protected void changeState(STATES toState){
            GD.Print(currentState.ToString());
			// Check for any current states
			switch (currentState){
				case STATES.DEAD:
					QueueFree();
					break;
				// Note: This is done so that melee has to finish and you cant move while attacking melee.
				case STATES.ATTACK_MELEE:
					SetPhysicsProcess(true);
					break;
				case STATES.ATTACK_MAGE:
					SetPhysicsProcess(true);
					break;
                case STATES.ATTACK_SHIELD:
					SetPhysicsProcess(true);
					break;
				case STATES.ROLL:
					SetPhysicsProcess(true);
					break;
				case STATES.MOVE:
					onBoard = false;
					break;
			}

			// Get the new state
			switch (toState){
				case STATES.IDLE:
					//aniPlayer.Play("IDLE");
                    SetPhysicsProcess(true);
					break;
				case STATES.IDLE_LONG:
					// TODO: Make More Idle Long Anims if possible
					//aniPlayer.Play("IDLE_LONG");
                    SetPhysicsProcess(true);
					break;
				case STATES.MOVE:
					//aniPlayer.Play("RUN");
					break;
				/* TODO: Implement Jump 
				case STATES.JUMP:
					aniPlayer.Play("RUN");
					break;
					*/
				case STATES.ROLL:
					SetPhysicsProcess(false);
					//aniPlayer.Play("ROLL");
					break;
				case STATES.JUMP:
					// TODO: Implement jump (Y movement UP, reduced air control by set margain)
					//aniPlayer.Play("IDLE");
					break;
				case STATES.STAGGER:
					// TODO: Only when hit 
					// Play knockback anim
					// Give IFrames
					//aniPlayer.Play("IDLE");
					break;
				case STATES.ATTACK_MAGE:
					// So here is the thing:
					// Emilia's sprite is broken up into many parts, allowing us to "blend" animations
					// Emilia will play the attack mage
					//aniPlayer.Play("IDLE");
					SetPhysicsProcess(false);
					this.attack(AttackType.MAGE_TRIBOLT);
					break;
                case STATES.ATTACK_SHIELD:
					SetPhysicsProcess(false);
					this.attack(AttackType.MAGE_SHIELD);
					break;
				case STATES.BOARD:
					onBoard = true;
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
        public enum ANGLES {
            NORTH,
            NORTHEAST,
            EAST,
            SOUTHEAST,
            SOUTH,
            SOUTHWEST,
            WEST,
            NORTHWEST,
        }
	    protected ANGLES currentAngle = ANGLES.NORTH;

        public void rotatePlayer(float radians, RayCast2D castRotater){
		//GD.Print(radians);
            float degrees = (float) System.Math.Round(Godot.Mathf.Rad2Deg(radians), 1);
            //GD.Print(degrees);
            ANGLES toAngle = ANGLES.NORTH;
            
            // Godot has a dumb angle system, where North is at -180, south at 0, east at -90, and west at 90 or -270
            // YandereDev approved!
            if ((-292.5 <= degrees) && (degrees < -247.5)){
                toAngle = ANGLES.WEST;
            }
            else if ((degrees < -202.5)){
                toAngle = ANGLES.NORTHWEST;
            }
            else if((degrees < -157.5)){
                toAngle = ANGLES.NORTH;
            }
            else if ( (degrees < -112.5)){
                toAngle = ANGLES.NORTHEAST;
            }
            else if ( (degrees < -67.5)){
                toAngle = ANGLES.EAST;
            }
            else if ( (degrees < -22.5)){
                toAngle = ANGLES.SOUTHEAST;
            }
            else if ((degrees < 22.5)){
                toAngle = ANGLES.SOUTH;
            }
            else if ( (degrees < 67.5)){
                toAngle = ANGLES.SOUTHWEST;
            }
            // Note that 90 can also be -270, so we also have to check for that
            else if ((degrees < 112.5)){
                toAngle = ANGLES.WEST;
            }
            
            // We want only the Interact Cast (Ray Cast2D) to rotate according to the true rotation
            // Only for AimMode, which is PLAYER ONLY!!!! - Out of date
            if(castRotater != null){
                if (aimMode){
                    Vector2 mousePos = GetGlobalMousePosition();
			        Vector2 globalPos = this.GlobalPosition;
                    this.Rotation = Godot.Mathf.Atan2(mousePos.y - globalPos.y, mousePos.x - globalPos.x) + Godot.Mathf.Deg2Rad(-90.0F);
                }
                else{
                    castRotater.RotationDegrees = ((float) toAngle) * 45.0F + 180.0F;
                }
            }
		    currentAngle = toAngle;
	    }


        public void enterSlope(float xs, float ys, int slopedir){
            //TODO: acutally use the function params
            // NOTE: direction and slopeType is unused for now
            GD.Print("slope enter");
            xSlopeModifier = xs;
            ySlopeModifier = ys;
            slopeVec.x = xs;
            slopeVec.y = ys;
            onSlope = true;
        }
        public void exitSlope(){
            GD.Print("Slope exit");
            xSlopeModifier = 0.0F;
            ySlopeModifier = 0.0F;
            slopeVec.x = xSlopeModifier;
            slopeVec.y = ySlopeModifier;
            slopeDir = -1;
            onSlope = false;
        }

        public void roll(){
            
        }
        protected void playAnimation(string animationName, ANGLES Angles){
            newAnim.Insert(0, animationName);
            
            switch(Angles){
                case ANGLES.NORTH:
                    newAnim.Append("_NORTH");
                    break;
                case ANGLES.NORTHEAST:
                    newAnim.Append("_NORTHEAST");
                    break;
                case ANGLES.EAST:
                    newAnim.Append("_EAST");
                    break;
                case ANGLES.SOUTHEAST:
                    newAnim.Append("_SOUTHEAST");
                    break;
                case ANGLES.SOUTH:
                    newAnim.Append("_SOUTH");
                    break;
                case ANGLES.SOUTHWEST:
                    newAnim.Append("_SOUTHWEST");
                    break;
                case ANGLES.WEST:
                    newAnim.Append("_WEST");
                    break;
                case ANGLES.NORTHWEST:
                    newAnim.Append("_NORTHWEST");
                    break;
            }
            animationPlayer.Play(newAnim.ToString());
        }
        
        public void takeDamage(float damage){
            // TODO: Take into account damage vulnarabilities
            this.healthPoints += damage * -1;
            if (this.healthPoints <= 0.0F){
                changeState(STATES.DYING);
            }
        }

        virtual protected void attack(AttackType? attackType){
            // Do Nothing! Let each class that inherits `override` and define their own behaviour!
        }
        public void takeKnockback(float angle, float mag){

        }

        public void takeKnockback(float mag){
            float angleToKB = this.movementVector.Angle() + Mathf.Deg2Rad(180);
            
        }

        public void stun(float duration){
            
        }

        public void assignToGroup(EntityType ent){
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
            interactCast = GetNode<RayCast2D>("InteractCast");
            animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
            physicsTween = GetNode<Tween>("PhysicsTween");
            spawnPos = GetNode<Position2D>("InteractCast/Spawn");
        }

        public virtual void UpdateItemHeld(HotbarItems to){
            if (this.currentItem == to){
                this.currentItem = HotbarItems.NONE;
            }
            else{
                this.currentItem = to;
            }
        }
    }
}