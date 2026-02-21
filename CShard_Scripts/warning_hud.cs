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

		if(Globals.Started)
		{
			if(Globals.Gamemode != "survival")
			{
				Label.Text = "Current Disasters/Weather is: \n" + Globals.CurrentWeatherAndDisaster + "\nTime:\n" + Str(Globals.Hour) + ":" + Str(Globals.Minute);
			}
			else
			{
				Label.Text = "Current Disasters/Weather is: \n" + Globals.CurrentWeatherAndDisaster + "\nTime Left for the next disasters: \n" + Str(Int(Globals.Timer.TimeLeft)) + "\nTime:\n" + Str(Globals.Hour) + ":" + Str(Globals.Minute);
			}
		}
		else
		{
			Label.Text = "Waiting for players... Time remain: \n" + Str(Int(Globals.TimeLeft));
		}
	}


}