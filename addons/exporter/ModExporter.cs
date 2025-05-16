using Godot;
using System;
using System.IO;
using Godot.Collections;

namespace ModdingEngine.addons.exporter
{
	public static partial class ModExporter
	{
		/// <summary>
		/// Exports the specified mod folder into a .pck file at the given output path.
		/// </summary>
		/// <param name="sourceFolder">Godot resource path to the mod folder, e.g. "res://MyMod".</param>
		/// <param name="outputPath">Filesystem or resource path where the .pck will be saved, e.g. "res://MyMod.pck".</param>
		public static void ExportMod(string sourceFolder, string outputPath)
		{
			// Derive folder name from the source folder (e.g. "MyMod")
			string modName = GetModName(outputPath);

			var packer = new PckPacker();
			Error startErr = packer.PckStart(outputPath);
			if (startErr != Error.Ok)
			{
				GD.PrintErr($"Failed to start PCK packer for '{outputPath}': {startErr}");
				return;
			}

			// Recursively add the directory under its folder name in the pack
			AddDirectoryToPck(packer, sourceFolder, modName);

			Error flushErr = packer.Flush();
			if (flushErr != Error.Ok)
			{
				GD.PrintErr($"Failed to flush PCK packer for '{outputPath}': {flushErr}");
				return;
			}

			GD.Print($"Mod '{modName}' exported successfully to '{outputPath}'");
		}

		/// <summary>
		/// Gets the folder name from a Godot resource path.
		/// Converts "res://SomeFolder" to the actual directory name "SomeFolder".
		/// </summary>
		private static string GetModName(string resPath)
		{
			string systemPath = ProjectSettings.GlobalizePath(resPath);
			string pathName = new DirectoryInfo(systemPath).Name;
			string modName = Path.GetFileNameWithoutExtension(pathName);
			return modName;
		}

		/// <summary>
		/// Adds all files and subdirectories from dirPath into the PCK under pckRoot.
		/// </summary>
		private static void AddDirectoryToPck(PckPacker packer, string dirPath, string pckRoot)
		{
			var dir = DirAccess.Open(dirPath);
			if (dir == null)
			{
				GD.PrintErr($"Cannot open mod directory: '{dirPath}'");
				return;
			}

			dir.ListDirBegin();
			string entry = dir.GetNext();
			while (!string.IsNullOrEmpty(entry))
			{
				if (entry == "." || entry == "..")
				{
					entry = dir.GetNext();
					continue;
				}

				string fullResPath = $"{dirPath}/{entry}";
				string packPath = $"{pckRoot}/{entry}";

				if (dir.CurrentIsDir())
				{
					AddDirectoryToPck(packer, fullResPath, packPath);
				}
				else
				{
					if (Godot.FileAccess.FileExists(fullResPath))
					{
						Error addErr = packer.AddFile($"res://{packPath}", fullResPath);
						if (addErr != Error.Ok)
							GD.PrintErr($"Failed to add file '{fullResPath}' to pack: {addErr}");
					}
					else
					{
						GD.PrintErr($"File '{fullResPath}' does not exist, skipping.");
					}
				}

				entry = dir.GetNext();
			}
			dir.ListDirEnd();
		}
	}
}
