using Godot;
using Godot.Collections;

[GlobalClass]
public partial class unloadscene : Node
{
	[Signal]
	public delegate void ProgressChangedEventHandler(Variant progress);
	[Signal]
	public delegate void UnloadDoneEventHandler();

	public string UnloadingScreenPath = "res://Scenes/loading_screen.tscn";
	public Resource UnloadingScreen = Load(UnloadingScreenPath);
	public PackedScene UnloaderResource;
	public Variant Scene;
	public string ScenePath;
	public Godot.Collections.Array Progress = new Godot.Collections.Array{};

	public bool UseSubTheads = false;

	public async void UnloadScene(Variant current_scene)
	{

		if(current_scene != null && current_scene is Node node)
		{
			ScenePath = node.SceneFilePath;
			Scene = current_scene;
		}
	}

	public async void UnloadScene()
	{
		UnloadScene(null);

		var unloading_screen_scene = UnloadingScreen.Instantiate();
		Globals.Main.AddChild(unloading_screen_scene);

		this.ProgressChanged += unloading_screen_scene.UpdateProgressBar;
		this.UnloadDone += unloading_screen_scene.FadeOutLoadingScreen;

		await ToSignal(this, "unload_done");

		if(current_scene != null)
		{
			if(GodotObject.IsInstanceValid(current_scene))
			{
				current_scene.QueueFree();
			}
		}

		var loader_next_scene = ResourceLoader.LoadThreadedRequest(ScenePath, "", UseSubTheads);
		if(loader_next_scene == OK)
		{
			Globals.PrintRole("unloading...");
			SetProcess(true);
		}
	}

	public void ClearNodegameExceptSpawner()
	{
		if(!GodotObject.IsInstanceValid(Globals.Main))
		{
			return ;
		}

		foreach(Node child in Globals.Main.GetChildren())
		{
			if(child.Name != "MapSpawner")
			{
				// aqu� pon el nombre exacto de tu spawner en la escena
				child.QueueFree();
			}
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
				EmitSignal("progress_changed", 1.0);
				EmitSignal("unload_done");
				SetProcess(false);
				break; }
		}
	}


}