using Godot;
using Godot.Collections;


// Variables para configurar el lanzamiento de bolas de fuego
[GlobalClass]
public partial class Volcano : Node3D
{
	public Resource FireballScene = /* preload has no equivalent, add a 'ResourcePreloader' Node in your scene */("res://Scenes/meteor.tscn");
	// Escena de la bola de fuego
	public Resource EarthquakeScene = /* preload has no equivalent, add a 'ResourcePreloader' Node in your scene */("res://Scenes/earthquake.tscn");


	[Export] public int LaunchInterval = 5;
	// Intervalo de lanzamiento en segundos
	[Export] public int LaunchForce = 50000;
	// Fuerza de lanzamiento de la bola de fuego
	[Export] public int LaunchAmount = 20;

	// Fuerza de lanzamiento de la bola de fuego
	public Vector3 LaunchPosition;

	[Export] public int LavaLevel = 125;
	[Export] public int Pressure = 0;
	[Export] public bool IsGoingToErupt = false;
	[Export] public bool IsPressureLeaking = false;
	[Export] public bool IsVolcanoAsh = false;

	public Node volcano;
	public Node VolcanoArea;

	public Node Smoke;
	public Node EruptSparks;
	public Node EruptSmoke;
	public Node EruptSound;
	public Marker3D LaunchMarker;

	public override void _Ready()
	{
		volcano = GetNode("Volcano");
		VolcanoArea = GetNode("Volcano_Area");
		Smoke = GetNode("Smoke");
		EruptSparks = GetNode("Erupt Sparks");
		EruptSmoke = GetNode("Erupt Smoke");
		EruptSound = GetNode("Erupt Sound");
		LaunchMarker = GetNode("launch_marker");
		LaunchPosition = LaunchMarker.GlobalPosition;
	}

	public void CheckPressure()
	{

		// Verifica si la presi�n del volc�n es mayor o igual a 100
		if(Pressure >= 100)
		{

			// Verifica si el volc�n no est� en proceso de erupci�n
			if(!IsGoingToErupt)
			{

				// Establece que el volc�n est� en proceso de erupci�n
				IsGoingToErupt = true;


				var earthquake;


				// Si un n�mero aleatorio entre 1 y 3 es igual a 3
				if(GD.Randi() % 3 == 0)
				{

					// Crea una instancia del objeto que representa el terremoto
					earthquake = EarthquakeScene.Instantiate();
					GetParent().AddChild(earthquake);
					earthquake.GlobalTransform.Origin = GlobalTransform.Origin;
				}


				// Llama a la funci�n Erupt despu�s de un tiempo aleatorio entre 10 y 20 segundos
				await ToSignal(GetTree().CreateTimer(GD.RandRange(10, 20)), "Timeout");
				if(GodotObject.IsInstanceValid(this))
				{
					Erupt();
					Pressure = 99;
					IsGoingToErupt = false;
					IsPressureLeaking = true;
				}

				await ToSignal(GetTree().CreateTimer(GD.RandRange(10, 20)), "Timeout");

				if(GodotObject.IsInstanceValid(earthquake))
				{
					earthquake.QueueFree();
				}
			}
		}
	}


	public void Erupt()
	{
		Smoke.Emitting = false;
		EruptSparks.Emitting = true;
		EruptSmoke.Emitting = true;
		EruptSound.Play();
		_LaunchFireball(LaunchAmount, LaunchInterval);

		await ToSignal(this, "Await");
		GetTree().CreateTimer(10).Timeout;

		IsVolcanoAsh = true;

		Smoke.Emitting = true;

		Globals.TemperatureTarget = GD.RandRange(30, 40);
		Globals.HumidityTarget = GD.RandRange(0, 10);
		Globals.BradiationTarget = 0;
		Globals.OxygenTarget = 0;
		Globals.PressureTarget = GD.RandRange(10000, 10020);
		Globals.WindDirectionTarget = new Vector3(GD.RandRange( - 1, 1), 0, GD.RandRange( - 1, 1));
		Globals.WindSpeedTarget = GD.RandRange(0, 50);

		if(IsVolcanoAsh)
		{
			Globals.SetWeatherAndDisaster.Rpc("Dust Storm");
		}
	}

	public override void _Process(double _delta)
	{
		CheckPressure();
	}

	protected void _LaunchFireball(int range, int time)
	{
		foreach(int i in range)
		{
			var fireball = FireballScene.Instantiate();
			var launch_direction = new Vector3(GD.RandRange( - 1, 1), 1, GD.RandRange( - 1, 1)).Normalized();
			// Direcci�n hacia arriba
			GetParent().AddChild(fireball, true);
			// Agregar la bola de fuego como hijo del volc�n primero
			fireball.GlobalPosition = LaunchPosition;
			// Posici�n inicial en el volc�n
			fireball.Scale = new Vector3(1, 1, 1);
			fireball.IsVolcanoRock = true;
			fireball.ApplyImpulse(launch_direction * LaunchForce, Vector3.Up);
			// Aplicar fuerza para lanzar la bola de fuego
			await ToSignal(GetTree().CreateTimer(time), "Timeout");
		}
	}


}