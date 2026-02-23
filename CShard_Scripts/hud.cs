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

		if(!IsMultiplayerAuthority())
		{
			this.Visible = false;
			return ;
		}

		this.Visible = true;

		AnimationPlayer.Play("Hearth_Animation");
	}


	public override void _Process(double _delta)
	{


		if(!IsMultiplayerAuthority())
		{
			this.Visible = false;
			return ;
		}

		this.Visible = true;

		float freq = (float)Mathf.Clamp((1 - (44 - Mathf.Round(GetParent<Player>().BodyTemperature)) / 20) * (180 / 60), 0.5, 20);

		if(GetParent<Player>().Hearth <= 0)
		{
			freq = 0.05f;
		}

		AnimationPlayer.SpeedScale = freq;

		if(Globals.Instance.GlobalsData.FPS)
		{
			Fps.Visible = true;
		}
		else
		{
			Fps.Visible = false;
		}

		Label.Text = "Temperature: " + Mathf.Snapped(Globals.Instance.Temperature, 0.1) + "C\n" + "Humidity: " + Mathf.Round(Globals.Instance.Humidity) + "%\n" + "Wind Direction: " + Mathf.Round(Globals.Instance.ConvertVectorToAngle(Globals.Instance.WindDirection)) + "\n" + "Wind Speed: " + Mathf.Round(Globals.Instance.WindSpeed) + "km/s\n" + "Body Hearth: " + Mathf.Round(Player.Hearth) + "%\n" + "Body Temperature: " + Mathf.Snapped(Player.BodyTemperature, 0.1) + "C\n" + "Body Oxygen: " + Mathf.Round(Player.BodyOxygen) + "%\n" + "Local Wind Speed: " + Mathf.Round(Player.BodyWind) + "km/s\n";
		Fps.Text = "FPS: " + Engine.GetFramesPerSecond();

		if(Time.GetUnixTimeFromSystem() >= NextHeartSoundTime)
		{
			HearthbeatSound.Play();
			NextHeartSoundTime = (float)Time.GetUnixTimeFromSystem() + freq / 1;
		}
	}


}