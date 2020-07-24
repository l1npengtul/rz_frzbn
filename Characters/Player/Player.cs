using Godot;
using System;
using System.Linq;

// Hours with dadadada tenshi on loop: 20

public class Player : Character.BaseCharacter {
	private new int healthPoints = 10;
	private new int manaPoints = 20;

	// All variables related to movement
	private Vector2 inputVector = new Vector2(0.0F, 0.0F);


	// Player Inventory related Variables

	// Boolean to see if AimMode is enabled. In aim mode, the character will follow the crosshair
	private new bool aimMode = false;
	private float previousAngleRadians = 0;
	private ANGLES previousAngleAngle = ANGLES.NORTH;

	// Nodes
	
	private new AnimationPlayer animationPlayer;
	private Tween tween;
	private Camera2D camera;
	private Timer idleLongTimer;
	private new RayCast2D interactCast;

	public enum AttackType{
		MAGE_TRIBOLT,
		MAGE_SHIELD,
		RANGED_CROSSBOW,
		MELEE_KICK,
		MELEE_PUNCH,
	}
	

	// Non FSM State Variables
	// Angle FSM
	public override void _Ready(){
		animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		camera = GetNode<Camera2D>("Camera");
		idleLongTimer = GetNode<Timer>("Timers/IdleLongTimer");
		interactCast = GetNode<RayCast2D>("InteractCast");
		tween = GetNode<Tween>("Tween");
	}

	private void getMovementInput(float delta){ 
		// What you are about to see is `if` statement hell. Proceed at your own risk.
		// This code is provided "as is", without any warrenty of any kind, implied or expressed, including but
		// not limited to unleashing unspeakable lovecraftian mosters to roam your head, giving you YandereDev PTSD flashbacks, 
		// horrors that shatter both your physical representation and psyche sending you to a empty void to drift forever,
		// constantly hearing "Who is rem?" jokes, and being isekai'd into Re:Zero as Subaru.

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
		
		// Ive been trying for at least 3 days now to get rolling working. I give up, you win.
		// HAHAHAAHAHAA FUCK YOU TWEENS I WIN INTO YOUR FUCKING TRASHCAN YOU GO LMAOOOOOOO I GOT ROLL WORKING YESYESYESYESYESYESYESYES
		if (Input.IsActionPressed("ui_roll") && (new [] {STATES.IDLE, STATES.IDLE_LONG, STATES.MOVE}.Contains(currentState)) && !onSlope){
			changeState(STATES.ROLL);
			currentSpeed = 0;
			movementVector = Vector2.Zero;
			rollPlayer();
		}

		// TODO: implement more (roll, jump) so change to if/elif
		switch (Input.IsActionPressed("ui_shift")){
			case true: currentSpeed = (int) (currentSpeed * runSpeedMultiplier);
					   break;
			case false: currentSpeed = baseSpeed;
					   break;
		}

		if (inputVector != Vector2.Zero){
			movementVector = movementVector.MoveToward(inputVector * currentSpeed, accelerationMultiplier * delta);
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
				rotatePlayer(inputVector.Angle() + Godot.Mathf.Deg2Rad(-90.0F), interactCast);
			}
		}
		else{
			Vector2 mousePos = GetGlobalMousePosition();
			Vector2 globalPos = this.GlobalPosition;
			rotatePlayer(Godot.Mathf.Atan2(mousePos.y - globalPos.y, mousePos.x - globalPos.x) + Godot.Mathf.Deg2Rad(-90.0F), interactCast);
		}
		//GD.Print(movementVector);
		movementVector = MoveAndSlide(movementVector);
	}

	private void getMovementOnSlope(float delta){
		inputVector = Vector2.Zero;

		// Check for movement. We want a the character to move in as many ways as possible
		
		if(Input.IsActionPressed("ui_up")){
			changeState(STATES.MOVE);
			inputVector.y -= 1.0F + ySlopeModifier;
		}

		if(Input.IsActionPressed("ui_down")){
			changeState(STATES.MOVE);
			inputVector.y -= -1.0F + ySlopeModifier;
		}

		if(Input.IsActionPressed("ui_left")){
			changeState(STATES.MOVE);
			inputVector.x -= 1.0F + xSlopeModifier;
		}

		if(Input.IsActionPressed("ui_right")){
			changeState(STATES.MOVE);			
			inputVector.x -= -1.0F + xSlopeModifier;
		}

		if (inputVector != Vector2.Zero){
			movementVector = movementVector.MoveToward(inputVector * maxSpeed, accelerationMultiplier * delta);
		}
		else{
			movementVector = movementVector.MoveToward(Vector2.Zero, frictionMultiplier * delta);
		}

		if(!aimMode){
			if(inputVector == Vector2.Zero){

			}
			else{
				//GD.Print(movementVector);
				rotatePlayer(inputVector.Normalized().Angle() + Godot.Mathf.Deg2Rad(-90.0F), interactCast);
			}
		}
		else{
			Vector2 mousePos = GetGlobalMousePosition();
			Vector2 globalPos = this.GlobalPosition;
			rotatePlayer(Godot.Mathf.Atan2(mousePos.y - globalPos.y, mousePos.x - globalPos.x) + Godot.Mathf.Deg2Rad(-90.0F), interactCast);
		}

		MoveAndSlide(movementVector);
		// Normalize puts vector on a unit circle, and as good as Normalize is its not desired behaviour here
		//inputVector = inputVector.Normalized();
	}

	private void getMovementOnBoard(float delta){
		Vector2 globalMouse = GetGlobalMousePosition();
		Vector2 localPos = this.GlobalPosition;
		inputVector = new Vector2(globalMouse.x - localPos.x, globalMouse.y - localPos.y).Normalized();

		rotatePlayer(inputVector.Angle() + Godot.Mathf.Deg2Rad(-90.0F), interactCast);
		movementVector = movementVector.MoveToward(inputVector * maxSpeed, accelerationMultiplier * delta);

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
		
		

		Vector2 initialVector = new Vector2(xDir * 50,yDir * 50);
		movementVector = Vector2.Zero;
		physicsTween.InterpolateMethod(this, "move_and_slide", initialVector, initialVector, rollDuration, Tween.TransitionType.Bounce, Tween.EaseType.OutIn);
		if (!physicsTween.IsActive()){
			GD.Print("start");
			physicsTween.Start();
		}
		//MoveAndSlide(initialVector);
		changeState(STATES.IDLE);
		GD.Print(currentState);
	}

	public override void _Input(InputEvent inputEvent){

		if(inputEvent.IsActionPressed("ui_jump") && (new [] {STATES.IDLE, STATES.IDLE_LONG, STATES.MOVE, STATES.BOARD}.Contains(currentState))){
			changeState(STATES.JUMP);
		}
		// Elifs because we want these to be mutually exclusive
		else if (inputEvent.IsActionPressed("ui_board") && (new [] {STATES.IDLE, STATES.IDLE_LONG, STATES.MOVE}.Contains(currentState)) && onSlope){
			changeState(STATES.BOARD);
		}
		else if (inputEvent.IsActionPressed("ui_attack") && (new [] {STATES.IDLE, STATES.IDLE_LONG, STATES.MOVE}.Contains(currentState))){
			// TODO: Do hotbar check to call correct change state
			GD.Print("attacc");
			changeState(STATES.ATTACK_MAGE);
		}
	}

	public override void _PhysicsProcess(float delta){
		if (!onSlope){
			getMovementInput(delta);
		}
		else if(onBoard){
			getMovementOnSlope(delta);
		}
		else{
			getMovementOnSlope(delta);
		}
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
		currentSpeed = 0;
		changeState(STATES.IDLE);
	}

	
	public void _on_RollTimer_timeout(){
		currentSpeed = baseSpeed;
		changeState(STATES.IDLE);
	}

	public void _on_AnimationPlayer_animation_finished(string anim_name){
		changeState(STATES.IDLE);
		if (anim_name == "ATTACK"){
			this.Rotation = previousAngleRadians;
			currentAngle = previousAngleAngle;
			interactCast.RotationDegrees = ((float) currentAngle) * 45.0F + 180.0F;
		}
	}

	new public void rotatePlayer(float radians, RayCast2D castRotater){
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
		
		// We decided to move to a system where if you attack the player automatically rotates
		castRotater.RotationDegrees = ((float) toAngle) * 45.0F + 180.0F;
		
		currentAngle = toAngle;
	}

	
	public void rotatePlayer(float radians, RayCast2D castRotater, bool trueRaycastRotate){
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
		
		// We decided to move to a system where if you attack the player automatically rotates
		if (trueRaycastRotate){
			castRotater.Rotation = radians + Godot.Mathf.Deg2Rad(180.0F);
		}
		currentAngle = toAngle;
	}

	
	public void attackWithRotateStart(AttackType attackType){
		previousAngleRadians = this.Rotation;
		previousAngleAngle = currentAngle;

		Vector2 mousePos = GetGlobalMousePosition();
		Vector2 globalPos = this.GlobalPosition;
		float mouseAndGlobalAngle = Godot.Mathf.Atan2(mousePos.y - globalPos.y, mousePos.x - globalPos.x) + Godot.Mathf.Deg2Rad(90.0F);
		interactCast.Rotation =  mouseAndGlobalAngle;
		rotatePlayer(mouseAndGlobalAngle,interactCast,true);

		switch(attackType){
			case AttackType.MAGE_TRIBOLT:
				animationPlayer.Play("ATTACK");
				break;
			case AttackType.MAGE_SHIELD:
				animationPlayer.Play("ATTACK");
				break;
			case AttackType.RANGED_CROSSBOW:
				animationPlayer.Play("ATTACK");
				break;
		}
	}

	new protected void changeState(STATES toState){
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
				SetPhysicsProcess(false);
				attackWithRotateStart(AttackType.MAGE_TRIBOLT);
				break;
			case STATES.BOARD:
				onBoard = true;
				break;
			// Skipping all other states until movement and roll works properly. 
			default:
				//aniPlayer.Play("IDLE");
				break;
		}
        currentState = toState;
        //GD.Print(currentState);
	}
}
