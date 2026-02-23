using Godot;
using Godot.Collections;

[GlobalClass]
public partial class DataResource : Resource
{
	public DataResource Data;

	public DataResource()
	{
		Data = this;
	}

	public static string Path = "user://GlobalsData.tres";


	//Globals Settings
	[Export] public bool Vsync = false;
	[Export] public bool FPS = false;
	[Export] public int Antialiasing = 0;
	[Export] public int Antitropic = 0;
	[Export] public int Volumen = 1;
	[Export] public int VolumenMusic = 1;
	[Export] public int TimerDisasters = 60;
	[Export] public bool Fullscreen = false;
	[Export] public int Resolution = 0;
	[Export] public int Quality = 0;



	public void SaveFile()
	{
		ResourceSaver.Save(this, Path);

	}

	public static DataResource LoadFile()
	{
		DataResource data = ResourceLoader.Load(Path) as DataResource;
		if(data == null)
		{
			data = new DataResource();
		}

		return data;
	}




}