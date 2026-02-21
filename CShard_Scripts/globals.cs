using Godot;
using Godot.Collections;


[GlobalClass]
public partial class Globals : Node
{
	[Signal]
	public delegate void CurrentWeatherAndDisasterChangedEventHandler(string new_disaster);


	//Editor
	public Variant Version = ProjectSettings.GetSetting("application/config/version");
	public Variant Gamename = ProjectSettings.GetSetting("application/config/name");
	public string Credits = "Miguelito2911";


	//Network
	[Export] public string Ip;
	[Export] public int Port = 5555;
	[Export] public int Points;
	[Export] public string Username = "Player";
	[Export] public Array<Node> PlayersConected;
	public Variant Multiplayerpeer;


	//Globals Weather
	[Export] public double Temperature = 23;
	[Export] public double Pressure = 10000;
	[Export] public double Oxygen = 100;
	[Export] public double Bradiation = 0;
	[Export] public double Humidity = 25;
	[Export] public Vector3 WindDirection = new Vector3(1, 0, 0);
	[Export] public double WindSpeed = 0;
	[Export] public bool IsRaining = false;
	public Variant Gravity = ProjectSettings.GetSetting("physics/3d/default_gravity");


	//Globals Time
	[Export] public double Time = 0.0;
	[Export] public double TimeLeft = 0.0;
	[Export] public int Day = 0;
	[Export] public int Hour = 0;
	[Export] public int Minute = 00;


	//Globals Weather target
	[Export] public double TemperatureTarget = 23;
	[Export] public double PressureTarget = 10000;
	[Export] public double OxygenTarget = 100;
	[Export] public double BradiationTarget = 0;
	[Export] public double HumidityTarget = 25;
	[Export] public Vector3 WindDirectionTarget = new Vector3(1, 0, 0);
	[Export] public double WindSpeedTarget = 0;


	//Globals Weather original
	[Export] public double TemperatureOriginal = 23;
	[Export] public double PressureOriginal = 10000;
	[Export] public double OxygenOriginal = 100;
	[Export] public double BradiationOriginal = 0;
	[Export] public double HumidityOriginal = 25;
	[Export] public Vector3 WindDirectionOriginal = new Vector3(1, 0, 0);
	[Export] public double WindSpeedOriginal = 0;

	[Export] public double Seconds = Time.GetUnixTimeFromSystem();

	[Export] public Node3D Main;
	[Export] public Control MainMenu;
	[Export] public Node3D Map;
	[Export] public Control ServerBrowser;
	[Export] public CharacterBody3D LocalPlayer;

	[Export] public Dictionary BoundingRadiusAreas = new Dictionary{};

	[Export] public string NodeGroup = "Destrollable";
	[Export] public Array DestrolledNode;

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

	public Resource PlayerScene = /* preload has no equivalent, add a 'ResourcePreloader' Node in your scene */("res://Scenes/player.tscn");
	public Resource ThunderstormScene = /* preload has no equivalent, add a 'ResourcePreloader' Node in your scene */("res://Scenes/thunder.tscn");
	public Resource MeteorScene = /* preload has no equivalent, add a 'ResourcePreloader' Node in your scene */("res://Scenes/meteor.tscn");
	public Resource TornadoScene = /* preload has no equivalent, add a 'ResourcePreloader' Node in your scene */("res://Scenes/tornado.tscn");
	public Resource TsunamiScene = /* preload has no equivalent, add a 'ResourcePreloader' Node in your scene */("res://Scenes/tsunami.tscn");
	public Resource VolcanoScene = /* preload has no equivalent, add a 'ResourcePreloader' Node in your scene */("res://Scenes/volcano.tscn");
	public Resource EarthquakeScene = /* preload has no equivalent, add a 'ResourcePreloader' Node in your scene */("res://Scenes/earthquake.tscn");

	public Node Timer;
	public Node BroadcastTimer;

	[Export] public Dictionary RoomList = new Dictionary{{"name", "name"},{"players", Int(0)},};
	[Export] public string BroadcasterIp = "192.168.1.255";
	[Export] public int LisenerPort = Port + 1;
	[Export] public int BroadcasterPort = Port - 1;
	public PacketPeerUdp Broadcaster;
	public PacketPeerUdp Lisener;

	[Export] public bool IsChatOpen = false;
	[Export] public bool IsPauseMenuOpen = false;
	[Export] public bool IsSpawnMenuOpen = false;

	[Export] public string Character = "blue";
	[Export] public Array<string> AvalibleCharacters = new Array<string>{"blue", "red", "green", "yellow"};
	[Export] public Dictionary AssignedCharacter;

	public double ConvertMetoSU(Variant metres)
	{
		return (int)(metres * 39.37) / 0.75;
	}

	public int ConvertKMPHtoMe(double kmph)
	{
		return (int)((kmph * 1000) / 3600);
	}

	public int ConvertVectorToAngle(Variant vector)
	{
		var x = vector.X;
		var y = vector.Z;

		return (int)(360 + Mathf.RadToDeg(Mathf.Atan2(y, x))) % 360;
	}

	protected PhysicsDirectSpaceState3D _GetDirectSpaceState(Node node)
	{

		// Intenta obtener el World3D a partir del nodo; si falla, intenta la escena actual.
		World3D world = null;
		if(node != null && GodotObject.IsInstanceValid(node) && node is Node3D node3D)
		{
			world = node3D.GetWorld3D();
		}
		if(world == null)
		{
			var scene = GetTree().GetCurrentScene();
			if(GodotObject.IsInstanceValid(scene) && scene is Node3D scene3D)
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

	public bool PerformTraceCollision(Variant ply, Variant direction)
	{
		var start_pos = ((Node3D)ply).GlobalPosition;
		var end_pos = start_pos + ((Vector3)direction) * 1000;
		var space_state = _GetDirectSpaceState(ply);
		if(space_state == null)
		{
			return false;
		}
		var ray = PhysicsRayQueryParameters3D.Create(start_pos, end_pos);
		ray.Exclude = new Array{((GodotObject)ply).GetRid(), };
		var result = space_state.IntersectRay(ray);
		return result != new Dictionary{};
	}


	public int PerformTraceWind(Variant ply, Variant direction)
	{
		var start_pos = ((Node3D)ply).GlobalPosition;
		var end_pos = start_pos + ((Vector3)direction) * 60000;
		var space_state = _GetDirectSpaceState(ply);
		if(space_state == null)
		{
			return end_pos;
		}
		var ray = PhysicsRayQueryParameters3D.Create(start_pos, end_pos);
		ray.Exclude = new Array{((GodotObject)ply).GetRid(), };
		var result = space_state.IntersectRay(ray);
		if(result != new Dictionary{} && result.Contains("position"))
		{
			return result.Position;
		}
		else
		{
			return end_pos;
		}
	}

	public Node GetNodeByIdRecursive(Node node, int node_id)
	{
		if(node.GetInstanceId() == node_id)
		{
			return node;
		}

		foreach(Node child in node.GetChildren())
		{
			var result = GetNodeByIdRecursive(child, node_id);
			if(result != null)
			{
				return result;
			}
		}

		return null;
	}

	public bool IsBelowSky(Variant ply)
	{
		var start_pos = ((Node3D)ply).GlobalPosition;
		var end_pos = start_pos + new Vector3(0, 48000, 0);
		var space_state = _GetDirectSpaceState((Node3D)ply);

		// Si no hay espacio de f�sicas disponible, asumimos "al aire libre" (true)
		if(space_state == null)
		{
			return true;
		}
		var ray = PhysicsRayQueryParameters3D.Create(start_pos, end_pos);
		ray.Exclude = new Array{ply.GetRid(), };
		var result = space_state.IntersectRay(ray);
		return !result;
	}


	public bool IsOutdoor(Variant ply)
	{
		var hit_sky = IsBelowSky(ply);

		if(ply.IsInGroup("player"))
		{
			if(hit_sky)
			{
				ply.Outdoor = true;
			}
			else
			{
				ply.Outdoor = false;
			}

			return hit_sky;
		}
		else
		{
			return hit_sky;
		}
	}

	public void IsInwater(Variant ply)
	{
		if(ply.IsInGroup("player"))
		{
			return ply.IsInWater;
		}
	}

	public void IsUnderwater(Variant ply)
	{
		if(ply.IsInGroup("player"))
		{
			return ply.IsUnderWater;
		}
	}

	public void IsInlava(Variant ply)
	{
		if(ply.IsInGroup("player"))
		{
			return ply.IsInLava;
		}
	}

	public void IsUnderlava(Variant ply)
	{
		if(ply.IsInGroup("player"))
		{
			return ply.IsUnderLava;
		}
	}


	public Vector3 Vec2ToVec3(Variant vector)
	{
		return new Vector3(vector.X, 0, vector.Y);
	}

	public bool IsSomethingBlockingWind(Variant entity)
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
		ray.Exclude = new Array{entity.GetRid(), };
		var result = space_state.IntersectRay(ray);
		return result != new Dictionary{};
	}

	public double CalculeBoundingRadius(Variant entity)
	{
		var max_radius = 0.0;

		foreach(Node child in entity.GetChildren())
		{
			if(child.GetChildCount() > 0)
			{
				return CalculeBoundingRadius(child);
			}

			if(child.IsClass("MeshInstance3D") && child != null)
			{
				var mesh = child.Mesh;
				var aabb = mesh.GetAabb();


				// Obtener los 8 v�rtices de la AABB original
				var vertices = new Array{
									aabb.Position, 
									aabb.Position + new Vector3(aabb.Size.X, 0, 0), 
									aabb.Position + new Vector3(0, aabb.Size.Y, 0), 
									aabb.Position + new Vector3(0, 0, aabb.Size.Z), 
									aabb.Position + new Vector3(aabb.Size.X, aabb.Size.Y, 0), 
									aabb.Position + new Vector3(aabb.Size.X, 0, aabb.Size.Z), 
									aabb.Position + new Vector3(0, aabb.Size.Y, aabb.Size.Z), 
									aabb.Position + aabb.Size, 
									};


				// Transformar los v�rtices con la matriz de transformaci�n del MeshInstance3D
				var transformed_vertices = new Array{};
				foreach(Variant vertex in vertices)
				{
					transformed_vertices.Append(child.Transform * vertex);


					// Calcular el nuevo AABB a partir de los v�rtices transformados

				}// Calcular el radio de contorno a partir de los v�rtices transformados
				foreach(Variant vertex in transformed_vertices)
				{
					var distance = vertex.Length();
					max_radius = Mathf.Max(max_radius, distance);
				}
			}
		}


		return max_radius;
	}


	public Array SearchInNode(Variant node, Vector3 origin, double radius, Array result)
	{
		foreach(int i in GD.Range(node.GetChildCount()))
		{
			var child = node.GetChild(i);
			if(child.IsClass("Spatial"))
			{
				// Solo considerar nodos Spatial (puedes ajustar esto seg�n tus necesidades)
				var distance = origin.DistanceTo(child.GlobalPosition);
				if(distance <= radius)
				{
					result.Append(child);
				}
			}

			// Recursi�n si el nodo tiene hijos
			if(child.GetChildCount() > 0)
			{
				SearchInNode(child, origin, radius, result);
			}
		}

		return result;
	}

	public Array FindInSphere(Vector3 origin, double radius)
	{
		var result = new Array{};
		var scene_root = GetTree().GetRoot();

		result = SearchInNode(scene_root, origin, radius, result);

		return result;
	}

	public void Wind(Variant obj)
	{

		// Verificar si el objeto es un jugador
		if(obj.IsInGroup("player"))
		{
			if(!GodotObject.IsInstanceValid(obj))
			{
				return ;
			}


			// Calcular la velocidad del viento local
			var local_wind = WindSpeed;
			if(!IsOutdoor(obj) || IsSomethingBlockingWind(obj))
			{
				local_wind = 0;
			}

			obj.BodyWind = local_wind;


			// Calcular la velocidad del viento y la fricci�n
			var wind_vel = WindDirection * local_wind;

			// Verificar si est� al aire libre y no hay obst�culos que bloqueen el viento
			if(IsOutdoor(obj) && !IsSomethingBlockingWind(obj) && local_wind >= 30)
			{
				var delta_velocity = wind_vel - obj.Velocity;
				obj.Velocity += delta_velocity * 0.3;
			}
		}

		else if(obj.IsInGroup("movable_objects") && obj.IsClass("RigidBody3D"))
		{
			if(GodotObject.IsInstanceValid(obj) && IsOutdoor(obj) && !IsSomethingBlockingWind(obj))
			{
				var wind_vel = WindDirection * WindSpeed;
				var delta_velocity = wind_vel - obj.LinearVelocity;


				// Aplica fuerza en vez de modificar directamente la velocidad
				obj.ApplyCentralForce(delta_velocity * 0.3);
			}
		}

		else if(obj.IsInGroup("movable_objects") && obj.IsClass("StaticBody3D"))
		{
			if(GodotObject.IsInstanceValid(obj))
			{
				if(obj.IsInGroup("Destrollable") || obj.IsInGroup("Hause"))
				{
					if(WindSpeed > 100)
					{
						obj.Destroy.Rpc();
					}
				}
			}
		}
	}


	public double Area(Variant entity)
	{
		if(entity || entity.BoundingRadiusArea == null.Contains(!"bounding_radius_area"))
		{
			var bounding_radius = CalculeBoundingRadius(entity);
			var bounding_radius_area = (2 * Mathf.Pi) * (bounding_radius * bounding_radius);
			BoundingRadiusAreas[entity] = bounding_radius_area;

			return bounding_radius_area;
		}
		else
		{
			return entity.BoundingRadiusArea;
		}
	}

	public int GetFrameMultiplier()
	{
		var frame_time = Engine.GetFramesPerSecond();
		if(frame_time == 0)
		{
			return 0;
		}
		else
		{
			return 60 / frame_time;
		}
	}

	public double GetPhysicsMultiplier()
	{
		var physics_interval = GetPhysicsProcessDeltaTime();
		return (200.0 / 3.0) / physics_interval;
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


	public void SyncPlayerList()
	{
		PlayersConected.Clear();

		foreach(Node p in GetTree().GetNodesInGroup("player"))
		{
			if(GodotObject.IsInstanceValid(p))
			{
				PlayersConected.Append(p);
			}
		}
	}


	// Funci�n para verificar si hay jugadores con el mismo nombre
	public bool HayJugadoresConMismoNombre(string nombre_a_verificar, Node excluir_jugador = null)
	{
		var contador = 0;
		foreach(Node player in GetTree().GetNodesInGroup("player"))
		{

			// Si se debe excluir un jugador espec�fico, saltarlo
			if(excluir_jugador != null && player == excluir_jugador)
			{
				continue;
			}


			// Verificar si el nombre coincide
			if(GodotObject.IsInstanceValid(player) && player.Has("username") && player.Username == nombre_a_verificar)
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
		var jugadores_duplicados = new Array{};

		foreach(Node player in GetTree().GetNodesInGroup("player"))
		{

			// Si se debe excluir un jugador espec�fico, saltarlo
			if(excluir_jugador != null && player == excluir_jugador)
			{
				continue;
			}


			// Verificar si el nombre coincide
			if(GodotObject.IsInstanceValid(player) && player.Has("username") && player.Username == nombre_a_verificar)
			{
				jugadores_duplicados.Append(player);
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


			// Verificar si el nombre coincide
			if(GodotObject.IsInstanceValid(player) && player.Has("username") && player.Username == nombre_a_verificar)
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
			Multiplayerpeer = ENetMultiplayerPeer.New();
			var error = Multiplayerpeer.CreateServer(Port);
			if(error == OK)
			{
				Multiplayer.MultiplayerPeer = Multiplayerpeer;
				if(Multiplayer.IsServer())
				{
					if(OS.HasFeature("dedicated_server") || OS.GetCmdlineUserArgs() || OS.GetCmdlineUserArgs().Contains("server").Contains("s"))
					{
						PrintRole("Dedicated server init");
	
						await ToSignal(GetTree().CreateTimer(2), "Timeout");
	
						SetUpBroadcast(Username);
						LoadScene.LoadScene(MainMenu, "map");
					}
					else
					{
						PrintRole("Server init");
						SetUpBroadcast(Username);
						LoadScene.LoadScene(MainMenu, "map");
					}
				}
			}
			else
			{
				PrintRole("Fatal Error in server");
			}
		}

		public void RequestPickObject(NodePath player_path, NodePath target_path)
		{

			// Solo el servidor debe ejecutar esta l�gica
			if(!Multiplayer.IsServer())
			{
				return ;
			}

			var root = GetTree().GetRoot();

			var player = root.GetNodeOrNull(player_path);
			var target = root.GetNodeOrNull(target_path);

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

			if(target is RigidBody3D)
			{
				target.LinearVelocity = new Vector3(0.1, 3, 0.1);
			}
		}

		public void PlayMultiplayerClient()
		{
			Multiplayerpeer = ENetMultiplayerPeer.New();
			var error = Multiplayerpeer.CreateClient(Ip, Port);
			if(error == OK)
			{
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

			Multiplayerpeer = OfflineMultiplayerPeer.New();
			Multiplayer.MultiplayerPeer = Multiplayerpeer;

			LoadScene.LoadScene(Map, "res://Scenes/main_menu.tscn");
		}

		public void AssingCharacter(string charac)
		{
			foreach(Variant c in AvalibleCharacters)
			{
				if(c == charac)
				{
					Character = charac;
					break;
				}
			}

			if(LocalPlayer && GodotObject.IsInstanceValid(LocalPlayer))
			{
				LocalPlayer.Character = charac;
			}

			PrintRole("Asignado el personaje: " + charac);
		}

		public bool AssingCharacterToPlayer(int id, string charac)
		{
			var chosen_char = charac;


			// Si el char recibido no es v�lido o ya est� ocupado, buscamos el siguiente disponible.
			if(chosen_char == null || chosen_char == "" || !IsCharacterAvalible(chosen_char))
			{
				chosen_char = GetNextAvalibleCharacter();
			}

			if(chosen_char == null || chosen_char == "" || !IsCharacterAvalible(chosen_char))
			{
				PrintRole("No hay personaje disponible para el id " + Str(id));
				return false;
			}

			AssignedCharacter[id] = chosen_char;
			assing_character.RpcId(id, chosen_char);
			PrintRole("Asignado al id " + Str(id) + " el personaje " + chosen_char);
			return true;
		}

		public void SyncAssignedCharacter(Dictionary data)
		{
			AssignedCharacter = data.Duplicate(true);
		}

		public bool IsCharacterAvalible(string charac)
		{
			foreach(Dictionary id in AssignedCharacter)
			{
				if(AssignedCharacter[id] == charac)
				{
					return false;
				}
			}

			return true;
		}


		public Variant GetNextAvalibleCharacter()
		{
			foreach(Variant charac in AvalibleCharacters)
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

			Multiplayerpeer = OfflineMultiplayerPeer.New();
			Multiplayer.MultiplayerPeer = Multiplayerpeer;

			LoadScene.LoadScene(Map, "res://Scenes/main_menu.tscn");
		}


		public void MultiplayerConnectionServerSucess()
		{
			PrintRole("connected to server");
			UnloadScene.UnloadScene(MainMenu);
		}

		public override void _ExitTree()
		{
			Multiplayer.PeerConnected -= MultiplayerPlayerSpawner;
			Multiplayer.PeerDisconnected -= MultiplayerPlayerRemover;
			Multiplayer.ServerDisconnected -= MultiplayerServerDisconnected;
			Multiplayer.ConnectedToServer -= MultiplayerConnectionServerSucess;
			Multiplayer.ConnectionFailed -= MultiplayerConnectionFailed;

			Globals.TemperatureTarget = Globals.TemperatureOriginal;
			Globals.HumidityTarget = Globals.HumidityOriginal;
			Globals.PressureTarget = Globals.PressureOriginal;
			Globals.WindDirectionTarget = Globals.WindDirectionOriginal;
			Globals.WindSpeedTarget = Globals.WindSpeedOriginal;

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

			TimeLeft = Timer.TimeLeft;
			Temperature = Mathf.Clamp(Temperature,  - 275.5, 275.5);
			Humidity = Mathf.Clamp(Humidity, 0, 100);
			Bradiation = Mathf.Clamp(Bradiation, 0, 100);
			Pressure = Mathf.Clamp(Pressure, 0, 100000);
			Oxygen = Mathf.Clamp(Oxygen, 0, 100);

			Temperature = Mathf.Lerp(Temperature, TemperatureTarget, 0.005);
			Humidity = Mathf.Lerp(Humidity, HumidityTarget, 0.005);
			Bradiation = Mathf.Lerp(Bradiation, BradiationTarget, 0.005);
			Pressure = Mathf.Lerp(Pressure, PressureTarget, 0.005);
			Oxygen = Mathf.Lerp(Oxygen, OxygenTarget, 0.005);
			WindDirection = Mathf.Lerp(WindDirection, WindDirectionTarget, 0.005);
			WindSpeed = Mathf.Lerp(WindSpeed, WindSpeedTarget, 0.005);
		}


		public override void _Ready()
		{
			Timer = GetNode("Timer");
			BroadcastTimer = GetNode("Broadcast_Timer");
			Multiplayer.PeerConnected += MultiplayerPlayerSpawner;
			Multiplayer.PeerDisconnected += MultiplayerPlayerRemover;
			Multiplayer.ServerDisconnected += MultiplayerServerDisconnected;
			Multiplayer.ConnectedToServer += MultiplayerConnectionServerSucess;
			Multiplayer.ConnectionFailed += MultiplayerConnectionFailed;
	
			Multiplayerpeer = OfflineMultiplayerPeer.New();
			Multiplayer.MultiplayerPeer = Multiplayerpeer;
			
			GlobalsData = DataResource.LoadFile();
		}


		public void MultiplayerPlayerSpawner(int peer_id = 1)
		{
			if(!Multiplayer.IsServer())
			{
				return ;
			}

			if(Map && GodotObject.IsInstanceValid(Map))
			{
				PrintRole("Joined player id: " + Str(peer_id));
				var player = PlayerScene.Instantiate();
				player.Name = Str(peer_id);
				Map.AddChild(player, true);


				var assigned_ok = true;

				if(AssignedCharacter.Contains(!peer_id))
				{
					var next_character = GetNextAvalibleCharacter();
					assigned_ok = AssingCharacterToPlayer(peer_id, next_character);
				}

				if(assigned_ok)
				{
					sync_assigned_character.Rpc(AssignedCharacter);
					SyncAssignedCharacter(AssignedCharacter);
					sync_player_list.Rpc();
					sync_destrolled_nodes.RpcId(peer_id, DestrolledNode);
					// envia al cliente
					set_weather_and_disaster.RpcId(peer_id, CurrentWeatherAndDisasterInt);
				}
				else
				{
					PrintRole("No se pudo asignar personaje al jugador con id: " + Str(peer_id));
				}
			}

			else
			{
				sync_assigned_character.Rpc(AssignedCharacter);
				SyncAssignedCharacter(AssignedCharacter);
				sync_player_list.Rpc();
				sync_destrolled_nodes.RpcId(peer_id, DestrolledNode);
				// broadcast
				PrintRole("No se pudo a�adir al jugador con el id: " + Str(peer_id));
			}
		}


		public void MultiplayerPlayerRemover(int peer_id = 1)
		{
			if(!Multiplayer.IsServer())
			{
				return ;
			}


			// Intentar obtener el jugador de forma segura
			var player_node = Map.GetNodeOrNull(Str(peer_id));
			if(player_node && GodotObject.IsInstanceValid(player_node))
			{
				PrintRole("Disconected player id: " + Str(peer_id));
				player_node.QueueFree();

				await ToSignal(player_node, "TreeExited");

				if(AssignedCharacter.Contains(peer_id))
				{
					AssignedCharacter.Erase(peer_id);
				}


				sync_assigned_character.Rpc(AssignedCharacter);
				SyncAssignedCharacter(AssignedCharacter);
				sync_player_list.Rpc();
			}


			else
			{
				if(AssignedCharacter.Contains(peer_id))
				{
					AssignedCharacter.Erase(peer_id);
				}

				sync_assigned_character.Rpc(AssignedCharacter);
				SyncAssignedCharacter(AssignedCharacter);
				sync_player_list.Rpc();
				PrintRole("player no found: " + Str(peer_id));
			}
		}


		public void SyncWeatherAndDisaster()
		{
			if(Multiplayer.IsServer())
			{
				var random_weather_and_disaster = GD.RandRange(0, 13);
				set_weather_and_disaster.Rpc(random_weather_and_disaster);
			}
		}

		public void SetWeatherAndDisaster(Variant weather_and_disaster_index)
		{

			if(weather_and_disaster_index == 0)
			{
				CurrentWeatherAndDisaster = "Sun";
				CurrentWeatherAndDisasterInt = weather_and_disaster_index;
			}
			if(weather_and_disaster_index == 1)
			{
				CurrentWeatherAndDisaster = "Cloud";
				CurrentWeatherAndDisasterInt = weather_and_disaster_index;
			}
			if(weather_and_disaster_index == 2)
			{
				CurrentWeatherAndDisaster = "Raining";
				CurrentWeatherAndDisasterInt = weather_and_disaster_index;
			}
			if(weather_and_disaster_index == 3)
			{
				CurrentWeatherAndDisaster = "Storm";
				CurrentWeatherAndDisasterInt = weather_and_disaster_index;
			}
			if(weather_and_disaster_index == 4)
			{
				CurrentWeatherAndDisaster = "Thunderstorm";
				CurrentWeatherAndDisasterInt = weather_and_disaster_index;
			}
			if(weather_and_disaster_index == 5)
			{
				CurrentWeatherAndDisaster = "Tsunami";
				CurrentWeatherAndDisasterInt = weather_and_disaster_index;
			}
			if(weather_and_disaster_index == 6)
			{
				CurrentWeatherAndDisaster = "Meteors shower";
				CurrentWeatherAndDisasterInt = weather_and_disaster_index;
			}
			if(weather_and_disaster_index == 7)
			{
				CurrentWeatherAndDisaster = "Volcano";
				CurrentWeatherAndDisasterInt = weather_and_disaster_index;
			}
			if(weather_and_disaster_index == 8)
			{
				CurrentWeatherAndDisaster = "Tornado";
				CurrentWeatherAndDisasterInt = weather_and_disaster_index;
			}
			if(weather_and_disaster_index == 9)
			{
				CurrentWeatherAndDisaster = "Acid rain";
				CurrentWeatherAndDisasterInt = weather_and_disaster_index;
			}
			if(weather_and_disaster_index == 10)
			{
				CurrentWeatherAndDisaster = "Earthquake";
				CurrentWeatherAndDisasterInt = weather_and_disaster_index;
			}
			if(weather_and_disaster_index == 11)
			{
				CurrentWeatherAndDisaster = "Sand Storm";
				CurrentWeatherAndDisasterInt = weather_and_disaster_index;
			}
			if(weather_and_disaster_index == 12)
			{
				CurrentWeatherAndDisaster = "blizzard";
				CurrentWeatherAndDisasterInt = weather_and_disaster_index;
			}
			if(weather_and_disaster_index == 13)
			{
				CurrentWeatherAndDisaster = "Dust Storm";
				CurrentWeatherAndDisasterInt = weather_and_disaster_index;
			}
			if(weather_and_disaster_index == "Sun")
			{
				CurrentWeatherAndDisaster = weather_and_disaster_index;
				CurrentWeatherAndDisasterInt = 0;
			}
			if(weather_and_disaster_index == "Cloud")
			{
				CurrentWeatherAndDisaster = weather_and_disaster_index;
				CurrentWeatherAndDisasterInt = 1;
			}
			if(weather_and_disaster_index == "Raining")
			{
				CurrentWeatherAndDisaster = weather_and_disaster_index;
				CurrentWeatherAndDisasterInt = 2;
			}
			if(weather_and_disaster_index == "Storm")
			{
				CurrentWeatherAndDisaster = weather_and_disaster_index;
				CurrentWeatherAndDisasterInt = 3;
			}
			if(weather_and_disaster_index == "Thunderstorm")
			{
				CurrentWeatherAndDisaster = weather_and_disaster_index;
				CurrentWeatherAndDisasterInt = 4;
			}
			if(weather_and_disaster_index == "Tsunami")
			{
				CurrentWeatherAndDisaster = weather_and_disaster_index;
				CurrentWeatherAndDisasterInt = 5;
			}
			if(weather_and_disaster_index == "Meteors shower")
			{
				CurrentWeatherAndDisaster = weather_and_disaster_index;
				CurrentWeatherAndDisasterInt = 6;
			}
			if(weather_and_disaster_index == "Volcano")
			{
				CurrentWeatherAndDisaster = weather_and_disaster_index;
				CurrentWeatherAndDisasterInt = 7;
			}
			if(weather_and_disaster_index == "Tornado")
			{
				CurrentWeatherAndDisaster = weather_and_disaster_index;
				CurrentWeatherAndDisasterInt = 8;
			}
			if(weather_and_disaster_index == "Acid rain")
			{
				CurrentWeatherAndDisaster = weather_and_disaster_index;
				CurrentWeatherAndDisasterInt = 9;
			}
			if(weather_and_disaster_index == "Earthquake")
			{
				CurrentWeatherAndDisaster = weather_and_disaster_index;
				CurrentWeatherAndDisasterInt = 10;
			}

			if(weather_and_disaster_index == "Sand Storm")
			{
				CurrentWeatherAndDisaster = weather_and_disaster_index;
				CurrentWeatherAndDisasterInt = 11;
			}
			if(weather_and_disaster_index == "blizzard")
			{
				CurrentWeatherAndDisaster = weather_and_disaster_index;
				CurrentWeatherAndDisasterInt = 12;
			}
			if(weather_and_disaster_index == "Dust Storm")
			{
				CurrentWeatherAndDisaster = weather_and_disaster_index;
				CurrentWeatherAndDisasterInt = 13;
			}
			else 
			{
				CurrentWeatherAndDisaster = "Original";
				CurrentWeatherAndDisasterInt =  - 1;
			}
	}

	public void AddPoints()
	{
		Points += 1;
	}


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
		if(peer == null || peer is OfflineMultiplayerPeer || peer.GetConnectionStatus() != MultiplayerPeer.ConnectionStatus.ConnectionConnected)
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

	public void SyncDestrolledNodes(Array Hauses)
	{
		foreach(Variant house_name in Hauses)
		{
			var house = GetTree().GetCurrentScene().GetNodeOrNull(house_name);
			if(house)
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
			DestrolledNode.Append(Name);
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
			DestrolledNode.Erase(Name);
		}
	}

	public void RemoveAllDestrolledNodes()
	{
		if(!Multiplayer.IsServer())
		{
			return;
		}

		foreach(Variant i in DestrolledNode)
		{
			RemoveDestrolledNodes(i);
		}
	}

	public void SetUpLisener()
	{
		Lisener = PacketPeerUDP.New();
		var ok = Lisener.Bind(LisenerPort);
		if(ok == OK)
		{
			PrintRole("Lisener port %s binded!!" % LisenerPort);
			if(ServerBrowser != null)
			{
				ServerBrowser.GetParent().GetNode("Label").Text = "Lisener port %s binded!!" % LisenerPort;
			}
		}
		else
		{
			PrintRole("Lisener port %s FAILED!!" % LisenerPort);
			if(ServerBrowser != null)
			{
				ServerBrowser.GetParent().GetNode("Label").Text = "Lisener port %s FAILED!!" % LisenerPort;
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

	public void SetUpBroadcast(Variant Name)
	{
		RoomList.Name = Name;
		RoomList.Players = PlayersConected.Size();

		Broadcaster = PacketPeerUDP.New();
		Broadcaster.SetBroadcastEnabled(true);
		Broadcaster.SetDestAddress(BroadcasterIp, LisenerPort);

		var ok = Broadcaster.Bind(BroadcasterPort);
		if(ok == OK)
		{
			PrintRole("Broadcaster port %s binded!!" % BroadcasterPort);
		}
		else
		{
			PrintRole("Broadcaster port %s FAILED!!" % BroadcasterPort);
		}

		if(BroadcastTimer != null)
		{
			BroadcastTimer.Start();
		}
	}

	protected void _OnBroadcastTimerTimeout()
	{
		RoomList.Players = PlayersConected.Size();
		var data = JSON.stringify(RoomList);
		var packet = data.ToAsciiBuffer();
		if(Broadcaster != null)
		{
			Broadcaster.PutPacket(packet);
		}
	}
}