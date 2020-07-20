using Godot;
using System;

public class Slope : Area2D
{   
    private enum SLOPTYP {
        START, // Top of slope
        DEADZONE, // Bottom
        SMALLSLOPE, // Small Accel
        MEDUIUMSLOPE, // Medium
        LARGESLOPE, // Large
    }
    [Export]
    private float xSpeed = 0.0F;
    [Export]
    private float ySpeed = 0.0F;
    [Export]
    private int slopeDirection = 0;
    //private Vector2 slopeSpeedMultiplier = new Vector2(1.0F,1.0F);

    public override void _Ready()
    {

    }

    public void _on_Area2D_body_entered(Node body){
        GD.Print("a");
        if (body.HasMethod("enterSlope")){
            body.Call("enterSlope",xSpeed, ySpeed,slopeDirection);
        }
    }

    public void _on_Area2D_body_exited(Node body){
        GD.Print("b");
        if (body.HasMethod("exitSlope")){
            body.Call("exitSlope");
        }
    }

}
