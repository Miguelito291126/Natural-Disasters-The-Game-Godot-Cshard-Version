using System.Linq;
using Godot;
using Godot.Collections;

[GlobalClass]
public partial class Chat : CanvasLayer
{
	public TextEdit TextEdit;

	public LineEdit LineEdit;
	public Button Button;


	public Array<string> AutocompleteMatches = new Array<string>();
	public int AutocompleteIndex = 0;
	public Array<string> AutocompleteMethods = new Array<string>();
	public Array<string> History = new Array<string>();
	public int HistoryIndex =  - 1;
	public bool UserIsScrolling = false;
	public int ScrollRetries = 0;
	public const int MAX_SCROLL_RETRIES = 5;


	public Dictionary<string, Dictionary<string, Variant>> DevCommands = new Dictionary<string, Dictionary<string, Variant>>{
			{"god_mode", new Dictionary<string, Variant>{
						{"desc", "Muestra todos los comandos."},
						{"method", "_cmd_god_mode_player"},
						{"args", 0},
						}},
			{"ungod_mode", new Dictionary<string, Variant>{
						{"desc", "Muestra todos los comandos."},
						{"method", "_cmd_ungod_mode_player"},
						{"args", 0},
						}},
			{"kill_player", new Dictionary<string, Variant>{
						{"desc", "Cambia la velocidad del jugador. Uso: /set_speed 10"},
						{"method", "_cmd_kill_player"},
						{"args", 1},
						}},
			{"teleport_player", new Dictionary<string, Variant>{
						{"desc", "Teletransporta al jugador a otro jugador. Uso: /teleport_player PlayerName"},
						{"method", "_cmd_teleport_player"},
						{"args", 1},
						}},
			{"teleport_position", new Dictionary<string, Variant>{
						{"desc", "Teletransporta al jugador a una posici�n. Uso: /teleport_position Vector3(x,y,z)"},
						{"method", "_cmd_teleport_position"},
						{"args", 1},
						}},
			{"kick_player", new Dictionary<string, Variant>{
						{"desc", "Expulsa a un jugador del servidor. Uso: /kick_player PlayerName"},
						{"method", "_cmd_kick_player"},
						{"args", 1},
						}},
			{"damage_player", new Dictionary<string, Variant>{
						{"desc", "Inflige da�o a un jugador. Uso: /damage_player PlayerName damage_amount"},
						{"method", "_cmd_damage_player"},
						{"args", 2},
						}},
			{"spawn_disaster", new Dictionary<string, Variant>{
						{"desc", "Genera un desastre o clima. Uso: /spawn_disaster disaster_name"},
						{"method", "_cmd_spawn_disaster_weather"},
						{"args", 1},
						}},
			{"admin", new Dictionary<string, Variant>{
						{"desc", "Genera un desastre o clima. Uso: /spawn_disaster disaster_name"},
						{"method", "_cmd_admin_mode_player"},
						{"args", 1},
						}},

			{"unadmin", new Dictionary<string, Variant>{
						{"desc", "Genera un desastre o clima. Uso: /spawn_disaster disaster_name"},
						{"method", "_cmd_unadmin_mode_player"},
						{"args", 1},
						}},

			};

	protected Player _GetLocalPlayer()
	{
		foreach(Player p in GetTree().GetNodesInGroup("player"))
		{
			if(p is Player player && player.IsMultiplayerAuthority())
				{
					return p;
				}
		}

		return null;
	}


	protected string _CmdGodModePlayer()
	{
		var player = _GetLocalPlayer() as Player;
		if(player == null || !player.AdminMode)
		{
			return "No tienes permisos";
		}
		player.GodMode = true;
		return "God Mode activado en ti";
	}

	protected string _CmdUngodModePlayer()
	{
		var player = _GetLocalPlayer() as Player;
		if(player == null || !player.AdminMode)
		{
			return "No tienes permisos";
		}
		player.GodMode = false;
		return "God Mode desactivado en ti";
	}


	protected string _CmdAdminModePlayer(string player_name)
	{
		var local = _GetLocalPlayer() as Player;
		if(local == null || !local.AdminMode)
		{
			return "No tienes permisos";
		}


		// Solo el servidor puede cambiar admin_mode
		if(!Multiplayer.IsServer())
		{
			return "Solo el servidor puede cambiar permisos de admin";
		}


		// Buscar el jugador por nombre
		Player jugador_encontrado = null;
		foreach(Player p in GetTree().GetNodesInGroup("player"))
		{
			if(GodotObject.IsInstanceValid(p) && p.Username == player_name)
			{
				jugador_encontrado = p;
				break;
			}
		}

		if(jugador_encontrado == null)
		{
			return $"Jugador no encontrado: {player_name}";


			// Usar RPC para sincronizar el cambio en todos los clientes

		}
		// call_local ya ejecuta la funcin localmente en el servidor
		Rpc(nameof(Player._SetAdminMode), true);
		return $"Ahora {player_name} es admin";
	}

	protected string _CmdUnadminModePlayer(string player_name)
	{
		var local = _GetLocalPlayer() as Player;
		if(local == null || !local.AdminMode)
		{
			return "No tienes permisos";
		}


		// Solo el servidor puede cambiar admin_mode
		if(!Multiplayer.IsServer())
		{
			return "Solo el servidor puede cambiar permisos de admin";
		}


		// Buscar el jugador por nombre
		Player jugador_encontrado = null;
		foreach(Player p in GetTree().GetNodesInGroup("player"))
		{
			if(GodotObject.IsInstanceValid(p) && p.Username == player_name)
			{
				jugador_encontrado = p;
				break;
			}
		}

		if(jugador_encontrado == null)
		{
			return $"Jugador no encontrado: {player_name}";


			// Usar RPC para sincronizar el cambio en todos los clientes

		}// call_local ya ejecuta la funcin localmente en el servidor
		Rpc(nameof(Player._SetAdminMode), false);
		return $"Ahora {player_name} ya no es admin";
	}


	protected string _CmdKillPlayer(string player_name)
	{
		var local = _GetLocalPlayer();
		if(local == null || !local.AdminMode)
		{
			return "No tienes permisos";
		}
		foreach(Node p in GetTree().GetNodesInGroup("player"))
		{
			if(p is Player player && player.Username == player_name)
			{
				player.Damage(999);
				return $"{player_name} ha sido eliminado";
			}
		}
		return "Jugador no encontrado";
	}


	protected string _CmdKickPlayer(string player_name)
	{
		var local = _GetLocalPlayer() as Player;
		if(local == null || !local.AdminMode)
		{
			return "No tienes permisos";
		}
		foreach(Node p in GetTree().GetNodesInGroup("player"))
		{
			if(p is Player player && player.Username == player_name)
			{
				Multiplayer.MultiplayerPeer.DisconnectPeer(player.PlayerId, true);
				return $"{player_name} expulsado";
			}
		}
		return "Jugador no encontrado";
	}


	protected string _CmdTeleportPlayer(string player_name, string target_name)
	{
		var local = _GetLocalPlayer();
		if (local == null || !local.AdminMode) return "No tienes permisos";

		Player playerToMove = null; // Cambiado de 'player' para evitar conflictos
		Player targetPlayer = null;

		foreach (Node p in GetTree().GetNodesInGroup("player"))
		{
			if (p is Player playerNode)
			{
				if (playerNode.Username == player_name) playerToMove = playerNode;
				if (playerNode.Username == target_name) targetPlayer = playerNode;
			}
		}

		if (playerToMove == null || targetPlayer == null) return "Jugador no encontrado";

		playerToMove.GlobalPosition = targetPlayer.GlobalPosition;
		return $"Teletransportado {player_name} a {target_name}";
	}

	protected string _CmdDamagePlayer(string player_name, Variant damage)
	{
		var local = _GetLocalPlayer();
		if (local == null || !local.AdminMode) return "No tienes permisos";
		
		// Los comandos suelen recibir Variant desde el sistema de chat, convertimos a int
		int damageAmount = damage.AsInt32();

		foreach (Node p in GetTree().GetNodesInGroup("player"))
		{
			if (p is Player player && player.Username == player_name)
			{
				player.Damage(damageAmount);
				return $"{player_name} recibió {damageAmount} de daño";
			}
		}
		return "Jugador no encontrado";
	}

	protected string _CmdSpawnDisasterWeather(string disaster_name)
	{
		var local = _GetLocalPlayer();
		if(local == null || !local.AdminMode)
		{
			return "No tienes permisos";
		}

		Globals.Instance.SetWeatherAndDisaster(disaster_name);
		return $"Clima/Desastre activado: {disaster_name}";
	}


	public override void _EnterTree()
	{
		SetMultiplayerAuthority(Multiplayer.GetUniqueId());
	}

	public override void _Ready()
	{
		TextEdit = GetNode<TextEdit>("Panel/TextEdit");
		LineEdit = GetNode<LineEdit>("Panel/Panel2/LineEdit");
		Button = GetNode<Button>("Panel/Panel2/Button");

		if(!IsMultiplayerAuthority())
		{
			this.Visible = false;
			return ;
		}

		this.Visible = true;

		AutocompleteMethods = new Array<string>(DevCommands.Keys);
	}

	public override void _Input(InputEvent @event)
	{
		if (!IsMultiplayerAuthority()) return;

		if (@event.IsActionPressed("ui_accept")) // Tecla Enter
		{
			if (LineEdit.HasFocus())
			{
				// Aquí procesarías el texto: _OnSendButtonPressed();
				LineEdit.ReleaseFocus();
			}
			else
			{
				LineEdit.GrabFocus();
			}
		}
	}



	protected bool _IsAtBottom()
	{
		var scroll_bar = TextEdit.GetVScrollBar();
		if(scroll_bar == null)
		{
			return true;

			// Considerar que est� al final si est� dentro de 20 p�xeles del m�ximo

		}// Esto permite un peque�o margen para detectar si el usuario est� scrolleando
		if(scroll_bar.MaxValue <= 0)
		{
			return true;
		}

		return scroll_bar.Value >= (scroll_bar.MaxValue - 20);
	}

	protected void _ScrollToBottom()
	{
		ScrollRetries = 0;
		CallDeferred("_do_scroll_to_bottom");
	}

	protected void _DoScrollToBottom()
	{

		// Si el nodo ya no existe, parar
		if(!GodotObject.IsInstanceValid(this) || !IsInsideTree())
		{
			return ;
		}

		if(!GodotObject.IsInstanceValid(TextEdit))
		{
			return ;
		}

		var scroll_bar = TextEdit.GetVScrollBar();

		if(scroll_bar == null)
		{
			ScrollRetries += 1;
			if(ScrollRetries < MAX_SCROLL_RETRIES)
			{
				CallDeferred("_do_scroll_to_bottom");
			}
			return ;
		}

		var max_val = scroll_bar.MaxValue;
		if(max_val <= 0)
		{
			ScrollRetries += 1;
			if(ScrollRetries < MAX_SCROLL_RETRIES)
			{
				CallDeferred("_do_scroll_to_bottom");
			}
			return ;
		}


		// Scroll final
		TextEdit.ScrollVertical = max_val;
		scroll_bar.Value = max_val;
	}


	protected void _ConsolePrint(string text)
	{

		// Verificar si estaba al final ANTES de a�adir el texto
		var was_at_bottom = _IsAtBottom();
		TextEdit.Text += text + "\n";

		// Solo hacer scroll si estaba al final antes de a�adir el texto
		if(was_at_bottom)
		{
			_ScrollToBottom();
		}
	}

	protected void _RunCommand(string cmd)
	{
		if (!IsMultiplayerAuthority()) return;

		// 1. Limpiamos y dividimos el comando
		string[] parts = cmd.StripEdges().Split(" ");
		if (parts.Length == 0) return;

		string command_name = parts[0];

		// 2. Creamos un Array de Godot con los argumentos (saltando el primero)
		var args = new Godot.Collections.Array();
		for (int i = 1; i < parts.Length; i++)
		{
			args.Add(parts[i]);
		}

		if (DevCommands.ContainsKey(command_name))
		{
			var cmd_info = DevCommands[command_name];

			// 3. Verificamos cantidad de argumentos usando .Count
			if (args.Count < cmd_info["args"].AsInt32())
			{
				_ConsolePrint($"Faltan argumentos. Uso: /{command_name} {cmd_info["desc"]}");
				return;
			}

			string method_name = cmd_info["method"].AsString();
			
			// 4. Ejecutamos el método
			if (HasMethod(method_name))
			{
				// Callv requiere un Godot.Collections.Array
				var result = Callv(method_name, args);
				if (result.VariantType != Variant.Type.Nil)
				{
					_ConsolePrint(result.ToString());
				}
			}
			else
			{
				_ConsolePrint("Error interno: método no encontrado.");
			}
			return;
		}

		_ConsolePrint($"Comando desconocido: {command_name}");
	}


	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	public void MsgRpc(string username, string data)
	{

		// Esta funcin se ejecuta en todos los clientes (call_local)

		// Asegurar que el scroll funcione incluso si este chat no tiene autoridad
		if(data.StartsWith("/"))
		{

			// Buscar el jugador que envi el comando
			Player jugador_encontrado = null;
			foreach(Player player in GetTree().GetNodesInGroup("player"))
			{
				if(GodotObject.IsInstanceValid(player) && player is CharacterBody3D)
				{
					var player_username = player.Username;
					if(player_username == username)
					{
						jugador_encontrado = player;
						break;
					}
				}
			}


			// Si no se encuentra el jugador, bloquear el comando
			if(jugador_encontrado == null)
			{
				_ConsolePrint("Error: Jugador no encontrado");
				return ;
			}


			// Verificar si el jugador es admin
			if(!jugador_encontrado.AdminMode)
			{
				_ConsolePrint("No tienes permisos para ejecutar comandos");
				return ;
			}


			// Validar que el comando no est� vac�o
			var comando_limpio = data.StripEdges();
			if(comando_limpio.Length <= 1)
			{
				// Solo tiene "/" o est� vac�o
				return ;
			}


			// Verificar si estaba al final ANTES de a�adir el texto
			var was_at_bottom = _IsAtBottom();

			// Mostrar el comando en el chat
			TextEdit.Text += username + ": " + data + "\n";

			// Solo hacer scroll si estaba al final antes de aadir el texto
			if(was_at_bottom)
			{
				_ScrollToBottom();
			}


			// Ejecutar el comando solo si este chat tiene autoridad
			if(IsMultiplayerAuthority())
			{

				// Ejecutar el comando (quitar el "/" del inicio)
				data = data.Remove(0, 1);
				Globals.Instance.PrintRole(data);
				_RunCommand(data);
			}
		}
		else
		{

			// Mensaje normal (no comando)
			var mensaje_limpio = data.StripEdges();
			if(mensaje_limpio.Length > 0)
			{

				// Verificar si estaba al final ANTES de a�adir el texto
				var was_at_bottom = _IsAtBottom();
				TextEdit.Text += username + ": " + data + "\n";

				// Solo hacer scroll si estaba al final antes de aadir el texto
				if(was_at_bottom)
				{
					_ScrollToBottom();
				}
			}
		}
	}


	protected void _OnButtonPressed()
	{
		if(!IsMultiplayerAuthority())
		{
			return ;
		}

		Rpc(nameof(MsgRpc), Globals.Instance.Username, LineEdit.Text);

		LineEdit.Text = "";
		LineEdit.ReleaseFocus();
		Button.ReleaseFocus();

		// Asegurar que is_chat_open se establece en false cuando se cierra el chat
		Globals.Instance.IsChatOpen = false;
	}


	protected void _OnLineEditFocusEntered()
	{
		Globals.Instance.IsChatOpen = true;
	}

	protected void _OnLineEditFocusExited()
	{
		Globals.Instance.IsChatOpen = false;
	}


}