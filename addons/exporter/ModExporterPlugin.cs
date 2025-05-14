#if TOOLS
using Godot;
using System;

namespace ModdingEngine.addons.exporter
{
	[Tool]
	public partial class ModExporterPlugin : EditorPlugin
	{
		private Button _exportButton;
		private EditorFileDialog _saveDialogue;

		public override void _EnterTree()
		{
			_exportButton = new Button
			{
				Text = "Export Mod as .pck"
			};
			_exportButton.Pressed += OnExportButtonPressed;
			AddControlToContainer(CustomControlContainer.Toolbar, _exportButton);

			_saveDialogue = new EditorFileDialog
			{
				Title = "Save Mod .pck As…",
				FileMode = EditorFileDialog.FileModeEnum.SaveFile,
				Access = EditorFileDialog.AccessEnum.Resources,
				CurrentDir = "res://",
				CurrentFile = "mod_export.pck"
			};

			_saveDialogue.AddFilter("*.pck", "PCK Files");
			_saveDialogue.FileSelected += OnFileSelected;
			EditorInterface.Singleton.GetBaseControl().AddChild(_saveDialogue);
		}

		public override void _ExitTree()
		{
			RemoveControlFromContainer(CustomControlContainer.Toolbar, _exportButton);
			_exportButton.QueueFree();
			_saveDialogue.QueueFree();
		}

		private void OnExportButtonPressed()
		{
			_saveDialogue.PopupCentered(new Vector2I(600, 400));
		}


		private static void OnFileSelected(string path)
		{
			GD.Print($"Exporting to: {path}");
			ModExporter.ExportMod(path);
		}
	}
}
#endif
