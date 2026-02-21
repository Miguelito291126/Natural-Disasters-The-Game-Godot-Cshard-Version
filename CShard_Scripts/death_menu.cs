using Godot;
using Godot.Collections;

[GlobalClass]
public partial class DeathMenu : CanvasLayer
{
	public override void _Ready()
	{
		this.Hide();
	}

	protected void _OnReturnPressed()
	{
		if(Multiplayer.MultiplayerPeer is OfflineMultiplayerPeer)
		{
			GetTree().Paused = false;
		}

		GetParent()._ResetPlayer();
		Input.SetMouseMode(Input.MouseMode.MouseModeCaptured);
		this.Hide();
	}


	protected void _OnExitPressed()
	{
		Globals.CloseConection();
	}


}