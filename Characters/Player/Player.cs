using Godot;
using System;
using System.Linq;

// Hours with dadadada tenshi on loop: 7

public class Player : KinematicBody2D {
	private int healthPoints = 10;
	private int manaPoints = 20;

	// All variables related to movement
	private Vector2 movementVector = new Vector2(0,0);
	private Vector2 inputVector = new Vector2(0.0F, 0.0F);

	private const float frictionMultiplier = 4000.0F;
	private const float accelerationMultiplier = 4000.0F;
	private const float rollDuration = 0.5F;
	private float walkSpeedMultiplier = 1.0F;
	private float runSpeedMultiplier = 1.6F;
	private const int baseSpeed = 600;
	private const int maxSpeed = 700;
	private const int rollSpeed = 1500;
	private int currentSpeed = 600;

	private bool onBoard = false;
	
	
	// Player Inventory related Variables

	// Boolean to see if AimMode is enabled. In aim mode, the character will follow the crosshair
	private bool aimMode = false;

	// Nodes
	
	private AnimationPlayer animationPlayer;
	private Tween tween;
	private Camera2D camera;
	private Timer idleLongTimer;
	private RayCast2D interactCast;
	

	// Non FSM State Variables
	private bool onSlope = false;

	//FSM
	public enum STATES {
		IDLE,
		IDLE_LONG,
		MOVE,
		JUMP,
		ROLL,
		BOARD,
		STAGGER,
		ATTACK_MAGE,
		ATTACK_RANGED,
		ATTACK_MELEE,
		//TALK,
		//TALK_SHOP,
		DYING,
		DEAD,
	}
	private STATES currentState = STATES.IDLE;

	// Angle FSM
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
	ANGLES currentAngle = ANGLES.NORTH;
	public override void _Ready(){
		animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		camera = GetNode<Camera2D>("Camera");
		idleLongTimer = GetNode<Timer>("Timers/IdleLongTimer");
		interactCast = GetNode<RayCast2D>("InteractCast");
		tween = GetNode<Tween>("Tween");
	}

	private void getMovementInput(float delta){ 
		currentSpeed = baseSpeed;
		inputVector = Vector2.Zero;

		// Check for movement. We want a the character to move in as many ways as possible
		
		if(Input.IsActionPressed("ui_up")){
			changeState(STATES.MOVE);
			inputVector.y -= 1.0F;
		}

		if(Input.IsActionPressed("ui_down")){
			changeState(STATES.MOVE);
			inputVector.y -= -1.0F;
		}

		if(Input.IsActionPressed("ui_left")){
			changeState(STATES.MOVE);
			inputVector.x -= 1.0F;
		}

		if(Input.IsActionPressed("ui_right")){
			changeState(STATES.MOVE);			
			inputVector.x -= -1.0F;
		}
		inputVector = inputVector.Normalized();


		

		// TODO: implement more (roll, jump) so change to if/elif
		switch (Input.IsActionPressed("ui_shift")){
			case true: currentSpeed = (int) (currentSpeed * runSpeedMultiplier);
					   break;
			case false: currentSpeed = baseSpeed;
					   break;
		}

		if (inputVector != Vector2.Zero){
			movementVector = movementVector.MoveToward(inputVector.Normalized() * currentSpeed, accelerationMultiplier * delta);
		}
		else{
			movementVector = movementVector.MoveToward(Vector2.Zero, frictionMultiplier * delta);
		}


		if(inputVector == Vector2.Zero){
			changeState(STATES.IDLE);
		}

		if(!aimMode){
			if(inputVector == Vector2.Zero){

			}
			else{
				//GD.Print(movementVector);
				rotatePlayer(inputVector.Angle() + Godot.Mathf.Deg2Rad(-90.0F));
			}
		}
		else{
			Vector2 mousePos = GetGlobalMousePosition();
			Vector2 globalPos = this.GlobalPosition;
			rotatePlayer(Godot.Mathf.Atan2(mousePos.y - globalPos.y, mousePos.x - globalPos.x) + Godot.Mathf.Deg2Rad(-90.0F));
		}

		

		//GD.Print(movementVector);
		movementVector = MoveAndSlide(movementVector);
		
	}
	
	private void changeState(STATES toState){
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
				rollPlayer();
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
		GD.Print(currentState);
	}

	private void rollPlayer(){
		GD.Print("aa");
		Tween physicsTween = GetNode<Tween>("PhysicsTween");

		float xDir = 0.0F;
		float yDir = 0.0F;

		if(Input.IsActionPressed("ui_up")){
			yDir -= 1.0F;
		}

		if(Input.IsActionPressed("ui_down")){
			yDir -= -1.0F;
		}

		if(Input.IsActionPressed("ui_left")){
			xDir -= 1.0F;
		}

		if(Input.IsActionPressed("ui_right")){
			xDir -= -1.0F;
		}
		
		

		Vector2 initialVector = new Vector2(xDir * rollSpeed,yDir * rollSpeed);

		physicsTween.InterpolateMethod(this, "move_and_slide", initialVector, initialVector, rollDuration, Tween.TransitionType.Bounce, Tween.EaseType.OutIn);
		if (!physicsTween.IsActive()){
			GD.Print("start");
			physicsTween.Start();
		}
		MoveAndSlide(initialVector);
		changeState(STATES.IDLE);
		GD.Print(currentState);
	}

	public override void _Input(InputEvent inputEvent){

		if(inputEvent.IsActionPressed("ui_jump") && (new [] {STATES.IDLE, STATES.IDLE_LONG, STATES.MOVE, STATES.BOARD}.Contains(currentState))){
			changeState(STATES.JUMP);
		}
		// Ive been trying for at least 3 days now to get rolling working. I give up, you win.
		// HAHAHAAHAHAA FUCK YOU TWEENS I WIN INTO YOUR FUCKING TRASHCAN YOU GO LMAOOOOOOO I GOT ROLL WORKING YESYESYESYESYESYESYESYES
		else if (Input.IsActionPressed("ui_roll") && (new [] {STATES.IDLE, STATES.IDLE_LONG, STATES.MOVE}.Contains(currentState))){
			changeState(STATES.ROLL);
		}
		// Elifs because we want these to be mutually exclusive
		else if (inputEvent.IsActionPressed("ui_board") && (new [] {STATES.IDLE, STATES.IDLE_LONG, STATES.MOVE}.Contains(currentState)) && onSlope){
			changeState(STATES.BOARD);
		}
		

		if(inputEvent.IsActionPressed("ui_aimmode")){
			if(aimMode){
				aimMode = false;
			}
			else{
				aimMode = true;
			}
		}

		
		
	}

	public override void _PhysicsProcess(float delta){
		getMovementInput(delta);
	}

	// Signals
	public void _onIdleLongTimerTimeout(){
		// change_state(STATES.IDLE);
		// Reset self
	}

	public void _on_Tween_tween_completed(Godot.Object o, NodePath key){
		//GD.Print("a");
		currentSpeed = baseSpeed;
		changeState(STATES.IDLE);
	}

	public void _on_PhysicsTween_tween_completed(Godot.Object o, NodePath key){
		currentSpeed = baseSpeed;
		changeState(STATES.IDLE);
	}


	public void rotatePlayer(float radians){
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
		// Only for AimMode
		if (aimMode){
			interactCast.Rotation = radians;
		}
		else{
			interactCast.RotationDegrees = ((float) toAngle) * 45.0F + 180.0F;
		}
		// TODO remove this if statement its a test
		//GD.Print(toAngle);

		currentAngle = toAngle;
	}

	public void playSpriteAnimation(string animation){
		
	}

	public void _on_RollTimer_timeout(){
		currentSpeed = baseSpeed;
		changeState(STATES.IDLE);
	}
}
