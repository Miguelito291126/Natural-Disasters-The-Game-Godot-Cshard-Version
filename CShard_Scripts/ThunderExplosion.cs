using Godot;
using Godot.Collections;

[GlobalClass]
public partial class ThunderExplosion : Node3D
{
	public int ExplosionForce = 100;
	public int ExplosionDamage = 100;
	public float ExplosionRadius;
	public GpuParticles3D Parks;

	public Array<PackedScene> Lol = new Array<PackedScene>() {ResourceLoader.Load<PackedScene>("res://Sounds/disasters/nature/closethunder01.mp3"), ResourceLoader.Load<PackedScene>("res://Sounds/disasters/nature/closethunder02.mp3"), ResourceLoader.Load<PackedScene>("res://Sounds/disasters/nature/closethunder03.mp3"), ResourceLoader.Load<PackedScene>("res://Sounds/disasters/nature/closethunder04.mp3"), ResourceLoader.Load<PackedScene>("res://Sounds/disasters/nature/closethunder05.mp3")};
	public AudioStreamPlayer3D AudioPlayer;


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		float ExplosionRadius = ((SphereShape3D)GetNode<CollisionShape3D>("Area3D/CollisionShape3D").Shape).Radius;
		GpuParticles3D Parks = GetNode<GpuParticles3D>("Parks");
		AudioStreamPlayer3D AudioPlayer = GetNode<AudioStreamPlayer3D>("AudioStreamPlayer3D");
		Parks.Emitting = true;


		// Configurar el sonido del trueno
		AudioPlayer.Stream = Lol[GD.RandRange(0, Lol.Count - 1)].Instantiate<AudioStream>();
		AudioPlayer.Play();
	}


	protected void _OnFinished()
	{
		this.QueueFree();
	}


}