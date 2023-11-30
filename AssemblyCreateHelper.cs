using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Editor.Utils.AssemblyCreateHelper
{
	internal class AssemblyCreateHelper : EditorWindow
	{
		private const string ASSETS_PATH = "Assets/";
		
		[SerializeField]
		private AssemblyCreateConfig _config;

		private string _currentPath;
		private readonly Dictionary<AssemblyCreateConfig.ConfigItem, bool> _enabledAssemblies = new();
		private readonly Dictionary<string, AssemblyCreateConfig.ConfigItem> _nameToAssembly = new();

		private void Init(string path)
		{
			_currentPath = path;

			foreach (var item in _config.Items)
			{
				_enabledAssemblies[item] = item.DefaultAssembly;
				_nameToAssembly[item.Name] = item;
			}
		}

		private void OnGUI()
		{
			foreach (var item in _config.Items)
			{
				if (!_enabledAssemblies.ContainsKey(item))
					continue;
				
				_enabledAssemblies[item] = GUILayout.Toggle(_enabledAssemblies[item], item.Name);
			}

			if (GUILayout.Button("create"))
			{
				var assemblies = _enabledAssemblies.Where(p => p.Value).Select(p => p.Key).ToArray();
				foreach (var assemblyDescription in assemblies)
				{
					var relativePath = _currentPath;
					if (relativePath.StartsWith(ASSETS_PATH))
					{
						relativePath = relativePath[ASSETS_PATH.Length..];
					}

					var assemblyName = assemblyDescription.Name;
					var assemblyNamePrefix = relativePath.Replace(Path.DirectorySeparatorChar, '.');
					var assemblyFullName = $"{assemblyNamePrefix}.{assemblyName}";

					var assemblyFolder = Path.Combine(_currentPath, assemblyName);
					var asmDefPath = Path.Combine(assemblyFolder, assemblyFullName);

					var dependencies = new List<string>();
					foreach (var dependencyName in assemblyDescription.Dependencies)
					{
						if (dependencies.Contains(dependencyName))
							continue;

						if (_nameToAssembly.TryGetValue(dependencyName, out var dependencyAssembly))
						{
							if (_enabledAssemblies.TryGetValue(dependencyAssembly, out var enabled) && enabled)
								dependencies.Add($"{assemblyNamePrefix}.{dependencyName}");
						}
						else
						{
							dependencies.Add(dependencyName);
						}
					}
					
					var asmDefCode = CreateAsmDefFile(assemblyFullName, dependencies);
				
					Directory.CreateDirectory(assemblyFolder);

					if (Application.platform == RuntimePlatform.WindowsEditor)
						asmDefPath = asmDefPath.Replace('/', '\\');
					
					File.WriteAllText($"{asmDefPath}.asmdef", asmDefCode);
					
					AssetDatabase.Refresh();
				}
			}
		}

		[MenuItem("Assets/Create/Module AsmDefs", priority = 3)]
		public static void OpenWizard()
		{
			var obj = Selection.activeObject;
			var path = (obj == null) ? "Assets" : AssetDatabase.GetAssetPath(obj.GetInstanceID());

			var result = TryOpenWizard(path); 
			if (!result.Successful)
			{
				Debug.Log($"failed: {result.Error}");
				return;
			}
			
			var window = GetWindow<AssemblyCreateHelper>();
			window.Init(path);
			window.Show();
		}

		private static Result TryOpenWizard(string path)
		{
			if (path.Length > 0 && Directory.Exists(path))
			{
				var fullPath = Path.GetFullPath(path);
				if (fullPath == Application.dataPath)
					return Result.Failed("can't create module in Assets root folder");
			}
			else
			{
				return Result.Failed("Not in assets folder");
			}
			
			return Result.Success();
		}

		private static string CreateAsmDefFile(string name, IEnumerable<string> dependencies)
		{
			return JsonUtility.ToJson(new AssemblyDefinitionSchema(name, dependencies.ToArray()), true);
		}
	}
}
