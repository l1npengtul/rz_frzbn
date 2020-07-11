using Godot;
using System.Linq;

public class Player : KinematicBody2D {
	private int healthPoints = 10;
	private int manaPoints = 20;

	// All variables related to movement
	private Vector2 movementVector = new Vector2(0,0);
	private const int speed = 300;
	private const int rollSpeed = 500;
	
	// Player Inventory related Variables

	// Boolean to see if AimMode is enabled. In aim mode, the character will follow the crosshair
	private bool aimMode = false;

	// Nodes
	/*
	private AnimationPlayer animationPlayer = (AnimationPlayer)GetNode("AnimationPlayer");
	private Tween tween = (Tween)GetNode("Tween");
	private Camera2D camera = (Camera2D)GetNode("Camera");
	private Timer attackMageTimer = (Timer)GetNode("Node2D/AttackMageTimer");
	private Timer attackMeleeTimer = (Timer)GetNode("Node2D/AttackMeleeTimer");
	private Timer attackRangedTimer = (Timer)GetNode("Node2D/AttackRangedTimer");
	private Timer idleLongTimer = (Timer)GetNode("Node2D/IdleLongTimer");
	*/

	// Non FSM State Variables
	private bool onSlope = false;

	//FSM
	private enum STATES {
		IDLE,
		IDLE_LONG,
		RUN,
		WALK,
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

	public override void _Ready(){
		GetNode("Timers/IdleLongTimer");
	}

	private void getMovementInput(){ 
		movementVector = new Vector2();

		// Check for movement. We want a the character to move in as many ways as possible
		// 
		if(Input.IsActionPressed("ui_up")){
			movementVector.y -= 1;
		}

		if(Input.IsActionPressed("ui_down")){
			movementVector.y -= -1;
		}

		if(Input.IsActionPressed("ui_left")){
			movementVector.x -= 1;
		}

		if(Input.IsActionPressed("ui_right")){
			movementVector.x -= -1;
		}

		movementVector = movementVector.Normalized() * speed;
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

		var aniPlayer = (AnimationPlayer)GetNode("AnimationPlayer");
		// Get the new state
		switch (toState){
			case STATES.IDLE:
				aniPlayer.Play("IDLE");
				break;
			case STATES.IDLE_LONG:
				// TODO: Make More Idle Long Anims if possible
				aniPlayer.Play("IDLE_LONG");
				break;
			case STATES.RUN:
				aniPlayer.Play("RUN");
				break;
			case STATES.WALK:
				aniPlayer.Play("WALK");
				break;
			/* TODO: Implement Jump 
			case STATES.JUMP:
				aniPlayer.Play("RUN");
				break;
				*/
			case STATES.ROLL:
				SetPhysicsProcess(false);
				aniPlayer.Play("ROLL");
				break;
			case STATES.JUMP:
				// TODO: Implement jump (Y movement UP, reduced air control by set margain)
				aniPlayer.Play("IDLE");
				break;
			case STATES.BOARD:
				// TODO Implement board
				aniPlayer.Play("BOARD");
				break;
			case STATES.STAGGER:
				// TODO: Only when hit 
				// Play knockback anim
				// Give IFrames
				aniPlayer.Play("IDLE");
				break;
			case STATES.ATTACK_MAGE:
				// So here is the thing:
				// Emilia's sprite is broken up into many parts, allowing us to "blend" animations
				// Emilia will play the attack mage
				aniPlayer.Play("IDLE");
				break;
			// Skipping all other states until movement and roll works properly. 
			default:
				aniPlayer.Play("IDLE");
				break;
			

		}
	}
	

	public override void _Input(InputEvent inputEvent){
		// See if the player pressed jump and is not in banned states
		if(inputEvent.IsActionPressed("ui_jump") && (new [] {STATES.IDLE, STATES.IDLE_LONG, STATES.WALK, STATES.RUN, STATES.BOARD}.Contains(currentState))){
			changeState(STATES.JUMP);
		}
		// Elifs because we want these to be mutually exclusive
		else if (inputEvent.IsActionPressed("ui_shift") && (new [] {STATES.IDLE, STATES.IDLE_LONG, STATES.WALK, STATES.RUN}.Contains(currentState))){
			changeState(STATES.RUN);
		}
		else if (inputEvent.IsActionPressed("ui_roll") && (new [] {STATES.IDLE, STATES.IDLE_LONG, STATES.WALK, STATES.RUN}.Contains(currentState))){
			changeState(STATES.ROLL);
		}
		else if (inputEvent.IsActionPressed("ui_board") && (new [] {STATES.IDLE, STATES.IDLE_LONG, STATES.WALK, STATES.RUN}.Contains(currentState)) && onSlope){
			changeState(STATES.BOARD);
		}

		
	}

	public override void _PhysicsProcess(float delta){
		getMovementInput();
		movementVector = MoveAndSlide(movementVector);
	}

	// Signals
	public void _onIdleLongTimerTimeout(){
		// change_state(STATES.IDLE);
		// Reset self
	}


}
