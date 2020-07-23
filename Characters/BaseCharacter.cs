using Godot;

namespace Character{
    public class BaseCharacter : KinematicBody2D{

        // Nodes
        protected RayCast2D interactCast;
        protected AnimationPlayer animationPlayer;
        protected Tween physicsTween;
        public override void _Ready(){
            interactCast = GetNode<RayCast2D>("InteractCast");
            animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
            physicsTween = GetNode<Tween>("PhysicsTween");
        }

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

        protected bool aimMode = false;

        // HP and Fighting
        [Export]
        protected float healthPoints = 100.0F;
        [Export]
        protected float manaPoints = 100.0F;
        [Export]
        protected float baseMeleeDamage = 10.0F;
        [Export] 
        protected float strength = 1.0F;

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
            ATTACK_RANGED,
            ATTACK_MELEE,
            //TALK,
            //TALK_SHOP,
            DYING,
            DEAD,
        }
	    protected STATES currentState = STATES.IDLE;

        protected void changeState(STATES toState){
		// Check for any current states
            switch (currentState){
                case STATES.DEAD:
                    QueueFree();
                    break;
                // Note: This is done so that melee has to finish and you cant move while attacking melee.
                case STATES.ATTACK_MELEE:
                    SetPhysicsProcess(true);
                    break;
                case STATES.ROLL:
                    SetPhysicsProcess(true);
                    break;
                
            }
            // Get the new state
            switch (toState){
                case STATES.IDLE:
                    //aniPlayer.Play("IDLE");
                    break;
                case STATES.IDLE_LONG:
                    // TODO: Make More Idle Long Anims if possible
                    //aniPlayer.Play("IDLE_LONG");
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
            // Only for AimMode, which is PLAYER ONLY!!!!
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

        // FSM State Modifiers
        protected bool onSlope = false;
        protected bool onBoard = false;

        public void enterSlope(float xs, float ys, int slopedir){
            //TODO: acutally use the function params
            // NOTE: direction and slopeType is unused for now
            GD.Print("slope enter");
            xSlopeModifier = xs;
            ySlopeModifier = ys;
            /*float setX = 0.0F;
            float setY = 0.0F;
            switch(slopeDir){
                // No Direction
                case -1:
                    onSlope = false;
                    break;
                // North
                case 0:
                    setY = -1.0F;
                    setX = 0.0F;
                    break;
                // East
                case 1:
                    setY = 0.0F;
                    setX = 1.0F;
                    break;
                // South
                case 2:
                    setY = 1.0F;
                    setX = 0.0F;
                    break;
                // West
                case 3:
                    setY = 0.0F;
                    setX = -1.0F;
                    break;
            }
            slopeDir = slopedir;*/
            onSlope = true;
        }
        public void exitSlope(){
            GD.Print("Slope exit");
            xSlopeModifier = 0.0F;
            ySlopeModifier = 0.0F;
            slopeDir = -1;
            onSlope = false;
        }

        public void roll(){
            
        }

        
    }
}