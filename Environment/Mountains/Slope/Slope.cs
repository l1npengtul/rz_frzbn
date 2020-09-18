using Godot;
using System;

// This script controls the Area 2D that controls the slope. It expects a CollisionShape/Polygon under it and autmatically attaches signals to itself

public class Slope : Area2D {   
    private enum SlopeType {
        NORTH,
        EAST,
        SOUTH,
        WEST
    }
    

    private float xSpeed = 0.0F;
    private float ySpeed = 0.0F;
    

    [Export]
    private float SpeedMultiplier = 0.0F;
    [Export]
    SlopeType SlopeDir = SlopeType.NORTH;


    public override void _Ready(){
        Area2D area = GetNode<Area2D>(".");
        area.Connect("body_entered", this, nameof(_on_Area2D_body_entered));
        area.Connect("body_exited", this, nameof(_on_Area2D_body_exited));

        if (SpeedMultiplier <= 0){
            throw new ArgumentOutOfRangeException("SpeedMultiplier",SpeedMultiplier,"Must be greater than 0!");
        }

        switch (SlopeDir){
            case SlopeType.SOUTH:
                xSpeed = 0.0F;
                ySpeed = -1.0F * SpeedMultiplier;
                break;
            case SlopeType.WEST:
                xSpeed = 1.0F * SpeedMultiplier;
                ySpeed = 0.0F;
                break;
            case SlopeType.NORTH:
                xSpeed = 0.0F;
                ySpeed = 1.0F * SpeedMultiplier;
                break;
            case SlopeType.EAST:
                xSpeed = -1.0F * SpeedMultiplier;
                ySpeed = 0.0F;
                break;
        }
    }

    public void _on_Area2D_body_entered(Node body){
        GD.Print("a");
        if (body.HasMethod("EnterSlope")){
            body.Call("EnterSlope",xSpeed, ySpeed);
        }
    }

    public void _on_Area2D_body_exited(Node body){
        GD.Print("b");
        if (body.HasMethod("ExitSlope")){
            body.Call("ExitSlope");
        }
    }
}
