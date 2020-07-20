using Godot;
using System;
using System.Linq;

// Hours with dadadada tenshi on loop: 10

public class Player : Character.BaseCharacter {
	private new int healthPoints = 10;
	private new int manaPoints = 20;

	// All variables related to movement
	private Vector2 inputVector = new Vector2(0.0F, 0.0F);


	// Player Inventory related Variables

	// Boolean to see if AimMode is enabled. In aim mode, the character will follow the crosshair
	private new bool aimMode = false;

	// Nodes
	
	private new AnimationPlayer animationPlayer;
	private Tween tween;
	private Camera2D camera;
	private Timer idleLongTimer;
	private new RayCast2D interactCast;
	

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
				rotatePlayer(inputVector.Angle() + Godot.Mathf.Deg2Rad(-90.0F), interactCast);
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
		if (!onSlope){
			getMovementInput(delta);
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

	private void playAnimation(string animationName){
		string newAnimationName = animationName;
		switch(currentAngle){
			case ANGLES.NORTH:
				newAnimationName += "_NORTH";
				break;
			case ANGLES.NORTHEAST:
				newAnimationName += "_NORTHEAST";
				break;
			case ANGLES.EAST:
				newAnimationName += "_EAST";
				break;
			case ANGLES.SOUTHEAST:
				newAnimationName += "_SOUTHEAST";
				break;
			case ANGLES.SOUTH:
				newAnimationName += "_SOUTH";
				break;
			case ANGLES.SOUTHWEST:
				newAnimationName += "_SOUTHWEST";
				break;
			case ANGLES.WEST:
				newAnimationName += "_WEST";
				break;
			case ANGLES.NORTHWEST:
				newAnimationName += "_NORTHWEST";
				break;
		}
		animationPlayer.Play(newAnimationName);
	}

	public void _on_RollTimer_timeout(){
		currentSpeed = baseSpeed;
		changeState(STATES.IDLE);
	}
}
