using Godot;
using Godot.Collections;

[GlobalClass]
public partial class Main : Node3D
{
	public override void _Ready()
	{
		Globals.Main = this;
		LoadScene.LoadScene(null, "res://Scenes/main_menu.tscn");
	}


}