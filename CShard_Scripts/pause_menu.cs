using Godot;
using Godot.Collections;

[GlobalClass]
public partial class PauseMenu : CanvasLayer
{
	public bool MouseActionState = false;

	public Node Worldenvironment;
	public Node Light;
	public Node Light2;

	public Variant MainMenu;
	public Variant Settings;
	public Variant Fullscreen;
	public Variant Vsync;
	public Variant Fps;
	public Variant AntiAliasing;
	public Variant AntiTropic;
	public Variant Volumen;
	public Node VolumenMusic;
	public Time time;
	public Variant Quality;
	public Variant Resolutions;


	public Dictionary ResolutionsDic = new() {
			{"2400x1080 ", new Vector2i(2400, 1080)},
			{"1920x1080", new Vector2i(1920, 1080)},
			{"1600x900", new Vector2i(1600, 900)},
			{"1440x1080", new Vector2i(1440, 1080)},
			{"1440x900", new Vector2i(1440, 900)},
			{"1366x768", new Vector2i(1366, 768)},
			{"1360x768", new Vector2i(1360, 768)},
			{"1280x1024", new Vector2i(1280, 1024)},
			{"1280x962", new Vector2i(1280, 962)},
			{"1280x960", new Vector2i(1280, 960)},
			{"1280x800", new Vector2i(1280, 800)},
			{"1280x768", new Vector2i(1280, 768)},
			{"1280x720", new Vector2i(1280, 720)},
			{"1176x664", new Vector2i(1176, 664)},
			{"1152x648", new Vector2i(1152, 648)},
			{"1024x768", new Vector2i(1024, 768)},
			{"800x600", new Vector2i(800, 600)},
			{"720x480", new Vector2i(720, 480)},
			};

	public DataResource GlobalsData = DataResource.LoadFile();

	public void Addresolutions()
	{
		var current_resolution = Globals.GlobalsData.Resolution;
		var index = 0;

		foreach(Dictionary r in ResolutionsDic)
		{
			Resolutions.AddItem(r, index);
			index += 1;
		}
	}


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Worldenvironment = Globals.Map.GetNode("WorldEnvironment");
		Light = Worldenvironment.GetNode("Sun");
		Light2 = Worldenvironment.GetNode("Moon");
		MainMenu = GetNode<Control>("Panel/Menu");
		Settings = GetNode<Control>("Panel/Settings");
		Fullscreen = GetNode<CheckButton>("Panel/Settings/Fullscreen");
		Vsync = GetNode<CheckButton>("Panel/Settings/Vsync");
		Fps = GetNode<CheckButton>("Panel/Settings/Fps");
		AntiAliasing = GetNode<OptionButton>("Panel/Settings/antialiasing");
		AntiTropic = GetNode<OptionButton>("Panel/Settings/antitropic");
		Volumen = GetNode<HSlider>("Panel/Settings/Volumen");
		VolumenMusic = GetNode<HSlider>("Panel/Settings/Volumen Music");
		Time = GetNode<HSlider>("Panel/Settings/Time");
		Quality = GetNode<OptionButton>("Panel/Settings/Quality");
		Resolutions = GetNode<OptionButton>("Panel/Settings/Resolutions");
		if(!IsMultiplayerAuthority())
		{
			this.Hide();
			return ;
		}

		this.Hide();
		MainMenu.Show();
		Settings.Hide();

		LoadGameScene();
	}


	public void LoadGameScene()
	{
		Addresolutions();

		_OnAntialiasingItemSelected(Globals.GlobalsData.Antialiasing);
		_OnAntitropicItemSelected(Globals.GlobalsData.Antitropic);
		_OnVsycnToggled(Globals.GlobalsData.Vsync);
		_OnVolumenValueChanged(Globals.GlobalsData.Volumen);
		_OnVolumenMusicValueChanged(Globals.GlobalsData.VolumenMusic);
		_OnResolutionsItemSelected(Globals.GlobalsData.Resolution);
		_OnFullscreenToggled(Globals.GlobalsData.Fullscreen);
		_OnFpsToggled(Globals.GlobalsData.FPS);
		_OnTimeValueChanged(Globals.GlobalsData.TimerDisasters);
		_OnOptionButtonItemSelected(Globals.GlobalsData.Quality);


		Fullscreen.ButtonPressed = Globals.GlobalsData.Fullscreen;
		Fps.ButtonPressed = Globals.GlobalsData.FPS;
		Vsync.ButtonPressed = Globals.GlobalsData.Vsync;
		Volumen.Value = Globals.GlobalsData.Volumen;
		VolumenMusic.Value = Globals.GlobalsData.VolumenMusic;
		Time.Value = Globals.GlobalsData.TimerDisasters;
		Quality.Selected = Globals.GlobalsData.Quality;
		Resolutions.Selected = Globals.GlobalsData.Resolution;
		AntiAliasing.Selected = Globals.GlobalsData.Antialiasing;
		AntiTropic.Selected = Globals.GlobalsData.Antitropic;
	}


	protected void _OnIpTextChanged(string new_text)
	{
		Globals.Ip = new_text;
	}


	protected void _OnPortTextChanged(string new_text)
	{
		Globals.Port = Int(new_text);
	}


	protected void _OnPlayPressed()
	{
		MainMenu.Hide();
		Settings.Hide();
	}


	protected void _OnSettingsPressed()
	{
		MainMenu.Hide();
		Settings.Show();
	}


	protected void _OnExitPressed()
	{
		Pause();
		Globals.CloseConection();
	}

	public override void _ExitTree()
	{
		Globals.TemperatureTarget = Globals.TemperatureOriginal;
		Globals.HumidityTarget = Globals.HumidityOriginal;
		Globals.PressureTarget = Globals.PressureOriginal;
		Globals.WindDirectionTarget = Globals.WindDirectionOriginal;
		Globals.WindSpeedTarget = Globals.WindSpeedOriginal;
	}

	protected void _OnFpsToggled(bool toggled_on)
	{
		Globals.GlobalsData.FPS = toggled_on;
		Globals.GlobalsData.SaveFile();
	}


	protected void _OnVsycnToggled(bool toggled_on)
	{
		Globals.GlobalsData.Vsync = toggled_on;

		if(toggled_on)
		{
			DisplayServer.WindowSetVsyncMode(DisplayServer.VSyncMode.VsyncEnabled);
		}
		else
		{
			DisplayServer.WindowSetVsyncMode(DisplayServer.VSyncMode.VsyncDisabled);
		}

		Globals.GlobalsData.SaveFile();
	}

	protected void _OnBackPressed()
	{
		MainMenu.Show();
		Settings.Hide();
	}


	protected Node _GetLocalPlayer()
	{
		foreach(Node p in GetTree().GetNodesInGroup("player"))
		{
			if(p.IsMultiplayerAuthority())
			{
				return p;
			}
		}

		return null;
	}


	public void MouseAction()
	{
		if(MouseActionState)
		{
			Input.SetMouseMode(Input.MouseMode.MouseModeCaptured);
		}
		else
		{
			Input.SetMouseMode(Input.MouseMode.MouseModeVisible);
		}

		MouseActionState = !MouseActionState;
	}

	public void Pause()
	{
		Globals.IsPauseMenuOpen = !Globals.IsPauseMenuOpen;

		if(Multiplayer.MultiplayerPeer is OfflineMultiplayerPeer)
		{
			GetTree().Paused = false;
		}

		if(!Globals.IsPauseMenuOpen)
		{
			Input.SetMouseMode(Input.MouseMode.MouseModeCaptured);
		}
		else
		{
			Input.SetMouseMode(Input.MouseMode.MouseModeVisible);
		}

		this.Visible = Globals.IsPauseMenuOpen;
	}


	public override void _Process(double _delta)
	{
		if(!IsMultiplayerAuthority())
		{
			return ;
		}

		if(Input.IsActionJustPressed("Mouse Action"))
		{
			MouseAction();
		}

		if(Input.IsActionJustPressed("Pause"))
		{
			Pause();
		}
	}


	protected void _OnTimeValueChanged(Variant value)
	{
		var player = _GetLocalPlayer();
		if(player == null || !player.AdminMode)
		{
			Globals.PrintRole("You dont have perms");
			return ;
		}

		if(!Globals.Started)
		{
			return ;
		}

		Globals.GlobalsData.TimerDisasters = value;
		Globals.GlobalsData.SaveFile();
		Globals.Timer.WaitTime = value;
	}


	protected void _OnVolumenValueChanged(double value)
	{
		Globals.GlobalsData.Volumen = value;
		AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("Master"), Mathf.LinearToDb(value));
		Globals.GlobalsData.SaveFile();
	}


	protected void _OnResolutionsItemSelected(int index)
	{
		Globals.GlobalsData.Resolution = index;
		var size = ResolutionsDic.Get(Resolutions.GetItemText(index));
		DisplayServer.WindowSetSize(size);
		GetViewport().SetSize(size);
		Globals.GlobalsData.SaveFile();
	}


	protected void _OnFullscreenToggled(bool toggled_on)
	{
		if(toggled_on == true)
		{
			DisplayServer.WindowSetMode(DisplayServer.WindowMode.WindowModeFullscreen);
		}
		else
		{
			DisplayServer.WindowSetMode(DisplayServer.WindowMode.WindowModeWindowed);
		}
		Globals.GlobalsData.Fullscreen = toggled_on;
		Globals.GlobalsData.SaveFile();
	}

	protected void _OnResetPlayerPressed()
	{
		GetParent()._ResetPlayer();
	}

	protected void _OnReturnPressed()
	{
		Pause();
	}

	protected void _OnVolumenMusicValueChanged(Variant value)
	{
		Globals.GlobalsData.VolumenMusic = value;
		AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("Music"), Mathf.LinearToDb(value));
		Globals.GlobalsData.SaveFile();
	}

	protected void _OnOptionButtonItemSelected(int index)
	{

		switch(index)
		{
			case 0:
			{
				Light.ShadowEnabled = false;
				Light2.ShadowEnabled = false;
				Worldenvironment.Environment.SdfgiEnabled = false;
				Worldenvironment.Environment.GlowEnabled = false;
				Worldenvironment.Environment.SsaoEnabled = false;
				break; }
			case 1:
			{
				Light.ShadowEnabled = true;
				Light2.ShadowEnabled = true;
				Worldenvironment.Environment.SdfgiEnabled = false;
				Worldenvironment.Environment.GlowEnabled = true;
				Worldenvironment.Environment.SsaoEnabled = false;
				break; }
			case 2:
			{
				Light.ShadowEnabled = true;
				Light2.ShadowEnabled = true;
				Worldenvironment.Environment.SdfgiEnabled = true;
				Worldenvironment.Environment.GlowEnabled = true;
				Worldenvironment.Environment.SsaoEnabled = true;
				break; }
		}

		Globals.GlobalsData.Quality = index;
		Globals.GlobalsData.SaveFile();
	}

	protected void _OnAntialiasingItemSelected(int index)
	{
		Globals.GlobalsData.Antialiasing = index;

		var viewport = GetViewport();

		switch(index)
		{
			case 0:
			{viewport.Msaa3d = Viewport.Msaa.MsaaDisabled;
				break; }
			case 1:
			{viewport.Msaa3d = Viewport.Msaa.Msaa2x;
				break; }
			case 2:
			{viewport.Msaa3d = Viewport.Msaa.Msaa4x;
				break; }
			case 3:
			{viewport.Msaa3d = Viewport.Msaa.Msaa8x;
				break; }
		}

		Globals.GlobalsData.SaveFile();
	}


	protected void _OnAntitropicItemSelected(int index)
	{
		Globals.GlobalsData.Antitropic = index;

		var levels = new Godot.Collections.Array{1, 2, 4, 8, 16, };

		if(index >= 0 && index < levels.Size())
		{


			ProjectSettings.SetSetting("rendering/textures/default_filters/anisotropic_filtering_level", levels[index]);
			ProjectSettings.Save();
		}

		Globals.GlobalsData.SaveFile();
	}

}