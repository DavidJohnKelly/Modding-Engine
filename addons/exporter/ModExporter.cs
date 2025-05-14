using Godot;

namespace ModdingEngine.addons.exporter
{
	public static partial class ModExporter
	{
		public static void ExportMod(string outputPath)
		{
			var packer = new PckPacker();
			Error err = packer.PckStart(outputPath);
			if (err != Error.Ok)
			{
				GD.PrintErr($"Failed to start PCK packer: {err}");
				return;
			}

			AddDirectoryToPck(packer, "res://mod", "mod");

			err = packer.Flush();
			if (err != Error.Ok)
			{
				GD.PrintErr($"Failed to flush PCK packer: {err}");
				return;
			}

			GD.Print($"Mod exported successfully to {outputPath}");
		}

		private static void AddDirectoryToPck(PckPacker packer, string dirPath, string pckPath)
		{
			var dir = DirAccess.Open(dirPath);
			if (dir == null)
			{
				GD.PrintErr($"Cannot open directory: {dirPath}");
				return;
			}

			dir.ListDirBegin();
			string fileName = dir.GetNext();
			while (!string.IsNullOrEmpty(fileName))
			{
				if (fileName == "." || fileName == "..")
				{
					fileName = dir.GetNext();
					continue;
				}

				string fullPath = $"{dirPath}/{fileName}";
				string fullPckPath = $"{pckPath}/{fileName}";

				if (dir.CurrentIsDir())
				{
					AddDirectoryToPck(packer, fullPath, fullPckPath);
				}
				else
				{
					GD.Print($"added file: {fullPath} to: {fullPckPath}", fullPath, fullPckPath);
					packer.AddFile($"res://{fullPckPath}", fullPath);
				}

				fileName = dir.GetNext();
			}
			dir.ListDirEnd();
		}
	}
}
