using Godot;
using Godot.Collections;

[GlobalClass]
public partial class HUD : CanvasLayer
{
	public Node Player;
	public double NextHeartSoundTime = Time.GetUnixTimeFromSystem();

	public Variant Hearth;
	public Label Label;
	public Node Fps;
	public Node HearthbeatSound;
	public AnimationPlayer AnimationPlayer;


	public override void _EnterTree()
	{
		SetMultiplayerAuthority(GetParent().Name.ToInt());
	}

	public override void _Ready()
	{
		Player = GetParent();
		Hearth = GetNode<Control>("Panel/Panel2/Heart");
		Label = GetNode<Label>("Panel/Label");
		Fps = GetNode("FPS");
		HearthbeatSound = GetNode("Heartbeat");
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

		var freq = Mathf.Clamp((1 - Float((44 - Mathf.Round(GetParent().BodyTemperature)) / 20)) * (180 / 60), 0.5, 20);

		if(GetParent().Hearth <= 0)
		{
			freq = 0.05;
		}

		AnimationPlayer.SpeedScale = freq;

		if(Globals.GlobalsData.FPS)
		{
			Fps.Visible = true;
		}
		else
		{
			Fps.Visible = false;
		}

		Label.Text = "Temperature: " + Str(Mathf.Snapped(Globals.Temperature, 0.1)) + "�C\n" + "Humidity: " + Str(Mathf.Round(Globals.Humidity)) + "%\n" + "Wind Direction: " + Str(Mathf.Round(Globals.ConvertVectorToAngle(Globals.WindDirection))) + "�\n" + "Wind Speed: " + Str(Mathf.Round(Globals.WindSpeed)) + "km/s\n" + "Body Hearth: " + Str(Mathf.Round(Player.Hearth)) + "%\n" + "Body Temperature: " + Str(Mathf.Snapped(Player.BodyTemperature, 0.1)) + "�C\n" + "Body Oxygen: " + Str(Mathf.Round(Player.BodyOxygen)) + "%\n" + "Local Wind Speed: " + Str(Mathf.Round(Player.BodyWind)) + "km/s\n";
		Fps.Text = "FPS: " + Str(Engine.GetFramesPerSecond());

		if(Time.GetUnixTimeFromSystem() >= NextHeartSoundTime)
		{
			HearthbeatSound.Play();
			NextHeartSoundTime = Time.GetUnixTimeFromSystem() + freq / 1;
		}
	}


}