using Godot;
using System;

public class Slope : Area2D
{   

    [Export]
    private int slopeDirection = 0; // NOTE: only N, E, S, W supported

    private enum SLOPTYP {
        START, // Top of slope
        DEADZONE, // Bottom
        SMALLSLOPE, // Small Accel
        MEDUIUMSLOPE, // Medium
        LARGESLOPE, // Large
    }
    [Export]
    private SLOPTYP slope = SLOPTYP.DEADZONE;

    private float xSpeed = 1.0F;
    private float ySpeed = 1.0F;
    private Vector2 slopeSpeedMultiplier = new Vector2(1.0F,1.0F);

    public override void _Ready()
    {

    }

    public void _on_Area2D_body_entered(Node body){
        if (body.HasMethod("enterSlope")){
            body.Call("enterSlope", slopeDirection, slope);
        }
    }

    public void _on_Area2D_body_exited(Node body){
        if (body.HasMethod("exitSlope")){
            body.exitSlope();
        }
    }

}
