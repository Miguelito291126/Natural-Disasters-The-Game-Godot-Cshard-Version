using Godot;
using Godot.Collections;

[GlobalClass]
public partial class PlayersListMenu : CanvasLayer
{
	public Variant List;
	public Resource PlayerInfo = /* preload has no equivalent, add a 'ResourcePreloader' Node in your scene */("res://Scenes/player_info.tscn");

	public override void _Ready()
	{
		List = GetNode<VBoxContainer>("Panel/List");
	}
	//PANIC! <self . visible = false> unexpected at Token(type='TEXT', value='self', lineno=7, index=132, end=136)


	// --- RPC: recibir lista desde el servidor ---
	public void SyncPlayers(Array players_array)
	{
	}
	//PANIC! <update_list ( players_array )> unexpected at Token(type='TEXT', value='update_list', lineno=12, index=277, end=288)
	
	public void UpdateList(Array players_array)
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
		foreach(var p in players_array)
		{
			var inst = PlayerInfo.Instantiate();
			inst.GetNode("Username").Text = (string)p["username"];
			inst.GetNode("Points").Text = Str(p["points"]);
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
		foreach(var player_data in (Array)GD.GetGlobal("Globals.players_conected"))
		{
			if(GodotObject.IsInstanceValid(player_data))
			{
				data.Add(new Dictionary {
					{"username", player_data},
					{"points", player_data}
				});
			}
		}

		// Enviar a todos
		RpcUnreliable(nameof(SyncPlayers), data);
	}

	public override void _Input(InputEvent _event)
	{
		// Check if chat is open - replace with your actual Globals implementation
		if(Globals.IsChatOpen)
		{
			return;
		}

		if(Input.IsActionJustPressed("List of players"))
		{
			this.Visible = !this.Visible;
		}
	}
}