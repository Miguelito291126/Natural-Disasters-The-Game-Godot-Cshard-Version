using Godot;
using Godot.Collections;

[GlobalClass]
public partial class LoadScene : Node
{

	internal static LoadScene Instance;

	public LoadScene()
	{
		Instance = this;
	}

	[Signal]
	public delegate void ProgressChangedEventHandler(float progress);

	[Signal]
	public delegate void LoadDoneEventHandler();

	public Dictionary<string, string> GAME_SCENE = new() {
			{"map", "res://Scenes/map.tscn"},
			};

	public static string LoadingScreenPath = "res://Scenes/loading_screen.tscn";
	public PackedScene LoadingScreen = ResourceLoader.Load<PackedScene>(LoadingScreenPath);
	public PackedScene LoaderResource;
	public string ScenePath;
	public Array Progress = new Array();

	public bool UseSubTheads = false;


	public async void loadscene(Node current_scene, string next_scene)
	{

		if(next_scene != null)
		{
			ScenePath = next_scene;
		}

		LoadingScreen loading_screen_intance = LoadingScreen.Instantiate<LoadingScreen>();
		Globals.Instance.Main.AddChild(loading_screen_intance);

		ProgressChanged += loading_screen_intance.UpdateProgressBar;
		LoadDone += loading_screen_intance.FadeOutLoadingScreen;

		await ToSignal(loading_screen_intance, "safe_to_load");

		if(current_scene != null && IsInstanceValid(current_scene))
		{
			current_scene.QueueFree();
		}
		else
		{
			Globals.Instance.PrintRole("No current scene to free");
		}


		if(GAME_SCENE.ContainsKey(ScenePath))
		{
			ScenePath = GAME_SCENE[ScenePath];
		}

		var loader_next_scene = ResourceLoader.LoadThreadedRequest(ScenePath, "", UseSubTheads);
		if(loader_next_scene == Error.Ok)
		{
			Globals.Instance.PrintRole("loading...");
			SetProcess(true);
		}
	}


	public override void _Process(double _delta)
	{
		ResourceLoader.ThreadLoadStatus load_status = ResourceLoader.LoadThreadedGetStatus(ScenePath, Progress);
		switch(load_status)
		{
			case ResourceLoader.ThreadLoadStatus.InvalidResource:
				Globals.Instance.PrintRole("failed to load: invalid resource");
				SetProcess(false);
				return ;

			case ResourceLoader.ThreadLoadStatus.Failed:
			
				Globals.Instance.PrintRole("failed to load");
				SetProcess(false);
				return ;

			case ResourceLoader.ThreadLoadStatus.InProgress:
			{
				EmitSignal("progress_changed", Progress[0]);
				break; }
			case ResourceLoader.ThreadLoadStatus.Loaded:
			{
				Globals.Instance.PrintRole("Completed");

				if(ScenePath == "res://Scenes/main.tscn")
				{

				}
				else
				{
					Node new_scene = ((PackedScene)ResourceLoader.LoadThreadedGet(ScenePath)).Instantiate();
					if(IsInstanceValid(new_scene))
					{
						Globals.Instance.Main.AddChild(new_scene);
					}
				}

				EmitSignal("progress_changed", 1.0);
				EmitSignal("load_done");
				SetProcess(false);
				break; }
		}
	}


}