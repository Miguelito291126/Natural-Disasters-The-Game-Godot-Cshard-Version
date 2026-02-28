using Godot;
using System;

[GlobalClass]
public partial class MapEnvironment : WorldEnvironment
{
    [Export] public DirectionalLight3D Sun;
    [Export] public DirectionalLight3D Moon;

    [Export] public int IngameSpeed = 60; // 1 = Tiempo real, 60 = 1 hora por minuto real
    [Export] public float InitialHour = 12.0f;
    [Export] public float SunBaseEnergy = 2.0f; // Energía normal del sol
    [Export] public float MoonBaseEnergy = 0.2f; // Energía normal del sol
    public bool IsCloudy = false; // El Map cambiará esto
    public bool IsRaining = false; // El Map cambiará esto
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
        // Calculamos el progreso del día (0.0 a 1.0)
        // 86400 son los segundos totales en un día
        float dayProgress = (float)((Globals.Instance.Seconds % 86400) / 86400.0);

        // 360 grados * progreso. Restamos 90 para que a las 12:00 esté arriba.
        float rotationAngle = (dayProgress * 360.0f) - 90.0f;
        if (Sun != null)
        {
            Sun.RotationDegrees = new Vector3(-rotationAngle, 0, 0);
            
            // Calculamos la intensidad normal por la hora
            float sunIntensity = Mathf.Clamp(Mathf.Sin(Mathf.DegToRad(rotationAngle)), 0, 1);
            
            // SI ESTÁ NUBLADO, multiplicamos por 0 para apagarlo, si no, usamos la energía base
            float cloudMultiplier = IsCloudy ? 0.0f : 1.0f;
            
            Sun.LightEnergy = sunIntensity * SunBaseEnergy * cloudMultiplier;
            Sun.Visible = Sun.LightEnergy > 0.05f; // Apagar si es casi 0
        }

        if (Moon != null)
        {
            // La luna al lado opuesto
            Moon.RotationDegrees = new Vector3(-(rotationAngle + 180.0f), 0, 0);
            
            float moonIntensity = Mathf.Clamp(Mathf.Sin(Mathf.DegToRad(rotationAngle + 180.0f)), 0, 1);
            float cloudMultiplier = IsCloudy ? 0.0f : 1.0f;
            
            Moon.LightEnergy = moonIntensity * MoonBaseEnergy * cloudMultiplier; // La luna debe ser más débil
            Moon.Visible = Moon.LightEnergy > 0;
        }
    }
}