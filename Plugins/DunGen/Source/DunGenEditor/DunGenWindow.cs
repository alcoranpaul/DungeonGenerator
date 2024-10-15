#if FLAX_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using DunGen;
using FlaxEditor;
using FlaxEditor.CustomEditors;
using FlaxEditor.CustomEditors.Elements;
using FlaxEngine;
using FlaxEngine.GUI;

namespace DunGenEditor;

/// <summary>
/// DunGenWindow Script.
/// </summary>
public class DunGenWindow : CustomEditorWindow
{
	private DungeonGenSettings settings;
	private const string SETTINGS_NAME = "DunGenSettings";
	public string SETTINGS_PATH_FOLDER = "Data";
	public string SettingsPath => Path.Combine(Globals.ProjectContentFolder + "/" + SETTINGS_PATH_FOLDER, SETTINGS_NAME + ".json");

	public bool EnableDebugDraw = false;
	public override void Initialize(LayoutElementsContainer layout)
	{
		Debug.Log($"Dungeon Gen class is initialized: {Generator.Instance != null}");
		LoadData();

		layout.Label("Dungeon Generation (DunGen)", TextAlignment.Center);
		layout.Space(20);

		// Settings group - location of settings json data
		var settingsGroup = layout.VerticalPanel();

		var settingNameHP = layout.HorizontalPanel();
		settingNameHP.ContainerControl.Height = 20f;
		settingsGroup.AddElement(settingNameHP);

		settingNameHP.AddElement(CreateLabel(layout, $"Settings Name:"));
		settingNameHP.AddElement(CreateTextBox(layout, SETTINGS_NAME, textboxEnabled: false));

		var settingFolderHP = layout.HorizontalPanel();
		settingFolderHP.ContainerControl.Height = 20f;
		settingsGroup.AddElement(settingFolderHP);

		settingFolderHP.AddElement(CreateLabel(layout, $"Settings Folder:"));
		settingFolderHP.AddElement(CreateTextBox(layout, SETTINGS_PATH_FOLDER, tooltip: "The folder where the settings is located"));

		var enabbleDebugHP = layout.HorizontalPanel();
		enabbleDebugHP.ContainerControl.Height = 20f;
		settingsGroup.AddElement(enabbleDebugHP);
		enabbleDebugHP.AddElement(CreateLabel(layout, $"Settings Folder:"));
		var enableDebugBox = layout.Checkbox("Enable Debug Draw");
		enabbleDebugHP.AddElement(enableDebugBox);
		enableDebugBox.CheckBox.StateChanged += ToggleDebugDraw;



		// Buttons group - Load, Save, Generate
		layout.Space(20);
		var loadButton = layout.Button("Load", Color.DarkGray, $"Load settings from {SettingsPath}");
		loadButton.Button.TextColor = Color.Black;
		loadButton.Button.TextColorHighlighted = Color.Black;
		loadButton.Button.Bold = true;
		loadButton.Button.Clicked += LoadData;
		layout.Space(10);
		var saveButton = layout.Button("Open", Color.DarkGray, $"Open settings file @ {SettingsPath}");
		saveButton.Button.TextColor = Color.Black;
		saveButton.Button.TextColorHighlighted = Color.Black;
		saveButton.Button.Bold = true;
		saveButton.Button.Clicked += OpenData;
		layout.Space(10);

		var generateButton = layout.Button("Generate Dungeon", Color.DarkRed);
		generateButton.Button.Clicked += GenerateDungeon;

		var destroyButton = layout.Button("Destroy Dungeon", Color.DarkRed);
		destroyButton.Button.Clicked += DestroyDungeon;
	}

	private void ToggleDebugDraw(CheckBox box)
	{
		EnableDebugDraw = box.Checked;
		Debug.Log($"Debug Draw is {EnableDebugDraw}");
	}

	private TextBoxElement CreateTextBox(LayoutElementsContainer layout, string textValue, string tooltip = "", bool textboxEnabled = true)
	{
		var retVal = layout.TextBox();
		retVal.Text = textValue;
		retVal.TextBox.Enabled = textboxEnabled;
		if (!string.IsNullOrEmpty(tooltip))
			retVal.TextBox.TooltipText = tooltip;
		return retVal;
	}

	private LabelElement CreateLabel(LayoutElementsContainer layout, string name, string tooltip = "", bool textboxEnabled = true)
	{
		var retVal = layout.Label(name);
		retVal.Label.Enabled = textboxEnabled;
		if (!string.IsNullOrEmpty(tooltip))
			retVal.Label.TooltipText = tooltip;
		return retVal;
	}

	private void GenerateDungeon()
	{
		DebugDraw.UpdateContext(IntPtr.Zero, float.MaxValue);
		Generator generator = new Generator();
		generator.GenerateDungeon();

		if (!EnableDebugDraw)
			DebugDraw.UpdateContext(IntPtr.Zero, float.MaxValue);
	}

	private void DestroyDungeon()
	{
		DebugDraw.UpdateContext(IntPtr.Zero, float.MaxValue);
		Actor DungeonGenActor = Level.FindActor("DungeonGenActor");
		// If Actor has children, destroy them
		if (DungeonGenActor != null && DungeonGenActor.ChildrenCount > 0)
		{
			DungeonGenActor.DestroyChildren();
		}
	}


	private void LoadData()
	{
		var asset = Content.Load(SettingsPath);
		if (asset == null) Debug.LogWarning($"Failed to load settings @ {SettingsPath}");
		// TODO: Add auto create settings if a bool is enabled

		if (asset is not JsonAsset) Debug.LogWarning($"Settings @ {SettingsPath} is not a JsonAsset");

		JsonAsset json = asset as JsonAsset;
		settings = json.CreateInstance<DungeonGenSettings>();


	}

	private void OpenData()
	{
		var asset = Content.Load(SettingsPath);
		if (asset == null) Debug.LogWarning($"Failed to load settings @ {SettingsPath}");
		// TODO: Add auto create settings if a bool is enabled

		if (asset is not JsonAsset) Debug.LogWarning($"Settings @ {SettingsPath} is not a JsonAsset");
		Editor.Instance.ContentEditing.Open(asset);
	}


	private void SaveData()
	{


	}
}

#endif