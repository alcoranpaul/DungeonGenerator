#if FLAX_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using FlaxEditor;
using FlaxEditor.CustomEditors;
using FlaxEditor.CustomEditors.Elements;
using FlaxEngine;

namespace DunGenEditor;

/// <summary>
/// DunGenWindow Script.
/// </summary>
public class DunGenWindow : CustomEditorWindow
{
	private TextBoxElement folderName;
	private const string SETTINGS_NAME = "DunGenSettings";
	public string SETTINGS_PATH_FOLDER = "Data";
	public string SettingsPath => Path.Combine(Globals.ProjectContentFolder + "/" + SETTINGS_PATH_FOLDER, SETTINGS_NAME + ".json");
	public override void Initialize(LayoutElementsContainer layout)
	{


		layout.Label("Dungeon Generation (DunGen)", TextAlignment.Center);
		layout.Space(20);
		var settingsGroup = layout.Group("Settings");

		var hp = layout.HorizontalPanel();
		hp.ContainerControl.Height = 20f;
		settingsGroup.AddElement(hp);

		var name = layout.Label($"Settings Folder:");
		hp.AddElement(name);
		folderName = layout.TextBox();
		folderName.Text = SETTINGS_PATH_FOLDER;
		hp.AddElement(folderName);

		layout.Space(20);
		var button = layout.Button("Click me", Color.Blue);
		button.Button.Clicked += OnButtonClicked;
	}


	private void OnButtonClicked()
	{


	}
}

#endif