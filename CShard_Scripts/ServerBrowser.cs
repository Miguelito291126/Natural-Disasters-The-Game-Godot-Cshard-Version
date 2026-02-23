using Godot;
using Godot.Collections;

[GlobalClass]
public partial class ServerBrowser : Panel
{
	public Node List;
	public PackedScene Serverinfo = ResourceLoader.Load<PackedScene>("res://Scenes/server_info.tscn");

	public const float TIMEOUT = 3.0f;

	public override void _Ready()
	{
		List = GetNode("List");
		Globals.Instance.ServerBrowser = this;
	}

	public override void _Process(double _delta)
	{

		// 1) Eliminar servidores que llevan demasiado sin actualizar
		var now = (int)Time.GetUnixTimeFromSystem();
		Reload(now);

		if (Globals.Instance.Lisener.GetAvailablePacketCount() > 0)
		{
			string server_ip = Globals.Instance.Lisener.GetPacketIP();
			int server_port = Globals.Instance.Lisener.GetPacketPort();
			byte[] bytes = Globals.Instance.Lisener.GetPacket();
			string data = System.Text.Encoding.ASCII.GetString(bytes);
			
			// 1. Convertimos el JSON a Diccionario
			var room_list = Json.ParseString(data).AsGodotDictionary();
			
			// 2. Extraemos el nombre y jugadores (esto crea las variables que te faltaban)
			string rName = room_list["Name"].AsString();
			string rPlayers = room_list["Players"].ToString();

			foreach (Node i in List.GetChildren())
			{
				// 3. Casteamos 'i' a tu clase específica (ej: ServerInfo)
				// Si no tienes una clase, usa: if (i is Node iNode) y luego iNode.Set(...)
				if (i is ServerInfo item && item.Name == rName)
				{
					item.GetNode<Label>("Name").Text = rName + " - ";
					item.GetNode<Label>("Players").Text = rPlayers + " - ";
					item.ServerIp = server_ip;
					item.ServerPort = server_port.ToString();
					item.LastSeen = now;
					return;
				}
			}

			// 4. Instanciar nuevo servidor
			var currentinfo = Serverinfo.Instantiate<ServerInfo>(); // Instanciar directamente como el tipo
			currentinfo.Name = rName;
			currentinfo.GetNode<Label>("Name").Text = rName + " - ";
			currentinfo.GetNode<Label>("Players").Text = rPlayers + " - ";
			currentinfo.ServerIp = server_ip;
			currentinfo.ServerPort = server_port.ToString();
			currentinfo.LastSeen = now;
			List.AddChild(currentinfo, true);
		}

	}

	public void Reload(float now)
	{
		foreach(Node i in List.GetChildren())
		{
			if(i is ServerInfo item)
			{
				if(now - item.LastSeen > TIMEOUT)
				{
					Globals.Instance.PrintRole("Removing inactive server:" + i.Name);
					i.QueueFree();
				}
			}
		}
	}


}