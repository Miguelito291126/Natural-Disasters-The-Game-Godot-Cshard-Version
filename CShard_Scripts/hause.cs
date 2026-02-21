using Godot;
using Godot.Collections;

[GlobalClass]
public partial class House : StaticBody3D
{
	public Variant Door;
	public Node DoorCollisionShape;
	public Node HauseModel;
	[Export] public AudioStreamPlayer3D DoorOpenSound;
	[Export] public AudioStreamPlayer3D DoorCloseSound;

	[Export] public bool DoorOpen = false;
	[Export] public bool Destrolled = false;

	[Export] public Resource Bokenhause = /* preload has no equivalent, add a 'ResourcePreloader' Node in your scene */("res://Scenes/breakable_hause.tscn");

	//# Factor extra de escala para las piezas destruidas. Las mallas del Breakable
	//# est�n en unidades m�s peque�as que la casa; aumenta si se ven diminutas.
	[Export] public double BreakableScaleFactor = 2.6;

	public void OpenDoor()
	{
		Globals.PrintRole("Open the door");
		Door.Rotation.Y = Mathf.DegToRad(145);
		DoorCollisionShape.Disabled = true;
		if(!DoorOpenSound.Playing)
		{
			DoorOpenSound.Play();
		}
		DoorOpen = true;
	}

	public void CloseDoor()
	{
		Globals.PrintRole("Close the door");
		Door.Rotation.Y = Mathf.DegToRad(0);
		DoorCollisionShape.Disabled = false;
		if(!DoorCloseSound.Playing)
		{
			DoorCloseSound.Play();
		}
		DoorOpen = false;
	}


	public void Interact()
	{

		if(!DoorOpen)
		{
			open_door.Rpc();
		}
		else
		{
			close_door.Rpc();
		}
	}


	public void Destroy()
	{
		if(Destrolled)
		{
			return ;
		}

		var Broken_Hause = Bokenhause.Instantiate();
		GetParent().AddChild(Broken_Hause);
		Broken_Hause.GlobalTransform = HauseModel.GlobalTransform;
		Destrolled = true;

		// Guardar path en Globals
		Globals.AddDestrolledNodes(this.GetPath());
		this.QueueFree();
	}

	protected void _OnArea3dBodyEntered(Node3D body)
	{
		if(body.IsInGroup("Meteor"))
		{
			destroy.Rpc();
		}
	}

	protected void _OnArea3dAreaEntered(Area3D area)
	{
		if(area.IsInGroup("Tornado") || area.IsInGroup("Water_Area") || area.IsInGroup("Explosion") || area.IsInGroup("Lava_Area"))
		{
			destroy.Rpc();
		}
	}

	public override void _Ready()
	{
		Door = GetNode<Node3D>("hause/pivot");
		DoorCollisionShape = GetNode("DoorCollision");
		HauseModel = GetNode("hause");
	}
}