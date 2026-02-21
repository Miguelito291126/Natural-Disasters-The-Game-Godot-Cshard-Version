using Godot;
using Godot.Collections;

[GlobalClass]
public partial class Meteors : RigidBody3D
{
	public Resource ExplosionScene = /* preload has no equivalent, add a 'ResourcePreloader' Node in your scene */("res://Scenes/explosion.tscn");
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


	protected void _OnBodyEntered(Variant body)
	{
		if(body == this)
		{
			return ;
		}

		var explosion_node = ExplosionScene.Instantiate();
		explosion_node.GlobalPosition = this.GlobalPosition;
		explosion_node.GetNode("Area3D/CollisionShape3D").Shape.Radius = RandNum;
		GetParent().AddChild(explosion_node, true);
		this.QueueFree();
	}


}