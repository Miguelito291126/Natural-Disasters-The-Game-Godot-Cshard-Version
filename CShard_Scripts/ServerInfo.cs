using Godot;
using Godot.Collections;

[GlobalClass]
public partial class ServerInfo : HBoxContainer
{
    public string ServerIp = "";
    public string ServerPort = "";
	public string ServerLocalIp = ""; // Se llena desde el JSON del Master Server

    // Lo hacemos público para que ServerBrowser lo vea
    public void JoinServer() 
    {
		string cleanPort = ServerPort.Replace(".0", "").Trim();
		string targetIp = ServerIp;

		// 1. Obtenemos nuestra IP externa (puedes guardarla en Globals al hacer el UPNP)
		// Si la IP de la lista es la mía, uso localhost para evitar el bloqueo del router
		if (targetIp == Globals.Instance.PublicIp) 
		{
			GD.Print("Detectada IP propia, usando " + ServerLocalIp + " para evitar bloqueo NAT Loopback");
			targetIp =  ServerLocalIp;
		}
		
		GD.Print($"Conectando a: {targetIp}:{cleanPort}");
		
		Globals.Instance.Ip = targetIp;
		Globals.Instance.Port = cleanPort.ToInt();
		Globals.Instance.PlayMultiplayerClient();
    }
}