using Godot;
using Godot.Collections;

[GlobalClass]
public partial class PlayersListMenu : CanvasLayer
{
	public VBoxContainer List;
	public PackedScene PlayerInfo = ResourceLoader.Load<PackedScene>("res://Scenes/player_info.tscn");

	public override void _Ready()
	{
		List = GetNode<VBoxContainer>("Panel/List");
	}
	//PANIC! <self . visible = false> unexpected at Token(type='TEXT', value='self', lineno=7, index=132, end=136)


	// --- RPC: recibir lista desde el servidor ---
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	public void SyncPlayers(Array<Player> players_array)
	{
		UpdateList(players_array);
	}
	//PANIC! <update_list ( players_array )> unexpected at Token(type='TEXT', value='update_list', lineno=12, index=277, end=288)
	
	public void UpdateList(Array<Player> players_array)
	{
		// Limpiar lista
		foreach(Node child in List.GetChildren())
		{
			if(child.Name == "Info")
			{
				continue;
			}
			child.QueueFree();
		}

		// Rellenar UI
		foreach(Player p in players_array)
		{
			var inst = PlayerInfo.Instantiate();
			inst.GetNode<Label>("Username").Text = p.Username;
			inst.GetNode<Label>("Points").Text =  p.Points.ToString();
			List.AddChild(inst);
		}
	}
	public override void _Process(double _delta)
	{
		// Solo el servidor sincroniza
		if (!Multiplayer.IsServer())
		{
			return;
		}

		// Construir arreglo de datos
		Array data = new();
		foreach(Player player_data in Globals.Instance.PlayersConected)
		{
			if(IsInstanceValid(player_data))
			{
				data.Add(new Dictionary {
					{"username", player_data},
					{"points", player_data}
				});
			}
		}

		// Enviar a todos
		Rpc(nameof(SyncPlayers), data);
	}

	public override void _Input(InputEvent _event)
	{
		// Check if chat is open - replace with your actual Globals implementation
		if(Globals.Instance.IsChatOpen)
		{
			return;
		}

		if(Input.IsActionJustPressed("List of players"))
		{
			this.Visible = !this.Visible;
		}
	}
}