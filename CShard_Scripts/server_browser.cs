using Godot;
using Godot.Collections;

[GlobalClass]
public partial class ServerBrowser : Panel
{
	public Node List;
	public Resource Serverinfo = /* preload has no equivalent, add a 'ResourcePreloader' Node in your scene */("res://Scenes/server_info.tscn");

	public const double TIMEOUT = 3.0;

	public override void _Ready()
	{
		List = GetNode("List");
		Globals.ServerBrowser = this;
	}

	public override void _Process(double _delta)
	{

		// 1) Eliminar servidores que llevan demasiado sin actualizar
		var now = Time.GetUnixTimeFromSystem();
		Reload(now);

		if(Globals.Lisener.GetAvailablePacketCount() > 0)
		{
			var server_ip = Globals.Lisener.GetPacketIp();
			var server_port = Globals.Lisener.GetPacketPort();
			var bytes = Globals.Lisener.GetPacket();
			var data = bytes.GetStringFromAscii();
			var room_list = JSON.ParseString(data);

			foreach(Node i in List.GetChildren())
			{
				if(i.Name == room_list.Name)
				{
					i.GetNode("Name").Text = room_list.Name + " - ";
					i.GetNode("Players").Text = Str(room_list.Players) + " - ";
					i.ServerIp = server_ip;
					i.ServerPort = Str(server_port);
					i.LastSeen = now;
					return ;
				}
			}

			var currentinfo = Serverinfo.Instantiate();
			currentinfo.Name = room_list.Name;
			currentinfo.GetNode("Name").Text = room_list.Name + " - ";
			currentinfo.GetNode("Players").Text = Str(room_list.Players) + " - ";
			currentinfo.ServerIp = server_ip;
			currentinfo.ServerPort = Str(server_port);
			currentinfo.LastSeen = now;
			List.AddChild(currentinfo, true);
		}
	}

	public void Reload(Variant now)
	{
		foreach(Node i in List.GetChildren())
		{
			if(i is HBoxContainer)
			{
				if(now - i.LastSeen > TIMEOUT)
				{
					Globals.PrintRole("Removing inactive server:" + i.Name);
					i.QueueFree();
				}
			}
		}
	}


}