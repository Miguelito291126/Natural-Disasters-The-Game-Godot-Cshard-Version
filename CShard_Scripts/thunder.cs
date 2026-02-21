using Godot;
using Godot.Collections;

[GlobalClass]
public partial class Thunder : Node3D
{
	public Resource ExplosionScene = /* preload has no equivalent, add a 'ResourcePreloader' Node in your scene */("res://Scenes/thunder_explosion.tscn");
	public Node Spark;
	public Node Light;
	public Node Star;


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Spark = GetNode("spark");
		Light = GetNode("light");
		Star = GetNode("star");

		// Configurar la posici�n de la explosi�n en la posici�n del suelo
		var explosion = ExplosionScene.Instantiate();
		explosion.Position = this.Position;
		GetParent().AddChild(explosion);

		Spark.Emitting = true;
		Light.Emitting = true;
		Star.Emitting = true;
	}


	protected void _OnSparkFinished()
	{
		this.QueueFree();
	}

}