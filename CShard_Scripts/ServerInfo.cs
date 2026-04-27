using Godot;
using Godot.Collections;

[GlobalClass]
public partial class ServerInfo : HBoxContainer
{
    public string ServerIp = "";
    public string ServerPort = "";

    // Lo hacemos público para que ServerBrowser lo vea
    public void JoinServer() 
    {
		string cleanPort = ServerPort.Replace(".0", "").Trim();
		string targetIp = ServerIp;

		// 1. Obtenemos nuestra IP externa (puedes guardarla en Globals al hacer el UPNP)
		// Si la IP de la lista es la mía, uso localhost para evitar el bloqueo del router
		if (targetIp == Globals.Instance.MyPublicIp) 
		{
			GD.Print("Detectada IP propia, usando 127.0.0.1 para evitar bloqueo NAT Loopback");
			targetIp = "127.0.0.1";
		}
		
		GD.Print($"Conectando a: {targetIp}:{cleanPort}");
		
		Globals.Instance.Ip = targetIp;
		Globals.Instance.Port = cleanPort.ToInt();
		Globals.Instance.PlayMultiplayerClient();
    }
}