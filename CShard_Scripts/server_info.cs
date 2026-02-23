using Godot;
using Godot.Collections;

[GlobalClass]
public partial class ServerInfo : HBoxContainer
{
	public string ServerIp = "";
	public string ServerPort = "";
	public float LastSeen;

	protected void _OnButtonPressed()
	{
		Globals.Instance.Ip = ServerIp;
		Globals.Instance.Port = ServerPort.ToInt() + 1;
		Globals.Instance.PlayMultiplayerClient();
	}


}