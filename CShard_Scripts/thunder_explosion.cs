using Godot;
using Godot.Collections;

[GlobalClass]
public partial class ThunderExplosion : Node3D
{
	public int ExplosionForce = 100;
	public int ExplosionDamage = 100;
	public Variant ExplosionRadius;
	public Node Parks;

	public Array Lol = new() {Load("res://Sounds/disasters/nature/closethunder01.mp3"), Load("res://Sounds/disasters/nature/closethunder02.mp3"), Load("res://Sounds/disasters/nature/closethunder03.mp3"), Load("res://Sounds/disasters/nature/closethunder04.mp3"), Load("res://Sounds/disasters/nature/closethunder05.mp3")};
	public Node AudioPlayer;


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		ExplosionRadius = GetNode<Area3D>("Area3D/CollisionShape3D").Shape.Radius;
		Parks = GetNode("Parks");
		AudioPlayer = GetNode("AudioStreamPlayer3D");
		Parks.Emitting = true;


		// Configurar el sonido del trueno
		AudioPlayer.Stream = Lol[GD.RandRange(0, Lol.Size() - 1)];
		AudioPlayer.Play();
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