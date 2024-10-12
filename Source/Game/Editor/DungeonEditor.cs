#if FLAX_EDITOR
using FlaxEditor.CustomEditors;
using FlaxEditor.CustomEditors.Editors;
using FlaxEngine;

namespace Game
{
	[CustomEditor(typeof(DungeonGenerator))]
	public class DungeonEditor : GenericEditor
	{
		// public override void Initialize(LayoutElementsContainer layout)
		// {

		// 	layout.Space(20);
		// 	var generateButton = layout.Button("Generate Dungeon", Color.Green);

		// 	// Use Values[] to access the script or value being edited.
		// 	// It is an array, because custom editors can edit multiple selected scripts simultaneously.
		// 	generateButton.Button.Clicked += () => (Values[0] as DungeonGenerator).GenerateDungeon();

		// 	var destroyButton = layout.Button("Destroy Dungeon", Color.Red);

		// 	destroyButton.Button.Clicked += () => (Values[0] as DungeonGenerator).DestroyDungeon();

		// }



	}
}
#endif