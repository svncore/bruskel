using System;
using Bruskel.Core;

namespace Bruskel.Avalonia.Input;

public sealed class BruskelViewSettings
{
	public const int DefaultGridSize = 32;
	public const int MinGridSize = 2;
	public const int MaxGridSize = 512;
	private readonly ISettingsService? settings;

	public BruskelViewSettings (ISettingsService? settings = null)
	{
		this.settings = settings;
		Load ();
	}

	public bool ShowGrid { get; set; }
	public int GridSize { get; set; } = DefaultGridSize;
	public bool ShowRulers { get; set; }
	public bool ShowToolbar { get; set; } = true;
	public bool ShowToolbox { get; set; } = true;
	public bool ShowStatusBar { get; set; } = true;
	public bool ShowToolWindows { get; set; } = true;
	public bool ShowColors { get; set; } = true;
	public bool ShowLayers { get; set; } = true;
	public bool ShowImageTabs { get; set; } = true;

	public void Save ()
	{
		if (settings is null) return;
		settings.PutSetting ("Bruskel.View.ShowGrid", ShowGrid);
		settings.PutSetting ("Bruskel.View.GridSize", Math.Clamp (GridSize, MinGridSize, MaxGridSize));
		settings.PutSetting ("Bruskel.View.ShowRulers", ShowRulers);
		settings.PutSetting ("Bruskel.View.ShowToolbar", ShowToolbar);
		settings.PutSetting ("Bruskel.View.ShowToolbox", ShowToolbox);
		settings.PutSetting ("Bruskel.View.ShowStatusBar", ShowStatusBar);
		settings.PutSetting ("Bruskel.View.ShowToolWindows", ShowToolWindows);
		settings.PutSetting ("Bruskel.View.ShowColors", ShowColors);
		settings.PutSetting ("Bruskel.View.ShowLayers", ShowLayers);
		settings.PutSetting ("Bruskel.View.ShowImageTabs", ShowImageTabs);
	}

	public void Load ()
	{
		if (settings is null) return;
		ShowGrid = settings.GetSetting ("Bruskel.View.ShowGrid", ShowGrid);
		GridSize = Math.Clamp (settings.GetSetting ("Bruskel.View.GridSize", GridSize), MinGridSize, MaxGridSize);
		ShowRulers = settings.GetSetting ("Bruskel.View.ShowRulers", ShowRulers);
		ShowToolbar = settings.GetSetting ("Bruskel.View.ShowToolbar", ShowToolbar);
		ShowToolbox = settings.GetSetting ("Bruskel.View.ShowToolbox", ShowToolbox);
		ShowStatusBar = settings.GetSetting ("Bruskel.View.ShowStatusBar", ShowStatusBar);
		ShowToolWindows = settings.GetSetting ("Bruskel.View.ShowToolWindows", ShowToolWindows);
		ShowColors = settings.GetSetting ("Bruskel.View.ShowColors", ShowColors);
		ShowLayers = settings.GetSetting ("Bruskel.View.ShowLayers", ShowLayers);
		ShowImageTabs = settings.GetSetting ("Bruskel.View.ShowImageTabs", ShowImageTabs);
	}
}
