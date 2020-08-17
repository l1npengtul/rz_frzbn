using System;
using Godot;

namespace rz_frzbn.Weapons.Mage.sheild{
    // NOTE: KinematicBody acts like a StaticBody if we dont call any `move_and_collide()`
    public class sheild : KinematicBody2D{
        [Export]
        protected float sheildHealthPoints = 50.0F;
        [Export]
        protected int sheildLifeTimeSeconds = 25;
        [Export]
        protected float sheildKnockback = 0.5F;
        
        
        protected AudioStreamPlayer2D audio;
        protected AnimatedSprite sprite;
        protected Timer timer;

        protected bool toBreak_anim = false, toBreak_audio = false;

        public override void _Ready(){
            audio = GetNode<AudioStreamPlayer2D>("AudioStreamPlayer2D");
            sprite = GetNode<AnimatedSprite>("AnimatedSprite");
            timer = GetNode<Timer>("Timer");

        }

        public void _on_Timer_timeout(){
            sprite.Play("BREAK");
            audio.Play();
        }

        public void _on_AudioStreamPlayer2D_finished(){
            toBreak_audio = true;
        }

        public void _on_AnimatedSprite_animation_finished(){
            toBreak_anim = true;
        }

        public override void _PhysicsProcess(float delta){
            if (toBreak_anim && toBreak_audio){
                QueueFree();
            } 
        }

        public void _on_Area2D_body_entered(Godot.Node body){
            if(body.HasMethod("takeKnockback") && body.HasMethod("takeDamage")){
                if(body.IsInGroup("Enemy")){
                    // do math angle shit
                    body.Call("takeKnockback", sheildHealthPoints*1.5F);
                    body.Call("takeDamage", this.sheildHealthPoints);
                    _on_Timer_timeout(); // FIXME: Dirty Hack to call a signal function when we arn't calling a signal. 
                }
            }
        }

        public void _on_Area2D_body_exited(Godot.Node body){
            //
        }

        public void takeDamage(float damage){
            this.sheildHealthPoints -= damage;
            if (this.sheildHealthPoints <= 0){
                _on_Timer_timeout(); // FIXME: Dirty Hack to call a signal function when we arn't calling a signal. 
            }

        }
    }
}