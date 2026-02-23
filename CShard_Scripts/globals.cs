using System.Data.Common;
using System.Linq;
using Godot;
using Godot.Collections;


[GlobalClass]
public partial class Globals : Node
{
	[Signal]
	public delegate void CurrentWeatherAndDisasterChangedEventHandler(string new_disaster);

	public static Globals Instance { get; private set; }

	public Globals()
	{
		if(Instance != null)
		{
			GD.PrintErr("Ya existe una instancia de Globals. Esto no deber�a pasar, pero si est� pasando, se est� creando una nueva instancia de Globals para evitar errores fatales. Si este mensaje aparece m�s de una vez, por favor reporta este error a los desarrolladores.");
		}
		Instance = this;
	}

	//Editor
	public Variant Version = ProjectSettings.GetSetting("application/config/version");
	public Variant Gamename = ProjectSettings.GetSetting("application/config/name");
	public string Credits = "Miguelito2911";


	//Network
	[Export] public string Ip;
	[Export] public int Port = 5555;
	[Export] public int Points;
	[Export] public string Username = "Player";
	[Export] public Array<Player> PlayersConected;
	MultiplayerPeer Multiplayerpeer;


	//Globals Weather
	[Export] public float Temperature = 23;
	[Export] public float Pressure = 10000;
	[Export] public float Oxygen = 100;
	[Export] public float Bradiation = 0;
	[Export] public float Humidity = 25;
	[Export] public Vector3 WindDirection = new Vector3(1, 0, 0);
	[Export] public float WindSpeed = 0;
	[Export] public bool IsRaining = false;
	public Variant Gravity = ProjectSettings.GetSetting("physics/3d/default_gravity");


	//Globals Time
	[Export] public float Time = 0.0f;
	[Export] public float TimeLeft = 0.0f;
	[Export] public int Day = 0;
	[Export] public int Hour = 0;
	[Export] public int Minute = 00;


	//Globals Weather target
	[Export] public float TemperatureTarget = 23;
	[Export] public float PressureTarget = 10000;
	[Export] public float OxygenTarget = 100;
	[Export] public float BradiationTarget = 0;
	[Export] public float HumidityTarget = 25;
	[Export] public Vector3 WindDirectionTarget = new Vector3(1, 0, 0);
	[Export] public float WindSpeedTarget = 0;


	//Globals Weather original
	[Export] public float TemperatureOriginal = 23;
	[Export] public float PressureOriginal = 10000;
	[Export] public float OxygenOriginal = 100;
	[Export] public float BradiationOriginal = 0;
	[Export] public float HumidityOriginal = 25;
	[Export] public Vector3 WindDirectionOriginal = new Vector3(1, 0, 0);
	[Export] public float WindSpeedOriginal = 0;

	[Export] public float Seconds = 0;

	[Export] public Main Main;
	[Export] public MainMenu MainMenu;
	[Export] public Map Map;
	[Export] public ServerBrowser ServerBrowser;
	[Export] public Player LocalPlayer;

	[Export] public Dictionary BoundingRadiusAreas = new Dictionary{};

	[Export] public string NodeGroup = "Destrollable";
	[Export] public Array<string> DestrolledNode;
	[Export] public bool Started = false;
	[Export] public string Gamemode = "survival";
	[Export] public DataResource GlobalsData;

	public string CurrentWeatherAndDisaster
	{
		set
		{
			if(_CurrentWeatherAndDisaster != value)
			{
				_CurrentWeatherAndDisaster = value;
				EmitSignal("CurrentWeatherAndDisasterChanged", value);
			}
		}
		get { return _CurrentWeatherAndDisaster; }
	}
	private string _CurrentWeatherAndDisaster = "Original";


	[Export] public int CurrentWeatherAndDisasterInt = 0;

	public PackedScene PlayerScene = ResourceLoader.Load<PackedScene>("res://Scenes/player.tscn");
	public PackedScene ThunderstormScene = ResourceLoader.Load<PackedScene>("res://Scenes/thunder.tscn");
	public PackedScene MeteorScene = ResourceLoader.Load<PackedScene>("res://Scenes/meteor.tscn");
	public PackedScene TornadoScene = ResourceLoader.Load<PackedScene>("res://Scenes/tornado.tscn");
	public PackedScene TsunamiScene = ResourceLoader.Load<PackedScene>("res://Scenes/tsunami.tscn");
	public PackedScene VolcanoScene = ResourceLoader.Load<PackedScene>("res://Scenes/volcano.tscn");
	public PackedScene EarthquakeScene = ResourceLoader.Load<PackedScene>("res://Scenes/earthquake.tscn");

	public Timer Timer;
	public Timer BroadcastTimer;

	[Export] public Dictionary<string, Variant> RoomList = new Dictionary<string, Variant>{{"name", "name"},{"players", 0}};
	[Export] public string BroadcasterIp = "192.168.1.255";
	[Export] public int LisenerPort = 5556;
	[Export] public int BroadcasterPort = 5554;
	public PacketPeerUdp Broadcaster;
	public PacketPeerUdp Lisener;

	[Export] public bool IsChatOpen = false;
	[Export] public bool IsPauseMenuOpen = false;
	[Export] public bool IsSpawnMenuOpen = false;

	[Export] public string Character = "blue";
	[Export] public Array<string> AvalibleCharacters = new Array<string>{"blue", "red", "green", "yellow"};
	[Export] public Dictionary<int, string> AssignedCharacter;

	public float ConvertMetoSU(float metres)
	{
		return (int)(metres * 39.37f) / 0.75f;
	}

	public int ConvertKMPHtoMe(float kmph)
	{
		return (int)((kmph * 1000) / 3600);
	}

	public int ConvertVectorToAngle(Vector3 vector)
	{
		var x = vector.X;
		var y = vector.Z;

		return (int)(360 + Mathf.RadToDeg(Mathf.Atan2(y, x))) % 360;
	}

	protected PhysicsDirectSpaceState3D _GetDirectSpaceState(Node node)
	{

		// Intenta obtener el World3D a partir del nodo; si falla, intenta la escena actual.
		World3D world = null;
		if(node != null && IsInstanceValid(node) && node is Node3D node3D)
		{
			world = node3D.GetWorld3D();
		}
		if(world == null)
		{
			var scene = GetTree().GetCurrentScene();
			if(IsInstanceValid(scene) && scene is Node3D scene3D)
			{
				world = scene3D.GetWorld3D();
			}
		}
		if(world == null)
		{
			return null;
		}
		return world.DirectSpaceState;
	}

	public bool PerformTraceCollision(Node3D ply, Vector3 direction)
	{
		var start_pos = ply.GlobalPosition;
		var end_pos = start_pos + direction * 1000;
		var space_state = _GetDirectSpaceState(ply);
		if(space_state == null)
		{
			return false;
		}
		var ray = PhysicsRayQueryParameters3D.Create(start_pos, end_pos);
		if (ply is CollisionObject3D collisionEntity)
		{
			ray.Exclude = new Godot.Collections.Array<Rid> { collisionEntity.GetRid() };
		}
		var result = space_state.IntersectRay(ray);
		return result != new Dictionary{};
	}


	public Vector3 PerformTraceWind(Node3D ply,Vector3 direction)
	{
		Vector3 start_pos = ply.GlobalPosition;
		Vector3 end_pos = start_pos + direction * 60000;
		PhysicsDirectSpaceState3D space_state = _GetDirectSpaceState(ply);
		if(space_state == null)
		{
			return end_pos;
		}
		PhysicsRayQueryParameters3D ray = PhysicsRayQueryParameters3D.Create(start_pos, end_pos);

		if (ply is CollisionObject3D collisionEntity)
		{
			ray.Exclude = new Godot.Collections.Array<Rid> { collisionEntity.GetRid() };
		}
		
		Dictionary result = space_state.IntersectRay(ray);
		if(result != new Dictionary{} && result.ContainsKey("position"))
		{
			return (Vector3)result["position"];
		}
		else
		{
			return end_pos;
		}
	}

	public Dictionary<Node, int> GetNodeByIdRecursive(Node node, int node_id)
	{
		if(node.GetInstanceId().Equals(node_id))
		{
			return new Dictionary<Node, int>{{node, node_id}};
		}

		foreach(Node child in node.GetChildren())
		{
			Dictionary<Node, int> result = GetNodeByIdRecursive(child, node_id);
			if(result != null)
			{
				return result;
			}
		}

		return null;
	}

	public bool IsBelowSky(Node3D ply)
	{
		// Hacemos el cast a Node3D una sola vez
		if (ply is not Node3D) return true;

		Vector3 start_pos = ply.GlobalPosition;
		Vector3 end_pos = start_pos + new Vector3(0, 48000, 0);
		PhysicsDirectSpaceState3D space_state = ply.GetWorld3D().DirectSpaceState; // Forma estándar de obtenerlo en Godot 4

		if (space_state == null) return true;

		PhysicsRayQueryParameters3D ray = PhysicsRayQueryParameters3D.Create(start_pos, end_pos);
		
		// Solo CollisionObject3D (PhysicsBody3D, Area3D) tiene GetRid()
		if (ply is CollisionObject3D collisionEntity)
		{
			ray.Exclude = new Godot.Collections.Array<Rid> { collisionEntity.GetRid() };
		}

		Dictionary result = space_state.IntersectRay(ray);
		return !result.ContainsKey("position");
	}



	public bool IsOutdoor(Node3D ply)
	{
		bool hitSky = IsBelowSky(ply);

		// Si es un Player, actualizamos su propiedad Outdoor
		if (ply is Player player && ply.IsInGroup("player"))
		{
			player.Outdoor = hitSky;
		}

		return hitSky;
	}


	public bool IsInwater(Node ply)
	{
		if(ply.IsInGroup("player")&& ply is Player player)
		{
			return player.IsInWater;
		}
		return false;
	}

	public bool IsUnderwater(Node ply)
	{
		if(ply.IsInGroup("player") && ply is Player player)
		{
			return player.IsUnderWater;
		}
		return false;
	}

	public bool IsInlava(Node ply)
	{
		if(ply.IsInGroup("player") && ply is Player player)
		{
			return player.IsInLava;
		}
		return false;
	}

	public bool IsUnderlava(Node ply)
	{
		if(ply.IsInGroup("player") && ply is Player player)
		{
			return player.IsUnderLava;
		}
		return false;
	}


	public Vector3 Vec2ToVec3(Vector3 vector)
	{
		return new Vector3(vector.X, 0, vector.Y);
	}

	public bool IsSomethingBlockingWind(Node3D entity)
	{
		var start_pos = entity.GlobalPosition;
		var end_pos = start_pos - (WindDirection * 300);
		var space_state = _GetDirectSpaceState(entity);
		if(space_state == null)
		{

			// Sin informaci�n del mundo, no asumimos bloqueo
			return false;
		}
		var ray = PhysicsRayQueryParameters3D.Create(start_pos, end_pos);

		if (entity is CollisionObject3D collisionEntity)
		{
			ray.Exclude = new Godot.Collections.Array<Rid> { collisionEntity.GetRid() };
		}

		var result = space_state.IntersectRay(ray);
		return result != new Dictionary{};
	}

	public float CalculeBoundingRadius(Node3D entity)
	{
		float max_radius = 0.0f;

		foreach (Node child in entity.GetChildren())
		{
			// Recursividad: Si tiene hijos, acumulamos el radio máximo
			if (child.GetChildCount() > 0 && child is Node3D childNode)
			{
				max_radius = Mathf.Max(max_radius, CalculeBoundingRadius(childNode));
			}

			if (child is MeshInstance3D meshInstance)
			{
				Mesh mesh = meshInstance.Mesh;
				if (mesh == null) continue;

				Aabb aabb = mesh.GetAabb();
				
				// 1. Definir los 8 vértices locales de la AABB
				Vector3[] vertices = new Vector3[] {
					aabb.Position,
					aabb.Position + new Vector3(aabb.Size.X, 0, 0),
					aabb.Position + new Vector3(0, aabb.Size.Y, 0),
					aabb.Position + new Vector3(0, 0, aabb.Size.Z),
					aabb.Position + new Vector3(aabb.Size.X, aabb.Size.Y, 0),
					aabb.Position + new Vector3(aabb.Size.X, 0, aabb.Size.Z),
					aabb.Position + new Vector3(0, aabb.Size.Y, aabb.Size.Z),
					aabb.Position + aabb.Size,
				};

				// 2. Transformar vértices y calcular radio en un solo paso
				foreach (Vector3 v in vertices)
				{
					// Transformamos el vértice al espacio global o del padre
					Vector3 globalVertex = meshInstance.Transform * v;
					
					// Calculamos la distancia al origen del objeto original
					float distance = globalVertex.Length();
					max_radius = Mathf.Max(max_radius, distance);
				}
			}
		}
		return max_radius;
	}



	public Array SearchInNode(Node node, Vector3 origin, float radius, Array result)
	{
		foreach(int i in GD.Range(node.GetChildCount()))
		{
			Node child = node.GetChild(i);
			if(child is Node3D child3D && IsInstanceValid(child3D))
			{
				// Solo considerar nodos Spatial (puedes ajustar esto segn tus necesidades)
				var distance = origin.DistanceTo(child3D.GlobalPosition);
				if(distance <= radius)
				{
					result.Add(child3D);
				}
			}

			// Recursin si el nodo tiene hijos
			if(child.GetChildCount() > 0)
			{
				SearchInNode(child, origin, radius, result);
			}
		}

		return result;
	}

	public Array FindInSphere(Vector3 origin, float radius)
	{
		var result = new Array();
		var scene_root = GetTree().GetRoot();

		result = SearchInNode(scene_root, origin, radius, result);

		return result;
	}

	public void Wind(Node3D obj)
	{

		// Verificar si el objeto es un jugador
		if(obj.IsInGroup("player") && obj is Player player)
		{
			if(!IsInstanceValid(obj))
			{
				return ;
			}


			// Calcular la velocidad del viento local
			var local_wind = WindSpeed;
			if(!IsOutdoor(obj) || IsSomethingBlockingWind(obj))
			{
				local_wind = 0;
			}

			player.BodyWind = local_wind;


			// Calcular la velocidad del viento y la friccin
			Vector3 wind_vel = WindDirection * (float)local_wind;

			// Verificar si est al aire libre y no hay obstculos que bloqueen el viento
			if(IsOutdoor(obj) && !IsSomethingBlockingWind(obj) && local_wind >= 30)
			{
				var delta_velocity = wind_vel - player.Velocity;
				player.Velocity += delta_velocity * (float)0.3;
			}
		}

		else if(obj.IsInGroup("movable_objects") && obj is RigidBody3D body)
		{
			if(GodotObject.IsInstanceValid(body) && IsOutdoor(body) && !IsSomethingBlockingWind(body))
			{
				var wind_vel = WindDirection * (float)WindSpeed;
				var delta_velocity = wind_vel - body.LinearVelocity;


				// Aplica fuerza en vez de modificar directamente la velocidad
				body.ApplyCentralForce(delta_velocity * 0.3f * body.Mass);
			}
		}

		else if(obj.IsInGroup("movable_objects") && obj is StaticBody3D staticBody)
		{
			if(GodotObject.IsInstanceValid(staticBody))
			{
				if((staticBody.IsInGroup("Destrollable") || staticBody.IsInGroup("Hause")) && staticBody is House house)
				{
					if(WindSpeed > 100)
					{
						house.Destroy();
					}
				}
			}
		}
	}


	public float GetArea(Node3D entity)
	{
		// Intentamos obtener el valor desde el objeto (funciona si existe la propiedad en un script)
		Variant value = entity.Get("BoundingRadiusArea");

		if (value.VariantType == Variant.Type.Nil) 
		{
			// No existe la propiedad, calculamos y guardamos (opcionalmente)
			float area = Mathf.Pi * Mathf.Pow(CalculeBoundingRadius(entity), 2);
			
			// Si quieres intentar guardarlo en el objeto mismo:
			// entity.Set("BoundingRadiusArea", area); 
			
			return area;
		}

		return value.AsSingle();
	}



	public float GetFrameMultiplier()
	{
		var frame_time = (float)Engine.GetFramesPerSecond();
		if(frame_time == 0)
		{
			return 0;
		}
		else
		{
			return (float)60 / frame_time;
		}
	}

	public float GetPhysicsMultiplier()
	{
		var physics_interval = (float)GetPhysicsProcessDeltaTime();
		return (200.0f / 3.0f) / physics_interval;
	}

	public bool HitChance(int chance)
	{
		if(Multiplayer.IsServer())
		{

			// En el servidor
			return GD.Randf() < (Mathf.Clamp(chance * GetPhysicsMultiplier(), 0, 100) / 100);
		}
		else
		{

			// En el cliente
			return GD.Randf() < (Mathf.Clamp(chance * GetFrameMultiplier(), 0, 100) / 100);
		}
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer)]
	public void SyncPlayerList()
	{
		PlayersConected.Clear();

		foreach(Player p in GetTree().GetNodesInGroup("player"))
		{
			if(IsInstanceValid(p))
			{
				PlayersConected.Add(p);
			}
		}
	}


	// Funci�n para verificar si hay jugadores con el mismo nombre
	public bool HayJugadoresConMismoNombre(string nombre_a_verificar, Node excluir_jugador = null)
	{
		var contador = 0;
		foreach(Player player in GetTree().GetNodesInGroup("player"))
		{

			// Si se debe excluir un jugador especfico, saltarlo
			if(excluir_jugador != null && player == excluir_jugador)
			{
				continue;
			}

			Variant username = player.Get("username");
			// Verificar si el nombre coincide
			if(IsInstanceValid(player) && username.VariantType != Variant.Type.Nil  && username.AsString() == nombre_a_verificar)
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
	public Array ObtenerJugadoresConMismoNombre(string nombre_a_verificar, Node excluir_jugador = null)
	{
		var jugadores_duplicados = new Array();

		foreach(Player player in GetTree().GetNodesInGroup("player"))
		{

			// Si se debe excluir un jugador espec�fico, saltarlo
			if(excluir_jugador != null && player == excluir_jugador)
			{
				continue;
			}


			// Verificar si el nombre coincide
			if(GodotObject.IsInstanceValid(player) && player.Username == nombre_a_verificar)
			{
				jugadores_duplicados.Add(player);
			}
		}

		return jugadores_duplicados;
	}


	// Funci�n para contar cu�ntos jugadores tienen el mismo nombre
	public int ContarJugadoresConMismoNombre(string nombre_a_verificar, Node excluir_jugador = null)
	{
		var contador = 0;
		foreach(Node player in GetTree().GetNodesInGroup("player"))
		{

			// Si se debe excluir un jugador espec�fico, saltarlo
			if(excluir_jugador != null && player == excluir_jugador)
			{
				continue;
			}

			var username = player.Get("username");
			// Verificar si el nombre coincide
			if(GodotObject.IsInstanceValid(player) && username.VariantType != Variant.Type.Nil && username.AsString() == nombre_a_verificar)
			{
				contador += 1;
			}
		}

		return contador;
	}


	public void PrintRole(string msg)
	{
		var peer = Multiplayer.MultiplayerPeer;

		if(peer == null || peer is OfflineMultiplayerPeer)
		{
			GD.Print(msg);
			return;
		}

		bool IsServer = Multiplayer.IsServer();
		if(IsServer)
		{
			// Azul
			GD.PrintRich("[color=blue][Server] " + msg + "[/color]");
		}
		else
		{
			// Amarillo
			GD.PrintRich("[color=yellow][Client] " + msg + "[/color]");
		}
	}


	public async void PlayMultiplayerServer()
	{
		var peer = new ENetMultiplayerPeer();
		Error error = peer.CreateServer(Port);
		if(error == Error.Ok)
		{
			Multiplayerpeer = peer;
			Multiplayer.MultiplayerPeer = Multiplayerpeer;
			if(Multiplayer.IsServer())
			{
				if(OS.HasFeature("dedicated_server") || OS.GetCmdlineUserArgs() != null ||  OS.GetCmdlineUserArgs().Contains("server"))
				{
					PrintRole("Dedicated server init");

					await ToSignal(GetTree().CreateTimer(2), "Timeout");

					SetUpBroadcast(Username);
					LoadScene.Instance.loadscene(MainMenu, "map");
				}
				else
				{
					PrintRole("Server init");
					SetUpBroadcast(Username);
					LoadScene.Instance.loadscene(MainMenu, "map");
				}
			}
		}
		else
		{
			PrintRole("Fatal Error in server");
		}
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer)]
	public void RequestPickObject(NodePath player_path, NodePath target_path)
	{

		// Solo el servidor debe ejecutar esta lgica
		if(!Multiplayer.IsServer())
		{
			return ;
		}

		var root = GetTree().GetRoot();

		var player = root.GetNodeOrNull<Player>(player_path);
		var target = root.GetNodeOrNull<CollisionObject3D>(target_path);

		if(player == null || target == null)
		{
			return ;
		}

		if(!target.IsInGroup("Pickable"))
		{
			return ;
		}


		// Colocar el objeto en la mano del jugador
		target.GlobalPosition = player.HandNode.GlobalPosition;
		target.GlobalRotation = player.HandNode.GlobalRotation;
		target.CollisionLayer = 2;

		if(target is RigidBody3D rigidBody3)
		{
			rigidBody3.LinearVelocity = new Vector3(0.1f, 3, 0.1f);
		}
	}

	public void PlayMultiplayerClient()
	{
		ENetMultiplayerPeer peer = new ENetMultiplayerPeer();
		var error = peer.CreateClient(Ip, Port);
		if(error == Error.Ok)
		{
			Multiplayerpeer = peer;
			Multiplayer.MultiplayerPeer = Multiplayerpeer;
			if(!Multiplayer.IsServer())
			{
				PrintRole("Client Init");
			}
		}
		else
		{
			PrintRole("Fatal Error in client");
		}
	}

	public void MultiplayerConnectionFailed()
	{
		PrintRole("Client disconected");

		PlayersConected.Clear();
		AssignedCharacter.Clear();
		DestrolledNode.Clear();

		CloseUp();

		Multiplayerpeer = new OfflineMultiplayerPeer();
		Multiplayer.MultiplayerPeer = Multiplayerpeer;

		LoadScene.Instance.loadscene(Map, "res://Scenes/main_menu.tscn");
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	public void AssingCharacter(string charac)
	{
		foreach(string c in AvalibleCharacters)
		{
			if(c == charac)
			{
				Character = charac;
				break;
			}
		}

		if(LocalPlayer != null && GodotObject.IsInstanceValid(LocalPlayer))
		{
			LocalPlayer.Character = charac;
		}

		PrintRole("Asignado el personaje: " + charac);
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	public bool AssingCharacterToPlayer(long id, string charac)
	{
		var chosen_char = charac;


		// Si el char recibido no es v�lido o ya est� ocupado, buscamos el siguiente disponible.
		if(chosen_char == null || chosen_char == "" || !IsCharacterAvalible(chosen_char))
		{
			chosen_char = GetNextAvalibleCharacter();
		}

		if(chosen_char == null || chosen_char == "" || !IsCharacterAvalible(chosen_char))
		{
			PrintRole("No hay personaje disponible para el id " + id.ToString());
			return false;
		}

		AssignedCharacter[(int)id] = chosen_char;
		AssingCharacter(chosen_char);
		PrintRole("Asignado al id " + id.ToString() + " el personaje " + chosen_char);
		return true;
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer)]
	public void SyncAssignedCharacter(Dictionary<int, string> data)
	{
		AssignedCharacter = data.Duplicate(true);
	}

	public bool IsCharacterAvalible(string charac)
	{
		foreach(int id in AssignedCharacter.Keys)
		{
			if(AssignedCharacter[id] == charac)
			{
				return false;
			}
		}

		return true;
	}


	public string GetNextAvalibleCharacter()
	{
		foreach(string charac in AvalibleCharacters)
		{
			if(IsCharacterAvalible(charac))
			{
				return charac;
			}
		}

		return null;
	}


	public void MultiplayerServerDisconnected()
	{
		PrintRole("Client disconected");

		PlayersConected.Clear();
		AssignedCharacter.Clear();
		DestrolledNode.Clear();

		CloseUp();

		Multiplayerpeer = new OfflineMultiplayerPeer();
		Multiplayer.MultiplayerPeer = Multiplayerpeer;

		LoadScene.Instance.loadscene(Map, "res://Scenes/main_menu.tscn");
	}


	public void MultiplayerConnectionServerSucess()
	{
		PrintRole("connected to server");
		UnloadScene.Instance.unloadscene(MainMenu);
	}

	public override void _ExitTree()
	{
		Multiplayer.PeerConnected -= MultiplayerPlayerSpawner;
		Multiplayer.PeerDisconnected -= MultiplayerPlayerRemover;
		Multiplayer.ServerDisconnected -= MultiplayerServerDisconnected;
		Multiplayer.ConnectedToServer -= MultiplayerConnectionServerSucess;
		Multiplayer.ConnectionFailed -= MultiplayerConnectionFailed;

		TemperatureTarget = Globals.Instance.TemperatureOriginal;
		HumidityTarget = HumidityOriginal;
		PressureTarget = PressureOriginal;
		WindDirectionTarget = WindDirectionOriginal;
		WindSpeedTarget = WindSpeedOriginal;

		CloseUp();
	}


	public override void _Process(double _delta)
	{
		if(!Multiplayer.HasMultiplayerPeer())
		{
			return ;
		}

		if(!Multiplayer.IsServer())
		{
			return ;
		}

		TimeLeft = (float)Timer.TimeLeft;
		Temperature = Mathf.Clamp(Temperature,  - 275.5f, 275.5f);
		Humidity = Mathf.Clamp(Humidity, 0, 100);
		Bradiation = Mathf.Clamp(Bradiation, 0, 100);
		Pressure = Mathf.Clamp(Pressure, 0, 100000);
		Oxygen = Mathf.Clamp(Oxygen, 0, 100);

		Temperature = Mathf.Lerp(Temperature, TemperatureTarget, 0.005f);
		Humidity = Mathf.Lerp(Humidity, HumidityTarget, 0.005f);
		Bradiation = Mathf.Lerp(Bradiation, BradiationTarget, 0.005f);
		Pressure = Mathf.Lerp(Pressure, PressureTarget, 0.005f);
		Oxygen = Mathf.Lerp(Oxygen, OxygenTarget, 0.005f);
		WindDirection = WindDirection.Lerp(WindDirectionTarget, 0.005f).Normalized();
		WindSpeed = Mathf.Lerp(WindSpeed, WindSpeedTarget, 0.005f);
	}


	public override void _Ready()
	{
		Timer = GetNode<Timer>("Timer");
		BroadcastTimer = GetNode<Timer>("Broadcast_Timer");

		Multiplayer.PeerConnected += MultiplayerPlayerSpawner;
		Multiplayer.PeerDisconnected += MultiplayerPlayerRemover;
		Multiplayer.ServerDisconnected += MultiplayerServerDisconnected;
		Multiplayer.ConnectedToServer += MultiplayerConnectionServerSucess;
		Multiplayer.ConnectionFailed += MultiplayerConnectionFailed;

		Multiplayerpeer = new OfflineMultiplayerPeer();
		Multiplayer.MultiplayerPeer = Multiplayerpeer;
		
		GlobalsData = DataResource.LoadFile();
	}


	public void MultiplayerPlayerSpawner(long peer_id = 1)
	{
		if(!Multiplayer.IsServer())
		{
			return ;
		}

		if(Map != null && IsInstanceValid(Map))
		{
			PrintRole("Joined player id: " + peer_id.ToString());
			var player = PlayerScene.Instantiate();
			player.Name = peer_id.ToString();
			Map.AddChild(player, true);


			var assigned_ok = true;

			if(!AssignedCharacter.ContainsKey((int)peer_id))
			{
				var next_character = GetNextAvalibleCharacter();
				assigned_ok = AssingCharacterToPlayer(peer_id, next_character);
			}

			if(assigned_ok)
			{
				Rpc(MethodName.SyncAssignedCharacter, AssignedCharacter);
				SyncAssignedCharacter(AssignedCharacter);
				Rpc(MethodName.SyncPlayerList);
				RpcId(peer_id, MethodName.SyncDestrolledNodes, DestrolledNode);
				// envia al cliente
				RpcId(peer_id, MethodName.SetWeatherAndDisaster, CurrentWeatherAndDisasterInt);
			}
			else
			{
				PrintRole("No se pudo asignar personaje al jugador con id: " + peer_id.ToString());
			}
		}

		else
		{
			Rpc(MethodName.SyncAssignedCharacter, AssignedCharacter);
			SyncAssignedCharacter(AssignedCharacter);
			Rpc(MethodName.SyncPlayerList);
			RpcId(peer_id, MethodName.SyncDestrolledNodes, DestrolledNode);
			// broadcast
			PrintRole("No se pudo aadir al jugador con el id: " + peer_id.ToString());
		}
	}


	public async void MultiplayerPlayerRemover(long peer_id = 1)
	{
		if(!Multiplayer.IsServer())
		{
			return ;
		}


		// Intentar obtener el jugador de forma segura
		Player player_node = Map.GetNodeOrNull<Player>(peer_id.ToString());
		if(player_node != null && IsInstanceValid(player_node))
		{
			PrintRole("Disconected player id: " + peer_id.ToString());
			player_node.QueueFree();

			await ToSignal(player_node, "TreeExited");

			if(AssignedCharacter.ContainsKey((int)peer_id))
			{
				AssignedCharacter.Remove((int)peer_id);
			}


			
			Rpc(MethodName.SyncAssignedCharacter);
			SyncAssignedCharacter(AssignedCharacter);
			Rpc(MethodName.SyncPlayerList);
		}


		else
		{
			if(AssignedCharacter.ContainsKey((int)peer_id))
			{
				AssignedCharacter.Remove((int)peer_id);
			}
			Rpc(MethodName.SyncAssignedCharacter, AssignedCharacter);
			SyncAssignedCharacter(AssignedCharacter);
			Rpc(MethodName.SyncPlayerList);
			PrintRole("player no found: " + peer_id.ToString());
		}
	}


	public void SyncWeatherAndDisaster()
	{
		if(Multiplayer.IsServer())
		{
			var random_weather_and_disaster = GD.RandRange(0, 13);
			Rpc(MethodName.SetWeatherAndDisaster, random_weather_and_disaster);
		}
	}

	// 1. Define la lista de nombres fuera del método (como variable de clase)
	private string[] _weatherNames = {
		"Sun", "Cloud", "Raining", "Storm", "Thunderstorm", 
		"Tsunami", "Meteors shower", "Volcano", "Tornado", 
		"Acid rain", "Earthquake", "Sand Storm", "blizzard", "Dust Storm"
	};

	[Rpc(MultiplayerApi.RpcMode.AnyPeer)]
	public void SetWeatherAndDisaster(Variant weather_and_disaster_index)
	{
		// Por defecto, asumimos que no se encontró
		CurrentWeatherAndDisaster = "Original";
		CurrentWeatherAndDisasterInt = -1;

		// Caso A: Si recibimos un número (int)
		if (weather_and_disaster_index.VariantType == Variant.Type.Int)
		{
			int idx = (int)weather_and_disaster_index;
			if (idx >= 0 && idx < _weatherNames.Length)
			{
				CurrentWeatherAndDisaster = _weatherNames[idx];
				CurrentWeatherAndDisasterInt = idx;
			}
		}
		// Caso B: Si recibimos un texto (string)
		else if (weather_and_disaster_index.VariantType == Variant.Type.String)
		{
			string name = weather_and_disaster_index.AsString();
			int idx = System.Array.IndexOf(_weatherNames, name);
			
			if (idx != -1)
			{
				CurrentWeatherAndDisaster = name;
				CurrentWeatherAndDisasterInt = idx;
			}
		}
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer)]
	public void AddPoints()
	{
		Points += 1;
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer)]
	public void RemovePoints()
	{
		Points -= 1;

		if(Points < 0)
		{
			Points = 0;
		}
	}


	public void CloseConection()
	{
		var peer = Multiplayer.MultiplayerPeer;

		// Si no hay peer o est� desconectado o es offline volver al men�
		if(peer == null || peer is OfflineMultiplayerPeer || peer.GetConnectionStatus() != MultiplayerPeer.ConnectionStatus.Connected)
		{
			MultiplayerServerDisconnected();
			return;
		}

		// Si est� conectado cerrar conexi�n
		Multiplayerpeer.Close();
	}

	protected void _OnTimerTimeout()
	{
		if(Gamemode == "survival")
		{
			if(Started)
			{
				SyncWeatherAndDisaster();
			}
			else
			{
				Multiplayer.MultiplayerPeer.Close();
			}
		}
	}

	public void SyncDestrolledNodes(Array<string> Hauses)
	{
		foreach(string house_name in Hauses)
		{
			var house = GetTree().GetCurrentScene().GetNodeOrNull(house_name);
			if(house != null && IsInstanceValid(house))
			{
				house.QueueFree();
			}
		}
	}

	public void AddDestrolledNodes(string Name)
	{
		if(!Multiplayer.IsServer())
		{
			return;
		}

		if(!DestrolledNode.Contains(Name))
		{
			DestrolledNode.Add(Name);
		}
	}

	public void RemoveDestrolledNodes(string Name)
	{
		if(!Multiplayer.IsServer())
		{
			return;
		}

		if(DestrolledNode.Contains(Name))
		{
			DestrolledNode.Remove(Name);
		}
	}

	public void RemoveAllDestrolledNodes()
	{
		if(!Multiplayer.IsServer())
		{
			return;
		}

		foreach(string i in DestrolledNode)
		{
			RemoveDestrolledNodes(i);
		}
	}

	public void SetUpLisener()
	{
		Lisener = new PacketPeerUdp();
		var ok = Lisener.Bind(LisenerPort);
		if(ok == Error.Ok)
		{
			PrintRole($"Lisener port {LisenerPort} binded!!");
			if(ServerBrowser != null)
			{
				ServerBrowser.GetParent().GetNode<Label>("Label").Text = $"Lisener port {LisenerPort} binded!!";
			}
		}
		else
		{
			PrintRole($"Lisener port {LisenerPort} FAILED!!");
			if(ServerBrowser != null)
			{
				ServerBrowser.GetParent().GetNode<Label>("Label").Text = $"Lisener port {LisenerPort} FAILED!!";
			}
		}
	}

	public void CloseUp()
	{
		if(Lisener != null)
		{
			Lisener.Close();
		}

		if(Broadcaster != null)
		{
			Broadcaster.Close();
		}

		if(BroadcastTimer != null)
		{
			BroadcastTimer.Stop();
		}
	}

	public void SetUpBroadcast(string Name)
	{
		RoomList["name"] = Name;
		RoomList["players"] = (int)PlayersConected.Count;

		Broadcaster = new PacketPeerUdp();
		Broadcaster.SetBroadcastEnabled(true);
		Broadcaster.SetDestAddress(BroadcasterIp, LisenerPort);

		var ok = Broadcaster.Bind(BroadcasterPort);
		if(ok == Error.Ok)
		{
			// Usamos $ al principio y metemos la variable entre { }
			PrintRole($"Broadcaster port {BroadcasterPort} binded!!");
		}
		else
		{
			PrintRole($"Broadcaster port {BroadcasterPort} FAILED!!");
		}

		if(BroadcastTimer != null)
		{
			BroadcastTimer.Start();
		}
	}

	protected void _OnBroadcastTimerTimeout()
	{
		RoomList["players"] = PlayersConected.Count;
		var data = Json.Stringify(RoomList);
		var packet = data.ToAsciiBuffer();
		if(Broadcaster != null)
		{
			Broadcaster.PutPacket(packet);
		}
	}
}