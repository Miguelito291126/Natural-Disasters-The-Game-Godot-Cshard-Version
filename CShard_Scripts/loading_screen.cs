using Godot;
using Godot.Collections;

[GlobalClass]
public partial class LoadingScreen : CanvasLayer
{
	[Signal]
	public delegate void SafeToLoadEventHandler();

	public ProgressBar ProgressBar;
	public Node Animationplayer;

	public void UpdateProgressBar(double new_value)
	{
		ProgressBar.SetValueNoSignal(new_value * 100);
	}

	public void FadeOutLoadingScreen()
	{
		Animationplayer.Play("fade_out");
		await ToSignal(Animationplayer, "AnimationFinished");
		this.QueueFree();
	}

	public override void _Ready()
	{
		ProgressBar = GetNode<ProgressBar>("Control/ProgressBar");
		Animationplayer = GetNode("AnimationPlayer");
	}
}