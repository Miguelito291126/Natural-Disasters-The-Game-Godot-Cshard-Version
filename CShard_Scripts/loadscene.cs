using Godot;
using Godot.Collections;

[GlobalClass]
public partial class loadscene : Node
{
	[Signal]
	public delegate void ProgressChangedEventHandler(Variant progress);
	[Signal]
	public delegate void LoadDoneEventHandler();

	public Dictionary GAME_SCENE = new() {
			{"map", "res://Scenes/map.tscn"},
			};

	public string LoadingScreenPath = "res://Scenes/loading_screen.tscn";
	public Resource LoadingScreen = Load(LoadingScreenPath);
	public PackedScene LoaderResource;
	public string ScenePath;
	public Godot.Collections.Array Progress = new();

	public bool UseSubTheads = false;


	public void LoadScene(Variant current_scene, Variant next_scene)
	{

		if(next_scene != null)
		{
			ScenePath = next_scene;
		}

		var loading_screen_intance = LoadingScreen.Instantiate();
		Globals.Main.AddChild(loading_screen_intance);

		this.ProgressChanged += loading_screen_intance.UpdateProgressBar;
		this.LoadDone += loading_screen_intance.FadeOutLoadingScreen;

		await ToSignal(loading_screen_intance, "safe_to_load");

		if(current_scene != null && GodotObject.IsInstanceValid(current_scene))
		{
			current_scene.QueueFree();
		}
		else
		{
			Globals.PrintRole("No current scene to free");
		}


		if(GAME_SCENE.ContainsKey(ScenePath))
		{
			ScenePath = GAME_SCENE[ScenePath];
		}
		else
		{
			ScenePath = ScenePath;
		}

		var loader_next_scene = ResourceLoader.LoadThreadedRequest(ScenePath, "", UseSubTheads);
		if(loader_next_scene == OK)
		{
			Globals.PrintRole("loading...");
			SetProcess(true);
		}
	}


	public override void _Process(double _delta)
	{
		var load_status = ResourceLoader.LoadThreadedGetStatus(ScenePath, Progress);
		switch(load_status)
		{
			case 0:
			{
				Globals.PrintRole("failed to load: invalid resource");
				SetProcess(false);
				return ;
				break; }
			case 2:
			{
				Globals.PrintRole("failed to load");
				SetProcess(false);
				return ;
				break; }
			case 1:
			{
				EmitSignal("progress_changed", Progress[0]);
				break; }
			case 3:
			{
				Globals.PrintRole("Completed");

				if(ScenePath == "res://Scenes/main.tscn")
				{

				}
				else
				{
					var new_scene = ResourceLoader.LoadThreadedGet(ScenePath).Instantiate();
					if(GodotObject.IsInstanceValid(new_scene))
					{
						Globals.Main.AddChild(new_scene);
					}
				}

				EmitSignal("progress_changed", 1.0);
				EmitSignal("load_done");
				SetProcess(false);
				break; }
		}
	}


}