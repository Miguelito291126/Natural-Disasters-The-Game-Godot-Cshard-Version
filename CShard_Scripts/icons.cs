using Godot;
using Godot.Collections;

[GlobalClass]
public partial class Icons : Node
{
	public override void _Ready()
	{
		_GenerateIcon();
	}

	protected void _GenerateIcon()
	{
		foreach(Node child in GetChildren())
		{
			await ToSignal(RenderingServer, "FramePostDraw");
			var img = child.GetTexture().GetImage();
			img.SavePng("res://icons/" + child.Name + ".png");
		}
	}

}