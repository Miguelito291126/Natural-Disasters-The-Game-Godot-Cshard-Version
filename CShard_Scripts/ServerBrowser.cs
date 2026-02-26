using Godot;
using Godot.Collections;

[GlobalClass]
public partial class ServerBrowser : Panel
{
	public VBoxContainer List;
	public PackedScene Serverinfo = ResourceLoader.Load<PackedScene>("res://Scenes/server_info.tscn");
	public const float TIMEOUT = 3.0f;

	public override void _Ready()
	{
		List = GetNode<VBoxContainer>("List");
		Globals.Instance.ServerBrowser = this;

		// Crear un timer que limpie la lista cada 1 segundo en lugar de cada frame
		Timer cleanTimer = new Timer();
		cleanTimer.WaitTime = 1.0f;
		cleanTimer.Autostart = true;
		cleanTimer.Timeout += () => Reload(); // Quitamos el parámetro
		AddChild(cleanTimer);
	}

	public override void _Process(double _delta)
	{
		int currentTime = (int)Time.GetUnixTimeFromSystem();

		if (Globals.Instance.Lisener.GetAvailablePacketCount() > 0)
		{
			string server_ip = Globals.Instance.Lisener.GetPacketIP();
			int server_port = Globals.Instance.Lisener.GetPacketPort();
			byte[] bytes = Globals.Instance.Lisener.GetPacket();
			string data = System.Text.Encoding.ASCII.GetString(bytes);
			
			// 1. Convertimos el JSON a Diccionario
			var jsonResult = Json.ParseString(data);
			if (jsonResult.VariantType != Variant.Type.Dictionary)
			{
				GD.PrintErr("Error: El paquete recibido no es un JSON válido o no es un objeto.");
				return;
			}
			var room_list = jsonResult.AsGodotDictionary();
			
			// 2. Extraemos el nombre y jugadores (esto crea las variables que te faltaban)
			string rName = room_list.ContainsKey("Name") ? room_list["Name"].AsString() : "Unknown Server";
			string rPlayers = room_list.ContainsKey("Players") ? room_list["Players"].ToString() : "0";

			if (string.IsNullOrEmpty(rName) || rName == "Unknown Server") return;

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
					item.LastSeen = currentTime;
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
			currentinfo.LastSeen = currentTime;
			List.AddChild(currentinfo, true);
		}

	}

	public void Reload()
	{

		int currentTime = (int)Time.GetUnixTimeFromSystem();

		foreach(Node i in List.GetChildren())
		{
			if(i is ServerInfo item)
			{
				if(currentTime - item.LastSeen > TIMEOUT)
				{
					Globals.Instance.PrintRole("Removing inactive server:" + i.Name);
					i.QueueFree();
				}
			}
		}
	}


}