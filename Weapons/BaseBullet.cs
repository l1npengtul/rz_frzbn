using Godot;
using System;

namespace rz_frzbn.Weapons.BaseBullet{
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
        protected int BulletVelocity = 100;
        [Export]
        protected int BulletLastTime = 10;

        protected Vector2 moveTo = Vector2.Zero;
        protected Vector2 setTo = Vector2.Zero;
        protected float xvar = 0,yvar = 0;

        protected Timer timer;
        protected VisibilityNotifier2D visibility;
        protected AudioStreamPlayer2D audio;

        protected bool isTimerExpired = false;
        protected bool isNotVisible = false;

        public override void _Ready(){
            SetProcess(false);
            this.timer = GetNode<Timer>("Timer");
            this.visibility = GetNode<VisibilityNotifier2D>("VisibilityNotifier2D");
            this.audio = GetNode<AudioStreamPlayer2D>("AudioStreamPlayer2D");
            this.timer.WaitTime = this.BulletLastTime;
            this.timer.Start();
        }

        // The reason that this is here instead of `_Init` is because Godot messes with constructors. 
        public void setBullet(float rot){
            this.Rotation = rot;
            moveTo = rz_frzbn.Singletons.Math.Vector.VectorMathF.VectorMathF.fromAngleRadians(rot);
            SetProcess(true);
        }

        public override void _Process(float delta){
            setTo.x = this.Position.x + moveTo.x * (float)BulletVelocity;
            setTo.y = this.Position.y + moveTo.y * (float)BulletVelocity;
            this.Position = setTo;

            if(isTimerExpired && isNotVisible){
                // No Programmer-Chan, you're getting this all wrong! We dont **kill** objects, we ***FREE*** them!
                this.QueueFree();
            }
        }

        public void _on_BaseBullet_body_entered(Godot.Node body){
            // TODO: Sound effect
            if(body.HasMethod("takeDamage")){
                // this.audio.Play("HitBody:);
                body.Call("takeDamage", (float)this.BulletDamage);
            } 
            else{
                // this.audio.Play("HitOther:);
            }
        }

        public void _on_AudioStreamPlayer2D_finished(){
            this.QueueFree();
        }

        public void _on_VisibilityNotifier2D_screen_exited(){
            this.isNotVisible = true;
        }

        public void _on_VisibilityNotifier2D_screen_entered(){
            this.isNotVisible = false;
        }

        public void _on_Timer_timeout(){
            this.isTimerExpired = true;
        }
    }
}