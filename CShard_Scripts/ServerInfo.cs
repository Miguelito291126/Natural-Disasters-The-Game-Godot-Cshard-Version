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
        // Limpiamos cualquier espacio o caracter raro
        string cleanPort = ServerPort.Replace(".0", "").Trim();
        
        GD.Print($"Conectando a: {ServerIp}:{cleanPort}");
        
        Globals.Instance.Ip = ServerIp;
        Globals.Instance.Port = cleanPort.ToInt();
        Globals.Instance.PlayMultiplayerClient();
    }
}