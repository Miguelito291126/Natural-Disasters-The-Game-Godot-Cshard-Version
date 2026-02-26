using Godot;
using Godot.Collections;

[GlobalClass]
public partial class Meteors : RigidBody3D
{
	public PackedScene ExplosionScene = ResourceLoader.Load<PackedScene>("res://Scenes/explosion.tscn");
	[Export] public int RandNum = GD.RandRange(1, 50);
	[Export] public bool IsVolcanoRock = false;


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{

		// Solo mover hacia arriba si NO es una roca del volc�n
		if(!IsVolcanoRock)
		{
			this.GlobalPosition += new Vector3(0, 1000, 0);
		}
	}


	protected void _OnBodyEntered(Node3D body)
	{
		if(body == this)
		{
			return;
		}

		Node3D explosion_node = ExplosionScene.Instantiate<Node3D>();
		GetParent().AddChild(explosion_node, true);
		explosion_node.GlobalPosition = this.GlobalPosition;
		this.QueueFree();
	}


}