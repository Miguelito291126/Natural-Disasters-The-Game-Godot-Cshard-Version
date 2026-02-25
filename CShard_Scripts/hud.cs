using Godot;
using Godot.Collections;

[GlobalClass]
public partial class HUD : CanvasLayer
{
	public Player Player;
	public double NextHeartSoundTime = Time.GetUnixTimeFromSystem();

	public TextureRect Hearth;
	public Label Label;
	public Label Fps;
	public AudioStreamPlayer HearthbeatSound;
	public AnimationPlayer AnimationPlayer;


	public override void _EnterTree()
	{
		SetMultiplayerAuthority(int.Parse(GetParent().Name));
	}

	public override void _Ready()
	{
		Player = GetParent<Player>();
		Hearth = GetNode<TextureRect>("Panel/Panel2/Heart");
		Label = GetNode<Label>("Panel/Label");
		Fps = GetNode<Label>("FPS");
		HearthbeatSound = GetNode<AudioStreamPlayer>("Heartbeat");
		AnimationPlayer = GetNode<AnimationPlayer>("Panel/Panel2/Heart/AnimationPlayer");

		this.Visible = IsMultiplayerAuthority();
		if(!IsMultiplayerAuthority())
		{
			return ;
		}

		AnimationPlayer.Play("Hearth_Animation");
	}


	public override void _Process(double _delta)
	{


		this.Visible = IsMultiplayerAuthority();
		if(!IsMultiplayerAuthority())
		{
			return ;
		}

		float normalTemp = 37f;
		float temp = Player.BodyTemperature;

		float delta = Mathf.Abs(temp - normalTemp);
		float freq = 1.0f + (delta * 0.15f);
		freq = Mathf.Clamp(freq, 0.8f, 4.0f);

		AnimationPlayer.SpeedScale = freq;

		float interval = 1.0f / freq;

		if(Time.GetUnixTimeFromSystem() >= NextHeartSoundTime)
		{
			HearthbeatSound.Play();
			NextHeartSoundTime = Time.GetUnixTimeFromSystem() + interval;
		}

		if(Globals.Instance.GlobalsData.FPS)
		{
			Fps.Visible = true;
		}
		else
		{
			Fps.Visible = false;
		}

		Label.Text = "Temperature: " + Mathf.Snapped(Globals.Instance.Temperature, 0.1) + "Cº\n" + "Humidity: " + Mathf.Round(Globals.Instance.Humidity) + "%\n" + "Wind Direction: " + Mathf.Round(Globals.Instance.ConvertVectorToAngle(Globals.Instance.WindDirection)) + "\n" + "Wind Speed: " + Mathf.Round(Globals.Instance.WindSpeed) + "km/s\n" + "Body Hearth: " + Mathf.Round(Player.Hearth) + "%\n" + "Body Temperature: " + Mathf.Snapped(Player.BodyTemperature, 0.1) + "C\n" + "Body Oxygen: " + Mathf.Round(Player.BodyOxygen) + "%\n" + "Local Wind Speed: " + Mathf.Round(Player.BodyWind) + "km/s\n";
		Fps.Text = "FPS: " + Engine.GetFramesPerSecond();

	}


}