using Godot;
using Godot.Collections;


[Tool]
[GlobalClass]
public partial class AddToGroupScript : EditorScript
{
	public override void _Run()
	{
		var group_name = "movable_objects";
		var group_name2 = "wind_effected_objects";
		var nodes = new Array{};


		// Obtener la selecci�n del editor
		var selection = EditorInterface.GetSelection().GetSelectedNodes();


		// Filtrar solo los nodos que se pueden agregar a grupos
		foreach(Node node in selection)
		{
			if(node is Node)
			{
				nodes.Append(node);
			}
		}


		// Agregar los nodos al grupo
		foreach(Variant node in nodes)
		{
			node.AddToGroup(group_name);
			node.AddToGroup(group_name2);
		}
	}


}