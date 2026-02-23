using Godot;
using Godot.Collections;

[GlobalClass]
public partial class Icons : Node
{
	public override void _Ready()
	{
		_GenerateIcon();
	}

	protected async void _GenerateIcon()
	{
		foreach (SubViewport child in GetChildren())
		{
			// 1. Usar Singleton para acceder a la instancia que emite señales
			// 2. Usar SignalName para evitar errores de escritura
			await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
			
			var img = child.GetTexture().GetImage();
			
			// Es buena práctica verificar si la imagen existe antes de guardar
			if (img != null)
			{
				img.SavePng("res://icons/" + child.Name + ".png");
				GD.Print($"Icono guardado: {child.Name}");
			}
		}
	}


}