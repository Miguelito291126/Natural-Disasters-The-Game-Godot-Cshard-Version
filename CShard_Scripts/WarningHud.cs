using Godot;
using Godot.Collections;

[GlobalClass]
public partial class WarningHud : CanvasLayer
{
	public Label Label;

	public override void _EnterTree()
	{
		SetMultiplayerAuthority(Multiplayer.GetUniqueId());
	}

	public override void _Ready()
	{
		Label = GetNode<Label>("Panel/Label");

		this.Visible = IsMultiplayerAuthority();
		if(!IsMultiplayerAuthority())
		{
			return ;
		}
	}


	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double _delta)
	{

		if(!IsMultiplayerAuthority())
		{
			return ;
		}


		if(!Multiplayer.IsServer())
		{
			return ;
		}

		if(Globals.Instance.Started)
		{
			if(Globals.Instance.Gamemode != "survival")
			{
				Label.Text = "Current Disasters/Weather is: \n" + Globals.Instance.CurrentWeatherAndDisaster + "\nTime:\n" + Globals.Instance.Hour.ToString("D2") + ":" + Globals.Instance.Minute.ToString("D2");
			}
			else
			{
				Label.Text = "Current Disasters/Weather is: \n" + Globals.Instance.CurrentWeatherAndDisaster + "\nTime Left for the next disasters: \n" + Globals.Instance.Timer.TimeLeft.ToString("F2") + "\nTime:\n" + Globals.Instance.Hour.ToString("D2") + ":" + Globals.Instance.Minute.ToString("D2");
			}
		}
		else
		{
			Label.Text = "Waiting for players... Time remain: \n" + Globals.Instance.TimeLeft.ToString("F2");
		}
	}


}