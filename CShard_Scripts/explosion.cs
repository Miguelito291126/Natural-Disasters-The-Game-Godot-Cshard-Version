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

}