#if TOOLS
using Godot;
using System;

namespace ModdingEngine.addons.exporter
{
	[Tool]
	public partial class ModExporterPlugin : EditorPlugin
	{
		private Button _exportButton;
		private EditorFileDialog _folderDialog;
		private EditorFileDialog _saveDialog;
		private string _selectedModFolder = "res://mod";

		public override void _EnterTree()
		{
			// Button to start export flow
			_exportButton = new Button
			{
				Text = "Export Mod as .pck"
			};
			_exportButton.Pressed += OnExportButtonPressed;
			AddControlToContainer(CustomControlContainer.Toolbar, _exportButton);

			// Dialog for selecting folder
			_folderDialog = new EditorFileDialog
			{
				Title = "Select Mod Folder",
				FileMode = EditorFileDialog.FileModeEnum.OpenDir,
				Access = EditorFileDialog.AccessEnum.Resources,
				CurrentDir = "res://"
			};
			_folderDialog.DirSelected += OnFolderSelected;
			EditorInterface.Singleton.GetBaseControl().AddChild(_folderDialog);

			// Dialog for saving PCK file
			_saveDialog = new EditorFileDialog
			{
				Title = "Save Mod .pck As…",
				FileMode = EditorFileDialog.FileModeEnum.SaveFile,
				Access = EditorFileDialog.AccessEnum.Resources,
				CurrentDir = "res://",
				CurrentFile = "mod_export.pck"
			};
			_saveDialog.AddFilter("*.pck", "PCK Files");
			_saveDialog.FileSelected += OnSavePathSelected;
			EditorInterface.Singleton.GetBaseControl().AddChild(_saveDialog);
		}

		public override void _ExitTree()
		{
			RemoveControlFromContainer(CustomControlContainer.Toolbar, _exportButton);
			_exportButton.QueueFree();
			_folderDialog.QueueFree();
			_saveDialog.QueueFree();
		}

		private void OnExportButtonPressed()
		{
			// First, choose the mod folder
			_folderDialog.PopupCentered(new Vector2I(400, 300));
		}

		private void OnFolderSelected(string path)
		{
			_selectedModFolder = path;
			GD.Print($"Mod folder selected: {_selectedModFolder}");

			// After folder selection, open save dialog if folder exists
			if (DirectoryExists(_selectedModFolder))
			{
				_saveDialog.PopupCentered(new Vector2I(600, 400));
			}
			else
			{
				GD.PrintErr($"Selected mod folder does not exist: {_selectedModFolder}");
			}
		}

		private void OnSavePathSelected(string savePath)
		{
			GD.Print($"Exporting folder '{_selectedModFolder}' to: {savePath}");
			ModExporter.ExportMod(_selectedModFolder, savePath);
		}

		private bool DirectoryExists(string resPath)
		{
			var dir = DirAccess.Open(resPath);
			return dir != null;
		}
	}
}
#endif
