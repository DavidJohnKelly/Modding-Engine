using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Godot;

namespace ModdingEngine.autoload
{

	/// <summary>
	/// ModManager singleton for scanning, loading, and managing mod overrides.
	/// Scans user://mods for .pck files, loads them, registers overrides, and
	/// provides a LoadGameResource method that respects mod priorities.
	/// </summary>
	public partial class ModManager : Node
	{
		public static ModManager Instance { get; private set; }

		private class ModMetaData
		{
			public required string Name;
			public required int Priority;
			public required Dictionary<string, string> Overrides;
		}

		// List of loaded mods, sorted by priority
		private List<ModMetaData> _mods = [];

		// Registry: original resource path -> list of (modId, priority, overridePath)
		private readonly Dictionary<string, List<(string name, int priority, string path)>> _overrideRegistry = [];

		public override void _Ready()
		{
			Instance ??= this;

			GD.Print("Entered Mod Manager");
			ScanAndLoadMods();
			RegisterAllOverrides();
			GD.Print("FINISHED");
		}

		/// <summary>
		/// Scans the user://mods directory for .pck files, loads each pack,
		/// and parses its mod.json to collect metadata and override mappings.
		/// </summary>
		private void ScanAndLoadMods()
		{
			string globalModsPath = ProjectSettings.GlobalizePath("user://mods");
			if (!Directory.Exists(globalModsPath))
			{
				Directory.CreateDirectory(globalModsPath);
			}

			foreach (string file in Directory.GetFiles(globalModsPath, "*.pck"))
			{
				// Load the pack
				if (!ProjectSettings.LoadResourcePack(Path.Combine("user://mods", Path.GetFileName(file))))
				{
					GD.PushError($"ModManager: Failed to load mod '{file}'");
					continue;
				}

				// Attempt to read the manifest from inside the pack
				string manifestPath = $"user://mods/{Path.GetFileNameWithoutExtension(file)}/mod.json";

				if (!File.Exists(manifestPath))
				{
					GD.PushWarning($"ModManager: mod.json not found for '{file}'");
					continue;
				}

				try
				{
					string jsonText = File.ReadAllText(manifestPath);
					Variant jsonParsed = Json.ParseString(jsonText);

					if (jsonParsed.VariantType != Variant.Type.Dictionary)
					{
						GD.PrintErr("ModManager: JSON root is not a Dictionary.");
						return;
					}

					var jsonData = jsonParsed.AsGodotDictionary();

					var meta = new ModMetaData
					{
						Name = jsonData["name"].AsString(),
						Priority = jsonData["priority"].AsInt16(),
						Overrides = new Dictionary<string, string>(
							jsonData["overrides"].AsGodotDictionary<string, string>()
								.ToDictionary(kv => kv.Key, kv => kv.Value)
						)
					};

					_mods.Add(meta);
				}
				catch (Exception e)
				{
					GD.PushError($"ModManager: Error parsing manifest for '{file}': {e}");
				}
			}

			// Sort mods by ascending priority so highest loads last
			_mods = [.. _mods.OrderBy(m => m.Priority)];
		}

		/// <summary>
		/// Registers all overrides into the override registry.
		/// Ensures each list is sorted so the highest priority override is last.
		/// </summary>
		private void RegisterAllOverrides()
		{
			foreach (ModMetaData mod in _mods)
			{
				foreach (KeyValuePair<string, string> kv in mod.Overrides)
				{
					if (!_overrideRegistry.ContainsKey(kv.Key))
						_overrideRegistry[kv.Key] = [];

					_overrideRegistry[kv.Key].Add((mod.Name, mod.Priority, kv.Value));
					_overrideRegistry[kv.Key] = [.. _overrideRegistry[kv.Key].OrderBy(tuple => tuple.priority)];
				}
			}
		}

		/// <summary>
		/// Loads a Resource, checking the override registry first.
		/// If any mod has overridden the given path, the highest priority override is used.
		/// </summary>
		/// <param name="path">Original resource path (e.g., "res://scenes/tree.tscn").</param>
		/// <returns>The loaded Resource, or null if loading failed.</returns>
		public Resource LoadGameResource(string path)
		{
			if (_overrideRegistry.TryGetValue(path, out var list) && list.Count > 0)
				path = list.Last().path;

			return ResourceLoader.Load(path);
		}
	}
}
