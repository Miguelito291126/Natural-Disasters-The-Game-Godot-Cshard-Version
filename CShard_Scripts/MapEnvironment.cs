using Godot;
using System;

[GlobalClass]
public partial class MapEnvironment : WorldEnvironment
{
    [Export] public DirectionalLight3D Sun;
    [Export] public DirectionalLight3D Moon;

    [Export] public int IngameSpeed = 60; // 1 = Tiempo real, 60 = 1 hora por minuto real
    [Export] public float InitialHour = 12.0f;


    public override void _Ready()
    {
        // Si no se asignaron en el inspector, buscarlos
        if (Sun == null) Sun = GetNodeOrNull<DirectionalLight3D>("Sun");
        if (Moon == null) Moon = GetNodeOrNull<DirectionalLight3D>("Moon");

        // Inicializar el tiempo en segundos totales
        Globals.Instance.Seconds = InitialHour * 3600.0f; // Convertir horas a segundos
    }

    public override void _Process(double delta)
    {
        // Avanzar tiempo en segundos
        Globals.Instance.Seconds += (float)delta * IngameSpeed;

        _RecalculateTime();
        _UpdateLamps();
    }

    private void _RecalculateTime()
    {
        double secondsInDay = Globals.Instance.Seconds % 86400; // Segundos en un día (24*3600)
        
        Globals.Instance.Day = (int)(Globals.Instance.Seconds / 86400);
        Globals.Instance.Hour = (int)(secondsInDay / 3600);
        Globals.Instance.Minute = (int)((secondsInDay % 3600) / 60);
        
        // Valor de 0.0 a 1.0 que representa el progreso del día
        Globals.Instance.Day = (int)(secondsInDay / 86400.0);
    }

    private void _UpdateLamps()
    {
        // Calculamos la rotación (0.0 a 360.0 grados)
        // Restamos 90 grados para que a las 12:00 el sol esté en lo más alto (Zenit)
        float rotationAngle = (Globals.Instance.Day * 360.0f) - 90.0f;

        if (Sun != null)
        {
            // El sol rota en el eje X
            Sun.RotationDegrees = new Vector3(-rotationAngle, 0, 0);
            // Opcional: Apagar el sol si está bajo el horizonte para ahorrar rendimiento
            Sun.Visible = Sun.RotationDegrees.X < 0; 
        }

        if (Moon != null)
        {
            // La luna está al lado opuesto (180 grados de diferencia)
            Moon.RotationDegrees = new Vector3(-(rotationAngle + 180.0f), 0, 0);
            Moon.Visible = Moon.RotationDegrees.X < 0;
        }
    }
}