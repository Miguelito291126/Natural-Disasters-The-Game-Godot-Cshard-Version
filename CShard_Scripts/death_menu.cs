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

		GetParent<Player>()._ResetPlayer();
		Input.SetMouseMode(Input.MouseModeEnum.Captured);
		this.Hide();
	}


	protected void _OnExitPressed()
	{
		Globals.Instance.CloseConection();
	}


}