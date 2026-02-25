using Godot;
using Godot.Collections;

[GlobalClass]
public partial class Thunder : Node3D
{
	public PackedScene ExplosionScene = ResourceLoader.Load<PackedScene>("res://Scenes/thunder_explosion.tscn");
	public GpuParticles3D Spark;
	public GpuParticles3D Light;
	public GpuParticles3D Star;


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Spark = GetNode<GpuParticles3D>("spark");
		Light = GetNode<GpuParticles3D>("light");
		Star = GetNode<GpuParticles3D>("star");
		
		Spark.Emitting = true;
		Light.Emitting = true;
		Star.Emitting = true;

		// Configurar la posici�n de la explosi�n en la posici�n del suelo
		ThunderExplosion explosion = ExplosionScene.Instantiate<ThunderExplosion>();
		explosion.Position = this.Position;
		GetParent().AddChild(explosion);


	}


	protected void _OnSparkFinished()
	{
		this.QueueFree();
	}

}