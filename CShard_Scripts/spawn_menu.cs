using Godot;
using Godot.Collections;

[GlobalClass]
public partial class SpawnMenu : CanvasLayer
{
	public GridContainer Container;
	[Export] public Array<Node> Spawnlist;
	[Export] public Array<Button> Buttonlist;
	[Export] public Array<Node> Spawnedobject;
	public Node Camera;

	public Resource EntityScene = /* preload has no equivalent, add a 'ResourcePreloader' Node in your scene */("res://Scenes/entity.tscn");
public Array<string> SpawnList = new() {
			"res://Scenes/meteor.tscn", 
			"res://Scenes/tornado.tscn", 
			"res://Scenes/volcano.tscn", 
			"res://Scenes/tsunami.tscn", 
			"res://Scenes/earthquake.tscn", 
			"res://Scenes/thunder.tscn", 
			"res://Scenes/cube.tscn", 
			"res://Scenes/sphere.tscn", 
			"res://Scenes/hause.tscn", 
			};

	public override void _EnterTree()
	{
		SetMultiplayerAuthority(GetParent().Name.ToInt());
	}

	public override void _Ready()
	{
		Container = GetNode<GridContainer>("Panel/GridContainer");
		Camera = GetParent().GetNode("head/Camera3D");
		this.Visible = false;

		LoadSpawnlistEntities();
		LoadButtons();
	}

	protected Node _GetLocalPlayer()
	{
		foreach(Node p in GetTree().GetNodesInGroup("player"))
		{

			if(p.IsMultiplayerAuthority())
			{
				return p;
			}
		}

		return null;
	}


	public void LoadSpawnlistEntities()
	{
		foreach(string spawn in SpawnList)
		{
			var node = Load(spawn).Instantiate();
			Spawnlist.Add(node);
		}
	}


	public void LoadButtons()
	{
		foreach(Node i in Spawnlist)
		{
			var entity = EntityScene.Instantiate();
			var label = entity.GetNode("Label");
			label.Text = i.Name;
			label.AddThemeFontSizeOverride("FontSize", 20);
			label.CustomMinimumSize = new Vector2(150, 150);

			// cada celda fija
			var icon = entity.GetNode("Icon");
			icon.StretchMode = TextureRect.StretchMode.StretchKeepAspectCentered;
			icon.CustomMinimumSize = new Vector2(64, 64);

			// icono fijo
			// Intentar cargar icono con varias variantes (por si hay espacios/may�sculas)
var candidates = new Array {
						$"res://Icons/{i.Name}_icon.png",
						$"res://Icons/{i.Name.Replace(" ", "_")}_icon.png",
						$"res://Icons/{i.Name.ToLower().Replace(" ", "_")}_icon.png",
						$"res://Icons/{i.Name.ToLower().Replace(" ", "")}_icon.png",
							};

			var icon_image = null;
			foreach(Variant p in candidates)
			{
				icon_image = Load(p);
				if(icon_image != null)
				{
					break;
				}
			}


			// Fallback a un icono por defecto si no se encuentra ninguno
			if(icon_image == null)
			{
				icon_image = Load("res://Icons/default_icon.png");
				if(icon_image == null)
				{
					Globals.PrintRole("spawn_menu.gd: icon not found for '%s' (tried %s). Create 'res://Icons/default_icon.png' to avoid this message." % new Array{i.Name, Str(candidates), });
				}
			}

			if(icon_image != null)
			{
				icon.TextureNormal = icon_image;
			}

			Container.AddChild(entity);
			icon.Pressed.Connect(() =>
			{
				OnPress(i);
			});
		}
	}

	public void OnPress(Node i)
	{
		var player = _GetLocalPlayer() as Player;
		if(player == null || !player.AdminMode)
		{
			Globals.PrintRole("You dont have perms");
			return ;
		}

		if(!IsMultiplayerAuthority())
		{
			return ;
		}

		var raycast = GetParent().Interactor;

		if(raycast.IsColliding())
		{
			var collision_point = raycast.GetCollisionPoint();
			var collision_normal = raycast.GetCollisionNormal();

			var new_i = i.Duplicate();
			new_i.Transform.Origin = collision_point + collision_normal * 0.5;
			Spawnedobject.Add(new_i);


			// Asignar autoridad al servidor (peer_id = 1 es el servidor)
			new_i.SetMultiplayerAuthority(1);


			// A�adir al mapa como propiedad de la escena
			Globals.Map.AddChild(new_i, true);
		}
	}


	public void Spawnmenu()
	{
		Globals.IsSpawnMenuOpen = !Globals.IsSpawnMenuOpen;

		if(Globals.IsSpawnMenuOpen)
		{
			Input.SetMouseMode(Input.MouseMode.MouseModeVisible);
		}
		else
		{
			Input.SetMouseMode(Input.MouseMode.MouseModeCaptured);
		}

		this.Visible = Globals.IsSpawnMenuOpen;
	}


	public void Remove()
	{
		if(Spawnedobject.Size() > 0)
		{
			var last = Spawnedobject.PopBack();
			if(GodotObject.IsInstanceValid(last))
			{
				last.QueueFree();
			}
		}
	}


	public override void _Process(double _delta)
	{

		if(!IsMultiplayerAuthority())
		{
			return ;
		}

		if(Globals.Gamemode == "survival")
		{
			return ;
		}

		if(Input.IsActionJustPressed("Spawnmenu"))
		{
			Spawnmenu();
		}

		if(Input.IsActionJustPressed("Remove"))
		{
			Remove();
		}
	}


}