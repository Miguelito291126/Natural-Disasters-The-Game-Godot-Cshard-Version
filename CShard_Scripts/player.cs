using Godot;
using Godot.Collections;

[GlobalClass]
public partial class Player : CharacterBody3D
{
	[Export] public int PlayerId = 1;
	[Export] public string Username = "Player";
	[Export] public int Points = 0;

	public float SPEED = 0;

	public const float SPEED_RUN = 25.0f;
	public const float SPEED_WALK = 15.0f;
	public const float SPEED_NOCLIP = 100.0f;
	public const float JUMP_VELOCITY = 14.0f;
	public const float SENSIBILITY = 0.3f;
	public const float LERP_VAL = 0.15f;

	public const float BobFreq = 2.0f;
	public const float BobAm = 0.08f;
	[Export] public float TBob = 0.0f;

	[Export] public float Mass = 0.5f;


	public int MaxHearth = 100;
	public int MaxTemp = 44;
	public int MaxOxygen = 100;
	public int MaxBradiation = 100;

	[Export] public float FallStrength = 0f;


	public int MinHearth = 0;
	public int MinTemp = 24;
	public int MinOxygen = 0;
	public int MinBdradiation = 0;


	[Export] public float Hearth = 100.0f;

	[Export] public float BodyTemperature = 37.0f;
	[Export] public float BodyOxygen = 100.0f;
	[Export] public float BodyBradiation = 0.0f;
	[Export] public float BodyWind = 0.0f;

	[Export] public bool Outdoor = false;
	[Export] public bool IsInWater = false;
	[Export] public bool IsInLava = false;
	[Export] public bool IsUnderWater = false;
	[Export] public bool IsUnderLava = false;
	[Export] public bool IsOnFire = false;
	[Export] public bool IsAlive = true;

	[Export] public float SwimFactor = 0.25f;
	[Export] public float SwimCap = 50.0f;

	public GpuParticles3D RainNode;
	public GpuParticles3D SplashNode;
	public GpuParticles3D DustNode;
	public GpuParticles3D SandNode;
	public GpuParticles3D SnowNode;
	public PauseMenu PauseMenuNode;
	public AnimationPlayer AnimationplayerNode;
	public AnimationTree AnimationTreeNode;
	public Camera3d CameraNode;
	public Node3D HeadNode;
	public Node3D HandNode;
	public Node3D EsqueletoNode;
	public Label3D Label;
	public CanvasLayer TempEffect;
	public DeathMenu DeathMenu;
	public GpuParticles3D FireParticles;

	public AudioStreamPlayer3D SneezeAudio;
	public GpuParticles3D Sneeze;

	public AudioStreamPlayer3D VomitAudio;
	public GpuParticles3D Vomit;

	public CanvasLayer Underwatereffect;
	public CanvasLayer Underlavaeffect;


	public AudioStreamPlayer RainSound;
	public AudioStreamPlayer WindSound;
	public AudioStreamPlayer WindModerateSound;
	public AudioStreamPlayer WindExtremeSound;

	public RayCast3D Interactor;
	public SpotLight3D SpotLight3D;
	public Marker3D Spawn;

	public Chat chat_node;

	public Skeleton3D Skeleton;
	public PhysicalBoneSimulator3D SkeletonPhy;
	public CollisionShape3D Capsule;
	public MeshInstance3D Mesh;


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
	[Export] public Array<Material> PlayerMaterials = new Array<Material>{
		(Material)ResourceLoader.Load("res://Materials/player blue.tres"), 
		(Material)ResourceLoader.Load("res://Materials/player red.tres"), 
		(Material)ResourceLoader.Load("res://Materials/player green.tres"), 
		(Material)ResourceLoader.Load("res://Materials/player yellow.tres"), };

	private Vector3 velocity = Vector3.Zero;
	private Vector3 externalForce = Vector3.Zero;

	private Area3D currentWaterArea = null;
	private Area3D currentLavaArea = null;

	public override void _EnterTree()
	{
		PlayerId = int.Parse(Name.ToString());
		Globals.Instance.PrintRole("set authority to: " + Name);
		SetMultiplayerAuthority(PlayerId);
	}

	public override void _ExitTree()
	{
		Callable.From(() => {
			Input.MouseMode = Input.MouseModeEnum.Visible;
		}).CallDeferred();
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	public void _SetAdminMode(bool enable)
	{
		AdminMode = enable;
		if(Multiplayer.IsServer())
		{
			Globals.Instance.PrintRole($"Admin mode cambiado para {Username}: {enable}");
		}
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	protected void _SetRagdollState(bool enable)
	{
		RagdollEnabled = enable;

		// 2. Desactivar el procesamiento de animaciones inmediatamente
		if(AnimationTreeNode != null) AnimationTreeNode.Active = !enable;
		if(AnimationplayerNode != null) AnimationplayerNode.PlaybackActive = !enable;

		// 3. La colisión de la cápsula SÍ puede ser diferida si da problemas, 
		// pero para el reset es mejor intentar directo:
		if(Capsule != null) Capsule.Disabled = enable;

		if(enable)
		{
			_StartPhysicalBonesSim();
		}
		else
		{
			_StopPhysicalBonesSim();

			// Restaurar transforms inmediatamente
			if(HeadNode != null) HeadNode.Transform = HeadDefaultLocalTransform;
			if(CameraNode != null) CameraNode.Transform = CameraDefaultLocalTransform;
		}
	}

	protected void _StartPhysicalBonesSim()
	{
		if(SkeletonPhy != null)
		{
			SkeletonPhy.PhysicalBonesStartSimulation();
		}
	}

	private void _StopPhysicalBonesSim()
	{
		if (SkeletonPhy != null)
		{
			SkeletonPhy.PhysicalBonesStopSimulation();
			
			foreach (var bone in SkeletonPhy.GetChildren())
			{
				if (bone is PhysicalBone3D b)
				{
					b.LinearVelocity = Vector3.Zero;
					b.AngularVelocity = Vector3.Zero;
					// Opcional: b.JointConstraints = false; si usas configuraciones complejas
				}
			}
		}
	}

	protected void _UpdateCameraFollowRagdoll()
	{

		// 1) Prioridad: seguir un hueso F�SICO (PhysicalBone3D), que s� se mueve con el ragdoll
		if(RagdollFollowBone != null && CameraNode != null)
		{
			var bone_transform = RagdollFollowBone.GlobalTransform;

			// Posicin: misma posicin relativa que la cmara viva, pero rotacin original (para que no mire al suelo)
			var local_origin = CameraDefaultLocalTransform.Origin;
			var target_position = bone_transform * local_origin;
			CameraNode.GlobalPosition = target_position;
			CameraNode.GlobalBasis = CameraDefaultTransform.Basis;
			return ;
		}


		// 2) Fallback: si por alguna raz�n no hay hueso f�sico, usar el hueso "cuello" del Skeleton
		if(Skeleton != null && HeadBoneIndex >= 0 && CameraNode != null)
		{
			var bone_global_pose = Skeleton.GetBoneGlobalPose(HeadBoneIndex);
			var bone_world_transform = Skeleton.GlobalTransform * bone_global_pose;

			var local_origin2 = CameraDefaultLocalTransform.Origin;
			var target_position2 = bone_world_transform * local_origin2;
			CameraNode.GlobalPosition = target_position2;
			CameraNode.GlobalBasis = CameraDefaultTransform.Basis;
		}
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer,CallLocal = true)]
	public void Damage(float amount)
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
		Globals.Instance.PrintRole($"damage applied:{amount}, hearth now:{Hearth}");

		if(Hearth <= 0)
		{
			IsAlive = false;


			// Solo ejecutar die() y quitar puntos en la instancia local del jugador que muri�
			if(IsMultiplayerAuthority())
			{
				Die();
				Globals.Instance.RemovePoints();
			}

			Rpc(MethodName._SetRagdollState, true);
		}

		else
		{
			IsAlive = true;
		}
	}


	public void Die()
	{
		Input.MouseMode = Input.MouseModeEnum.Visible;
		if(DeathMenu != null) DeathMenu.Show();

	}

	public async void Ignite(int time)
	{
		IsOnFire = true;
		await ToSignal(GetTree().CreateTimer(time), SceneTreeTimer.SignalName.Timeout);
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
		foreach(Player player in GetTree().GetNodesInGroup("player"))
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
	public Array ObtenerJugadoresConMismoNombre(string nombre_a_verificar, bool excluir_este_jugador = true)
	{
		Array jugadores_duplicados = new Array();

		foreach(Player player in GetTree().GetNodesInGroup("player"))
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
		// 1. RUTAS CORREGIDAS (Basadas en tu .tscn)
		// Usamos GetNodeOrNull para evitar que el juego muera si cambias un nombre en el editor
		RainNode = GetNodeOrNull<GpuParticles3D>("Rain");
		SplashNode = GetNodeOrNull<GpuParticles3D>("splash");
		DustNode = GetNodeOrNull<GpuParticles3D>("Dust");
		SandNode = GetNodeOrNull<GpuParticles3D>("Sand");
		SnowNode = GetNodeOrNull<GpuParticles3D>("Snow");
		FireParticles = GetNodeOrNull<GpuParticles3D>("Fire");
		
		PauseMenuNode = GetNodeOrNull<PauseMenu>("Pause menu");
		AnimationplayerNode = GetNodeOrNull<AnimationPlayer>("AnimationPlayer");
		AnimationTreeNode = GetNodeOrNull<AnimationTree>("AnimationTree");
		
		// Nodos dentro de la jerarquía de la cabeza
		HeadNode = GetNodeOrNull<Node3D>("head");
		CameraNode = GetNodeOrNull<Camera3d>("head/Camera3D");
		HandNode = GetNodeOrNull<Node3D>("head/hand");
		
		// Nodos dentro de la Camera3D (Rutas relativas completas)
		Sneeze = GetNodeOrNull<GpuParticles3D>("head/Camera3D/Sneeze");
		SneezeAudio = GetNodeOrNull<AudioStreamPlayer3D>("head/Camera3D/sneeze audio");
		Vomit = GetNodeOrNull<GpuParticles3D>("head/Camera3D/Vomit");
		VomitAudio = GetNodeOrNull<AudioStreamPlayer3D>("head/Camera3D/vomit audio"); 
		Interactor = GetNodeOrNull<RayCast3D>("head/Camera3D/Interactor");
		SpotLight3D = GetNodeOrNull<SpotLight3D>("head/Camera3D/SpotLight3D");

		// Sonidos (Asegúrate que coincidan con los nombres del Inspector)
		RainSound = GetNodeOrNull<AudioStreamPlayer>("RainSound");
		WindSound = GetNodeOrNull<AudioStreamPlayer>("WindSound");
		WindModerateSound = GetNodeOrNull<AudioStreamPlayer>("WindModeratesound");
		WindExtremeSound = GetNodeOrNull<AudioStreamPlayer>("WindExtremeSound");

		// Esqueleto y Física
		EsqueletoNode = GetNodeOrNull<Node3D>("Esqueleto");
		Skeleton = GetNodeOrNull<Skeleton3D>("Esqueleto/Skeleton3D");
		SkeletonPhy = GetNodeOrNull<PhysicalBoneSimulator3D>("Esqueleto/Skeleton3D/PhysicalBoneSimulator3D");
		Mesh = GetNodeOrNull<MeshInstance3D>("Esqueleto/Skeleton3D/human");
		Capsule = GetNodeOrNull<CollisionShape3D>("CollisionShape3D");

		// spawn y otros
		Spawn = GetNodeOrNull<Marker3D>("../Spawn");
		DeathMenu = GetNodeOrNull<DeathMenu>("DeathMenu");
		TempEffect = GetNodeOrNull<CanvasLayer>("TempEffect");
		Underwatereffect = GetNodeOrNull<CanvasLayer>("Underwater");
		Underlavaeffect = GetNodeOrNull<CanvasLayer>("UnderLava");
		Label = GetNodeOrNull<Label3D>("Name");
		chat_node = GetTree().Root.FindChild("Chat", true, false) as Chat;



		if (HeadNode != null)
		{
			HeadDefaultTransform = HeadNode.GlobalTransform;
			HeadDefaultLocalTransform = HeadNode.Transform;
		}

		if (CameraNode != null)
		{
			CameraDefaultTransform = CameraNode.GlobalTransform;
			CameraDefaultLocalTransform = CameraNode.Transform;
		}

		if (Skeleton != null)
		{
			HeadBoneIndex = Skeleton.FindBone("cuello"); // Asegúrate de que el hueso se llame exactamente "cuerpo" o cambia el nombre aquí
			if (HeadBoneIndex == -1 && Skeleton.GetBoneCount() > 9) // Verificamos que el esqueleto tenga suficientes huesos para evitar confusiones
			{
				HeadBoneIndex = 9; // Asignamos un índice por defecto (ajusta según tu esqueleto)
				GD.PrintErr("No se encontró el hueso 'cuello' en el esqueleto. Verifica el nombre del hueso o la estructura del esqueleto.");
			}
		}

		// 2. BLINDAJE INICIAL
		// Si algún nodo crítico es nulo, detenemos el proceso para que no crashee la PC
		if (RainNode != null) RainNode.Emitting = false;
		if (SandNode != null) SandNode.Emitting = false;
		if (SplashNode != null) SplashNode.Emitting = false;
		if (DustNode != null) DustNode.Emitting = false;
		if (SnowNode != null) SnowNode.Emitting = false;

		if (IsMultiplayerAuthority())
		{
			Globals.Instance.LocalPlayer = this;
			
			// El modo captura se llama DEFERRED para dar tiempo a la ventana a cargar
			Callable.From(() => {
				Input.MouseMode = Input.MouseModeEnum.Captured;
			}).CallDeferred();

			if (CameraNode != null) CameraNode.Current = true;

			_ResetPlayer();
			Rpc(MethodName._SetRagdollState, false);
			
			// Verificar si hay jugadores con el mismo nombre y aadir nmero si es necesario
			var nombre_base = Globals.Instance.Username;
			var contador = 0;

			foreach(Node3D p in GetTree().GetNodesInGroup("player"))
			{
				if (p is Player player)
				{
					// Saltar el jugador actual
					if(player == this)
					{
						continue;
					}


					// Verificar si el nombre coincide (sin contar n�meros a�adidos)
					var player_username = player.Username;
					if(player_username == nombre_base || player_username.StartsWith(nombre_base + "_"))
					{
						contador += 1;
					}
				}
				else
				{
					GD.PrintErr("Nodo en grupo 'player' que no es Player: " + p.Name);
				}
			}


			// Si hay duplicados, a�adir n�mero al nombre
			if(contador > 0)
			{
				Globals.Instance.Username = nombre_base + (contador + 1).ToString();
				Username = Globals.Instance.Username;
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

		float body_heat_genK = (float)delta;
		float body_heat_genMAX = 0.01f / 4;
		float fire_heat_emission = 50;

		float heatscale = 0;
		float coolscale = 0;

		float core_equilibrium = (float)Mathf.Clamp((37 - BodyTemperature) * body_heat_genK,  - body_heat_genMAX, body_heat_genMAX);
		float heatsource_equilibrium = (float)Mathf.Clamp((fire_heat_emission * (heatscale)) * body_heat_genK, 0, body_heat_genMAX * 1.3);
		float coldsource_equilibrium = (float)Mathf.Clamp((fire_heat_emission * (coolscale)) * body_heat_genK, body_heat_genMAX *  - 1.3, 0);

		float ambient_equilibrium = (float)Mathf.Clamp(((Globals.Instance.Temperature - BodyTemperature) * body_heat_genK),  - body_heat_genMAX * 1.1, body_heat_genMAX * 1.1);

		if(Globals.Instance.Temperature >= 5 && Globals.Instance.Temperature <= 37)
		{
			ambient_equilibrium = 0;
		}

		BodyTemperature = Mathf.Clamp(BodyTemperature + core_equilibrium + heatsource_equilibrium + coldsource_equilibrium + ambient_equilibrium, MinTemp, MaxTemp);
		
		// 1. Verifica que la referencia principal exista
		if (TempEffect != null)
		{
			// 2. Intenta obtener el nodo hijo de forma segura
			var temp_effect_rect = TempEffect.GetNodeOrNull<ColorRect>("ColorRect");

			// 3. Verifica que el nodo hijo exista y que tenga el material asignado
			if (temp_effect_rect != null && temp_effect_rect.Material is ShaderMaterial sm)
			{
				sm.SetShaderParameter("temp", BodyTemperature);
				sm.SetShaderParameter("Temp", BodyTemperature);
			}
		}

		var alpha_hot = 1 - ((44 - Mathf.Clamp(BodyTemperature, 39, 44)) / 5);
		var alpha_cold = ((35 - Mathf.Clamp(BodyTemperature, 24, 35)) / 11);

		if(GD.RandRange(1, 25) == 25)
		{
			if(alpha_cold != 0)
			{
				Rpc(MethodName.Damage, alpha_hot + alpha_cold);
			}
			else if(alpha_hot != 0)
			{
				Rpc(MethodName.Damage, alpha_hot + alpha_cold);
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

		if(Globals.Instance.Oxygen <= 20 || Globals.Instance.IsInwater(this) || IsUnderWater || Globals.Instance.IsInlava(this) || IsUnderLava)
		{
			BodyOxygen = (float)Mathf.Clamp(BodyOxygen - 5 * delta, MinOxygen, MaxOxygen);
		}
		else
		{
			BodyOxygen = (float)Mathf.Clamp(BodyOxygen + 5 * delta, MinOxygen, MaxOxygen);
		}


		if(BodyOxygen <= 0)
		{
			if(GD.RandRange(1, 25) == 25)
			{
				Rpc(MethodName.Damage, GD.RandRange(1, 30));
			}
		}
	}

	public void BodyRad(double delta)
	{
		if(GodMode)
		{
			return ;
		}

		if(Globals.Instance.Bradiation >= 80 && Globals.Instance.IsOutdoor(this) && Outdoor)
		{
			BodyBradiation = (float)Mathf.Clamp(BodyBradiation + 5 * delta, MinBdradiation, MaxBradiation);
		}
		else
		{
			BodyBradiation = (float)Mathf.Clamp(BodyBradiation - 5 * delta, MinBdradiation, MaxBradiation);
		}

		if(BodyBradiation >= 100)
		{
			if(GD.RandRange(1, 25) == 25)
			{
				Rpc(MethodName.Damage, GD.RandRange(1, 30));
			}
		}
	}

	public void UpdateCharacter()
	{

		// Determinar el personaje deseado: si no somos autoridad, usamos el dict sincronizado.
		var desired_char = Character;
		if(!IsMultiplayerAuthority())
		{
			if(Globals.Instance.AssignedCharacter.ContainsKey(PlayerId))
			{
				desired_char = Globals.Instance.AssignedCharacter[PlayerId];
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

	public void UpdateMaterial(int index)
	{
		if(Mesh == null || PlayerMaterials == null || index >= PlayerMaterials.Count)
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
		// --- LÓGICA PARA AGUA ---
		if (IsInWater && currentWaterArea != null)
		{
			var collider = currentWaterArea.GetNodeOrNull<CollisionShape3D>("CollisionShape3D");
			if (collider != null && collider.Shape is BoxShape3D box)
			{
				// Calculamos la superficie real en el mundo
				float waterSurfaceY = currentWaterArea.GlobalPosition.Y + (box.Size.Y / 2);
				IsUnderWater = CameraNode.GlobalPosition.Y < waterSurfaceY;
			}
		}

		// --- LÓGICA PARA LAVA ---
		if (IsInLava && currentLavaArea != null)
		{
			var collider = currentLavaArea.GetNodeOrNull<CollisionShape3D>("CollisionShape3D");
			if (collider != null && collider.Shape is BoxShape3D box)
			{
				// Calculamos la superficie real en el mundo
				float lavaSurfaceY = currentLavaArea.GlobalPosition.Y + (box.Size.Y / 2);
				IsUnderLava = CameraNode.GlobalPosition.Y < lavaSurfaceY;
			}
			Ignite(10); // Aplicar fuego si estás en contacto con lava
		}

		// --- APLICAR VISIBILIDAD DE UI ---
		if (Underwatereffect != null) Underwatereffect.Visible = IsUnderWater;
		if (Underlavaeffect != null) Underlavaeffect.Visible = IsUnderLava;

		// Apagar fuego si entras al agua
		if (IsInWater && IsOnFire) IsOnFire = false;
	}

	public void IsOnFireEffects()
	{
		if(FireParticles != null)
		{
			FireParticles.Emitting = IsOnFire;
		}

		if(IsOnFire)
		{
			if(GD.RandRange(1, 5) == 5)
			{
				Rpc(MethodName.Damage, 5);
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
		if(chat_node != null)
		{
			LineEdit line_edit = chat_node.LineEdit;
			if(line_edit != null && line_edit.HasFocus())
			{
				return ;
			}
		}

		if(Globals.Instance.IsChatOpen)
		{
			return ;
		}

		if(ev is InputEventKey keyEvent && keyEvent.Pressed && !keyEvent.Echo)
		{
			if(!AdminMode)
			{
				return ;
			}

			if(Globals.Instance.Gamemode != "sandbox")
			{
				return ;
			}

			if(keyEvent.Keycode == Key.Key1)
			{
				Globals.Instance.Rpc(Globals.MethodName.SetWeatherAndDisaster, "", 1);
			}
			if(keyEvent.Keycode == Key.Key2)
			{
				Globals.Instance.Rpc(Globals.MethodName.SetWeatherAndDisaster, "", 2);
			}
			if(keyEvent.Keycode == Key.Key3)
			{
				Globals.Instance.Rpc(Globals.MethodName.SetWeatherAndDisaster, "", 3);
			}
			if(keyEvent.Keycode == Key.Key4)
			{
				Globals.Instance.Rpc(Globals.MethodName.SetWeatherAndDisaster, "", 4);
			}
			if(keyEvent.Keycode == Key.Key5)
			{
				Globals.Instance.Rpc(Globals.MethodName.SetWeatherAndDisaster, "", 5);
			}
			if(keyEvent.Keycode == Key.Key6)
			{
				Globals.Instance.Rpc(Globals.MethodName.SetWeatherAndDisaster, "", 6);
			}
			if(keyEvent.Keycode == Key.Key7)
			{
				Globals.Instance.Rpc(Globals.MethodName.SetWeatherAndDisaster, "", 7);
			}
			if(keyEvent.Keycode == Key.Key8)
			{
				Globals.Instance.Rpc(Globals.MethodName.SetWeatherAndDisaster, "", 8);
			}
			if(keyEvent.Keycode == Key.Key9)
			{
				Globals.Instance.Rpc(Globals.MethodName.SetWeatherAndDisaster, "", 9);
			}
			if(keyEvent.Keycode == Key.Key0)
			{
				Globals.Instance.Rpc(Globals.MethodName.SetWeatherAndDisaster, "", 0);
			}
		}
	}

	public void UpdateRainSound()
	{
		// 1. Verificamos si realmente debe sonar la lluvia
		// Combinamos: ¿El emisor está activo? Y ¿El jugador está a la intemperie?
		bool shouldPlay = RainNode.Emitting && Globals.Instance.IsOutdoor(this);

		// Actualizamos el estado global
		Globals.Instance.IsRaining = shouldPlay;

		if (RainSound == null) return; // Seguridad

		if (shouldPlay)
		{
			if (!RainSound.Playing) 
			{
				RainSound.Play();
			}
		}
		else
		{
			if (RainSound.Playing)
			{
				RainSound.Stop();
			}
		}
	}

	public void UpdateWindSound()
	{
		// 1. Verificación de seguridad (Blindaje)
		// Si falta alguno de los nodos, salimos para evitar el crash.
		if (WindSound == null || WindModerateSound == null || WindExtremeSound == null) 
			return;

		// 2. Determinamos qué sonido DEBERÍA estar sonando
		AudioStreamPlayer targetSound = null;

		if (BodyWind > 100)
		{
			targetSound = WindExtremeSound;
		}
		else if (BodyWind > 50)
		{
			targetSound = WindModerateSound;
		}
		else if (BodyWind > 0)
		{
			targetSound = WindSound;
		}

		// 3. Gestión de estados (Play / Stop)
		// Lista de todos para iterar y apagar los que no necesitamos
		AudioStreamPlayer[] allWinds = { WindSound, WindModerateSound, WindExtremeSound };

		foreach (var v in allWinds)
		{
			if (v == targetSound)
			{
				if (!v.Playing) v.Play();
			}
			else
			{
				if (v.Playing) v.Stop();
			}
		}
	}

	public override void _Process(double delta)
	{
		UpdateCharacter();

		// tambin para clientes no autoridad (solo material)
		if(!IsMultiplayerAuthority())
		{
			return ;
		}

		BodyTemp(delta);
		BodyOxy(delta);
		BodyRad(delta);
		UnderwaterOrUnderlavaEffects();
		IsOnFireEffects();
		UpdateRainSound();
		UpdateWindSound();
		UpdateLabels();
	}

	public void UpdateLabels()
	{
		if(!IsMultiplayerAuthority())
		{
			return ;
		}

		Username = Globals.Instance.Username;
		Points = Globals.Instance.Points;

		if (Label != null) Label.Text = Globals.Instance.Username;
	}

	public void ApplyDisastersPush(Vector3 force)
	{
		externalForce = force;
	}

	public override void _PhysicsProcess(double delta)
	{
		if(!IsMultiplayerAuthority())
		{
			return ;
		}

		if(Globals.Instance.IsPauseMenuOpen)
		{
			return ;
		}

		if(Globals.Instance.IsChatOpen)
		{
			return ;
		}

		
		if(chat_node != null)
		{
			LineEdit line_edit = chat_node.LineEdit;
			if(line_edit != null && line_edit.HasFocus())
			{
				return ;
			}
		}

		// Hacer que la c�mara siga al cuerpo en ragdoll
		if(RagdollEnabled)
		{
			_UpdateCameraFollowRagdoll();
			return ;

			// No procesar movimiento cuando el ragdoll est� activo

		}// Add the gravity.


		

		if (!Noclip)
		{
			if (!IsOnFloor())
			{
				if (IsInWater || IsInLava)
				{
					// 2. Modificamos la variable local
					velocity.Y = (float)Globals.Instance.Gravity * (float)delta * SwimFactor;
				}
				else
				{
					// Si está cayendo, aplica más gravedad
					// Nota: Aquí ambos casos restan lo mismo según tu código original
					velocity.Y -= (float)Globals.Instance.Gravity * (float)delta;
					
					FallStrength = velocity.Y;
				}
			}
			else
			{
				if (!(IsInWater || IsInLava))
				{
					if (FallStrength <= -90)
					{
						Rpc(MethodName.Damage, 50);
					}
				}
			}
		}
		else
		{
			velocity.Y = 0;
		}

		// 3. REASIGNAMOS el vector modificado de vuelta a la propiedad Velocity
		

		// Handle jump.
		if(Input.IsActionJustPressed("Jump"))
		{
			if(IsOnFloor())
			{
				velocity.Y = JUMP_VELOCITY;
			}

			if(IsInWater || IsInLava)
			{
				velocity.Y += JUMP_VELOCITY;
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
		}

		Vector2 input_dir = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
		Vector3 input_vector = new Vector3(input_dir.X, 0, input_dir.Y);
		Vector3 direction = (HeadNode.Transform.Basis * input_vector).Normalized();

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
			velocity = desired_velocity;
		}
		else
		{
			// Lgica normal cuando no es noclip
			if(IsOnFloor())
			{
				if(direction != Vector3.Zero)
				{
					velocity.X = direction.X * SPEED;
					velocity.Z = direction.Z * SPEED;
				}
				else
				{
					velocity.X = (float)Mathf.Lerp(velocity.X, direction.X * SPEED, delta * 7.0);
					velocity.Z = (float)Mathf.Lerp(velocity.Z, direction.Z * SPEED, delta * 7.0);
				}
			}
			else
			{
				velocity.X = (float)Mathf.Lerp(velocity.X, direction.X * SPEED, delta * 3.0);
				velocity.Z = (float)Mathf.Lerp(velocity.Z, direction.Z * SPEED, delta * 3.0);
			}
		}


		var horizontal_velocity = new Vector2(velocity.X, velocity.Z);

		if (AnimationTreeNode != null) AnimationTreeNode.Set("parameters/conditions/is_falling", !IsOnFloor() && velocity.Y < 0);
		if (AnimationTreeNode != null) AnimationTreeNode.Set("parameters/conditions/is_jumping", velocity.Y > 0);
		if (AnimationTreeNode != null) AnimationTreeNode.Set("parameters/conditions/is_swiming", IsInWater || IsInLava);
		if (AnimationTreeNode != null) AnimationTreeNode.Set("parameters/conditions/is_idle", IsOnFloor() && horizontal_velocity.Length() < 0.1);
		if (AnimationTreeNode != null) AnimationTreeNode.Set("parameters/conditions/is_walking", IsOnFloor() && horizontal_velocity.Length() > 0.1);

		
		if(IsInstanceValid(Interactor) && Interactor.IsColliding())
		{
			Node3D target = (Node3D)Interactor.GetCollider();
			if (target != null && target.HasMethod("Interact"))
			{
				if (Input.IsActionJustPressed("Interact"))
				{
					// En lugar de target.Interact();
					target.Call("Interact"); 
				}
			}
			else if(target != null && target.IsInGroup("Pickable"))
			{
				if(Input.IsActionPressed("Interact"))
				{
					if(Multiplayer.IsServer())
					{

						// Si somos el servidor/host, llamamos DIRECTO
						Globals.Instance.RequestPickObject(GetPath(), target.GetPath());
					}
					else
					{

						// Si somos cliente, usamos RPC hacia el servidor
						Globals.Instance.Rpc(Globals.MethodName.RequestPickObject, GetPath(), target.GetPath());
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
				Globals.Instance.PrintRole("You dont have perms");
			}
		}

		Velocity = velocity + externalForce;

		MoveAndSlide();
		externalForce = Vector3.Zero;
		velocity = Velocity; // Reiniciar el vector de velocidad para el próximo frame, excepto que se reasigne en el proceso de movimiento. Esto evita acumulaciones no deseadas.
	}

	protected void _Noclip()
	{
		Noclip = !Noclip;
		if(Noclip)
		{
			Capsule.Disabled = true;
			velocity = Vector3.Zero;
			FallStrength = 0f;
			Globals.Instance.PrintRole("Noclip activated");
		}
		else
		{
			Capsule.Disabled = false;
			Globals.Instance.PrintRole("Noclip desactivated");
		}
	}


	public override void _UnhandledInput(InputEvent ev)
	{
		if (!IsMultiplayerAuthority()) return;

		// Bloqueos de UI y estado
		if (Globals.Instance.IsChatOpen || RagdollEnabled) return;

		// Verificación de foco en Chat
		if(chat_node != null)
		{
			LineEdit line_edit = chat_node.LineEdit;
			if(line_edit != null && line_edit.HasFocus())
			{
				return ;
			}
		}

		if (Input.MouseMode == Input.MouseModeEnum.Captured)
		{
			if (ev is InputEventMouseMotion mm)
			{
				// 1. Rotación Vertical (Arriba/Abajo) -> Eje X
				// Usamos -= porque en Godot el eje Y del ratón está invertido respecto al ángulo X
				Vector3 camRot = CameraNode.RotationDegrees;
				camRot.X -= mm.Relative.Y * SENSIBILITY;
				camRot.X = Mathf.Clamp(camRot.X, -90f, 90f);
				CameraNode.RotationDegrees = camRot;

				// 2. Rotación Horizontal (Izquierda/Derecha) -> Eje Y
				Vector3 headRot = HeadNode.RotationDegrees;
				headRot.Y -= mm.Relative.X * SENSIBILITY;
				HeadNode.RotationDegrees = headRot;

				// 3. Sincronizar el esqueleto con la dirección de la cabeza
				Vector3 esqRot = EsqueletoNode.RotationDegrees;
				esqRot.Y = headRot.Y;
				EsqueletoNode.RotationDegrees = esqRot;
			}
			else if (ev is InputEventJoypadMotion jm)
			{
				// Joypad (Ejes 2 y 3 suelen ser el stick derecho)
				float deadzone = 0.2f;
				if (Mathf.Abs(jm.AxisValue) < deadzone) return;

				if (jm.Axis == JoyAxis.RightX) // Eje 2 (Normalmente)
				{
					HeadNode.RotateY(-jm.AxisValue * SENSIBILITY * 10f); // Multiplicador para compensar velocidad
				}
				else if (jm.Axis == JoyAxis.RightY) // Eje 3 (Normalmente)
				{
					CameraNode.RotateX(-jm.AxisValue * SENSIBILITY * 10f);
					// Clamp necesario después de rotar
					Vector3 rot = CameraNode.RotationDegrees;
					rot.X = Mathf.Clamp(rot.X, -90f, 90f);
					CameraNode.RotationDegrees = rot;
				}
				
				// Sincronizar esqueleto tras movimiento de joypad
				EsqueletoNode.RotationDegrees = new Vector3(EsqueletoNode.RotationDegrees.X, HeadNode.RotationDegrees.Y, EsqueletoNode.RotationDegrees.Z);
			}
		}
	}


	protected void _OnArea3dBodyEntered(Node3D body)
	{
		if(body.IsInGroup("Meteor"))
		{
			Rpc(MethodName.Damage, 100);
		}
	}


	protected void _OnArea3dBodyExited(Node3D body)
	{
		if(body.IsInGroup("Water_Area"))
		{
			IsInWater = false;
			IsUnderWater = false;
			currentWaterArea = null;
		}
	}


	protected void _OnArea3dAreaEntered(Area3D area)
	{
		if (area.IsInGroup("Explosion"))
		{
			var parent = area.GetParent();
			if (parent is Explosion explosion) {
				float distance = (GlobalPosition - area.GlobalPosition).Length();
				Vector3 direction = (GlobalPosition - area.GlobalPosition).Normalized();
				float force = explosion.ExplosionForce * (1.0f - Mathf.Clamp(distance / explosion.ExplosionRadius, 0, 1));
				velocity = direction * force;
				int damag = 0;
				if (explosion.HasMeta(nameof(explosion.ExplosionDamage))) 
				{
					damag = (int)explosion.GetMeta(nameof(explosion.ExplosionDamage));
				}
				if (damag > 0)
				{
					Rpc(MethodName.Damage, damag); 
				}
			}
			else if (parent is ThunderExplosion thunderExplosion)
			{
				float distance = (GlobalPosition - area.GlobalPosition).Length();
				Vector3 direction = (GlobalPosition - area.GlobalPosition).Normalized();
				float force = thunderExplosion.ExplosionForce * (1.0f - Mathf.Clamp(distance / thunderExplosion.ExplosionRadius, 0, 1));
				velocity = direction * force;
				int damag = 0;
				if (thunderExplosion.HasMeta(nameof(thunderExplosion.ExplosionDamage)))
				{
					damag = (int)thunderExplosion.GetMeta(nameof(ThunderExplosion.ExplosionDamage));
				} 
				if (damag > 0)
				{
					Rpc(MethodName.Damage, damag); 
				}
			}

		}
		else if(area.IsInGroup("Lava_Area"))
		{
			IsInLava = true;
			currentLavaArea = area;
		}

		else if(area.IsInGroup("Water_Area"))
		{
			IsInWater = true;
			currentWaterArea = area;
		}
	}

	protected void _OnArea3dAreaExited(Area3D area)
	{
		if(area.IsInGroup("Lava_Area"))
		{
			IsInLava = false;
			IsUnderLava = false;
			currentLavaArea = null;
		}

		else if(area.IsInGroup("Water_Area"))
		{
			IsInWater = false;
			IsUnderWater = false;
			currentWaterArea = null;
		}
	}

	public void _ResetPlayer()
	{
		Hearth = MaxHearth;
		BodyTemperature = 37;
		BodyOxygen = MaxOxygen;
		BodyBradiation = MinBdradiation;
		IsAlive = true;
		IsInWater = false;
		IsInLava = false;
		IsOnFire = false;
		FallStrength = 0f;

		if (!IsMultiplayerAuthority())
		{
			return;
		}

		Rpc(MethodName._SetRagdollState, false);

		if (Spawn != null)
		{
			GlobalPosition = Spawn.GlobalPosition;
		}
		else
		{
			GlobalPosition = Vector3.Zero;
		}

		Velocity = Vector3.Zero;
		velocity = Vector3.Zero;

		if (CameraNode != null)
		{
			CameraNode.Transform = CameraDefaultLocalTransform;
		}
	}
}