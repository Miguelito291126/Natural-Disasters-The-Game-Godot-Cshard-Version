using Godot;
[GlobalClass]
public partial class Map : Node3D
{
	public Node Worldenvironment;
	[Export] public PackedScene SnowDecalScene;
	[Export] public PackedScene SandDecalScene;


	public string CurrentDisaster = "";
	public Godot.Collections.Array ActiveDisasterNodes = new();
	public Godot.Collections.Array ActiveDecals = new();
	public bool IsSpawningLightning = false;


	public override void _ExitTree()
	{
		if(Multiplayer.IsServer())
		{
			Globals.SetWeatherAndDisaster.Rpc("Original");
			Globals.Timer.Stop();
			Globals.Started = false;
		}
	}

	public override void _Ready()
	{
		Worldenvironment = GetNode("WorldEnvironment");
		Globals.Map = this;

		if(!Globals.CurrentWeatherAndDisasterChanged.IsConnected(_on_disaster_changed))
		{
			Globals.CurrentWeatherAndDisasterChanged.Connect(_on_disaster_changed);
		}


		if(Multiplayer.IsServer())
		{
			Globals.SetWeatherAndDisaster.Rpc("Original");

			if(Globals.Gamemode == "survival")
			{
				if(!OS.HasFeature("dedicated_server"))
				{
					Globals.MultiplayerPlayerSpawner();
				}

				foreach(PackedInt32array i in Multiplayer.GetPeers())
				{
					Globals.MultiplayerPlayerSpawner(i);
				}

				Globals.Timer.WaitTime = Globals.GlobalsData.TimerDisasters;
				Globals.Timer.Start();
			}

			else
			{
				if(!OS.HasFeature("dedicated_server"))
				{
					Globals.MultiplayerPlayerSpawner();
				}

				foreach(PackedInt32array i in Multiplayer.GetPeers())
				{
					Globals.MultiplayerPlayerSpawner(i);
				}
			}
		}
	}


	// Llama a la función wind para cada objeto en la escena
	public override void _PhysicsProcess(double _delta)
	{
		foreach(Node node in GetChildren())
		{
			Globals.Wind(node);
		}
	}


	public override void _Process(double _delta)
	{
		if(Multiplayer.IsServer())
		{
			if(OS.HasFeature("dedicated_server") || OS.GetCmdlineUserArgs() || OS.GetCmdlineUserArgs().Contains("server").Contains("s"))
			{
				Globals.Started = true;
			}
			else
			{
				if(Multiplayer.MultiplayerPeer == null || Multiplayer.MultiplayerPeer is OfflineMultiplayerPeer)
				{
					Globals.Started = true;
					return;
				}

				if(Globals.PlayersConected.Size() > 1)
				{
					Globals.Started = true;
				}
				else
				{
					Globals.Started = false;
				}
			}
		}
	}

	protected void _StartSunOriginal()
		{
			Globals.TemperatureTarget = Globals.TemperatureOriginal;
			Globals.HumidityTarget = Globals.HumidityOriginal;
			Globals.BradiationTarget = Globals.BradiationOriginal;
			Globals.OxygenTarget = Globals.OxygenOriginal;
			Globals.PressureTarget = Globals.PressureOriginal;
			Globals.WindDirectionTarget = Globals.WindDirectionOriginal;
			Globals.WindSpeedTarget = Globals.WindSpeedOriginal;

			_UpdateEnvironment();
		}


		protected void _StartTsunami()
		{
			var tsunami = Globals.TsunamiScene.Instantiate();
			tsunami.Position = new Vector3(0, 0, 0);
			AddChild(tsunami, true);
ActiveDisasterNodes.Add(tsunami);

			Globals.TemperatureTarget = GD.RandRange(20, 31);
			Globals.HumidityTarget = GD.RandRange(0, 20);
			Globals.BradiationTarget = 0;
			Globals.OxygenTarget = 100;
			Globals.PressureTarget = GD.RandRange(10000, 10020);
			Globals.WindDirectionTarget = new Vector3(GD.RandRange( - 1, 1), 0, GD.RandRange( - 1, 1));
			Globals.WindSpeedTarget = GD.RandRange(0, 10);

			_UpdateEnvironment();
		}


		protected void _StartThunderstorm()
		{

			Globals.TemperatureTarget = GD.RandRange(5, 15);
			Globals.HumidityTarget = GD.RandRange(30, 40);
			Globals.BradiationTarget = 0;
			Globals.OxygenTarget = 100;
			Globals.PressureTarget = GD.RandRange(8000, 9000);
			Globals.WindDirectionTarget = new Vector3(GD.RandRange( - 1, 1), 0, GD.RandRange( - 1, 1));
			Globals.WindSpeedTarget = GD.RandRange(0, 30);

			_UpdateEnvironment();
			_SpawnLightningTimer();
		}


		protected void _StartMeteorShower()
		{
			Globals.TemperatureTarget = GD.RandRange(20, 31);
			Globals.HumidityTarget = GD.RandRange(0, 20);
			Globals.PressureTarget = GD.RandRange(10000, 10020);
			Globals.BradiationTarget = 0;
			Globals.OxygenTarget = 100;
			Globals.WindDirectionTarget = new Vector3(GD.RandRange( - 1, 1), 0, GD.RandRange( - 1, 1));
			Globals.WindSpeedTarget = GD.RandRange(0, 10);

			_SpawnMeteorShowerTimer();
			_UpdateEnvironment();
		}

		protected void _StartBlizzard()
		{
			Globals.TemperatureTarget = GD.RandRange( - 20,  - 35);
			Globals.HumidityTarget = GD.RandRange(20, 30);
			Globals.BradiationTarget = 0;
			Globals.OxygenTarget = 100;
			Globals.PressureTarget = GD.RandRange(8000, 9020);
			Globals.WindDirectionTarget = new Vector3(GD.RandRange( - 1, 1), 0, GD.RandRange( - 1, 1));
			Globals.WindSpeedTarget = GD.RandRange(40, 50);


			_UpdateEnvironment();
		}


		protected void _StartSandstorm()
		{
			Globals.TemperatureTarget = GD.RandRange(30, 35);
			Globals.HumidityTarget = GD.RandRange(0, 5);
			Globals.BradiationTarget = 0;
			Globals.OxygenTarget = 100;
			Globals.PressureTarget = GD.RandRange(10000, 10020);
			Globals.WindDirectionTarget = new Vector3(GD.RandRange( - 1, 1), 0, GD.RandRange( - 1, 1));
			Globals.WindSpeedTarget = GD.RandRange(30, 50);

			_UpdateEnvironment();
		}

		protected void _StartVolcano()
		{
			Globals.TemperatureTarget = GD.RandRange(20, 31);
			Globals.HumidityTarget = GD.RandRange(0, 20);
			Globals.BradiationTarget = 0;
			Globals.OxygenTarget = 100;
			Globals.PressureTarget = GD.RandRange(10000, 10020);
			Globals.WindDirectionTarget = new Vector3(GD.RandRange( - 1, 1), 0, GD.RandRange( - 1, 1));
			Globals.WindSpeedTarget = GD.RandRange(0, 10);

			var rand_pos = new Vector3(GD.RandRange(0, 4097), 1000, GD.RandRange(0, 4097));
			var space_state = GetWorld3d().DirectSpaceState;
			var ray = PhysicsRayQueryParameters3D.Create(rand_pos, rand_pos - new Vector3(0, 10000, 0));
			var result = space_state.IntersectRay(ray);

			var volcano = Globals.VolcanoScene.Instantiate();
			if(result.ContainsKey("position"))
			{
				volcano.Position = result.Position;
			}
			else
			{
				volcano.Position = new Vector3(GD.RandRange(0, 4097), 0, GD.RandRange(0, 4097));
			}
			ActiveDisasterNodes.Append(volcano);

			AddChild(volcano, true);

			_UpdateEnvironment();
		}


		protected void _StartTornado()
		{

			var rand_pos = new Vector3(GD.RandRange(0, 4097), 1000, GD.RandRange(0, 4097));
			var space_state = GetWorld3d().DirectSpaceState;
			var ray = PhysicsRayQueryParameters3D.Create(rand_pos, rand_pos - new Vector3(0, 10000, 0));
			var result = space_state.IntersectRay(ray);


			var tornado = Globals.TornadoScene.Instantiate();
			if(result.ContainsKey("position"))
			{
				tornado.Position = result.Position;
			}
			else
			{
				tornado.Position = new Vector3(GD.RandRange(0, 4097), 0, GD.RandRange(0, 4097));
			}
			AddChild(tornado, true);
			ActiveDisasterNodes.Append(tornado);

			Globals.TemperatureTarget = GD.RandRange(5, 15);
			Globals.HumidityTarget = GD.RandRange(30, 40);
			Globals.BradiationTarget = 0;
			Globals.OxygenTarget = 100;
			Globals.PressureTarget = GD.RandRange(8000, 9000);
			Globals.WindDirectionTarget = new Vector3(GD.RandRange( - 1, 1), 0, GD.RandRange( - 1, 1));
			Globals.WindSpeedTarget = GD.RandRange(0, 30);

			_UpdateEnvironment();
			_SpawnLightningTimer();
		}


		protected void _StartAcidRain()
		{
			Globals.TemperatureTarget = GD.RandRange(20, 31);
			Globals.HumidityTarget = GD.RandRange(0, 20);
			Globals.BradiationTarget = 100;
			Globals.OxygenTarget = 100;
			Globals.PressureTarget = GD.RandRange(10000, 10020);
			Globals.WindDirectionTarget = new Vector3(GD.RandRange( - 1, 1), 0, GD.RandRange( - 1, 1));
			Globals.WindSpeedTarget = GD.RandRange(0, 10);

			_UpdateEnvironment();
		}

		protected void _StartEarthquake()
		{
			Globals.TemperatureTarget = GD.RandRange(20, 31);
			Globals.HumidityTarget = GD.RandRange(0, 20);
			Globals.BradiationTarget = 0;
			Globals.OxygenTarget = 100;
			Globals.PressureTarget = GD.RandRange(10000, 10020);
			Globals.WindDirectionTarget = new Vector3(GD.RandRange( - 1, 1), 0, GD.RandRange( - 1, 1));
			Globals.WindSpeedTarget = GD.RandRange(0, 10);

			var earquake = Globals.EarthquakeScene.Instantiate();
			AddChild(earquake, true);
			ActiveDisasterNodes.Append(earquake);

			_UpdateEnvironment();
		}


		protected void _StartSun()
		{
			Globals.TemperatureTarget = GD.RandRange(20, 31);
			Globals.HumidityTarget = GD.RandRange(0, 20);
			Globals.BradiationTarget = 0;
			Globals.OxygenTarget = 100;
			Globals.PressureTarget = GD.RandRange(10000, 10020);
			Globals.WindDirectionTarget = new Vector3(GD.RandRange( - 1, 1), 0, GD.RandRange( - 1, 1));
			Globals.WindSpeedTarget = GD.RandRange(0, 10);

			_UpdateEnvironment();
		}


		protected void _StartCloud()
		{
			Globals.TemperatureTarget = GD.RandRange(20, 25);
			Globals.HumidityTarget = GD.RandRange(10, 30);
			Globals.BradiationTarget = 0;
			Globals.OxygenTarget = 100;
			Globals.PressureTarget = GD.RandRange(9000, 10000);
			Globals.WindDirectionTarget = new Vector3(GD.RandRange( - 1, 1), 0, GD.RandRange( - 1, 1));
			Globals.WindSpeedTarget = GD.RandRange(0, 10);


			_UpdateEnvironment();
		}


		protected void _StartRaining()
		{

			Globals.TemperatureTarget = GD.RandRange(10, 20);
			Globals.HumidityTarget = GD.RandRange(20, 40);
			Globals.BradiationTarget = 0;
			Globals.OxygenTarget = 100;
			Globals.PressureTarget = GD.RandRange(9000, 9020);
			Globals.WindDirectionTarget = new Vector3(GD.RandRange( - 1, 1), 0, GD.RandRange( - 1, 1));
			Globals.WindSpeedTarget = GD.RandRange(0, 20);

			_UpdateEnvironment();
		}

		protected void _StartStorm()
		{
			Globals.TemperatureTarget = GD.RandRange(5, 15);
			Globals.HumidityTarget = GD.RandRange(30, 40);
			Globals.BradiationTarget = 0;
			Globals.OxygenTarget = 100;
			Globals.PressureTarget = GD.RandRange(8000, 9000);
			Globals.WindDirectionTarget = new Vector3(GD.RandRange( - 1, 1), 0, GD.RandRange( - 1, 1));
			Globals.WindSpeedTarget = GD.RandRange(30, 60);

			_UpdateEnvironment();
			_SpawnLightningTimer();
		}


		protected void _StartDustStorm()
		{
			Globals.TemperatureTarget = GD.RandRange(30, 40);
			Globals.HumidityTarget = GD.RandRange(0, 10);
			Globals.BradiationTarget = 0;
			Globals.OxygenTarget = 0;
			Globals.PressureTarget = GD.RandRange(10000, 10020);
			Globals.WindDirectionTarget = new Vector3(GD.RandRange( - 1, 1), 0, GD.RandRange( - 1, 1));
			Globals.WindSpeedTarget = GD.RandRange(0, 50);

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
		foreach(Variant node in ActiveDisasterNodes)
		{
			if(GodotObject.IsInstanceValid(node))
			{
				node.QueueFree();
			}
		}
		ActiveDisasterNodes.Clear();

		if(Globals.Gamemode == "survival")
		{
			Globals.AddPoints.Rpc();
		}
	}

	protected void _SpawnDecals(PackedScene scene, int amount)
	{
		if(!Multiplayer.IsServer())
		{
			return ;
		}

		var space_state = GetWorld3d().DirectSpaceState;

		foreach(int i in amount)
		{


			var rand_pos = new Vector3(GD.RandRange(0, 4097), 1000, GD.RandRange(0, 4097));
	
			var ray = PhysicsRayQueryParameters3D.Create(rand_pos, rand_pos - new Vector3(0, 2000, 0));

			var result = space_state.IntersectRay(ray);

			if(result.ContainsKey("position"))
			{
				var decal = scene.Instantiate();


				// Tamañó aleatorio entre 3 y 500
				var random_size = GD.RandRange(3.0, 500.0);
				decal.Size = new Vector3(random_size, random_size, random_size);

				decal.Position = result.Position + new Vector3(0, 0.05, 0);
				decal.Rotation.Y = GD.RandRange(0, Mathf.Tau);

				AddChild(decal, true);
				ActiveDecals.Append(decal);
			}
		}
	}


	protected void _SpawnDecalsOverTime(Variant scene, Variant total, Variant delay)
	{
		foreach(Variant i in total)
		{
			_SpawnDecals(scene, 1);
			await ToSignal(GetTree().CreateTimer(delay), "Timeout");
		}
	}


	protected void _SpawnMeteorShowerTimer()
	{
		while(Globals.CurrentWeatherAndDisaster == "Meteors shower")
		{
			var meteor = Globals.MeteorScene.Instantiate();
			var rand_pos = new Vector3(GD.RandRange(0, 4097), 1000, GD.RandRange(0, 4097));
			meteor.Position = rand_pos;
			AddChild(meteor, true);
			ActiveDisasterNodes.Append(meteor);

			await ToSignal(GetTree().CreateTimer(1), "Timeout");
		}
	}

	protected void _UpdateEnvironment()
	{
		var player = Globals.LocalPlayer;

		if(!GodotObject.IsInstanceValid(player))
		{
			return ;
		}

		var is_outdoor = Globals.IsOutdoor(player);


		// Ajustes por desastre
		switch(CurrentDisaster)
		{
			case "blizzard":
			{
				player.SnowNode.Emitting = is_outdoor;
				GetNode("WorldEnvironment").Environment.VolumetricFogAlbedo = new Color(1, 1, 1);
				break; }
			case "Sand Storm":
			{
				player.SandNode.Emitting = is_outdoor;
				GetNode("WorldEnvironment").Environment.VolumetricFogAlbedo = new Color(1, 0.647059, 0);
				break; }
			case "Acid rain":
			{
				player.RainNode.Emitting = is_outdoor;
				GetNode("WorldEnvironment").Environment.VolumetricFogAlbedo = new Color(0, 1, 0);
				break; }
			case "Dust Storm":
			{
				player.DustNode.Emitting = is_outdoor;
				GetNode("WorldEnvironment").Environment.VolumetricFogAlbedo = new Color(0, 0, 0);
				break; }
			default:
			{
				player.SnowNode.Emitting = false;
				player.SandNode.Emitting = false;
				player.DustNode.Emitting = false;
				GetNode("WorldEnvironment").Environment.VolumetricFogAlbedo = new Color(1, 1, 1);
				break; }
		}


		// Cuando hay lluvia/tormenta u otros eventos que requieren niebla, activarla s�lo si el jugador est� al aire libre
		var foggy_disasters = new array{"Thunderstorm", "Raining", "Storm", "Tornado", "blizzard", "Sand Storm", "Cloud", "Acid rain", "Dust Storm", };
		var rain_disasters = new array{"Thunderstorm", "Raining", "Storm", "Tornado", "Acid rain", };
		GetNode("WorldEnvironment").Environment.VolumetricFogEnabled = foggy_disasters && is_outdoor.Contains(CurrentDisaster);


		// Nodos de part�culas generales
		player.RainNode.Emitting = (rain_disasters.Contains(CurrentDisaster)) && is_outdoor;


		// Ajuste de nubes

		GetNode("WorldEnvironment").Environment.Sky.SkyMaterial.SetShaderParameter("clouds_fuzziness", ( foggy_disasters.Contains(CurrentDisaster) ? 0.25 : 1 ));
	}

	protected void _SpawnLightningTimer()
	{
		if(IsSpawningLightning)
		{
			return ;
		}

		// Evitar m�ltiples instancias del timer
		IsSpawningLightning = true;

		while(Globals.CurrentWeatherAndDisaster == "Thunderstorm" && IsSpawningLightning)
		{
			var player = Globals.LocalPlayer;

			if(GodotObject.IsInstanceValid(player) && Globals.IsOutdoor(player))
			{
				if(GD.RandRange(1, 25) == 25)
				{
					var lighting = Globals.ThunderstormScene.Instantiate();
					var rand_pos = new Vector3(GD.RandRange(0, 4097), 1000, GD.RandRange(0, 4097));
					var space_state = GetWorld3d().DirectSpaceState;

					if(space_state != null)
					{
						var ray = PhysicsRayQueryParameters3D.Create(rand_pos, rand_pos - new Vector3(0, 10000, 0));
						var result = space_state.IntersectRay(ray);

						if(result.ContainsKey("position"))
						{
							lighting.Position = result.Position;
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
					ActiveDisasterNodes.Append(lighting);
				}
			}

			await ToSignal(GetTree().CreateTimer(0.5), "Timeout");
		}

		IsSpawningLightning = false;
	}

}