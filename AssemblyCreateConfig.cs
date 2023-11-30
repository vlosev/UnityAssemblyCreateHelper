using System;
using UnityEngine;

namespace Editor.Utils.AssemblyCreateHelper
{
	[CreateAssetMenu(fileName = nameof(AssemblyCreateConfig), menuName = "Tools/Settings/" + nameof(AssemblyCreateConfig), order = 500)]
	internal class AssemblyCreateConfig : ScriptableObject
	{
		[Serializable]
		public class ConfigItem
		{
			[field: SerializeField]
			public string Name { get; private set; }
			[field: SerializeField]
			public string[] Dependencies { get; private set; }
			[field: SerializeField]
			public bool DefaultAssembly { get; private set; } = false;
		}
		
		[field: SerializeField]
		public ConfigItem[] Items { get; private set; }
	}
}