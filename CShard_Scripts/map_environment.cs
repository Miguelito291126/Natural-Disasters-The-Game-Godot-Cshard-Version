using Godot;
using Godot.Collections;

[GlobalClass]
public partial class MapEnvironment : WorldEnvironment
{
    public Node3D Sun;
    public Node3D Moon;

    public int MinutesPerDay = 1440;
    public int MinutesPerHour = 60;
    public double IngameToRealMinuteDuration => (2.0 * Mathf.Pi) / MinutesPerDay;
    public Variant SunNode;
    // Referencia al nodo del sol
    public int CelestialSpeedPerHour = 15; // grados por hora
    public double SunAngle = -90.0; // ángulo inicial del sol
    public double MoonAngle = 90.0;
    public double InterpolationSpeed = 1.0;

    [Export] public int IngameSpeed = 1;
    [Export] public int InitialHour { get; set; } = 12;

    // Tiempo interno (en "real minutes" usados por la conversión)
    private double Time = 0.0;

    public double PastMinute = -1.0;

    // Valores calculados
    public int TotalMinutes { get; private set; }
    public int CurrentDayMinutes { get; private set; }
    public int Day { get; private set; }
    public int Hour { get; private set; }
    public int Minute { get; private set; }
    public double TimeOfDay { get; private set; }

    public override void _Ready()
    {
        Sun = GetNode<Node3D>("Sun");
        Moon = GetNode<Node3D>("Moon");

        // Inicializar tiempo según la hora inicial
        Time = IngameToRealMinuteDuration * InitialHour * MinutesPerHour;
        _RecalculateTime(0.0);
    }

    public override void _Process(double delta)
    {
        // Avanza el tiempo
        Time += delta * IngameToRealMinuteDuration * IngameSpeed;
        _RecalculateTime(delta);
    }

    protected void _RecalculateTime(double delta)
    {
        TotalMinutes = (int)(Time / IngameToRealMinuteDuration);
        Day = TotalMinutes / MinutesPerDay;
        CurrentDayMinutes = TotalMinutes % MinutesPerDay;
        Hour = CurrentDayMinutes / MinutesPerHour;
        Minute = CurrentDayMinutes % MinutesPerHour;

        if (Minute != PastMinute)
        {
            PastMinute = Minute;
            // Aquí puedes poner lógica que deba ejecutarse al cambiar el minuto
        }

        TimeOfDay = Hour + Minute / 60.0;

        // Calcular ángulos objetivo
        SunAngle = -90.0 + (TimeOfDay * CelestialSpeedPerHour);
        MoonAngle = SunAngle + 180.0;

        // Normalizar [0,360)
        if (SunAngle < 0.0) SunAngle += 360.0;
        SunAngle = SunAngle % 360.0;
        if (MoonAngle < 0.0) MoonAngle += 360.0;
        MoonAngle = MoonAngle % 360.0;

        // Interpolación
        float t = Mathf.Clamp((float)(InterpolationSpeed * delta), 0f, 1f);

        if (Sun != null)
        {
            var sRot = Sun.RotationDegrees;
            float targetSunX = Mathf.RadToDeg(Mathf.LerpAngle(Mathf.DegToRad(sRot.X), Mathf.DegToRad((float)SunAngle), t));
            Sun.RotationDegrees = new Vector3(targetSunX, sRot.Y, sRot.Z);
        }

        if (Moon != null)
        {
            var mRot = Moon.RotationDegrees;
            float targetMoonX = Mathf.RadToDeg(Mathf.LerpAngle(Mathf.DegToRad(mRot.X), Mathf.DegToRad((float)MoonAngle), t));
            Moon.RotationDegrees = new Vector3(targetMoonX, mRot.Y, mRot.Z);
        }
    }
}