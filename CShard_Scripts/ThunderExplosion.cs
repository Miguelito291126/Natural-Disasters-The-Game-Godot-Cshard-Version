using Godot;
using Godot.Collections;

[GlobalClass]
public partial class ThunderExplosion : Node3D
{
	public int ExplosionForce = 100;
	public int ExplosionDamage = 100;
	public float ExplosionRadius;
	public GpuParticles3D Parks;

	public Array<AudioStream> Lol = new Array<AudioStream>() 
    {
        ResourceLoader.Load<AudioStream>("res://Sounds/disasters/nature/closethunder01.mp3"),
        ResourceLoader.Load<AudioStream>("res://Sounds/disasters/nature/closethunder02.mp3"),
        ResourceLoader.Load<AudioStream>("res://Sounds/disasters/nature/closethunder03.mp3"),
        ResourceLoader.Load<AudioStream>("res://Sounds/disasters/nature/closethunder04.mp3"),
        ResourceLoader.Load<AudioStream>("res://Sounds/disasters/nature/closethunder05.mp3")
    };

	public AudioStreamPlayer3D AudioPlayer;


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		ExplosionRadius = ((SphereShape3D)GetNode<CollisionShape3D>("Area3D/CollisionShape3D").Shape).Radius;
		GpuParticles3D Parks = GetNode<GpuParticles3D>("Parks");
		AudioStreamPlayer3D AudioPlayer = GetNode<AudioStreamPlayer3D>("AudioStreamPlayer3D");
		Parks.Emitting = true;


		// Configurar el sonido del trueno
		if (Lol.Count > 0)
        {
            AudioPlayer.Stream = Lol[GD.RandRange(0, Lol.Count - 1)];
            AudioPlayer.Play();
        }
	}


	protected void _OnFinished()
	{
		this.QueueFree();
	}

	private void _OnArea3DBodyEntered(Node3D body)
	{
		// Aplicar fuerza de explosión a objetos RigidBody3D
		if (body is RigidBody3D rigidBody)
		{
			float distance = GlobalPosition.DistanceTo(rigidBody.GlobalPosition);
			
			// Calcular dirección desde la explosión hacia el objeto
			// Usamos Normalized() para obtener el vector de dirección
			Vector3 direction = (rigidBody.GlobalPosition - GlobalPosition).Normalized();

			// Calcular fuerza basada en la distancia (más cerca = más fuerza)
			// Usamos Mathf.Clamp para asegurar que el valor esté entre 0 y 1
			float forceMultiplier = 1.0f - Mathf.Clamp(distance / ExplosionRadius, 0.0f, 1.0f);
			float force = ExplosionForce * forceMultiplier;

			// Aplicar impulso al RigidBody3D
			// El segundo parámetro es la posición relativa (offset), Vector3.Zero aplica al centro
			rigidBody.ApplyImpulse(direction * force, Vector3.Zero);
		}
	}

}