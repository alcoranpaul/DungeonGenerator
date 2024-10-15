#if FLAX_EDITOR

using System.IO;
using DunGen;
using FlaxEditor;
using FlaxEditor.Content;
using FlaxEditor.Content.Settings;
using FlaxEditor.GUI;
using FlaxEngine;
namespace DunGenEditor;

/// <summary>
/// DunGenEditor Script.
/// </summary>

public class DunGenEditor : EditorPlugin
{
	private CustomSettingsProxy assetProxy;
	private const string SETTINGS_NAME = "DunGenSettings";
	private const string SETTINGS_PATH_FOLDER = "/Data";
	public static string SettingsPath => Path.Combine(Globals.ProjectContentFolder + SETTINGS_PATH_FOLDER, SETTINGS_NAME + ".json");

	private ToolStripButton _button;
	/// <inheritdoc />
	public override void InitializeEditor()
	{
		base.InitializeEditor();
		_button = Editor.UI.ToolStrip.AddButton("DunGen");
		_button.Clicked += () => new DunGenWindow().Show(); ;


		// bool saveJson = Editor.SaveJsonAsset(SettingsPath, new DungeonGenSettings());
		// bool setsettings = GameSettings.SetCustomSettings(SETTINGS_NAME, Content.LoadAsync<JsonAsset>(SettingsPath));


		// Debug.Log($"DunGenEditor Initialize: JSON-{!saveJson} Settings-{!setsettings}");
		// assetProxy = new CustomSettingsProxy(typeof(RoomSettings), "Room Settings");
		// Editor.ContentDatabase.AddProxy(assetProxy);
		// Debug.Log("DunGenEditor Initialize");


	}

	public void Test()
	{
		Debug.Log($"{Content.GetAsset(SettingsPath)}");
	}

	/// <inheritdoc />
	public override void DeinitializeEditor()
	{
		if (_button != null)
		{
			_button.Dispose();
			_button = null;
		}
		// Editor.ContentDatabase.RemoveProxy(assetProxy);
		// Debug.Log("DunGenEditor Deinitialize");
		base.DeinitializeEditor();
	}
}
#endif