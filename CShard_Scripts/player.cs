using Godot;
using Godot.Collections;

[GlobalClass]
public partial class Player : CharacterBody3D
{
	[Export] public int PlayerId = 1;
	[Export] public string Username = "Player";
	[Export] public int Points = 0;

	public int SPEED = 0;

	public const double SPEED_RUN = 25.0;
	public const double SPEED_WALK = 15.0;
	public const double SPEED_NOCLIP = 100.0;
	public const double JUMP_VELOCITY = 14.0;
	public const double SENSIBILITY = 0.02;
	public const double LERP_VAL = 0.15;

	public const double BobFreq = 2.0;
	public const double BobAm = 0.08;
	[Export] public double TBob = 0.0;

	[Export] public double Mass = 0.5;


	public int MaxHearth = 100;
	public int MaxTemp = 44;
	public int MaxOxygen = 100;
	public int MaxBradiation = 100;

	[Export] public int FallStrength = 0;


	public int MinHearth = 0;
	public int MinTemp = 24;
	public int MinOxygen = 0;
	public int MinBdradiation = 0;


	[Export] public double Hearth = 100;

	[Export] public double BodyTemperature = 37;
	[Export] public double BodyOxygen = 100;
	[Export] public double BodyBradiation = 0;
	[Export] public double BodyWind = 0;

	[Export] public bool Outdoor = false;
	[Export] public bool IsInWater = false;
	[Export] public bool IsInLava = false;
	[Export] public bool IsUnderWater = false;
	[Export] public bool IsUnderLava = false;
	[Export] public bool IsOnFire = false;
	[Export] public bool IsAlive = true;

	[Export] public double SwimFactor = 0.25;
	[Export] public double SwimCap = 50;

	public GpuParticles3D RainNode;
	public GpuParticles3D SplashNode;
	public GpuParticles3D DustNode;
	public GpuParticles3D SandNode;
	public GpuParticles3D SnowNode;
	public Control PauseMenuNode;
	public AnimationPlayer AnimationplayerNode;
	public AnimationTree AnimationTreeNode;
	public Camera3D CameraNode;
	public Node3D HeadNode;
	public Node3D HandNode;
	public Node3D EsqueletoNode;
	public Node Label;
	public ColorRect TempEffect;
	public Control DeathMenu;
	public GpuParticles3D FireParticles;

	public AudioStreamPlayer3D SneezeAudio;
	public GpuParticles3D Sneeze;

	public AudioStreamPlayer3D VomitAudio;
	public GpuParticles3D Vomit;

	public Control Underwatereffect;
	public Control Underlavaeffect;


	public AudioStreamPlayer3D RainSound;
	public AudioStreamPlayer3D WindSound;
	public AudioStreamPlayer3D WindModerateSound;
	public AudioStreamPlayer3D WindExtremeSound;

	public RayCast3D Interactor;
	public SpotLight3D SpotLight3D;
	public Node Spawn;

	public Skeleton3D Skeleton;
	public PhysicalBoneSimulator3D SkeletonPhy;
	public CollisionShape3D Capsule;
	public Variant Mesh;


	// Hueso f�sico de referencia para el ragdoll (cerca del cuello/torso)
	public Node3D RagdollFollowBone;


	// �ndice del hueso de la cabeza para seguir en ragdoll
	public int HeadBoneIndex =  - 1;


	// Transforms originales de cabeza y c�mara para restaurar al revivir / salir del ragdoll
	public Transform3D HeadDefaultTransform;
	public Transform3D HeadDefaultLocalTransform;
	public Transform3D CameraDefaultTransform;

	// Transform local original de la c�mara (offset respecto al padre/head)
	public Transform3D CameraDefaultLocalTransform;

	[Export] public bool Noclip = false;
	[Export] public bool GodMode = false;
	[Export] public bool AdminMode = false;
	[Export] public bool RagdollEnabled = false;

	[Export] public string Character = "blue";
	protected string _LastAppliedCharacter = "";
	[Export] public Array PlayerMaterials = new Array{/* preload has no equivalent, add a 'ResourcePreloader' Node in your scene */("res://Materials/player blue.tres"), /* preload has no equivalent, add a 'ResourcePreloader' Node in your scene */("res://Materials/player red.tres"), /* preload has no equivalent, add a 'ResourcePreloader' Node in your scene */("res://Materials/player green.tres"), /* preload has no equivalent, add a 'ResourcePreloader' Node in your scene */("res://Materials/player yellow.tres"), };

	public override void _EnterTree()
	{
		PlayerId = int.Parse(Name.ToString());
		Globals.PrintRole("set authority to: " + Name);
		SetMultiplayerAuthority(PlayerId);
	}

	public override void _ExitTree()
	{
		Input.SetMouseMode(Input.MouseModeEnum.Visible);
	}


	protected void _SetAdminMode(bool enable)
	{
		AdminMode = enable;
		if(Multiplayer.IsServer())
		{
			Globals.PrintRole($"Admin mode cambiado para {Username}: {enable}");
		}
	}

	protected void _SetRagdollState(bool enable)
	{
		RagdollEnabled = enable;


		// Propiedades que afectan al servidor de fisica -> tambi�n deferidas por seguridad
		if(SkeletonPhy != null)
		{
			SkeletonPhy.SetDeferred("active", enable);
		}

		if(AnimationTreeNode != null)
		{
			AnimationTreeNode.SetDeferred("active", !enable);
		}

		if(AnimationplayerNode != null)
		{
			AnimationplayerNode.SetDeferred("active", !enable);
		}

		if(Capsule != null)
		{
			Capsule.SetDeferred("disabled", enable);
		}


		// Iniciar/parar la simulaci�n f�sica � tambi�n lo deferimos para evitar condiciones
		if(enable)
		{
			_StartPhysicalBonesSim();
		}
		else
		{
			_StopPhysicalBonesSim();

			// Al salir del ragdoll, restaurar la posici�n/rotaci�n de la cabeza y la c�mara
			if(HeadNode != null)
			{
				HeadNode.Transform = HeadDefaultLocalTransform;
			}
			if(CameraNode != null)
			{
				CameraNode.Transform = CameraDefaultLocalTransform;
			}
		}
	}

	protected void _StartPhysicalBonesSim()
	{
		if(SkeletonPhy != null)
		{
			SkeletonPhy.PhysicalBonesStartSimulation();
		}
	}

	protected void _StopPhysicalBonesSim()
	{
		if(SkeletonPhy != null)
		{
			SkeletonPhy.PhysicalBonesStopSimulation();
		}
	}

	protected void _UpdateCameraFollowRagdoll()
	{

		// 1) Prioridad: seguir un hueso F�SICO (PhysicalBone3D), que s� se mueve con el ragdoll
		if(RagdollFollowBone && CameraNode)
		{
			var bone_transform = RagdollFollowBone.GlobalTransform;

			// Posici�n: misma posici�n relativa que la c�mara viva, pero rotaci�n original (para que no mire al suelo)
			var local_origin = CameraDefaultLocalTransform.Origin;
			var target_position = bone_transform * local_origin;
			CameraNode.GlobalPosition = target_position;
			CameraNode.GlobalBasis = CameraDefaultTransform.Basis;
			return ;
		}


		// 2) Fallback: si por alguna raz�n no hay hueso f�sico, usar el hueso "cuello" del Skeleton
		if(Skeleton && HeadBoneIndex >= 0 && CameraNode)
		{
			var bone_global_pose = Skeleton.GetBoneGlobalPose(HeadBoneIndex);
			var bone_world_transform = Skeleton.GlobalTransform * bone_global_pose;

			var local_origin2 = CameraDefaultLocalTransform.Origin;
			var target_position2 = bone_world_transform * local_origin2;
			CameraNode.GlobalPosition = target_position2;
			CameraNode.GlobalBasis = CameraDefaultTransform.Basis;
		}
	}


	public void Damage(double amount)
	{
		if(GodMode)
		{
			return ;
		}

		if(!IsAlive)
		{
			return ;
		}

		Hearth = Mathf.Clamp(Hearth - amount, MinHearth, MaxHearth);
		Globals.PrintRole($"damage applied:{amount}, hearth now:{Hearth}");

		if(Hearth <= 0)
		{
			IsAlive = false;


			// Solo ejecutar die() y quitar puntos en la instancia local del jugador que muri�
			if(IsMultiplayerAuthority())
			{
				Die();
				Globals.RemovePoints();
			}

			RpcMethod(nameof(_SetRagdollState), true);
		}

		else
		{
			IsAlive = true;
		}
	}


	public void Die()
	{
		Input.SetMouseMode(Input.MouseModeEnum.Visible);
		if(DeathMenu != null)
		{
			DeathMenu.Show();
		}
	}

	public async void Ignite(Variant time)
	{
		IsOnFire = true;
		await ToSignal(GetTree().CreateTimer(time), "Timeout");
		IsOnFire = false;
	}

	public void sneeze()
	{
		SneezeAudio.Play();
		Sneeze.Emitting = true;
	}

	public void vomit()
	{
		VomitAudio.Play();
		Vomit.Emitting = true;
	}


	// Funci�n para verificar si hay jugadores con el mismo nombre
	public bool HayJugadoresConMismoNombre(string nombre_a_verificar, bool excluir_este_jugador = true)
	{
		var contador = 0;
		foreach(Node player in GetTree().GetNodesInGroup("player"))
		{

			// Si se debe excluir este jugador, saltarlo
			if(excluir_este_jugador && player == this)
			{
				continue;
			}


			// Verificar si el nombre coincide
			if(player.Username == nombre_a_verificar)
			{
				contador += 1;

				// Si encontramos al menos uno con el mismo nombre, retornar true
				if(contador >= 1)
				{
					return true;
				}
			}
		}

		return false;
	}


	// Funci�n para obtener todos los jugadores que tienen el mismo nombre
	public Godot.Collections.Array ObtenerJugadoresConMismoNombre(string nombre_a_verificar, bool excluir_este_jugador = true)
	{
		var jugadores_duplicados = new Godot.Collections.Array{};

		foreach(Node player in GetTree().GetNodesInGroup("player"))
		{

			// Si se debe excluir este jugador, saltarlo
			if(excluir_este_jugador && player == this)
			{
				continue;
			}


			// Verificar si el nombre coincide
			if(player.Username == nombre_a_verificar)
			{
				jugadores_duplicados.Add(player);
			}
		}

		return jugadores_duplicados;
	}

	public override void _Ready()
	{
		RainNode = GetNode<GpuParticles3D>("Rain");
		SplashNode = GetNode<GpuParticles3D>("splash");
		DustNode = GetNode<GpuParticles3D>("Dust");
		SandNode = GetNode<GpuParticles3D>("Sand");
		SnowNode = GetNode<GpuParticles3D>("Snow");
		PauseMenuNode = GetNode<Control>("Pause menu");
		AnimationplayerNode = GetNode<AnimationPlayer>("AnimationPlayer");
		AnimationTreeNode = GetNode<AnimationTree>("AnimationTree");
		CameraNode = GetNode<Camera3D>("head/Camera3D");
		HeadNode = GetNode<Node3D>("head");
		HandNode = GetNode<Node3D>("head/hand");
		EsqueletoNode = GetNode<Node3D>("Esqueleto");
		Label = GetNode("Name");
		TempEffect = GetNode<ColorRect>("Temp_Effect");
		DeathMenu = GetNode<Control>("Death Menu");
		FireParticles = GetNode<GpuParticles3D>("Fire");
		SneezeAudio = GetNode<AudioStreamPlayer3D>("head/Camera3D/sneeze audio");
		Sneeze = GetNode<GpuParticles3D>("head/Camera3D/Sneeze");
		VomitAudio = GetNode<AudioStreamPlayer3D>("head/Camera3D/VomitAudio");
		Vomit = GetNode<GpuParticles3D>("head/Camera3D/Vomit");
		Underwatereffect = GetNode<Control>("Underwater");
		Underlavaeffect = GetNode<Control>("UnderLava");
		RainSound = GetNode<AudioStreamPlayer3D>("Rain sound");
		WindSound = GetNode<AudioStreamPlayer3D>("Wind sound");
		WindModerateSound = GetNode<AudioStreamPlayer3D>("Wind Morerate sound");
		WindExtremeSound = GetNode<AudioStreamPlayer3D>("Wind Extreme sound");
		Interactor = GetNode<RayCast3D>("head/Camera3D/Interactor");
		SpotLight3D = GetNode<SpotLight3D>("head/Camera3D/SpotLight3D");
		Spawn = GetNode("../Spawn");
		Skeleton = GetNode<Skeleton3D>("Esqueleto/Skeleton3D");
		SkeletonPhy = GetNode<PhysicalBoneSimulator3D>("Esqueleto/Skeleton3D/PhysicalBoneSimulator3D");
		Capsule = GetNode("CollisionShape3D");
		Mesh = GetNode("Esqueleto/Skeleton3D/human");
		RagdollFollowBone = GetNode("Esqueleto/Skeleton3D/PhysicalBoneSimulator3D/Physical Bone clumna3");
		RainNode.Emitting = false;
		SandNode.Emitting = false;
		SplashNode.Emitting = false;
		DustNode.Emitting = false;
		SnowNode.Emitting = false;


		Globals.PrintRole($"player name: {int.Parse(Name.ToString())}");
		Globals.PrintRole($"is authority: {IsMultiplayerAuthority()}");
		Globals.PrintRole($"get authority: {GetMultiplayerAuthority()}");

		CameraNode.Current = IsMultiplayerAuthority();


		// Guardar transform original de la cabeza y de la c�mara
		if(HeadNode)
		{
			HeadDefaultTransform = HeadNode.GlobalTransform;
			HeadDefaultLocalTransform = HeadNode.Transform;
		}
		if(CameraNode)
		{
			CameraDefaultTransform = CameraNode.GlobalTransform;
			CameraDefaultLocalTransform = CameraNode.Transform;
		}


		// Obtener el �ndice del hueso "cuello" para seguir en ragdoll
		if(Skeleton)
		{
			HeadBoneIndex = Skeleton.FindBone("cuello");

			// Si por alguna raz�n no lo encuentra, usar un �ndice conocido del esqueleto (9 = cuello en la escena)
			if(HeadBoneIndex ==  - 1 && Skeleton.GetBoneCount() > 9)
			{
				HeadBoneIndex = 9;
			}
		}

		if(IsMultiplayerAuthority())
		{
			Globals.LocalPlayer = this;
			Input.SetMouseMode(Input.MouseMode.MouseModeCaptured);
			_ResetPlayer();
		RpcMethod(nameof(_SetRagdollState), false);


			// Verificar si hay jugadores con el mismo nombre y a�adir n�mero si es necesario
			var nombre_base = Globals.Username;
			var contador = 0;

			foreach(Node player in GetTree().GetNodesInGroup("player"))
			{

				// Saltar el jugador actual
				if(player == this)
				{
					continue;
				}


				// Verificar si el nombre coincide (sin contar n�meros a�adidos)
				var player_username = player.Username;
				if(player_username == nombre_base || player_username.BeginsWith(nombre_base))
				{
					contador += 1;
				}
			}


			// Si hay duplicados, a�adir n�mero al nombre
			if(contador > 0)
			{
				Globals.Username = nombre_base + (contador + 1).ToString();
				Username = Globals.Username;
			}

			if(Multiplayer.IsServer())
			{
				AdminMode = true;
			}
		}
	}


	public void BodyTemp(double delta)
	{
		if(GodMode)
		{
			return ;
		}

		var body_heat_genK = delta;
		var body_heat_genMAX = 0.01 / 4;
		var fire_heat_emission = 50;

		var heatscale = 0;
		var coolscale = 0;

		var core_equilibrium = Mathf.Clamp((37 - BodyTemperature) * body_heat_genK,  - body_heat_genMAX, body_heat_genMAX);
		var heatsource_equilibrium = Mathf.Clamp((fire_heat_emission * (heatscale)) * body_heat_genK, 0, body_heat_genMAX * 1.3);
		var coldsource_equilibrium = Mathf.Clamp((fire_heat_emission * (coolscale)) * body_heat_genK, body_heat_genMAX *  - 1.3, 0);

		var ambient_equilibrium = Mathf.Clamp(((Globals.Temperature - BodyTemperature) * body_heat_genK),  - body_heat_genMAX * 1.1, body_heat_genMAX * 1.1);

		if(Globals.Temperature >= 5 && Globals.Temperature <= 37)
		{
			ambient_equilibrium = 0;
		}

		BodyTemperature = Mathf.Clamp(BodyTemperature + core_equilibrium + heatsource_equilibrium + coldsource_equilibrium + ambient_equilibrium, MinTemp, MaxTemp);
		TempEffect.Material.SetShaderParameter("temp", BodyTemperature);
		TempEffect.Material.SetShaderParameter("Temp", BodyTemperature);

		var alpha_hot = 1 - ((44 - Mathf.Clamp(BodyTemperature, 39, 44)) / 5);
		var alpha_cold = ((35 - Mathf.Clamp(BodyTemperature, 24, 35)) / 11);

		if(GD.RandRange(1, 25) == 25)
		{
			if(alpha_cold != 0)
			{
				RpcMethod(nameof(Damage), alpha_hot + alpha_cold);
			}
			else if(alpha_hot != 0)
			{
				RpcMethod(nameof(Damage), alpha_hot + alpha_cold);
			}
		}


		if(BodyTemperature > 39 && GD.Randi() % 400 == 0)
		{
			vomit();
		}

		if(BodyTemperature < 35 && GD.Randi() % 400 == 0)
		{
			sneeze();
		}
	}

	public void BodyOxy(double delta)
	{
		if(GodMode)
		{
			return ;
		}

		if(Globals.Oxygen <= 20 || Globals.IsInwater(this) || IsUnderWater || Globals.IsInlava(this) || IsUnderLava)
		{
			BodyOxygen = Mathf.Clamp(BodyOxygen - 5 * delta, MinOxygen, MaxOxygen);
		}
		else
		{
			BodyOxygen = Mathf.Clamp(BodyOxygen + 5 * delta, MinOxygen, MaxOxygen);
		}


		if(BodyOxygen <= 0)
		{
			if(GD.RandRange(1, 25) == 25)
			{
				RpcMethod(nameof(Damage), GD.RandRange(1, 30));
			}
		}
	}

	public void BodyRad(double delta)
	{
		if(GodMode)
		{
			return ;
		}

		if(Globals.Bradiation >= 80 && Globals.IsOutdoor(this) && Outdoor)
		{
			BodyBradiation = Mathf.Clamp(BodyBradiation + 5 * delta, MinBdradiation, MaxBradiation);
		}
		else
		{
			BodyBradiation = Mathf.Clamp(BodyBradiation - 5 * delta, MinBdradiation, MaxBradiation);
		}

		if(BodyBradiation >= 100)
		{
			if(GD.RandRange(1, 25) == 25)
			{
				RpcMethod(nameof(Damage), GD.RandRange(1, 30));
			}
		}
	}

	public void UpdateCharacter()
	{

		// Determinar el personaje deseado: si no somos autoridad, usamos el dict sincronizado.
		var desired_char = Character;
		if(!IsMultiplayerAuthority())
		{
			if(Globals.AssignedCharacter.Has(PlayerId))
			{
				desired_char = Globals.AssignedCharacter[PlayerId];
			}
		}

		if(desired_char == null || desired_char == "" || desired_char == _LastAppliedCharacter)
		{
			return ;
		}

		_LastAppliedCharacter = desired_char;
		Character = desired_char;

		if(desired_char == "blue")
		{
			UpdateMaterial(0);
		}
		else if(desired_char == "red")
		{
			UpdateMaterial(1);
		}
		else if(desired_char == "green")
		{
			UpdateMaterial(2);
		}
		else if(desired_char == "yellow")
		{
			UpdateMaterial(3);
		}
		else
		{
			UpdateMaterial(0);
		}
	}

	public void UpdateMaterial(Variant index)
	{
		if(!Mesh)
		{
			return ;
		}


		// MeshInstance3D usa overrides de superficie; aplicamos a las tres superficies.
		Mesh.SetSurfaceOverrideMaterial(0, PlayerMaterials[index]);
		Mesh.SetSurfaceOverrideMaterial(1, PlayerMaterials[index]);
		Mesh.SetSurfaceOverrideMaterial(2, PlayerMaterials[index]);
	}

	public void UnderwaterOrUnderlavaEffects()
	{
		Underwatereffect.Visible = IsUnderWater;
		Underlavaeffect.Visible = IsUnderLava;

		if(IsInLava)
		{
			Ignite(10);
		}

		if(IsInWater)
		{
			if(IsOnFire)
			{
				IsOnFire = false;
			}
		}
	}

	public void IsOnFireEffects()
	{
		FireParticles.Emitting = IsOnFire;
		if(IsOnFire)
		{
			if(GD.RandRange(1, 5) == 5)
			{
				RpcMethod(nameof(Damage), 5);
			}
		}
	}

	public override void _Input(InputEvent ev)
	{
		if(!IsMultiplayerAuthority())
		{
			return ;
		}

		// Bloquear input cuando el chat est� abierto
		// Verificar tanto la variable global como si alg�n LineEdit tiene foco
		var chat_node = GetTree().GetRoot().FindChild("Chat", true, false);
		if(chat_node != null)
		{
			var line_edit = chat_node.GetNodeOrNull("Panel/Panel2/LineEdit");
			if(line_edit != null && line_edit.HasFocus())
			{
				return ;
			}
		}

		if(Globals.IsChatOpen)
		{
			return ;
		}

		if(ev is InputEventKey keyEvent && keyEvent.Pressed && !keyEvent.Echo)
		{
			if(!AdminMode)
			{
				return ;
			}

			if(Globals.Gamemode != "sandbox")
			{
				return ;
			}

			if(keyEvent.Keycode == KEY_1)
			{
				Globals.SetWeatherAndDisaster.Rpc(1);
			}
			if(keyEvent.Keycode == KEY_2)
			{
				Globals.SetWeatherAndDisaster.Rpc(2);
			}
			if(keyEvent.Keycode == KEY_3)
			{
				Globals.SetWeatherAndDisaster.Rpc(3);
			}
			if(keyEvent.Keycode == KEY_4)
			{
				Globals.SetWeatherAndDisaster.Rpc(4);
			}
			if(keyEvent.Keycode == KEY_5)
			{
				Globals.SetWeatherAndDisaster.Rpc(5);
			}
			if(keyEvent.Keycode == KEY_6)
			{
				Globals.SetWeatherAndDisaster.Rpc(6);
			}
			if(keyEvent.Keycode == KEY_7)
			{
				Globals.SetWeatherAndDisaster.Rpc(7);
			}
			if(keyEvent.Keycode == KEY_8)
			{
				Globals.SetWeatherAndDisaster.Rpc(8);
			}
			if(keyEvent.Keycode == KEY_9)
			{
				Globals.SetWeatherAndDisaster.Rpc(9);
			}
			if(keyEvent.Keycode == KEY_0)
			{
				Globals.SetWeatherAndDisaster.Rpc(0);
			}
		}
	}

	public void rainsound()
	{
		Globals.IsRaining = RainNode.Emitting && Globals.IsOutdoor(this) && Outdoor;
		if(Globals.IsRaining)
		{
			if(!RainSound.Playing)
			{
				RainSound.Play();
			}
		}
		else
		{
			RainSound.Stop();
		}
	}

	public void windsound()
	{
		if(BodyWind > 0 && BodyWind <= 50)
		{
			if(!WindSound.Playing)
			{
				WindSound.Play();
				WindModerateSound.Stop();
				WindModerateSound.Stop();
			}
		}
		else if(BodyWind > 50 && BodyWind <= 100)
		{
			if(!WindModerateSound.Playing)
			{
				WindSound.Stop();
				WindModerateSound.Play();
				WindExtremeSound.Stop();
			}
		}
		else if(BodyWind > 100)
		{
			if(!WindExtremeSound.Playing)
			{
				WindSound.Stop();
				WindModerateSound.Stop();
				WindExtremeSound.Play();
			}
		}
		else
		{
			WindSound.Stop();
			WindModerateSound.Stop();
			WindExtremeSound.Stop();
		}
	}


	public override void _Process(double delta)
	{
		UpdateCharacter();

		// tambi�n para clientes no autoridad (solo material)
		if(!IsMultiplayerAuthority())
		{
			return ;
		}

		BodyTemp(delta);
		BodyOxy(delta);
		BodyRad(delta);
		UnderwaterOrUnderlavaEffects();
		IsOnFireEffects();
		RainSound();
		WindSound();
		UpdateLabels();
	}

	public void UpdateLabels()
	{
		if(!IsMultiplayerAuthority())
		{
			return ;
		}

		Username = Globals.Username;
		Points = Globals.Points;
		Label.Text = Globals.Username;
	}

	public override void _PhysicsProcess(double delta)
	{
		if(!IsMultiplayerAuthority())
		{
			return ;
		}

		if(Globals.IsPauseMenuOpen)
		{
			return ;
		}

		if(Globals.IsChatOpen)
		{
			return ;
		}


		// Hacer que la c�mara siga al cuerpo en ragdoll
		if(RagdollEnabled)
		{
			_UpdateCameraFollowRagdoll();
			return ;

			// No procesar movimiento cuando el ragdoll est� activo

		}// Add the gravity.
		if(!Noclip)
		{
			if(!IsOnFloor())
			{
				if(IsInWater || IsInLava)
				{
					Velocity.Y = Globals.Gravity * delta * SwimFactor;
				}
				else
				{

					// Si est� cayendo, aplica m�s gravedad
					if(Velocity.Y < 0)
					{
						Velocity.Y -= Globals.Gravity * delta;
					}
					else
					{
						Velocity.Y -= Globals.Gravity * delta;
					}

					FallStrength = Velocity.Y;
				}
			}
			else
			{
				if(!(IsInWater || IsInLava))
				{
					if(FallStrength <=  - 90)
					{
						damage.Rpc(50);
					}
				}
			}
		}
		else
		{
			Velocity.Y = 0;
		}


		// Handle jump.
		if(Input.IsActionJustPressed("Jump"))
		{
			if(IsOnFloor())
			{
				Velocity.Y = JUMP_VELOCITY;
			}

			if(IsInWater || IsInLava)
			{
				Velocity.Y += JUMP_VELOCITY;
			}
		}


		if(Input.IsActionJustPressed("Flashligh"))
		{
			SpotLight3D.Visible = !SpotLight3D.Visible;
		}

		if(Input.IsActionPressed("Spring"))
		{
			SPEED = SPEED_RUN;
		}
		else
		{
			SPEED = SPEED_WALK;


			// Get the input direction and handle the movement/deceleration.

		}// As good practice, you should replace UI actions with custom gameplay actions.
		var input_dir = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
		var input_vector = new Vector3(input_dir.X, 0, input_dir.Y);
		var direction = (HeadNode.Transform.Basis * input_vector).Normalized();

		if(Noclip)
		{

			SPEED = SPEED_NOCLIP;


			// Movimiento directo en noclip (vuelo libre)
			var desired_velocity = direction * SPEED;


			// Control vertical en noclip
			if(Input.IsActionPressed("Jump"))
			{

				desired_velocity.Y = SPEED;
			}
			else if(Input.IsActionPressed("down"))
			{
				desired_velocity.Y =  - SPEED;
			}
			else
			{
				desired_velocity.Y = 0;
			}


			// Asignar directamente la velocidad (sin gravedad ni lerp)
			Velocity = desired_velocity;
		}
		else
		{

			// L�gica normal cuando no es noclip
			if(IsOnFloor())
			{
				if(direction)
				{
					Velocity.X = direction.X * SPEED;
					Velocity.Z = direction.Z * SPEED;
				}
				else
				{
					Velocity.X = Mathf.Lerp(Velocity.X, direction.X * SPEED, delta * 7.0);
					Velocity.Z = Mathf.Lerp(Velocity.Z, direction.Z * SPEED, delta * 7.0);
				}
			}
			else
			{
				Velocity.X = Mathf.Lerp(Velocity.X, direction.X * SPEED, delta * 3.0);
				Velocity.Z = Mathf.Lerp(Velocity.Z, direction.Z * SPEED, delta * 3.0);
			}
		}


		var horizontal_velocity = new Vector2(Velocity.X, Velocity.Z);

		AnimationTreeNode.Set("parameters/conditions/is_falling", !IsOnFloor() && Velocity.Y < 0);
		AnimationTreeNode.Set("parameters/conditions/is_jumping", Velocity.Y > 0);
		AnimationTreeNode.Set("parameters/conditions/is_swiming", IsInWater || IsInLava);
		AnimationTreeNode.Set("parameters/conditions/is_idle", IsOnFloor() && horizontal_velocity.Length() < 0.1);
		AnimationTreeNode.Set("parameters/conditions/is_walking", IsOnFloor() && horizontal_velocity.Length() > 0.1);

		if(Interactor.IsColliding())
		{
			var target = Interactor.GetCollider();
			if(target != null && target.HasMethod("Interact"))
			{
				if(Input.IsActionJustPressed("Interact"))
				{
					target.Interact();
				}
			}
			else if(target != null && target.IsInGroup("Pickable"))
			{
				if(Input.IsActionPressed("Interact"))
				{
					if(Multiplayer.IsServer())
					{

						// Si somos el servidor/host, llamamos DIRECTO
						Globals.RequestPickObject(GetPath(), target.GetPath());
					}
					else
					{

						// Si somos cliente, usamos RPC hacia el servidor
						Globals.RequestPickObject.Rpc(GetPath(), target.GetPath());
					}
				}
			}
		}

		if(Input.IsActionJustPressed("noclip"))
		{
			if(AdminMode)
			{
				_Noclip();
			}
			else
			{
				Globals.PrintRole("You dont have perms");
			}
		}


		MoveAndSlide();
	}

	protected void _Noclip()
	{
		Noclip = !Noclip;
		if(Noclip)
		{
			Capsule.Disabled = true;
			Velocity.Y = 0;
			FallStrength = 0;
			Globals.PrintRole("Noclip activated");
		}
		else
		{
			Capsule.Disabled = false;
			Globals.PrintRole("Noclip desactivated");
		}
	}


	public override void _UnhandledInput(InputEvent ev)
	{
		if(!IsMultiplayerAuthority())
		{
			return ;
		}

		// No permitir control de cmara cuando el chat est� abierto
		// Verificar tanto la variable global como si algn LineEdit tiene foco
		var chat_node = GetTree().GetRoot().FindChild("Chat", true, false);
		if(chat_node != null)
		{
			var line_edit = chat_node.GetNodeOrNull("Panel/Panel2/LineEdit");
			if(line_edit != null && line_edit.HasFocus())
			{
				return ;
			}
		}

		if(Globals.IsChatOpen)
		{
			return ;
		}

		// No permitir control de cmara cuando el ragdoll est activo
		if(RagdollEnabled)
		{
			return ;
		}

		if(Input.GetMouseMode() == Input.MouseMode.MouseModeCaptured)
		{
			if(ev is InputEventMouseMotion mm)
			{
				CameraNode.Rotation.X -= mm.Relative.Y * SENSIBILITY;
				CameraNode.RotationDegrees.X = Mathf.Clamp(CameraNode.RotationDegrees.X,  - 90, 90);
				HeadNode.Rotation.Y -= mm.Relative.X * SENSIBILITY;
				EsqueletoNode.RotationDegrees.Y = HeadNode.RotationDegrees.Y;
			}
			else if(ev is InputEventJoypadMotion jm)
			{
				if(jm.Axis == 2)
				{
					HeadNode.Rotation.Y += jm.AxisValue * SENSIBILITY;
					EsqueletoNode.RotationDegrees.Y = HeadNode.RotationDegrees.Y;
				}
				else if(jm.Axis == 3)
				{
					CameraNode.Rotation.X += jm.AxisValue * SENSIBILITY;
					CameraNode.RotationDegrees.X = Mathf.Clamp(CameraNode.RotationDegrees.X,  - 90, 90);
				}
			}
		}
	}


	protected void _OnArea3dBodyEntered(Node3D body)
	{
		if(body.IsInGroup("Meteor"))
		{
			RpcMethod(nameof(Damage), 100);
		}
	}


	protected void _OnArea3dBodyExited(Node3D body)
	{
		if(body.IsInGroup("Water_Area"))
		{
			IsInWater = false;
			IsUnderWater = false;
		}
	}


	protected void _OnArea3dAreaEntered(Area3D area)
	{
		if(area.IsInGroup("Explosion"))
		{
			var area_parent = area.GetParent();
			var distance = (area.GlobalPosition - GlobalPosition).Length();
			var direction = (area.GlobalPosition - GlobalPosition).Normalized();


			// Comprobaciones seguras
			if(!area_parent.HasMeta("explosion_force") && area_parent.Contains(!"explosion_force"))
			{
				return ;
			}

			var force = area_parent.ExplosionForce * (1 - distance / area_parent.ExplosionRadius);
			Velocity = direction * force;


			// Da�o seguro (si no existe, asigna 0)
			var damag = 0;
			if(area_parent.Contains("explosion_damage"))
			{
				damag = area_parent.ExplosionDamage;
			}

			if(damag > 0)
			{
				RpcMethod(nameof(Damage), damag);
			}
		}

		else if(area.IsInGroup("Lava_Area"))
		{
			IsInLava = true;


			// Obtener la altura de la lava desde el collider del volc�n
			var collider = area.GetNodeOrNull("CollisionShape3D");
			if(collider && collider.Shape)
			{
				var shape = collider.Shape;

				// Si es una caja (BoxShape3D)
				if(shape is BoxShape3D)
				{
					var lava_surface = area.GlobalPosition.Y + (shape.Size.Y / 2);
					if(CameraNode && CameraNode.GlobalPosition.Y < lava_surface)
					{
						IsUnderLava = true;
					}
					else
					{
						IsUnderLava = false;
					}
				}

				// Si es un cilindro (CylinderShape3D)
				else if(shape is CylinderShape3D)
				{
					var lava_surface = area.GlobalPosition.Y + (shape.Height / 2);
					if(CameraNode && CameraNode.GlobalPosition.Y < lava_surface)
					{
						IsUnderLava = true;
					}
					else
					{
						IsUnderLava = false;
					}
				}

				// Si es una esfera (SphereShape3D)
				else if(shape is SphereShape3D)
				{
					var lava_surface = area.GlobalPosition.Y + shape.Radius;
					if(CameraNode && CameraNode.GlobalPosition.Y < lava_surface)
					{
						IsUnderLava = true;
					}
					else
					{
						IsUnderLava = false;
					}
				}
				else
				{

					// Fallback para otras formas
					if(CameraNode)
					{
						IsUnderLava = true;
					}
				}
			}
			else
			{

				// Sin collider, asumir que est�s bajo la lava
				if(CameraNode)
				{
					IsUnderLava = true;
				}
			}
		}

		else if(area.IsInGroup("Water_Area"))
		{
			IsInWater = true;


			// Obtener la altura del agua desde el collider del tsunami
			var collider = area.GetNodeOrNull("CollisionShape3D");
			if(collider && collider.Shape)
			{
				var shape = collider.Shape;

				// Si es una caja (BoxShape3D)
				if(shape is BoxShape3D)
				{
					var water_surface = area.GlobalPosition.Y + (shape.Size.Y / 2);
					if(CameraNode && CameraNode.GlobalPosition.Y < water_surface)
					{
						IsUnderWater = true;
					}
					else
					{
						IsUnderWater = false;
					}
				}

				// Si es un cilindro (CylinderShape3D)
				else if(shape is CylinderShape3D)
				{
					var water_surface = area.GlobalPosition.Y + (shape.Height / 2);
					if(CameraNode && CameraNode.GlobalPosition.Y < water_surface)
					{
						IsUnderWater = true;
					}
					else
					{
						IsUnderWater = false;
					}
				}

				// Si es una esfera (SphereShape3D)
				else if(shape is SphereShape3D)
				{
					var water_surface = area.GlobalPosition.Y + shape.Radius;
					if(CameraNode && CameraNode.GlobalPosition.Y < water_surface)
					{
						IsUnderWater = true;
					}
					else
					{
						IsUnderWater = false;
					}
				}
				else
				{

					// Fallback para otras formas
					if(CameraNode)
					{
						IsUnderWater = true;
					}
				}
			}
			else
			{

				// Sin collider, asumir que est�s bajo el agua
				if(CameraNode)
				{
					IsUnderWater = true;
				}
			}
		}
	}

	protected void _OnArea3dAreaExited(Area3D area)
	{
		if(area.IsInGroup("Lava_Area"))
		{
			IsInLava = false;
			IsUnderLava = false;
		}

		else if(area.IsInGroup("Water_Area"))
		{
			IsInWater = false;
			IsUnderWater = false;
		}
	}


	protected void _ResetPlayer()
	{
		Hearth = MaxHearth;
		BodyTemperature = 37;
		BodyOxygen = MaxOxygen;
		BodyBradiation = MinBdradiation;
		IsAlive = true;
		IsInWater = false;
		IsInLava = false;
		IsOnFire = false;
		FallStrength = 0;


		if(IsMultiplayerAuthority())
		{
			RpcMethod(nameof(_SetRagdollState), false);
			Position = Spawn.Position;
			Velocity = Vector3.Zero;
		}
	}


}