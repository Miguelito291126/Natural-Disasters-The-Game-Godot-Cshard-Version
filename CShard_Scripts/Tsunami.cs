using Godot;
using Godot.Collections;

[GlobalClass]
public partial class Tsunami : Area3D
{
	public Node tsunami;
	[Export] public int Speed = 100;
	[Export] public int TsunamiStrength = 100;
	[Export] public Vector3 Direction = new Vector3(0, 0, 1);
	[Export] public double DistanceTraveled = 0.0;
	[Export] public double TotalDistance = 4097.0;

	// Adjust this value based on your scene
	public override void _PhysicsProcess(double delta)
	{
		GlobalPosition += Direction * Speed * delta;

		foreach(Node3D body in GetOverlappingBodies())
		{
			if(body.IsInGroup("movable_objects") && body.IsClass("RigidBody3D"))
			{
				var force = Direction.Normalized() * TsunamiStrength * delta;
				body.ApplyCentralImpulse(force);
				body.Freeze = false;
			}
			else if(body.IsInGroup("player"))
			{
				body.Velocity = Direction * Speed * 100 * delta;
			}
		}
	}

	public override void _Ready()
	{
		tsunami = GetNode("tsunami");
	}
}