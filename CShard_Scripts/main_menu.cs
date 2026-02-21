using Godot;
using Godot.Collections;

[GlobalClass]
public partial class MainMenu : Control
{
	public Control MainMenuPanel;
	public Label Tittle;
	public new Control Multiplayer;
	public Control MultiplayerList;
	public Control Settings;
	public Control PlayMenu;
	public LineEdit Username;
	public LineEdit IpText;
	public LineEdit PortText;
	public CheckButton Fullscreen;
	public CheckButton Vsync;
	public CheckButton Fps;
	public OptionButton AntiAliasing;
	public OptionButton AntiTropic;
	public HSlider Volumen;
	public HSlider VolumenMusic;
	public Time time;
	public OptionButton Quality;
	public AudioStreamPlayer Music;
	public Label ErrorText;
	public OptionButton Resolutions;
	public Label Version;
	public Label Credits;

	public bool MultiplayerMode = false;

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
		MainMenuPanel = GetNode<Control>("Panel/Menu");
		Tittle = GetNode<Label>("Panel/Menu/HBoxContainer/Title");
		Multiplayer = GetNode<Control>("Panel/Multiplayer");
		MultiplayerList = GetNode<Control>("Panel/MultiplayerList");
		Settings = GetNode<Control>("Panel/Settings");
		PlayMenu = GetNode<Control>("Panel/Play");
		Username = GetNode<LineEdit>("Panel/Multiplayer/Username");
		IpText = GetNode<LineEdit>("Panel/Multiplayer/ip");
		PortText = GetNode<LineEdit>("Panel/Multiplayer/port");
		Fullscreen = GetNode<CheckButton>("Panel/Settings/Fullscreen");
		Vsync = GetNode<CheckButton>("Panel/Settings/Vsync");
		Fps = GetNode<CheckButton>("Panel/Settings/Fps");
		AntiAliasing = GetNode<OptionButton>("Panel/Settings/antialiasing");
		AntiTropic = GetNode<OptionButton>("Panel/Settings/antitropic");
		Volumen = GetNode<HSlider>("Panel/Settings/Volumen");
		VolumenMusic = GetNode<HSlider>("Panel/Settings/Volumen Music");
		Time = GetNode<Time>("Panel/Play/Time");
		Quality = GetNode<OptionButton>("Panel/Settings/Quality");
		Music = GetNode<AudioStreamPlayer>("Music");
		ErrorText = GetNode<Label>("Panel/Multiplayer/Label");
		Resolutions = GetNode<OptionButton>("Panel/Settings/Resolutions");
		Version = GetNode<Label>("Panel/Version");
		Credits = GetNode<Label>("Panel/Credits");
		Globals.MainMenu = this;

		MainMenuPanel.Show();
		Tittle.Show();
		Multiplayer.Hide();
		Settings.Hide();
		MultiplayerList.Hide();
		PlayMenu.Hide();

		Version.Text = "V" + Globals.Version;
		Tittle.Text = Globals.Gamename;
		Credits.Text = "by " + Globals.Credits;

		LoadGameScene();
		Globals.SetUpLisener();

		if(OS.HasFeature("dedicated_server") || OS.GetCmdlineUserArgs() || OS.GetCmdlineUserArgs().Contains("server").Contains("s"))
		{
			Globals.PrintRole("Starting server...");

			var args = OS.GetCmdlineUserArgs();
			foreach(int i in GD.Range(args.Count))
			{
				Globals.PrintRole("args: " + args[i]);

				if(args[i] == "--port")
				{
					if(i + 1 < args.Size())
					{
						Globals.Port = args[i + 1].ToInt();
						Globals.LisenerPort = Globals.Port + 1;
						Globals.BroadcasterPort = Globals.Port - 1;
					}
				}
				else if(args[i] == "--gamemode")
				{
					if(i + 1 < args.Size())
					{
						Globals.Gamemode = args[i + 1];
					}
				}

				//PANIC! <Globals . print_role ( port:  + str ( Globals . port ) )> unexpected at Token(type='TEXT', value='Globals', lineno=96, index=2923, end=2930)

				Globals.PrintRole("ip: " + IP.ResolveHostname(Str(OS.GetEnvironment("COMPUTERNAME")), IP.Type.TypeIpv4));
				Globals.PrintRole("Init dedicated server...");

				await ToSignal(GetTree().CreateTimer(2), "Timeout");

				Globals.Hostwithport(Globals.Port);
			}
		}
	}

	public void LoadGameScene()
	{
		Addresolutions();

		IpText.Text = Globals.Ip;
		PortText.Text = Str(Globals.Port);

		_OnAntialiasingItemSelected(Globals.GlobalsData.Antialiasing);
		_OnAntitropicItemSelected(Globals.GlobalsData.Antitropic);
		_OnVsycnToggled(Globals.GlobalsData.Vsync);
		_OnVolumenValueChanged(Globals.GlobalsData.Volumen);
		_OnVolumenMusicValueChanged(Globals.GlobalsData.VolumenMusic);
		_OnResolutionsItemSelected(Globals.GlobalsData.Resolution);
		_OnFullscreenToggled(Globals.GlobalsData.Fullscreen);
		_OnFpsToggled(Globals.GlobalsData.FPS);
		_OnUsernameTextChanged(Globals.Username);
		_OnHSlider2ValueChanged(Globals.GlobalsData.TimerDisasters);
		_OnOptionButtonItemSelected(Globals.GlobalsData.Quality);

		Fullscreen.ButtonPressed = Globals.GlobalsData.Fullscreen;
		Fps.ButtonPressed = Globals.GlobalsData.FPS;
		Vsync.ButtonPressed = Globals.GlobalsData.Vsync;
		Volumen.Value = Globals.GlobalsData.Volumen;
		VolumenMusic.Value = Globals.GlobalsData.VolumenMusic;
		Time.Value = Globals.GlobalsData.TimerDisasters;
		Quality.Selected = Globals.GlobalsData.Quality;
		AntiAliasing.Selected = Globals.GlobalsData.Antialiasing;
		Resolutions.Selected = Globals.GlobalsData.Resolution;
		AntiTropic.Selected = Globals.GlobalsData.Antitropic;
	}
		
	public override void _Process(double _delta)
	{
		if(this.Visible)
		{
			await ToSignal(Music, "Finished");
			Music.Play();
		}
		else
		{
			Music.Stop();
		}
	}

	protected void _OnIpTextChanged(string new_text)
	{
		Globals.Ip = new_text;
	}


	protected void _OnPortTextChanged(string new_text)
	{
		Globals.Port = Int(new_text);
		Globals.LisenerPort = Int(new_text) + 1;
		Globals.BroadcasterPort = Int(new_text) - 1;
		Globals.SetUpLisener();
	}


	protected void _OnJoinPressed()
	{
		if(Globals.Username.Length() < 10 && Globals.Username.Length() >= 1)
		{
			Globals.PlayMultiplayerClient();
		}
		else
		{
			ErrorText.Visible = true;
			await ToSignal(GetTree().CreateTimer(2), "Timeout");
			ErrorText.Visible = false;
		}
	}


	protected void _OnHostPressed()
	{
		MultiplayerMode = true;
		MainMenuPanel.Hide();
		Multiplayer.Hide();
		Settings.Hide();
		MultiplayerList.Hide();
		PlayMenu.Show();
	}


	protected void _OnMultiplayerPressed()
	{
		MainMenuPanel.Hide();
		Multiplayer.Show();
		Settings.Hide();
		MultiplayerList.Hide();
		PlayMenu.Hide();
	}

	protected void _OnSandboxPressed()
	{
		Globals.Gamemode = "sandbox";
		if(MultiplayerMode)
		{
			Globals.PlayMultiplayerServer();
		}
		else
		{
			LoadScene.LoadScene(this, "map");
		}
	}

	protected void _OnSurvivalPressed()
	{
		Globals.Gamemode = "survival";
		if(MultiplayerMode)
		{
			Globals.PlayMultiplayerServer();
		}
		else
		{
			LoadScene.LoadScene(this, "map");
		}
	}


	protected void _OnSettingsPressed()
	{
		MainMenuPanel.Hide();
		Multiplayer.Hide();
		Settings.Show();
		MultiplayerList.Hide();
		PlayMenu.Hide();
	}


	protected void _OnExitPressed()
	{
		GetTree().Quit();
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
		MainMenuPanel.Show();
		Multiplayer.Hide();
		Settings.Hide();
		MultiplayerList.Hide();
		PlayMenu.Hide();
	}


	protected void _OnUsernameTextChanged(string new_text)
	{
		Globals.Username = new_text;
		Globals.GlobalsData.SaveFile();
	}


	protected void _OnHSlider2ValueChanged(Variant value)
	{
		Globals.GlobalsData.TimerDisasters = value;
		Globals.GlobalsData.SaveFile();
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
		DisplayServer.WindowSetSize(Size);
		GetViewport().SetSize(Size);
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


	protected void _OnSingleplayerPressed()
	{
		MultiplayerMode = false;
		MainMenu.Hide();
		Multiplayer.Hide();
		Settings.Hide();
		MultiplayerList.Hide();
		PlayMenu.Show();
	}


	protected void _OnVolumenMusicValueChanged(Variant value)
	{
		Globals.GlobalsData.VolumenMusic = value;
		AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("Music"), Mathf.LinearToDb(value));
		Globals.GlobalsData.SaveFile();
	}


	protected void _OnOptionButtonItemSelected(int index)
	{
		Globals.GlobalsData.Quality = index;
		Globals.GlobalsData.SaveFile();
	}


	protected void _OnMultiplayerListPressed()
	{
		MainMenu.Hide();
		Multiplayer.Hide();
		Settings.Hide();
		MultiplayerList.Show();
		PlayMenu.Hide();
	}


	protected void _OnBackMultiplayerPressed()
	{
		MainMenu.Hide();
		Multiplayer.Show();
		Settings.Hide();
		MultiplayerList.Hide();
		PlayMenu.Hide();
	}


	protected void _OnBackSingleplayerPressed()
	{
		MainMenuPanel.Show();
		Multiplayer.Hide();
		Settings.Hide();
		MultiplayerList.Hide();
		PlayMenu.Hide();
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
		}

		Globals.GlobalsData.SaveFile();
	}
}