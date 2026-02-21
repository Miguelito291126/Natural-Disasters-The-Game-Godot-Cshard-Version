using Godot;
using Godot.Collections;

[GlobalClass]
public partial class Earthquake : Node3D
{
	[Export] public int Magnitude = 7;
	[Export] public int MagnitudeModifier = 0;
	public int NextPhysicsTime = Time.GetTicksMsec();
	public int SpawnTime = Time.GetTicksMsec();
	[Export] public Godot.Collections.Array Life = new() {15, 20};


	public Node StartWeakEarthquake;
	public Node StartStrongEarthquake;
	public Node EarthquakeSound;
	public Node EarthqueakeAftershotSound;

	public override void _PhysicsProcess(double delta)
	{
		MagnitudeModulateSound();
		ProcessMagnitude();
		MagnitudeModifierIncrement(delta);
	}


	public override void _Process(double delta)
	{
		DestroyAllHouses();
	}

	public override void _Ready()
	{
		StartWeakEarthquake = GetNode("earquake_start_sound_weak");
		StartStrongEarthquake = GetNode("earquake_start_sound_strong");
		EarthquakeSound = GetNode("earquake_sound");
		EarthqueakeAftershotSound = GetNode("earqueake_aftershot");
		PlayInitialSounds();
		DestroyAllHouses();

		await ToSignal(GetTree().CreateTimer(GD.RandRange(Life[0], Life[1])), "Timeout");
		EarthquakeDecay();
	}

	public void PlayInitialSounds()
	{
		if(Magnitude > 5)
		{
			StartStrongEarthquake.Play();
		}
		else
		{
			StartWeakEarthquake.Play();
		}
	}

	public void EarthquakeDecay()
	{
		if(GD.RandRange(1, 2) == 1)
		{
			CreateEarthquakeWithParent();
		}
		QueueFree();
	}

	// Esto libera el nodo actual, elimin�ndolo del escenario
	public void SendClientsideEffects(Variant ply, Variant amplitude)
	{
		if(GD.Randi() % 8 == 0)
		{
			ply.CameraNode.StartScreenShake(0.6, amplitude * 2, 25);
		}
	}

	public bool CanDoPhysics(Variant next_time)
	{
		if(Engine.GetFramesPerSecond() > 0)
		{
			// Aseg�rate de que no estemos dividiendo por cero
			var current_time = Engine.GetFramesDrawn() / Engine.GetFramesPerSecond();
			// Obtener el tiempo actual del juego
			if(current_time >= this.NextPhysicsTime)
			{
				if(Globals.HitChance(1))
				{
					this.NextPhysicsTime = current_time + (GD.RandRange(0, 250) / 100);
				}
				else
				{
					this.NextPhysicsTime = current_time + next_time;
				}
				return true;
			}
		}
		return false;
	}

	public void DoPhysics()
	{
		var t = 0.1;
		// Obtener el valor del ConVar "gdisasters_envearthquake_simquality"
		var mag = Magnitude * MagnitudeModifier;


		// Si no podemos hacer f�sica en este momento o la magnitud es menor que 3, no hacemos nada
		if(mag < 3)
		{
			Globals.PrintRole("Mag its low");
			return ;
		}

		var vec = (mag * 25) * new Vector3(GD.RandRange( - 15, 15) / 10, GD.RandRange( - 5, 4) / 10, GD.RandRange( - 15, 15) / 10);
		var ang_vv = new Vector3((GD.RandRange( - 15, 15) / 10), GD.RandRange( - 5, 4) / 10, GD.RandRange( - 15, 15) / 10) * (mag * 8);


		// Si hay una posibilidad de golpear, incrementamos la velocidad angular
		if(Globals.HitChance(2))
		{
			ang_vv *= 20;
		}


		// Aplicar efectos a los jugadores
		foreach(Node v in GetTree().GetNodesInGroup("player"))
		{
			if(v.IsOnFloor())
			{
				if(3 <= mag && mag < 4)
				{

				}
				else if(4 <= mag && mag < 5)
				{

				}
				else if(5 <= mag && mag < 6)
				{

				}
				else if(6 <= mag && mag < 7)
				{

				}
				else if(7 <= mag && mag < 8)
				{
					v.SetVelocity(vec);
				}
				else if(8 <= mag && mag < 9)
				{
					v.SetVelocity(vec * 1.125);
				}
				else if(9 <= mag && mag < 10)
				{
					v.SetVelocity(vec * 1.5);
				}
				else if(10 <= mag && mag < 11)
				{
					v.SetVelocity(vec * 2);
				}
				else if(11 <= mag && mag < 12)
				{
					v.SetVelocity(vec * 2.125);
				}
				else if(12 <= mag && mag < 13)
				{
					v.SetVelocity(vec * 2.5);
				}
			}
		}


		// Aplicar efectos a las entidades
		foreach(Node v in GetTree().GetNodesInGroup("movable_objects"))
		{
			if(v.IsClass("RigidBody3D"))
			{
				var vel_mod = 1 - Mathf.Clamp(v.GetLinearVelocity().Length() / 2000, 0, 1);
				var ang_v = ang_vv * vel_mod;

				if(3 <= mag && mag < 4)
				{
					if(GD.RandRange(1, 2) == 1)
					{
						v.ApplyImpulse(ang_v);
					}
				}
				else if(4 <= mag && mag < 5)
				{
					if(GD.RandRange(1, 2) == 1)
					{
						v.ApplyImpulse(ang_v);
						Unfreeze(v, mag);
					}
				}
				else if(5 <= mag && mag < 6)
				{
					if(GD.RandRange(1, 2) == 1)
					{
						v.ApplyImpulse(ang_v);
						Unfreeze(v, mag);
					}
				}
				else if(6 <= mag && mag < 7)
				{
					if(GD.RandRange(1, 2) == 1)
					{
						v.ApplyImpulse(ang_v * 2);
						Unfreeze(v, mag);
					}
				}
				else if(7 <= mag && mag < 8)
				{
					if(GD.RandRange(1, 2) == 1)
					{
						v.ApplyImpulse(ang_v * 4);
						Unfreeze(v, mag);
					}
				}
				else if(8 <= mag && mag < 9)
				{
					if(GD.RandRange(1, 2) == 1)
					{
						v.ApplyImpulse(ang_v * 8);
						Unfreeze(v, mag);
					}
				}
				else if(9 <= mag && mag < 10)
				{
					if(GD.RandRange(1, 2) == 1)
					{
						v.ApplyImpulse(ang_v * 12);
						Unfreeze(v, mag);
					}
				}
				else if(10 <= mag && mag < 11)
				{
					if(GD.RandRange(1, 2) == 1)
					{
						v.ApplyImpulse(ang_v * 24);
						Unfreeze(v, mag);
					}
				}
				else if(11 <= mag && mag < 12)
				{
					if(GD.RandRange(1, 2) == 1)
					{
						v.ApplyImpulse(ang_v * 36);
						Unfreeze(v, mag);
					}
				}
				else if(12 <= mag && mag < 13)
				{
					if(GD.RandRange(1, 2) == 1)
					{
						v.ApplyImpulse(ang_v * 40);
						Unfreeze(v, mag);
					}
				}
			}
			else if(v.IsClass("StaticBody3D"))
			{
				if(GD.RandRange(1, 2) == 1)
				{
					Destroy(v);
				}
			}
		}
	}

	public void Unfreeze(Variant v, Variant _mag)
	{
		if(GD.RandRange(1, 1024 - (25.6 * Magnitude)) == 1)
		{
			if(GodotObject.IsInstanceValid(v))
			{
				v.Freeze = false;
			}
		}
		if(GD.RandRange(1, 512 - (25.6 * Magnitude)) == 1)
		{
			if(GodotObject.IsInstanceValid(v))
			{
				v.Sleeping = false;
				v.Freeze = false;
				Destroy(v);
			}
		}
	}

	public void Destroy(Variant v)
	{
		if(GodotObject.IsInstanceValid(v))
		{
			if((v.IsInGroup("Destrollable") || v.IsInGroup("Hause")) && v.HasMethod("destroy"))
			{
				v.Destroy.Rpc();
			}
		}
	}

	public void DestroyAllHouses()
	{

		// Destruir todas las casas al iniciar el terremoto
		foreach(Node house in GetTree().GetNodesInGroup("Hause"))
		{
			if(GodotObject.IsInstanceValid(house))
			{
				Destroy(house);
			}
		}
	}


	public void MagnitudeModulateSound()
	{
		var volume = this.Magnitude;
		// Asumiendo que self.magnitude es una propiedad que representa la magnitud del terremoto
		var vol_mod = Mathf.Pow(volume / 10, 3);
		var distance_mod = 0;


		// Calcula la modulaci�n de volumen basada en la distancia al jugador (ejemplo simplificado)
		var local_player_pos = Globals.LocalPlayer.Position;
		// Obt�n la posici�n del jugador local
		var ray_params = PhysicsRayQueryParameters3D.Create(local_player_pos, local_player_pos + new Vector3(0, 0,  - 3000));
		var ray_result = GetWorld3d().DirectSpaceState.IntersectRay(ray_params);
		if(ray_result.Size() > 0)
		{
			distance_mod = 1 - (ray_result["position"].DistanceTo(local_player_pos) / 3000);
		}

		vol_mod *= distance_mod;


		if(!EarthquakeSound.Playing)
		{
			EarthquakeSound.Play();
		}

		EarthquakeSound.VolumeDb = vol_mod;
	}


	public void CreateEarthquakeWithParent()
	{
		var decider = GD.Randi() % Int(Mathf.Floor(Magnitude * 2)) == 1;
		if(!decider)
		{
			if(Int(Mathf.Floor(Magnitude)) > 1)
			{
				EarthqueakeAftershotSound.Play();
				var aftershock_magnitude = Mathf.Clamp(Int(Mathf.Floor(Magnitude)) - GD.Randi() % 3, 1, 12);
				var aftershock = Load("res://Scenes/earthquake.tscn").Instantiate();
				aftershock.Magnitude = aftershock_magnitude;
				aftershock.Position = Vector3.Zero;
				GetParent().AddChild(aftershock, true);
				aftershock.GlobalTransform.Origin = GetParent().GlobalTransform.Origin;
				aftershock.Show();
			}
		}

		else
		{
			EarthqueakeAftershotSound.Play();
			var foreshock_magnitude = Mathf.Clamp(Int(Mathf.Floor(Magnitude)) - GD.Randi() % 3, 1, 12);
			var foreshock = Load("res://Scenes/earthquake.tscn").Instantiate();
			foreshock.Magnitude = foreshock_magnitude;
			foreshock.Position = Position;
			GetParent().AddChild(foreshock, true);
			foreshock.GlobalTransform.Origin = GetParent().GlobalTransform.Origin;
			foreshock.Show();
		}
	}

	public void MagnitudeModifierIncrement(double delta)
	{

		// Ajustar el valor de MagnitudeModifier
		this.MagnitudeModifier = Mathf.Clamp(this.MagnitudeModifier + (delta / 4), 0, 1);
	}

	public int GetRealMagnitude()
	{
		return Magnitude * MagnitudeModifier;
	}

	public void ProcessMagnitude()
	{
		var mag = Magnitude * MagnitudeModifier;

		if(mag >= 0 && mag < 1)
		{
			Globals.PrintRole("Mag its very low");
		}
		else if(mag >= 1 && mag < 2)
		{
			MagnitudeOne();
		}
		else if(mag >= 2 && mag < 3)
		{
			MagnitudeTwo();
		}
		else if(mag >= 3 && mag < 4)
		{
			MagnitudeThree();
		}
		else if(mag >= 4 && mag < 5)
		{
			MagnitudeFour();
		}
		else if(mag >= 5 && mag < 6)
		{
			MagnitudeFive();
		}
		else if(mag >= 6 && mag < 7)
		{
			MagnitudeSix();
		}
		else if(mag >= 7 && mag < 8)
		{
			MagnitudeSeven();
		}
		else if(mag >= 8 && mag < 9)
		{
			MagnitudeEight();
		}
		else if(mag >= 9 && mag < 10)
		{
			MagnitudeNine();
		}
		else if(mag >= 10 && mag < 11)
		{
			MagnitudeTen();
		}
		else if(mag >= 11 && mag < 12)
		{
			MagnitudeEleven();
		}
		else if(mag >= 12 && mag < 13)
		{
			MagnitudeTwelve();
		}
		else
		{
			Globals.PrintRole("Mag its very high");
		}
	}

	public void MagnitudeOne()
	{
		var percentage = Mathf.Clamp(Magnitude / 1.99, 0, 1);
		var bxa = GD.RandRange( - 5, 5) / 100;
		var bya = GD.RandRange( - 5, 5) / 100;
		var mxa = (GD.RandRange( - 4, 4) / 100) * percentage;
		var mya = (GD.RandRange( - 4, 4) / 100) * percentage;
		var xa = bxa + mxa;
		var ya = bya + mya;
		foreach(Node v in GetTree().GetNodesInGroup("player"))
		{
			if(v.IsOnFloor())
			{
				SendClientsideEffects(v, 0.1);
			}
		}

		DoPhysics();
	}

	public void MagnitudeTwo()
	{
		var percentage = Mathf.Clamp(Magnitude / 2.99, 0, 1);
		var bxa = GD.RandRange( - 10, 10) / 100;
		var bya = GD.RandRange( - 10, 10) / 100;
		var mxa = (GD.RandRange( - 5, 5) / 100) * percentage;
		var mya = (GD.RandRange( - 5, 5) / 100) * percentage;
		var xa = bxa + mxa;
		var ya = bya + mya;
		foreach(Node v in GetTree().GetNodesInGroup("player"))
		{
			if(v.IsOnFloor())
			{
				SendClientsideEffects(v, 0.2);
			}
		}
		DoPhysics();
	}

	public void MagnitudeThree()
	{
		var percentage = Mathf.Clamp(Magnitude / 3.99, 0, 1);
		var bxa = GD.RandRange( - 15, 15) / 100;
		var bya = GD.RandRange( - 15, 15) / 100;
		var mxa = (GD.RandRange( - 5, 5) / 100) * percentage;
		var mya = (GD.RandRange( - 5, 5) / 100) * percentage;
		var xa = bxa + mxa;
		var ya = bya + mya;
		foreach(Node v in GetTree().GetNodesInGroup("players"))
		{
			if(v.IsOnFloor())
			{
				SendClientsideEffects(v, 0.3);
			}
		}
		DoPhysics();
	}

	public void MagnitudeFour()
	{
		var percentage = Mathf.Clamp(Magnitude / 4.99, 0, 1);
		var bxa = GD.RandRange( - 20, 20) / 100;
		var bya = GD.RandRange( - 20, 20) / 100;
		var mxa = (GD.RandRange( - 5, 5) / 100) * percentage;
		var mya = (GD.RandRange( - 5, 5) / 100) * percentage;
		var xa = bxa + mxa;
		var ya = bya + mya;
		foreach(Node v in GetTree().GetNodesInGroup("players"))
		{
			if(v.IsOnFloor())
			{
				SendClientsideEffects(v, 0.4);
			}
		}
		DoPhysics();
	}

	public void MagnitudeFive()
	{
		var percentage = Mathf.Clamp(Magnitude / 5.99, 0, 1);
		var bxa = GD.RandRange( - 25, 25) / 100;
		var bya = GD.RandRange( - 25, 25) / 100;
		var mxa = (GD.RandRange( - 5, 5) / 100) * percentage;
		var mya = (GD.RandRange( - 5, 5) / 100) * percentage;
		var xa = bxa + mxa;
		var ya = bya + mya;
		foreach(Node v in GetTree().GetNodesInGroup("player"))
		{
			if(v.IsOnFloor())
			{
				SendClientsideEffects(v, 0.5);
			}
		}
		DoPhysics();
	}

	public void MagnitudeSix()
	{
		var percentage = Mathf.Clamp(Magnitude / 6.99, 0, 1);
		var bxa = GD.RandRange( - 30, 30) / 100;
		var bya = GD.RandRange( - 30, 30) / 100;
		var mxa = (GD.RandRange( - 5, 5) / 100) * percentage;
		var mya = (GD.RandRange( - 5, 5) / 100) * percentage;
		var xa = bxa + mxa;
		var ya = bya + mya;
		foreach(Node v in GetTree().GetNodesInGroup("player"))
		{
			if(v.IsOnFloor())
			{
				SendClientsideEffects(v, 2);
			}
		}
		DoPhysics();
	}

	public void MagnitudeSeven()
	{
		var percentage = Mathf.Clamp(Magnitude / 7.99, 0, 1);
		var bxa = GD.RandRange( - 35, 35) / 100;
		var bya = GD.RandRange( - 35, 35) / 100;
		var mxa = (GD.RandRange( - 5, 5) / 100) * percentage;
		var mya = (GD.RandRange( - 5, 5) / 100) * percentage;
		var xa = bxa + mxa;
		var ya = bya + mya;
		foreach(Node v in GetTree().GetNodesInGroup("player"))
		{
			if(v.IsOnFloor())
			{
				SendClientsideEffects(v, 4);
			}
		}
		DoPhysics();
	}

	public void MagnitudeEight()
	{
		var percentage = Mathf.Clamp(Magnitude / 8.99, 0, 1);
		var bxa = GD.RandRange( - 40, 40) / 100;
		var bya = GD.RandRange( - 40, 40) / 100;
		var mxa = (GD.RandRange( - 5, 5) / 100) * percentage;
		var mya = (GD.RandRange( - 5, 5) / 100) * percentage;
		var xa = bxa + mxa;
		var ya = bya + mya;
		foreach(Node v in GetTree().GetNodesInGroup("player"))
		{
			if(v.IsOnFloor())
			{
				SendClientsideEffects(v, 8);
			}
		}
		DoPhysics();
	}

	public void MagnitudeNine()
	{
		var percentage = Mathf.Clamp(Magnitude / 9.99, 0, 1);
		var bxa = GD.RandRange( - 45, 45) / 100;
		var bya = GD.RandRange( - 45, 45) / 100;
		var mxa = (GD.RandRange( - 5, 5) / 100) * percentage;
		var mya = (GD.RandRange( - 5, 5) / 100) * percentage;
		var xa = bxa + mxa;
		var ya = bya + mya;
		foreach(Node v in GetTree().GetNodesInGroup("player"))
		{
			if(v.IsOnFloor())
			{
				SendClientsideEffects(v, 16);
			}
		}
		DoPhysics();
	}

	public void MagnitudeTen()
	{
		var percentage = Mathf.Clamp(Magnitude / 10.99, 0, 1);
		var bxa = GD.RandRange( - 50, 50) / 100;
		var bya = GD.RandRange( - 50, 50) / 100;
		var mxa = (GD.RandRange( - 5, 5) / 100) * percentage;
		var mya = (GD.RandRange( - 5, 5) / 100) * percentage;
		var xa = bxa + mxa;
		var ya = bya + mya;
		foreach(Node v in GetTree().GetNodesInGroup("player"))
		{
			if(v.IsOnFloor())
			{
				SendClientsideEffects(v, 38);
			}
		}
		DoPhysics();
	}

	public void MagnitudeEleven()
	{
		var percentage = Mathf.Clamp(Magnitude / 11.99, 0, 1);
		var bxa = GD.RandRange( - 55, 55) / 100;
		var bya = GD.RandRange( - 55, 55) / 100;
		var mxa = (GD.RandRange( - 5, 5) / 100) * percentage;
		var mya = (GD.RandRange( - 5, 5) / 100) * percentage;
		var xa = bxa + mxa;
		var ya = bya + mya;
		foreach(Node v in GetTree().GetNodesInGroup("player"))
		{
			if(v.IsOnFloor())
			{
				SendClientsideEffects(v, 38);
			}
		}
		DoPhysics();
	}

	public void MagnitudeTwelve()
	{
		var percentage = Mathf.Clamp(Magnitude / 12.99, 0, 1);
		var bxa = GD.RandRange( - 1250, 1250) / 100;
		var bya = GD.RandRange( - 1250, 1250) / 100;
		var mxa = (GD.RandRange( - 425, 425) / 100) * percentage;
		var mya = (GD.RandRange( - 425, 425) / 100) * percentage;
		var xa = bxa + mxa;
		var ya = bya + mya;
		foreach(Node v in GetTree().GetNodesInGroup("player"))
		{
			if(v.IsOnFloor())
			{
				SendClientsideEffects(v, 38);
			}
		}
		DoPhysics();
	}


}