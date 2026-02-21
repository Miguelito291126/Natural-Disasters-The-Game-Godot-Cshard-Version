using Godot;
using Godot.Collections;

[GlobalClass]
public partial class Chat : CanvasLayer
{
	public TextEdit TextEdit;

	public LineEdit LineEdit;
	public Button Button;


	public Array<string> AutocompleteMatches = new();
	public int AutocompleteIndex = 0;
	public Array AutocompleteMethods = new();
	public Array<string> History = new();
	public int HistoryIndex =  - 1;
	public bool UserIsScrolling = false;
	public int ScrollRetries = 0;
	public const int MAX_SCROLL_RETRIES = 5;


	public Dictionary DevCommands = new Dictionary{
			{"god_mode", new Dictionary{
						{"desc", "Muestra todos los comandos."},
						{"method", "_cmd_god_mode_player"},
						{"args", 0},
						}},
			{"ungod_mode", new Dictionary{
						{"desc", "Muestra todos los comandos."},
						{"method", "_cmd_ungod_mode_player"},
						{"args", 0},
						}},
			{"kill_player", new Dictionary{
						{"desc", "Cambia la velocidad del jugador. Uso: /set_speed 10"},
						{"method", "_cmd_kill_player"},
						{"args", 1},
						}},
			{"teleport_player", new Dictionary{
						{"desc", "Teletransporta al jugador a otro jugador. Uso: /teleport_player PlayerName"},
						{"method", "_cmd_teleport_player"},
						{"args", 1},
						}},
			{"teleport_position", new Dictionary{
						{"desc", "Teletransporta al jugador a una posici�n. Uso: /teleport_position Vector3(x,y,z)"},
						{"method", "_cmd_teleport_position"},
						{"args", 1},
						}},
			{"kick_player", new Dictionary{
						{"desc", "Expulsa a un jugador del servidor. Uso: /kick_player PlayerName"},
						{"method", "_cmd_kick_player"},
						{"args", 1},
						}},
			{"damage_player", new Dictionary{
						{"desc", "Inflige da�o a un jugador. Uso: /damage_player PlayerName damage_amount"},
						{"method", "_cmd_damage_player"},
						{"args", 2},
						}},
			{"spawn_disaster", new Dictionary{
						{"desc", "Genera un desastre o clima. Uso: /spawn_disaster disaster_name"},
						{"method", "_cmd_spawn_disaster_weather"},
						{"args", 1},
						}},
			{"admin", new Dictionary{
						{"desc", "Genera un desastre o clima. Uso: /spawn_disaster disaster_name"},
						{"method", "_cmd_admin_mode_player"},
						{"args", 1},
						}},

			{"unadmin", new Dictionary{
						{"desc", "Genera un desastre o clima. Uso: /spawn_disaster disaster_name"},
						{"method", "_cmd_unadmin_mode_player"},
						{"args", 1},
						}},

			};

	protected Player _GetLocalPlayer()
	{
		foreach(Node p in GetTree().GetNodesInGroup("player"))
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


	protected string _CmdAdminModePlayer(Variant player_name)
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
		var jugador_encontrado = null;
		foreach(Node p in GetTree().GetNodesInGroup("player"))
		{
			if(GodotObject.IsInstanceValid(p) && p.Username == player_name)
			{
				jugador_encontrado = p;
				break;
			}
		}

		if(jugador_encontrado == null)
		{
			return "Jugador no encontrado: %s" % player_name;


			// Usar RPC para sincronizar el cambio en todos los clientes

		}// call_local ya ejecuta la funci�n localmente en el servidor
		jugador_encontrado._SetAdminMode.Rpc(true);
		return $"Ahora {player_name} es admin";
	}

	protected string _CmdUnadminModePlayer(Variant player_name)
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
		var jugador_encontrado = null;
		foreach(Node p in GetTree().GetNodesInGroup("player"))
		{
			if(GodotObject.IsInstanceValid(p) && p.Username == player_name)
			{
				jugador_encontrado = p;
				break;
			}
		}

		if(jugador_encontrado == null)
		{
			return "Jugador no encontrado: %s" % player_name;


			// Usar RPC para sincronizar el cambio en todos los clientes

		}// call_local ya ejecuta la funci�n localmente en el servidor
		jugador_encontrado._SetAdminMode.Rpc(false);
		return $"Ahora {player_name} ya no es admin";
	}


	protected string _CmdKillPlayer(Variant player_name)
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


	protected string _CmdKickPlayer(Variant player_name)
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
				Multiplayer.MultiplayerPeer.DisconnectPeer(player.Id, true);
				return $"{player_name} expulsado";
			}
		}
		return "Jugador no encontrado";
	}


	protected string _CmdDamagePlayer(Variant player_name, Variant damage)
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
				player.Damage(Int(damage));
				return $"{player_name} recibió {damage} de daño";
			}
		}
		return "Jugador no encontrado";
	}


	protected string _CmdTeleportPlayer(Variant player_name, Variant target_name)
	{
		var local = _GetLocalPlayer() as Player;
		if(local == null || !local.AdminMode)
		{
			return "No tienes permisos";
		}

		var player = null as Player;
		var target = null as Player;

		foreach(Node p in GetTree().GetNodesInGroup("player"))
		{
				if(p is Player playerNode)
				{
					if(playerNode.Username == player_name)
					{
						player = playerNode;
					}
					if(playerNode.Username == target_name)
					{
						target = playerNode;
					}
				}
		}

		if(player == null || target == null)
		{
			return "Jugador no encontrado";
		}

		player.GlobalPosition = target.GlobalPosition;
		return $"Teletransportado {player_name} a {target_name}";
	}

	protected string _CmdSpawnDisasterWeather(Variant disaster_name)
	{
		var local = _GetLocalPlayer();
		if(local == null || !local.AdminMode)
		{
			return "No tienes permisos";
		}

		Globals.SetWeatherAndDisaster(disaster_name);
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

		AutocompleteMethods = DevCommands.Keys.ToArray();
	}

	public override void _Input(InputEvent _event)
	{

		// Solo procesar input si este chat tiene autoridad
		if(!IsMultiplayerAuthority())
		{
			return ;
		}

		if(LineEdit.HasFocus())
		{

			// Autocompletado con Tab
			if(Input.IsActionJustPressed("dev_console_autocomplete"))
			{
				var current = LineEdit.Text.Remove(0, 1);

				if(AutocompleteMatches.Count == 0)
				{
					foreach(Variant cmd in AutocompleteMethods)
					{
						if(cmd.BeginsWith(current))
						{
							AutocompleteMatches.Add(cmd);
						}
					}
				}

				if(AutocompleteMatches.Count > 0)
				{
					LineEdit.Text = "/" + AutocompleteMatches[AutocompleteIndex];
					LineEdit.CaretColumn = LineEdit.Text.Length();
					AutocompleteIndex = (AutocompleteIndex + 1) % AutocompleteMatches.Count;
				}
			}


			// Reset autocompletado si se escribe algo distinto
			if(Input.IsActionJustPressed("ui_text_indent"))
			{
				AutocompleteMatches.Clear();
				AutocompleteIndex = 0;
			}


			// Recorrer historial con flechas
			if(Input.IsActionJustPressed("dev_console_up"))
			{
				if(History.Size() > 0)
				{
					HistoryIndex = Mathf.Clamp(HistoryIndex + 1, 0, History.Count - 1);
					LineEdit.Text = "/" + History[HistoryIndex];
					LineEdit.CaretColumn = LineEdit.Text.Length();
				}
			}

			else if(Input.IsActionJustPressed("dev_console_down"))
			{
				if(History.Size() > 0)
				{
					HistoryIndex = Mathf.Clamp(HistoryIndex - 1, 0, History.Count - 1);
					LineEdit.Text = "/" + History[HistoryIndex];
					LineEdit.CaretColumn = LineEdit.Text.Length();
				}
			}


			// Ejecutar comando con Enter
			if(Input.IsActionJustPressed("Enter"))
			{
				History.PushFront(LineEdit.Text.Remove(0, 1));

				msg_rpc.Rpc(Globals.Username, LineEdit.Text);

				HistoryIndex =  - 1;
				LineEdit.Text = "";
				LineEdit.ReleaseFocus();
				Button.ReleaseFocus();

				// Asegurar que is_chat_open se establece en false cuando se cierra el chat
				Globals.IsChatOpen = false;
			}
		}


		// Seleccionar el LineEdit al presionar T
		if(Input.IsActionJustPressed("Chat"))
		{
			LineEdit.GrabFocus();

			// Asegurar que is_chat_open se establece cuando se abre el chat
			Globals.IsChatOpen = true;
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
		if(!IsMultiplayerAuthority())
		{
			return ;
		}

		var parts = cmd.StripEdges().Split(" ");
		var command_name = parts[0];
		var args = parts.Slice(1, parts.Size());

		if(DevCommands.ContainsKey(command_name))
		{
			var cmd_info = DevCommands[command_name];

			if(args.Size() < cmd_info["args"])
			{
				_ConsolePrint($"Faltan argumentos. Uso: /{command_name}");
				return ;
			}

			var method_name = cmd_info["method"];
			if(HasMethod(method_name))
			{
				var result = Callv(method_name, args);
				if(result != null)
				{
					_ConsolePrint(Str(result));
				}
				return ;
			}
			else
			{
				_ConsolePrint("Error interno: m�todo no encontrado.");
				return ;
			}
		}


		_ConsolePrint($"Comando desconocido: {command_name}");
	}


	public void MsgRpc(Variant username, Variant data)
	{

		// Esta funci�n se ejecuta en todos los clientes (call_local)

		// Asegurar que el scroll funcione incluso si este chat no tiene autoridad
		if(data.BeginsWith("/"))
		{

			// Buscar el jugador que envi� el comando
			var jugador_encontrado = null;
			foreach(Node player in GetTree().GetNodesInGroup("player"))
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
			TextEdit.Text += Str(username, ": ", data, "\n");

			// Solo hacer scroll si estaba al final antes de a�adir el texto
			if(was_at_bottom)
			{
				_ScrollToBottom();
			}


			// Ejecutar el comando solo si este chat tiene autoridad
			if(IsMultiplayerAuthority())
			{

				// Ejecutar el comando (quitar el "/" del inicio)
				data = data.Remove(0, 1);
				Globals.PrintRole(data);
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
				TextEdit.Text += Str(username, ": ", data, "\n");

				// Solo hacer scroll si estaba al final antes de a�adir el texto
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

		msg_rpc.Rpc(Globals.Username, LineEdit.Text);

		LineEdit.Text = "";
		LineEdit.ReleaseFocus();
		Button.ReleaseFocus();

		// Asegurar que is_chat_open se establece en false cuando se cierra el chat
		Globals.IsChatOpen = false;
	}


	protected void _OnLineEditFocusEntered()
	{
		Globals.IsChatOpen = true;
	}

	protected void _OnLineEditFocusExited()
	{
		Globals.IsChatOpen = false;
	}


}