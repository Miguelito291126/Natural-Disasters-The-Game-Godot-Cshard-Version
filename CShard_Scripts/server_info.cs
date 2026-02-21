using Godot;
using Godot.Collections;

[GlobalClass]
public partial class ServerInfo : HBoxContainer
{
	public string ServerIp = "";
	public string ServerPort = "";
	public int LastSeen;

	protected void _OnButtonPressed()
	{
		Globals.Ip = ServerIp;
		Globals.Port = ServerPort.ToInt() + 1;
		Globals.PlayMultiplayerClient();
	}


}