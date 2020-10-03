using Godot;
using System;

namespace rz_frzbn.Weapons.basebullet{
    public enum BulletType {
        ICEBOLT,
        ARROW,
        MAGEOTHER,
        // dont bully me if i spelled it wrong
        SHEILD,
    }
    public class BaseBullet : Area2D{
        [Export]
        protected int BulletDamage = 10;
        [Export]
        protected BulletType bulletType = BulletType.MAGEOTHER;
        [Export]
        protected float BulletVelocity = 1000.0F;
        [Export]
        protected int BulletLastTime = 10;

        protected Vector2 moveTo = Vector2.Zero;
        protected Vector2 setTo = Vector2.Zero;
        protected float xvar = 0,yvar = 0;

        protected Timer timer;
        //protected VisibilityNotifier2D visibility;
        protected AudioStreamPlayer2D audio;
        protected AnimatedSprite sprite;

        protected bool isTimerExpired = false;
        protected bool isNotVisible = false;
        protected bool ToBreakAnim = false, ToBreakAudio = false;
        protected bool HasHit = false;

        public override void _Ready(){
            //SetProcess(false);
            this.timer = GetNode<Timer>("Timer");
            //this.visibility = GetNode<VisibilityNotifier2D>("VisibilityNotifier2D");
            this.audio = GetNode<AudioStreamPlayer2D>("AudioStreamPlayer2D");
            this.sprite = GetNode<AnimatedSprite>("AnimatedSprite");
            sprite.Play("IDLE");
            this.timer.WaitTime = this.BulletLastTime;
            this.timer.Start();
        }

        // The reason that this is here instead of `_Init` is because Godot messes with constructors. 
        public void SetBullet(Vector2 pos, float rot){
            this.Rotation = rot;
            moveTo = rz_frzbn.Singletons.Math.Vector.VectorMathF.VectorMathF.fromAngleRadians(rot + Mathf.Deg2Rad(90.0F)).Normalized();
            GD.Print(moveTo.x);
            GD.Print(moveTo.y);
            this.GlobalPosition = pos;
            //SetProcess(true);
        }

        public override void _Process(float delta){
            if(isTimerExpired){
                ToBreakAnim = true;
            }
            else {
                setTo.x = this.Position.x + moveTo.x * BulletVelocity * delta;
                setTo.y = this.Position.y + moveTo.y  * BulletVelocity * delta;
                //GD.Print(setTo);
                this.Position = setTo;
            }

            if(ToBreakAnim && HasHit){
                // No Programmer-Chan, you're getting this all wrong! We dont **kill** objects, we ***FREE*** them
                QueueFree();
            }
        }

        public void _on_BaseBullet_body_entered(Godot.Node body){
            HasHit = true;
            // TODO: Sound effect
            if (body.HasMethod("TakeDamage") && body != this){
                body.Call("TakeDamage", (float)this.BulletDamage);
            }
            else if (body.HasMethod("TakeDamageWithKB") && body != this){
                body.Call("TakeDamageWithKB", (float)this.BulletDamage, this.GlobalPosition);
            }
            this.sprite.Play("BREAK");
        }

        public void _on_AudioStreamPlayer2D_finished(){
            ToBreakAudio = true;
        }

        public void _on_Timer_timeout(){
            HasHit = true;
            this.isTimerExpired = true;
        }

        public void _on_AnimatedSprite_animation_finished(){
            if (HasHit){
                ToBreakAnim = true;
            }
        }
    }
}
