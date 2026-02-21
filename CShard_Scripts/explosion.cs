using Godot;
using Godot.Collections;

[GlobalClass]
public partial class Explosion : Node3D
{
	public int ExplosionForce = 100;
	public int ExplosionDamage = 100;
	public Variant ExplosionRadius;
	public Node Smoke;
	public Node SmokeShockwaveExplosion;
	public Node Sparks;
	public Node SparksShock;


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		ExplosionRadius = GetNode<Area3D>("Area3D/CollisionShape3D").Shape.Radius;
		Smoke = GetNode("Smoke");
		SmokeShockwaveExplosion = GetNode("Smoke shock");
		Sparks = GetNode("Sparks");
		SparksShock = GetNode("Sparks shock");
		Sparks.Emitting = true;
		SmokeShockwaveExplosion.Emitting = true;
		Smoke.Emitting = true;
		SparksShock.Emitting = true;
	}


	protected void _OnFinished()
	{
		this.QueueFree();
	}


	protected void _OnArea3dBodyEntered(Node3D body)
	{

		// Aplicar fuerza de explosi�n a objetos RigidBody3D
		if(body is RigidBody3D)
		{
			var distance = (GlobalPosition - body.GlobalPosition).Length();

			// Calcular direcci�n desde la explosi�n hacia el objeto
			var direction = (body.GlobalPosition - GlobalPosition).Normalized();


			// Calcular fuerza basada en la distancia (m�s cerca = m�s fuerza)
			var force_multiplier = 1.0 - Mathf.Clamp(distance / ExplosionRadius, 0.0, 1.0);
			var force = ExplosionForce * force_multiplier;


			// Aplicar impulso al RigidBody3D
			body.ApplyImpulse(direction * force, Vector3.Zero);
		}
	}


}