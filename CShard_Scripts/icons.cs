using Godot;
using System.Linq;
using System.Threading.Tasks;

[GlobalClass]
public partial class Icons : Node
{
    public override void _Ready()
    {
        CallDeferred(MethodName.GenerateIconsSequentially);
    }

    private async void GenerateIconsSequentially()
    {
		foreach (SubViewport vp in GetChildren().OfType<SubViewport>())
		{
			vp.RenderTargetUpdateMode = SubViewport.UpdateMode.Disabled;
		}

        // 2. Procesamos uno por uno
        foreach (Node child in GetChildren())
        {
            if (child is SubViewport viewport)
            {
                
				Camera3D cam = FindCameraRecursive(viewport);
				if (cam == null)
				{
					GD.PrintErr($"[ERROR] No hay ninguna Camera3D dentro de {viewport.Name}. Saltando...");
					continue;
				}

				GD.Print($"Procesando: {viewport.Name}...");

				cam.Current = true;
				viewport.RenderTargetUpdateMode = SubViewport.UpdateMode.Once;

                await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
                await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);

				var texture = viewport.GetTexture();
				if (texture == null) continue;

                Image img = viewport.GetTexture().GetImage();
				

                if (img != null && !img.IsEmpty())
                {
                    string path = $"res://icons/{viewport.Name}.png";
                    img.SavePng(path);
                    GD.Print($"[EXITO] Guardado: {path}");
                }

				cam.Current = false;
                viewport.RenderTargetUpdateMode = SubViewport.UpdateMode.Disabled;

                await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            }
        }

        GD.Print(">>> Generación secuencial completada sin errores.");
    }

	private Camera3D FindCameraRecursive(Node parent)
	{
		if (parent is Camera3D camera) return camera;
		
		foreach (Node child in parent.GetChildren())
		{
			var found = FindCameraRecursive(child);
			if (found != null) return found;
		}
		return null;
	}
}


