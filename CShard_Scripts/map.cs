using System.Linq;
using Godot;
using Godot.Collections;

[GlobalClass]
public partial class Map : Node3D
{
	public MapEnvironment Worldenvironment;
	[Export] public PackedScene SnowDecalScene;
	[Export] public PackedScene SandDecalScene;
	


	public string CurrentDisaster = "";
	public Array<Node3D> ActiveDisasterNodes = new Array<Node3D>();
	public Array<Node3D> ActiveDecals = new Array<Node3D>();
	public bool IsSpawningLightning = false;


	public override void _ExitTree()
	{
		if(Multiplayer.IsServer())
		{
			Globals.Instance.Rpc(nameof(Globals.Instance.SetWeatherAndDisaster), "Original", -1);
			Globals.Instance.Timer.Stop();
			Globals.Instance.Started = false;
		}
	}

	public override void _Ready()
	{
		Worldenvironment = GetNode<MapEnvironment>("WorldEnvironment");
		Globals.Instance.Map = this;
		
		if (!Globals.Instance.IsConnected(nameof(Globals.CurrentWeatherAndDisasterChanged), Callable.From((System.Action<string>)_OnDisasterChanged)))
		{
			Globals.Instance.CurrentWeatherAndDisasterChanged += _OnDisasterChanged;
		}



		if(Multiplayer.IsServer())
		{
			Globals.Instance.Rpc(nameof(Globals.Instance.SetWeatherAndDisaster), "Original", -1);

			if(Globals.Instance.Gamemode == "survival")
			{
				if(!OS.HasFeature("dedicated_server"))
				{
					Globals.Instance.MultiplayerPlayerSpawner();
				}

				foreach(int i in Multiplayer.GetPeers())
				{
					Globals.Instance.MultiplayerPlayerSpawner(i);
				}

				Globals.Instance.Timer.WaitTime = Globals.Instance.GlobalsData.TimerDisasters;
				Globals.Instance.Timer.Start();
			}

			else
			{
				if(!OS.HasFeature("dedicated_server"))
				{
					Globals.Instance.MultiplayerPlayerSpawner();
				}

				foreach(int i in Multiplayer.GetPeers())
				{
					Globals.Instance.MultiplayerPlayerSpawner(i);
				}
			}
		}
	}


	// Llama a la función wind para cada objeto en la escena
	public override void _PhysicsProcess(double _delta)
	{
		foreach (var child in GetChildren())
		{
			if (child is Node3D node3D)
			{
				Globals.Instance.Wind(node3D);
			}
		}
	}


	public override void _Process(double _delta)
	{
		if(Multiplayer.IsServer())
		{
			if(OS.HasFeature("dedicated_server") || OS.GetCmdlineUserArgs() != null || OS.GetCmdlineUserArgs().Contains("server"))
			{
				Globals.Instance.Started = true;
			}
			else
			{
				if(Multiplayer.MultiplayerPeer == null || Multiplayer.MultiplayerPeer is OfflineMultiplayerPeer)
				{
					Globals.Instance.Started = true;
					return;
				}

				if(Globals.Instance.PlayersConected.Count > 1)
				{
					Globals.Instance.Started = true;
				}
				else
				{
					Globals.Instance.Started = false;
				}
			}
		}
	}

	protected void _StartSunOriginal()
		{
			Globals.Instance.TemperatureTarget = Globals.Instance.TemperatureOriginal;
			Globals.Instance.HumidityTarget = Globals.Instance.HumidityOriginal;
			Globals.Instance.BradiationTarget = Globals.Instance.BradiationOriginal;
			Globals.Instance.OxygenTarget = Globals.Instance.OxygenOriginal;
			Globals.Instance.PressureTarget = Globals.Instance.PressureOriginal;
			Globals.Instance.WindDirectionTarget = Globals.Instance.WindDirectionOriginal;
			Globals.Instance.WindSpeedTarget = Globals.Instance.WindSpeedOriginal;

			_UpdateEnvironment();
		}


		protected void _StartTsunami()
		{
			Tsunami tsunami = Globals.Instance.TsunamiScene.Instantiate<Tsunami>();
			tsunami.Position = new Vector3(0, 0, 0);
			AddChild(tsunami, true);
			ActiveDisasterNodes.Add(tsunami);

			Globals.Instance.TemperatureTarget = GD.RandRange(20, 31);
			Globals.Instance.HumidityTarget = GD.RandRange(0, 20);
			Globals.Instance.BradiationTarget = 0;
			Globals.Instance.OxygenTarget = 100;
			Globals.Instance.PressureTarget = GD.RandRange(10000, 10020);
			Globals.Instance.WindDirectionTarget = new Vector3(GD.RandRange( - 1, 1), 0, GD.RandRange( - 1, 1));
			Globals.Instance.WindSpeedTarget = GD.RandRange(0, 10);

			_UpdateEnvironment();
		}


		protected void _StartThunderstorm()
		{

			Globals.Instance.TemperatureTarget = GD.RandRange(5, 15);
			Globals.Instance.HumidityTarget = GD.RandRange(30, 40);
			Globals.Instance.BradiationTarget = 0;
			Globals.Instance.OxygenTarget = 100;
			Globals.Instance.PressureTarget = GD.RandRange(8000, 9000);
			Globals.Instance.WindDirectionTarget = new Vector3(GD.RandRange( - 1, 1), 0, GD.RandRange( - 1, 1));
			Globals.Instance.WindSpeedTarget = GD.RandRange(0, 30);

			_UpdateEnvironment();
			_SpawnLightningTimer();
		}


		protected void _StartMeteorShower()
		{
			Globals.Instance.TemperatureTarget = GD.RandRange(20, 31);
			Globals.Instance.HumidityTarget = GD.RandRange(0, 20);
			Globals.Instance.PressureTarget = GD.RandRange(10000, 10020);
			Globals.Instance.BradiationTarget = 0;
			Globals.Instance.OxygenTarget = 100;
			Globals.Instance.WindDirectionTarget = new Vector3(GD.RandRange( - 1, 1), 0, GD.RandRange( - 1, 1));
			Globals.Instance.WindSpeedTarget = GD.RandRange(0, 10);

			_SpawnMeteorShowerTimer();
			_UpdateEnvironment();
		}

		protected void _StartBlizzard()
		{
			Globals.Instance.TemperatureTarget = GD.RandRange( - 20,  - 35);
			Globals.Instance.HumidityTarget = GD.RandRange(20, 30);
			Globals.Instance.BradiationTarget = 0;
			Globals.Instance.OxygenTarget = 100;
			Globals.Instance.PressureTarget = GD.RandRange(8000, 9020);
			Globals.Instance.WindDirectionTarget = new Vector3(GD.RandRange( - 1, 1), 0, GD.RandRange( - 1, 1));
			Globals.Instance.WindSpeedTarget = GD.RandRange(40, 50);


			_UpdateEnvironment();
		}


		protected void _StartSandstorm()
		{
			Globals.Instance.TemperatureTarget = GD.RandRange(30, 35);
			Globals.Instance.HumidityTarget = GD.RandRange(0, 5);
			Globals.Instance.BradiationTarget = 0;
			Globals.Instance.OxygenTarget = 100;
			Globals.Instance.PressureTarget = GD.RandRange(10000, 10020);
			Globals.Instance.WindDirectionTarget = new Vector3(GD.RandRange( - 1, 1), 0, GD.RandRange( - 1, 1));
			Globals.Instance.WindSpeedTarget = GD.RandRange(30, 50);

			_UpdateEnvironment();
		}

		protected void _StartVolcano()
		{
			Globals.Instance.TemperatureTarget = GD.RandRange(20, 31);
			Globals.Instance.HumidityTarget = GD.RandRange(0, 20);
			Globals.Instance.BradiationTarget = 0;
			Globals.Instance.OxygenTarget = 100;
			Globals.Instance.PressureTarget = GD.RandRange(10000, 10020);
			Globals.Instance.WindDirectionTarget = new Vector3(GD.RandRange( - 1, 1), 0, GD.RandRange( - 1, 1));
			Globals.Instance.WindSpeedTarget = GD.RandRange(0, 10);

			var rand_pos = new Vector3(GD.RandRange(0, 4097), 1000, GD.RandRange(0, 4097));
			var space_state = GetWorld3D().DirectSpaceState;
			var ray = PhysicsRayQueryParameters3D.Create(rand_pos, rand_pos - new Vector3(0, 10000, 0));
			var result = space_state.IntersectRay(ray);

			Volcano volcano = Globals.Instance.VolcanoScene.Instantiate<Volcano>();
			if(result.ContainsKey("position"))
			{
				volcano.Position = (Vector3)result["position"];
			}
			else
			{
				volcano.Position = new Vector3(GD.RandRange(0, 4097), 0, GD.RandRange(0, 4097));
			}
			ActiveDisasterNodes.Add(volcano);

			AddChild(volcano, true);

			_UpdateEnvironment();
		}


		protected void _StartTornado()
		{

			Vector3 rand_pos = new Vector3(GD.RandRange(0, 4097), 1000, GD.RandRange(0, 4097));
			PhysicsDirectSpaceState3D space_state = GetWorld3D().DirectSpaceState;
			PhysicsRayQueryParameters3D ray = PhysicsRayQueryParameters3D.Create(rand_pos, rand_pos - new Vector3(0, 10000, 0));
			Dictionary result = space_state.IntersectRay(ray);


			Tornado tornado = Globals.Instance.TornadoScene.Instantiate<Tornado>();
			if(result.ContainsKey("position"))
			{
				tornado.Position = (Vector3)result["position"];
			}
			else
			{
				tornado.Position = new Vector3(GD.RandRange(0, 4097), 0, GD.RandRange(0, 4097));
			}
			AddChild(tornado, true);
			ActiveDisasterNodes.Add(tornado);

			Globals.Instance.TemperatureTarget = GD.RandRange(5, 15);
			Globals.Instance.HumidityTarget = GD.RandRange(30, 40);
			Globals.Instance.BradiationTarget = 0;
			Globals.Instance.OxygenTarget = 100;
			Globals.Instance.PressureTarget = GD.RandRange(8000, 9000);
			Globals.Instance.WindDirectionTarget = new Vector3(GD.RandRange( - 1, 1), 0, GD.RandRange( - 1, 1));
			Globals.Instance.WindSpeedTarget = GD.RandRange(0, 30);

			_UpdateEnvironment();
			_SpawnLightningTimer();
		}


		protected void _StartAcidRain()
		{
			Globals.Instance.TemperatureTarget = GD.RandRange(20, 31);
			Globals.Instance.HumidityTarget = GD.RandRange(0, 20);
			Globals.Instance.BradiationTarget = 100;
			Globals.Instance.OxygenTarget = 100;
			Globals.Instance.PressureTarget = GD.RandRange(10000, 10020);
			Globals.Instance.WindDirectionTarget = new Vector3(GD.RandRange( - 1, 1), 0, GD.RandRange( - 1, 1));
			Globals.Instance.WindSpeedTarget = GD.RandRange(0, 10);

			_UpdateEnvironment();
		}

		protected void _StartEarthquake()
		{
			Globals.Instance.TemperatureTarget = GD.RandRange(20, 31);
			Globals.Instance.HumidityTarget = GD.RandRange(0, 20);
			Globals.Instance.BradiationTarget = 0;
			Globals.Instance.OxygenTarget = 100;
			Globals.Instance.PressureTarget = GD.RandRange(10000, 10020);
			Globals.Instance.WindDirectionTarget = new Vector3(GD.RandRange( - 1, 1), 0, GD.RandRange( - 1, 1));
			Globals.Instance.WindSpeedTarget = GD.RandRange(0, 10);

			var earquake = Globals.Instance.EarthquakeScene.Instantiate<Earthquake>();
			AddChild(earquake, true);
			ActiveDisasterNodes.Add(earquake);

			_UpdateEnvironment();
		}


		protected void _StartSun()
		{
			Globals.Instance.TemperatureTarget = GD.RandRange(20, 31);
			Globals.Instance.HumidityTarget = GD.RandRange(0, 20);
			Globals.Instance.BradiationTarget = 0;
			Globals.Instance.OxygenTarget = 100;
			Globals.Instance.PressureTarget = GD.RandRange(10000, 10020);
			Globals.Instance.WindDirectionTarget = new Vector3(GD.RandRange( - 1, 1), 0, GD.RandRange( - 1, 1));
			Globals.Instance.WindSpeedTarget = GD.RandRange(0, 10);

			_UpdateEnvironment();
		}


		protected void _StartCloud()
		{
			Globals.Instance.TemperatureTarget = GD.RandRange(20, 25);
			Globals.Instance.HumidityTarget = GD.RandRange(10, 30);
			Globals.Instance.BradiationTarget = 0;
			Globals.Instance.OxygenTarget = 100;
			Globals.Instance.PressureTarget = GD.RandRange(9000, 10000);
			Globals.Instance.WindDirectionTarget = new Vector3(GD.RandRange( - 1, 1), 0, GD.RandRange( - 1, 1));
			Globals.Instance.WindSpeedTarget = GD.RandRange(0, 10);


			_UpdateEnvironment();
		}


		protected void _StartRaining()
		{

			Globals.Instance.TemperatureTarget = GD.RandRange(10, 20);
			Globals.Instance.HumidityTarget = GD.RandRange(20, 40);
			Globals.Instance.BradiationTarget = 0;
			Globals.Instance.OxygenTarget = 100;
			Globals.Instance.PressureTarget = GD.RandRange(9000, 9020);
			Globals.Instance.WindDirectionTarget = new Vector3(GD.RandRange( - 1, 1), 0, GD.RandRange( - 1, 1));
			Globals.Instance.WindSpeedTarget = GD.RandRange(0, 20);

			_UpdateEnvironment();
		}

		protected void _StartStorm()
		{
			Globals.Instance.TemperatureTarget = GD.RandRange(5, 15);
			Globals.Instance.HumidityTarget = GD.RandRange(30, 40);
			Globals.Instance.BradiationTarget = 0;
			Globals.Instance.OxygenTarget = 100;
			Globals.Instance.PressureTarget = GD.RandRange(8000, 9000);
			Globals.Instance.WindDirectionTarget = new Vector3(GD.RandRange( - 1, 1), 0, GD.RandRange( - 1, 1));
			Globals.Instance.WindSpeedTarget = GD.RandRange(30, 60);

			_UpdateEnvironment();
			_SpawnLightningTimer();
		}


		protected void _StartDustStorm()
		{
			Globals.Instance.TemperatureTarget = GD.RandRange(30, 40);
			Globals.Instance.HumidityTarget = GD.RandRange(0, 10);
			Globals.Instance.BradiationTarget = 0;
			Globals.Instance.OxygenTarget = 0;
			Globals.Instance.PressureTarget = GD.RandRange(10000, 10020);
			Globals.Instance.WindDirectionTarget = new Vector3(GD.RandRange( - 1, 1), 0, GD.RandRange( - 1, 1));
			Globals.Instance.WindSpeedTarget = GD.RandRange(0, 50);

			_UpdateEnvironment();
		}

		protected void _OnDisasterChanged(string new_disaster)
		{

			// Limpiar el desastre anterior
			_CleanupDisaster();
			CurrentDisaster = new_disaster;


			// Iniciar el nuevo desastre

			if(new_disaster == "Tsunami")
			{
				_StartTsunami();
			}
			if(new_disaster == "Thunderstorm")
			{
				_StartThunderstorm();
			}
			if(new_disaster == "Meteors shower")
			{
				_StartMeteorShower();
			}
			if(new_disaster == "blizzard")
			{
				_StartBlizzard();
				_SpawnDecals(SnowDecalScene, 200);
			}
			if(new_disaster == "Sand Storm")
			{
				_StartSandstorm();
				_SpawnDecals(SandDecalScene, 200);
			}
			if(new_disaster == "Volcano")
			{
				_StartVolcano();
			}
			if(new_disaster == "Tornado")
			{
				_StartTornado();
			}
			if(new_disaster == "Acid rain")
			{
				_StartAcidRain();
			}
			if(new_disaster == "Earthquake")
			{
				_StartEarthquake();
			}
			if(new_disaster == "Sun")
			{
				_StartSun();
			}
			if(new_disaster == "Cloud")
			{
				_StartCloud();
			}
			if(new_disaster == "Raining")
			{
				_StartRaining();
			}
			if(new_disaster == "Storm")
			{
				_StartStorm();
			}
			if(new_disaster == "Dust Storm")
			{
				_StartDustStorm();
			}
			else 
			{
				_StartSunOriginal();
			}
	}

	protected void _CleanupDisaster()
	{
		IsSpawningLightning = false;


		// Limpiar efectos del desastre anterior
		foreach(Node3D node in ActiveDisasterNodes)
		{
			if(IsInstanceValid(node))
			{
				node.QueueFree();
			}
		}
		ActiveDisasterNodes.Clear();

		if(Globals.Instance.Gamemode == "survival")
		{
			Globals.Instance.Rpc(nameof(Globals.Instance.AddPoints), 100);
		}
	}

	protected void _SpawnDecals(PackedScene scene, int amount)
	{
		if(!Multiplayer.IsServer())
		{
			return ;
		}

		var space_state = GetWorld3D().DirectSpaceState;

		for (int i = 0; i < amount; i++)
		{


			var rand_pos = new Vector3(GD.RandRange(0, 4097), 1000, GD.RandRange(0, 4097));
	
			var ray = PhysicsRayQueryParameters3D.Create(rand_pos, rand_pos - new Vector3(0, 2000, 0));

			var result = space_state.IntersectRay(ray);

			if(result.ContainsKey("position"))
			{
				Decal decal = scene.Instantiate<Decal>();


				// Tamañó aleatorio entre 3 y 500
				float random_size = (float)GD.RandRange(3.0, 500.0);
				decal.Size = new Vector3(random_size, random_size, random_size);

				decal.Position = (Vector3)result["position"] + new Vector3(0, 0.05f, 0);
				decal.Rotation = new Vector3(0, (float)GD.RandRange(0, Mathf.Tau), 0);

				AddChild(decal, true);
				ActiveDecals.Append(decal);
			}
		}
	}


	protected async void _SpawnDecalsOverTime(PackedScene scene, int total, float delay)
	{
		for (int i = 0; i < total; i++)
		{
			_SpawnDecals(scene, 1);
			await ToSignal(GetTree().CreateTimer(delay), SceneTreeTimer.SignalName.Timeout);
		}
	}


	protected async void _SpawnMeteorShowerTimer()
	{
		while(Globals.Instance.CurrentWeatherAndDisaster == "Meteors shower")
		{
			Meteors meteor = Globals.Instance.MeteorScene.Instantiate<Meteors>();
			Vector3 rand_pos = new Vector3(GD.RandRange(0, 4097), 1000, GD.RandRange(0, 4097));
			meteor.Position = rand_pos;
			AddChild(meteor, true);
			ActiveDisasterNodes.Add(meteor);

			await ToSignal(GetTree().CreateTimer(1), SceneTreeTimer.SignalName.Timeout);
		}
	}

	protected void _UpdateEnvironment()
	{
		var player = Globals.Instance.LocalPlayer;

		if(!GodotObject.IsInstanceValid(player))
		{
			return ;
		}

		var is_outdoor = Globals.Instance.IsOutdoor(player);


		// Ajustes por desastre
		switch(CurrentDisaster)
		{
			case "blizzard":
			{
				player.SnowNode.Emitting = is_outdoor;
				GetNode<WorldEnvironment>("WorldEnvironment").Environment.VolumetricFogAlbedo = new Color(1, 1, 1);
				break; }
			case "Sand Storm":
			{
				player.SandNode.Emitting = is_outdoor;
				GetNode<WorldEnvironment>("WorldEnvironment").Environment.VolumetricFogAlbedo = new Color(1, 0.647059f, 0);
				break; }
			case "Acid rain":
			{
				player.RainNode.Emitting = is_outdoor;
				GetNode<WorldEnvironment>("WorldEnvironment").Environment.VolumetricFogAlbedo = new Color(0, 1, 0);
				break; }
			case "Dust Storm":
			{
				player.DustNode.Emitting = is_outdoor;
				GetNode<WorldEnvironment>("WorldEnvironment").Environment.VolumetricFogAlbedo = new Color(0, 0, 0);
				break; }
			default:
			{
				player.SnowNode.Emitting = false;
				player.SandNode.Emitting = false;
				player.DustNode.Emitting = false;
				GetNode<WorldEnvironment>("WorldEnvironment").Environment.VolumetricFogAlbedo = new Color(1, 1, 1);
				break; }
		}


		// Cuando hay lluvia/tormenta u otros eventos que requieren niebla, activarla s�lo si el jugador est� al aire libre
		var foggy_disasters = new Array<string>{"Thunderstorm", "Raining", "Storm", "Tornado", "blizzard", "Sand Storm", "Cloud", "Acid rain", "Dust Storm"};
		var rain_disasters = new Array<string>{"Thunderstorm", "Raining", "Storm", "Tornado", "Acid rain"};
		GetNode<WorldEnvironment>("WorldEnvironment").Environment.VolumetricFogEnabled = foggy_disasters.Contains(CurrentDisaster) && is_outdoor;


		// Nodos de partculas generales
		player.RainNode.Emitting = (rain_disasters.Contains(CurrentDisaster)) && is_outdoor;


		// Ajuste de nubes

		((ShaderMaterial)GetNode<WorldEnvironment>("WorldEnvironment").Environment.Sky.SkyMaterial).SetShaderParameter("clouds_fuzziness", ( foggy_disasters.Contains(CurrentDisaster) ? 0.25 : 1 ));
	}

	protected async void _SpawnLightningTimer()
	{
		if(IsSpawningLightning)
		{
			return ;
		}

		// Evitar m�ltiples instancias del timer
		IsSpawningLightning = true;

		while(Globals.Instance.CurrentWeatherAndDisaster == "Thunderstorm" && IsSpawningLightning)
		{
			var player = Globals.Instance.LocalPlayer;

			if(GodotObject.IsInstanceValid(player) && Globals.Instance.IsOutdoor(player))
			{
				if(GD.RandRange(1, 25) == 25)
				{
					Thunder lighting = Globals.Instance.ThunderstormScene.Instantiate<Thunder>();
					var rand_pos = new Vector3(GD.RandRange(0, 4097), 1000, GD.RandRange(0, 4097));
					var space_state = GetWorld3D().DirectSpaceState;

					if(space_state != null)
					{
						var ray = PhysicsRayQueryParameters3D.Create(rand_pos, rand_pos - new Vector3(0, 10000, 0));
						var result = space_state.IntersectRay(ray);

						if(result.ContainsKey("position"))
						{
							lighting.Position = (Vector3)result["position"];
						}
						else
						{
							lighting.Position = new Vector3(GD.RandRange(0, 4097), 0, GD.RandRange(0, 4097));
						}
					}
					else
					{
						lighting.Position = new Vector3(GD.RandRange(0, 4097), 0, GD.RandRange(0, 4097));
					}

					AddChild(lighting, true);
					ActiveDisasterNodes.Add(lighting);
				}
			}

			await ToSignal(GetTree().CreateTimer(0.5), SceneTreeTimer.SignalName.Timeout);
		}

		IsSpawningLightning = false;
	}

}