using Godot;
using Godot.Collections;

[GlobalClass]
public partial class Tornado : Area3D
{
	[Export] public int MovementSpeed = 10;
	[Export] public int MovementRadius = 50;

	[Export] public int RayLength = 1000;
	[Export] public int GroundHeight = 0;

	[Export] public int TornadoStrength = 100;
	[Export] public int Radius = 10;


	public Node RayCast;

	public override void _Ready()
	{
		RayCast = GetNode("RayCast");
		RayCast.TargetPosition = new Vector3(0,  - RayLength, 0);
		RayCast.ForceRaycastUpdate();
		SetProcess(true);
	}

	public override void _Process(double delta)
	{
		if(RayCast.IsColliding())
		{
			GroundHeight = RayCast.GetCollisionPoint().Y;
			GlobalPosition.Y = GroundHeight;


			// Mantener el tornado a la altura del suelo

		}// Genera una nueva posici�n aleatoria dentro del radio de movimiento


		var new_position = new Vector3(GD.RandRange( - MovementRadius, MovementRadius), 0, GD.RandRange( - MovementRadius, MovementRadius));


		// Aplica movimiento hacia la nueva posici�n
		var direction = (new_position - GlobalPosition).Normalized();
		Translate(direction * MovementSpeed * delta);
	}


	public override void _PhysicsProcess(double _delta)
	{
		foreach(Node3D body in GetOverlappingBodies())
		{
			if(body.IsInGroup("movable_objects") && body.IsClass("RigidBody3D"))
			{
				var direction = (body.GlobalPosition - GlobalPosition).Normalized();
				var perpendicular_direction = new Vector3( - direction.Z, 0, direction.X);
				// Direcci�n perpendicular al vector hacia el tornado
				var force = perpendicular_direction * TornadoStrength;
				body.ApplyCentralImpulse(force);
				body.Freeze = false;
			}
			else if(body.IsInGroup("player"))
			{
				var direction = (body.GlobalPosition - GlobalPosition).Normalized();
				var perpendicular_direction = new Vector3( - direction.Z, 0, direction.X);
				// Direcci�n perpendicular al vector hacia el tornado
				var force = perpendicular_direction * TornadoStrength;
				body.Velocity = force;
			}
		}
	}


}