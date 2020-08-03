using System;
using Godot;

namespace rz_frzbn.Weapons.Mage.sheild{
    // NOTE: KinematicBody acts like a StaticBody if we dont call any `move_and_collide()`
    public class sheild : KinematicBody2D{
        [Export]
        protected float sheildHealthPoints = 50.0F;
        [Export]
        protected int sheildLifeTimeSeconds = 25;
        
        
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

        }

        public void _on_AnimatedSprite_animation_finished(){

        }

    }
}