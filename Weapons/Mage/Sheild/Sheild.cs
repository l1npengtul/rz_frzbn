using System;
using Godot;

namespace rz_frzbn.Weapons.Mage.shelid{
    // NOTE: KinematicBody acts like a StaticBody if we dont call any `move_and_collide()`
    public class Sheild : KinematicBody2D{
        [Export]
        protected float SheildHealthPoints = 50.0F;
        [Export]
        protected int SheildLifeTimeSeconds = 25;
        [Export]
        protected float SheildKnockback = 0.5F;
        
        
        protected AudioStreamPlayer2D Audio;
        protected AnimatedSprite Sprite;
        protected Timer Timer;

        protected bool ToBreakAnim = false, ToBreakAudio = false;

        public override void _Ready(){
            Audio = GetNode<AudioStreamPlayer2D>("AudioStreamPlayer2D");
            Sprite = GetNode<AnimatedSprite>("AnimatedSprite");
            Timer = GetNode<Timer>("Timer");

        }

        public void _on_Timer_timeout(){
            Sprite.Play("BREAK");
            QueueFree();
        }

        public void _on_AudioStreamPlayer2D_finished(){
            ToBreakAudio = true;
        }

        public void _on_AnimatedSprite_animation_finished(){
            ToBreakAnim = true;
        }

        public override void _PhysicsProcess(float delta){
            if (ToBreakAnim && ToBreakAudio){
                QueueFree();
            } 
        }

        private void BreakSheild(){
            Sprite.Play("BREAK");
            QueueFree();
        }

        public void _on_Area2D_body_entered(Godot.Node body){
            if(body.HasMethod("TakeDamage") && body != this){ // Check if the body we are colliding into is not ourselves
                    // do math angle shit
                
                if (body.HasMethod("TakeDamageWithKB")){
                    body.Call("TakeDamageWithKB", this.SheildHealthPoints, this.GlobalPosition);
                }
                else {
                    body.Call("TakeDamage", this.SheildHealthPoints);
                }
                this.TakeDamage(this.SheildHealthPoints);
            }
        }

        public void _on_Area2D_body_exited(Godot.Node body){
            //
        }

        public void TakeDamage(float damage){
            this.SheildHealthPoints -= damage;
            if (this.SheildHealthPoints <= 0){
                BreakSheild();
            }
        }

        public void SetSheild(Vector2 pos, float rot){
            this.GlobalPosition = pos;
            this.GlobalRotation = rot;
        }
    }
}