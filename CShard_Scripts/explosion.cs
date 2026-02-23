using Godot;
using Godot.Collections;

[GlobalClass]
public partial class Explosion : Node3D
{
	public int ExplosionForce = 100;
	public int ExplosionDamage = 100;
	public float ExplosionRadius;
	public GpuParticles3D Smoke;
	public GpuParticles3D SmokeShockwaveExplosion;
	public GpuParticles3D Sparks;
	public GpuParticles3D SparksShock;


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		ExplosionRadius = ((SphereShape3D)GetNode<CollisionShape3D>("Area3D/CollisionShape3D").Shape).Radius;
		Smoke = GetNode<GpuParticles3D>("Smoke");
		SmokeShockwaveExplosion = GetNode<GpuParticles3D>("Smoke shock");
		Sparks = GetNode<GpuParticles3D>("Sparks");
		SparksShock = GetNode<GpuParticles3D>("Sparks shock");
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
		if(body is RigidBody3D rigidBody3D)
		{
			float distance = (GlobalPosition - body.GlobalPosition).Length();

			// Calcular direccin desde la explosin hacia el objeto
			Vector3 direction = (body.GlobalPosition - GlobalPosition).Normalized();


			// Calcular fuerza basada en la distancia (m�s cerca = m�s fuerza)
			float force_multiplier = 1.0f - Mathf.Clamp(distance / ExplosionRadius, 0.0f, 1.0f);
			float force = ExplosionForce * force_multiplier;


			// Aplicar impulso al RigidBody3D
			rigidBody3D.ApplyImpulse(direction * force, Vector3.Zero);
		}
	}


}