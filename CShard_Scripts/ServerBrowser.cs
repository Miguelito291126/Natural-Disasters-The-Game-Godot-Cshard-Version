using Godot;
using Godot.Collections;

[GlobalClass]
public partial class ServerBrowser : Panel
{
	public VBoxContainer List;
	public HttpRequest http;
	public PackedScene Serverinfo = ResourceLoader.Load<PackedScene>("res://Scenes/server_info.tscn");
	public const float TIMEOUT = 3.0f;

	public override void _Ready()
	{
		List = GetNode<VBoxContainer>("List");
		Globals.Instance.ServerBrowser = this;

		http = GetNode<HttpRequest>("MasterServerRequest");
		http.RequestCompleted += OnRequestCompleted;

		Timer cleanTimer = new Timer();
		// CAMBIO: 5 segundos para que de tiempo a hacer clic sin que la lista desaparezca
		cleanTimer.WaitTime = 5.0f; 
		cleanTimer.Autostart = true;
		cleanTimer.Timeout += () => RefreshServerList(); 
		AddChild(cleanTimer);
	}

	public void RefreshServerList()
	{
		http.Request("http://79.112.95.69:5000/list");
	}

	// Conecta la señal request_completed del HTTPRequest a este método
	private void OnRequestCompleted(long result, long responseCode, string[] headers, byte[] body)
	{
		if (responseCode != 200) return;

		var jsonString = System.Text.Encoding.UTF8.GetString(body);
		var json = Json.ParseString(jsonString);
		if (json.VariantType != Variant.Type.Array) return;

		var serverArray = json.AsGodotArray();

		foreach (Node n in List.GetChildren()) 
		{
			if (n is ServerInfo)
			{
				n.Free();
			}
		}

		foreach (Dictionary serverData in serverArray)
		{
			var currentinfo = Serverinfo.Instantiate<ServerInfo>();
			
			// CORRECCIÓN DE DATOS: Aseguramos que el puerto sea string limpio
			currentinfo.ServerIp = serverData["ip"].ToString();
			// Usamos Mathf.Floor para quitar el ".0" si es que viene de Python como float
			int portInt = (int)GD.StrToVar(serverData["port"].ToString()); 
			currentinfo.ServerPort = portInt.ToString();

			float playersFloat = (float)GD.StrToVar(serverData["players"].ToString());
			int playersInt = (int)Mathf.Floor(playersFloat); // Convertimos 1.0 a 1

			currentinfo.GetNode<Label>("Name").Text = serverData["name"].ToString() + " - ";;
			currentinfo.GetNode<Label>("Players").Text = playersInt.ToString() + " - ";;

			// CONEXIÓN MANUAL: Esto asegura que el click funcione
			var btn = currentinfo.GetNode<Button>("Button"); // Asegúrate que el nombre sea "Button" en tu escena
			btn.Pressed += currentinfo.JoinServer;

			List.AddChild(currentinfo);
		}
	}

}