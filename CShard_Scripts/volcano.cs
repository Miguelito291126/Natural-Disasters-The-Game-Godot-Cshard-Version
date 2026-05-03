using System.ComponentModel.DataAnnotations;
using Godot;
using Godot.Collections;


// Variables para configurar el lanzamiento de bolas de fuego
[GlobalClass]
public partial class Volcano : Node3D
{
	public PackedScene FireballScene = ResourceLoader.Load<PackedScene>("res://Scenes/meteor.tscn");
	// Escena de la bola de fuego
	public PackedScene EarthquakeScene = ResourceLoader.Load<PackedScene>("res://Scenes/earthquake.tscn");


	// Intervalo de lanzamiento en segundos
	[Export] [Range(0, 10000)] public int LaunchForce = 1100;
	// Fuerza de lanzamiento de la bola de fuego
	[Export] public int LaunchAmount = 20;

	[Export] public int LavaLevel = 125;
	[Export] public int Pressure = 0;
	[Export] public bool IsGoingToErupt = false;
	[Export] public bool IsPressureLeaking = false;
	[Export] public bool IsVolcanoAsh = false;

	public Node3D volcano;
	public Node3D VolcanoArea;

	public GpuParticles3D Smoke;
	public GpuParticles3D EruptSparks;
	public GpuParticles3D EruptSmoke;
	public AudioStreamPlayer3D EruptSound;
	public Marker3D LaunchMarker;

	public override void _Ready()
	{
		volcano = GetNode<Node3D>("Volcano");
		VolcanoArea = GetNode<Node3D>("Volcano_Area");
		Smoke = GetNode<GpuParticles3D>("Smoke");
		EruptSparks = GetNode<GpuParticles3D>("Erupt Sparks");
		EruptSmoke = GetNode<GpuParticles3D>("Erupt Smoke");
		EruptSound = GetNode<AudioStreamPlayer3D>("Erupt Sound");
		LaunchMarker = GetNode<Marker3D>("launch_marker");
	}

	public async void CheckPressure()
	{

		// Verifica si la presin del volcn es mayor o igual a 100
		if(Pressure >= 100)
		{

			// Verifica si el volc�n no est� en proceso de erupci�n
			if(!IsGoingToErupt)
			{

				// Establece que el volc�n est� en proceso de erupci�n
				IsGoingToErupt = true;

				// 1. Declaramos la variable fuera para poder usarla después en el QueueFree
				Node3D earthquakeNode = null; 

				if(GD.Randi() % 3 == 0)
				{
					// 2. Instanciamos (ajusta 'Earthquake' al nombre real de tu clase de terremoto)
					var earthquakeInstance = EarthquakeScene.Instantiate<Node3D>(); 
					GetParent().AddChild(earthquakeInstance);
					
					// 3. CORRECCIÓN: Asignar la posición completa
					earthquakeInstance.GlobalPosition = GlobalPosition; 
					
					earthquakeNode = earthquakeInstance;
				}

				// ... esperar tiempo ...

				if(IsInstanceValid(earthquakeNode))
				{
					earthquakeNode.QueueFree();
				}


				// Llama a la funcin Erupt despus de un tiempo aleatorio entre 10 y 20 segundos
				await ToSignal(GetTree().CreateTimer(GD.RandRange(10, 20)), SceneTreeTimer.SignalName.Timeout);
				if(IsInstanceValid(this))
				{
					Erupt();
					Pressure = 99;
					IsGoingToErupt = false;
					IsPressureLeaking = true;
				}

				await ToSignal(GetTree().CreateTimer(GD.RandRange(10, 20)), SceneTreeTimer.SignalName.Timeout);

				if(GodotObject.IsInstanceValid(earthquakeNode))
				{
					earthquakeNode.QueueFree();
				}
			}
		}
	}


	public async void Erupt()
	{
		Smoke.Emitting = false;
		EruptSparks.Emitting = true;
		EruptSmoke.Emitting = true;
		EruptSound.Play();
		_LaunchFireball(LaunchAmount);

		await ToSignal(GetTree().CreateTimer(10), SceneTreeTimer.SignalName.Timeout);

		IsVolcanoAsh = true;

		Smoke.Emitting = true;

		if(IsVolcanoAsh)
		{
			Globals.Instance.Rpc(Globals.MethodName.SetWeatherAndDisaster, "Dust Storm", -1);
		}
	}

	public override void _Process(double _delta)
	{
		CheckPressure();
	}

	protected void _LaunchFireball(int range)
	{
		for(int i = 0; i < range; i++)
		{
			Meteors fireball = FireballScene.Instantiate<Meteors>();
			Vector3 spawnPos = LaunchMarker.GlobalPosition;

			Vector3 baseDirection = LaunchMarker.GlobalTransform.Basis.Y;
			Vector3 spread = new Vector3(
				(float)GD.RandRange(-0.4, 0.4),
				(float)GD.RandRange(-0.1, 0.2), // Variación en altura
				(float)GD.RandRange(-0.4, 0.4)
			);

			Vector3 finalDirection = (baseDirection + spread).Normalized();

			LaunchForce = GD.RandRange(2100, 5500);

			GetParent().AddChild(fireball, true);
			fireball.GlobalPosition = spawnPos;
			fireball.Scale = new Vector3(1, 1, 1);
			fireball.IsVolcanoRock = true;
			fireball.ApplyImpulse(finalDirection * LaunchForce);
		}
	}


}