using Godot;
using System;
using System.Linq;
using rz_frzbn.Characters.basecharacter;
using rz_frzbn.Singletons.InvItems.Items;
using rz_frzbn.Singletons.Character.attacktype;
using rz_frzbn.Singletons.Character.states;
using rz_frzbn.Singletons.Character.entitytype;
using rz_frzbn.Singletons.Character.Angles;
using rz_frzbn.Environment.Mountains.Slope;


// Hours with dadadada tenshi on loop: i dont fucking know anymore help
// I have achived a new state of dadadada tenshi 
// I have heard it so many times that the song is constantly playing in my head on loop
// Now every living moment is dadadada tenshi. - Aug 7 2020-2021
// I have reached an even higher level of dadadada tenshi: I have achived 120+ Concurrent streams of dadadada tenshi - Aug 27 2020-2021
// Proof: https://cdn.discordapp.com/attachments/217253743668756480/748395551678267422/2020-20210827_131300.mp4
// TODO: Cure disease using a noose

namespace rz_frzbn.Characters.player{
	public class Player : BaseCharacter {
		// All variables related to movement
		private new Vector2 InputVector = new Vector2(0.0F, 0.0F);
		private Vector2 RawInputVector = new Vector2(0.0F,0.0F);

		// Player Inventory related Variables
		private bool HasGottenCrossbow = false;
		private short CrossbowBolts = 0;

		private int Food = 0;
		private int Coins = 0;


		// Boolean to see if AimMode is enabled. In aim mode, the character will follow the crosshair
		private new bool AimMode = false;
		private new	float PreviousAngleRadians = 0;
		private new Angles PreviousAngleAngle = Angles.NORTH;

		// Nodes
		private new AnimationPlayer AnimationPlayer;
		private Tween Tween;
		private Camera2D Camera;
		private Timer IdleLongTimer;
		private new RayCast2D InteractCast;
		private new Position2D SpawnPos;

		// Non FSM State Variables
		// Angle FSM
		public override void _Ready(){
			this.SetupNodes();
			this.AssignToGroup(EntityType.Player);
			AnimationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
			Camera = GetNode<Camera2D>("Camera");
			IdleLongTimer = GetNode<Timer>("Timers/IdleLongTimer");
			InteractCast = GetNode<RayCast2D>("InteractCast");
			Tween = GetNode<Tween>("Tween");
			SpawnPos = GetNode<Position2D>("InteractCast/Spawn");
			//
			MaxManaPoints = 200.0F;
			MaxHealthPoints = 100.0F;
			HealthPoints = 100.0F;
			ManaPoints = 200.0F;
			CurrentItem = HotbarItems.ICEBOLT;
			EmitSignal(nameof(HPChangedSignal), 10.0F);
		}

		private void GetMovementInput(float delta){ 
			// What you are about to see is `if` statement hell. Proceed at your own risk.
			// This code is provided "as is", without any warrenty of any kind, implied or expressed, including but
			// not limited to unleashing unspeakable lovecraftian mosters to roam your head, giving you YandereDev PTSD flashbacks, 
			// horrors that shatter both your physical representation and psyche sending you to a empty void to drift forever,
			// constantly hearing "Who is rem?" jokes, and being isekai'd into Re: Zero as Subaru.

			CurrentSpeed = BaseSpeed;
			InputVector = Vector2.Zero;

			// Check for movement. We want a the character to move in as many ways as possible
			
			if(Input.IsActionPressed("ui_up")){
				ChangeState(States.MOVE);
				InputVector.y -= 1.0F;
			}

			if(Input.IsActionPressed("ui_down")){
				ChangeState(States.MOVE);
				InputVector.y -= -1.0F;
			}

			if(Input.IsActionPressed("ui_left")){
				ChangeState(States.MOVE);
				InputVector.x -= 1.0F;
			}

			if(Input.IsActionPressed("ui_right")){
				ChangeState(States.MOVE);			
				InputVector.x -= -1.0F;
			}
			InputVector = InputVector.Normalized();
			

			// TODO: implement more (roll, jump) so change to if/elif
			switch (Input.IsActionPressed("ui_shift")){
				case true: CurrentSpeed = (int) (CurrentSpeed * RunSpeedMultiplier);
						break;
				case false: CurrentSpeed = BaseSpeed;
						break;
			}

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


		private void GetMovementOnSlope(float delta){
			InputVector = Vector2.Zero;
			RawInputVector = Vector2.Zero;
			
			// Check for movement. We want a the character to move in as many ways as possible
			
			if(Input.IsActionPressed("ui_up")){
				ChangeState(States.MOVE);
				InputVector.y -= 1.0F + SlopeModifierY;
				RawInputVector.y -= 1.0F ;
			}

			if(Input.IsActionPressed("ui_down")){
				ChangeState(States.MOVE);
				InputVector.y -= -1.0F + SlopeModifierY;
				RawInputVector.y -= -1.0F;
			}

			if(Input.IsActionPressed("ui_left")){
				ChangeState(States.MOVE);
				InputVector.x -= 1.0F + SlopeModifierX;
				RawInputVector.x -= 1.0F;
			}

			if(Input.IsActionPressed("ui_right")){
				ChangeState(States.MOVE);			
				InputVector.x -= -1.0F + SlopeModifierX;
				RawInputVector.x -= -1.0F;
			}

			if (InputVector != Vector2.Zero){
				MovementVector = MovementVector.MoveToward(InputVector * MaxSpeed, AccelerationMultiplier * delta);
			}
			else{
				MovementVector = MovementVector.MoveToward(Vector2.Zero, FrictionMultiplier * delta);
			}

			if(!AimMode){
				if(InputVector == Vector2.Zero){

				}
				else{
					//GD.Print(movementVector);
					RotatePlayer(InputVector.Normalized().Angle() + Godot.Mathf.Deg2Rad(-90.0F), InteractCast);
				}
			}
			else{
				/*
				Vector2 mousePos = GetGlobalMousePosition();
				Vector2 globalPos = this.GlobalPosition;
				RotatePlayer(Godot.Mathf.Atan2(mousePos.y - globalPos.y, mousePos.x - globalPos.x) + Godot.Mathf.Deg2Rad(-90.0F), InteractCast);
				*/
				RotatePlayer(RawInputVector.Angle(), this.InteractCast);
			}
			if (Input.IsActionPressed("ui_shift")){
				MovementVector.Clamped(MaxSpeedOnSlope);
			}
			else {
				MovementVector.Clamped(MaxSpeedOnSlopeWhileRunning);
			}
			MoveAndSlide(MovementVector);
			// Normalize puts vector on a unit circle, and as good as Normalize is its not desired behaviour here
			//inputVector = inputVector.Normalized();
		}

		private void GetMovementOnBoard(float delta){
			InputVector = Vector2.Zero;

			GD.Print(SlopeDirType);
			switch (SlopeDirType){
				case SlopeType.SOUTH:
					if(Input.IsActionPressed("ui_left")){
						ChangeState(States.BOARD);
						InputVector.x -= 1.0F;
					}
					if(Input.IsActionPressed("ui_right")){
						ChangeState(States.BOARD);			
						InputVector.x -= -1.0F;
					}
					InputVector.y = 1.0F;
					break;
				case SlopeType.WEST:
					if(Input.IsActionPressed("ui_left")){
						ChangeState(States.BOARD);
						InputVector.y -= -1.0F;
					}
					if(Input.IsActionPressed("ui_right")){
						ChangeState(States.BOARD);			
						InputVector.y -= 1.0F;
					}
					if(Input.IsActionPressed("ui_up")){
						ChangeState(States.BOARD);
						InputVector.y -= 1.0F;
					}
					if(Input.IsActionPressed("ui_down")){
						ChangeState(States.BOARD);			
						InputVector.y -= -1.0F;
					}
					InputVector.x = -1.0F;
					break;	
				case SlopeType.NORTH:
					if(Input.IsActionPressed("ui_left")){
						ChangeState(States.BOARD);
						InputVector.x -= 1.0F;
					}
					if(Input.IsActionPressed("ui_right")){
						ChangeState(States.BOARD);			
						InputVector.x -= -1.0F;
					}
					InputVector.y = -1.0F;
					break;
				case SlopeType.EAST:
					if(Input.IsActionPressed("ui_left")){
						ChangeState(States.BOARD);
						InputVector.y -= -1.0F;
					}
					if(Input.IsActionPressed("ui_right")){
						ChangeState(States.BOARD);			
						InputVector.y -= 1.0F;
					}
					if(Input.IsActionPressed("ui_up")){
						ChangeState(States.BOARD);
						InputVector.y -= 1.0F;
					}
					if(Input.IsActionPressed("ui_down")){
						ChangeState(States.BOARD);			
						InputVector.y -= -1.0F;
					}
					InputVector.x = 1.0F;
					break;	
			}
			GD.Print(InputVector);
			InputVector = InputVector.Normalized();
			RotatePlayer(InputVector.Angle() + Godot.Mathf.Deg2Rad(-90.0F), InteractCast);
			MovementVector = MovementVector.MoveToward(InputVector * MaxSpeedOnBoard, AccelerationMultiplier * delta);
			MoveAndSlide(MovementVector);
		}

		public override void _Input(InputEvent inputEvent){
			// We cant use `switch` here because `IsActionPressed` returns a bool and there is no other way (except doing all input key filtering yourself)
			// that lets us use `switch` statements. 
			if(inputEvent.IsActionPressed("ui_jump") && (ValidMoveStates.Contains(currentState))){ // CHANGED - Dont allow the player to jump while boarding
				ChangeState(States.JUMP);
			}
			// Elifs because we want these to be mutually exclusive
			else if (inputEvent.IsActionPressed("ui_board") && (ValidMoveStates.Contains(currentState)) && OnSlope){
				ChangeState(States.BOARD);
			}
			else if (inputEvent.IsActionPressed("ui_attack") && (ValidMoveStates.Contains(currentState))){
				// TODO: Do hotbar check to call correct change state
				//GD.Print("attacc");
				//ChangeState(States.ATTACK_MAGE);
				
				switch(this.CurrentItem) {
					case HotbarItems.NONE:
						break;
					case HotbarItems.ICEBOLT:
						if (UseMana(10.0F)){
							ChangeState(States.ATTACK_MAGE);
						}
						break;
					case HotbarItems.SHIELD:
						if (UseMana(40.0F)){
							ChangeState(States.ATTACK_SHEILD);
						}						
						break;
				}
				
			}

			// Ive been trying for at least 3 days now to get rolling working. I give up, you win.
			// HAHAHAAHAHAA FUCK YOU TWEENS I WIN INTO YOUR FUCKING TRASHCAN YOU GO LMAOOOOOOO I GOT ROLL WORKING YESYESYESYESYESYESYESYES
			else if (inputEvent.IsActionPressed("ui_roll") && (ValidMoveStates.Contains(currentState)) && !OnSlope){
				ChangeState(States.ROLL);
				CurrentSpeed = 0;
				MovementVector = Vector2.Zero;
				RollPlayer();
			}

			// Check for UI inputs
			// Please god forgive me

			if(inputEvent.IsActionPressed("ui_1")){
				UpdateItemHeld(HotbarItems.ICEBOLT);
			}
			else if (inputEvent.IsActionPressed("ui_2")){
				UpdateItemHeld(HotbarItems.SHIELD);
			}
			else if (inputEvent.IsActionPressed("ui_3")){
				if (this.HasGottenCrossbow){
					UpdateItemHeld(HotbarItems.CROSSBOW);
				}
			}
		}

		public override void _PhysicsProcess(float delta){
			// TODO:just put board off anim in ToIdle state machine condition
			if (CanMove){
				if (!OnSlope){
				GetMovementInput(delta);
				}
				else if(OnBoard){
					GetMovementOnBoard(delta);
				}
				else{
					GetMovementOnSlope(delta);
				}
			}
			
			if (FSinceLastAtk >= long.MaxValue){
				FSinceLastAtk = 0;
			}
			else {
				FSinceLastAtk++;
			}
			if (FSinceLastHit >= long.MaxValue){
				FSinceLastHit = 0;
			}
			else {
				FSinceLastHit++;
			}
			DoRegeneration(delta);
		}

		// Signals
		public void _onIdleLongTimerTimeout(){
			// change_state(States.IDLE);
			// Reset self
		}

		public void _on_Tween_tween_completed(Godot.Object o, NodePath key){
			//GD.Print("a");
			CurrentSpeed = BaseSpeed;
			ChangeState(States.IDLE);
		}

		public void _on_PhysicsTween_tween_completed(Godot.Object o, NodePath key){
			CurrentSpeed = 0;
			ChangeState(States.IDLE);
		}

		
		public void _on_RollTimer_timeout(){
			CurrentSpeed = BaseSpeed;
			ChangeState(States.IDLE);
		}


		public override void _on_AnimationPlayer_animation_finished(string AnimName){
			//GD.Print("here");
			ChangeState(States.IDLE);
			if (AnimName == "ATTACK"){
				this.Rotation = PreviousAngleRadians;
				CurrentAngle = PreviousAngleAngle;
				InteractCast.RotationDegrees = ((float) CurrentAngle) * 45.0F + 180.0F;
			}
		}

		public override void RotatePlayer(float radians, RayCast2D castRotater){
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
			else if ((degrees < -112.5)){
				toAngle = Angles.NORTHEAST;
			}
			else if ((degrees < -67.5)){
				toAngle = Angles.EAST;
			}
			else if ((degrees < -22.5)){
				toAngle = Angles.SOUTHEAST;
			}
			else if ((degrees < 22.5)){
				toAngle = Angles.SOUTH;
			}
			else if ((degrees < 67.5)){
				toAngle = Angles.SOUTHWEST;
			}
			// Note that 90 can also be -270, so we also have to check for that
			else if ((degrees < 112.5)){
				toAngle = Angles.WEST;
			}
			
			// We decided to move to a system where if you attack the player automatically rotates
			castRotater.RotationDegrees = ((float) toAngle) * 45.0F + 180.0F;
			
			CurrentAngle = toAngle;
		}

		
		public void RotatePlayer(float radians, RayCast2D castRotater, bool trueRaycastRotate){
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
			
			// We decided to move to a system where if you attack the player automatically rotates
			if (trueRaycastRotate){
				castRotater.Rotation = radians + Godot.Mathf.Deg2Rad(180.0F);
			}
			CurrentAngle = toAngle;
		}

		
		protected override void Attack(AttackType? attackType){
			GD.Print("attack()");
			if(attackType.HasValue){
				PreviousAngleRadians = this.Rotation;
				PreviousAngleAngle = CurrentAngle;

				Vector2 mousePos = GetGlobalMousePosition();
				Vector2 globalPos = this.GlobalPosition;
				float mouseAndGlobalAngle = Godot.Mathf.Atan2(mousePos.y - globalPos.y, mousePos.x - globalPos.x) + Godot.Mathf.Deg2Rad(90.0F);
				InteractCast.Rotation =  mouseAndGlobalAngle;
				RotatePlayer(mouseAndGlobalAngle,InteractCast,true);


				switch(attackType){
					case AttackType.MAGE_ICEBOLT:
						AnimationPlayer.Play("ATTACK");
						if (IceBolt.CanInstance()){
							var inst = IceBolt.Instance();
							Owner.AddChild(inst);
							inst.Call("SetBullet", this.SpawnPos.GlobalPosition, this.InteractCast.Rotation);
							this.FSinceLastAtk = 0;
						}
						else{
							throw new InvalidOperationException("Could not instance attack type!");
						}
					
						break;
					case AttackType.MAGE_SHIELD:
						AnimationPlayer.Play("ATTACK");
						if (Sheild.CanInstance()){
							var inst = Sheild.Instance();
							Owner.AddChild(inst);
							inst.Call("SetSheild", this.SpawnPos.GlobalPosition, this.InteractCast.Rotation);
							this.FSinceLastAtk = 0;
						}
						else{
							throw new InvalidOperationException("Could not instance attack type!");
						}
						break;
					case AttackType.RANGED_CROSSBOW:
						AnimationPlayer.Play("ATTACK");
						if (Arrow.CanInstance()){
							var inst = Arrow.Instance();
							Owner.AddChild(inst);
							inst.Call("SetBullet", this.SpawnPos.GlobalPosition, this.InteractCast.Rotation);
							this.FSinceLastAtk = 0;
						}
						else{
							throw new InvalidOperationException("Could not instance attack type!");
						}
						break;
					case AttackType.MELEE_KICK:
						AnimationPlayer.Play("ATTACK");
						this.FSinceLastAtk = 0;
						break;
					default:
						//animationPlayer.Play("ATTACK"); //TODO: Remove this
						break;
				}
			}
		}
		private void RollPlayer(){
			//GD.Print("aa");
			Tween physicsTween = GetNode<Tween>("PhysicsTween");
			ChangeState(States.ROLL);

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
			
			GD.Print("a");
			Vector2 initialVector = new Vector2(xDir * RollSpeed,yDir * RollSpeed);
			GD.Print(initialVector);
			MovementVector = Vector2.Zero;
			physicsTween.InterpolateMethod(this, "move_and_slide", initialVector, initialVector, RollDuration, Tween.TransitionType.Bounce, Tween.EaseType.OutIn);
			if (!physicsTween.IsActive()){
				//GD.Print("start");
				physicsTween.Start();
			}
			//MoveAndSlide(initialVector);
			ChangeState(States.IDLE);
			//GD.Print(currentState);
		}

		public void AddCoins(int amt){
			this.Coins += amt;
		}

		public bool RemoveCoins(int amt){
			if ((this.Coins - amt) < 0){
				return false;
			}
			else {
				this.Coins -= amt;
				return true;
			}
		}
	}
}