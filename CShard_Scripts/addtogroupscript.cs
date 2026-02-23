using Godot;
using Godot.Collections;

[Tool]
public partial class AddToGroupScript : EditorScript
{
	public override void _Run()
	{
		var groupName1 = "movable_objects";
		var groupName2 = "wind_effected_objects";

		// 1. Usar GetEditorInterface() para obtener la instancia
		var selection = EditorInterface.Singleton.GetSelection().GetSelectedNodes();

		if (selection.Count == 0)
		{
			GD.Print("No hay nodos seleccionados.");
			return;
		}

		foreach (Node node in selection)
		{
			// 2. Comprobar que no esté ya en el grupo para no duplicar
			if (!node.IsInGroup(groupName1))
			{
				node.AddToGroup(groupName1, true); // El true lo hace persistente en la escena
			}
			
			if (!node.IsInGroup(groupName2))
			{
				node.AddToGroup(groupName2, true);
			}

			GD.Print($"Nodo '{node.Name}' agregado a los grupos.");
		}
	}
}
