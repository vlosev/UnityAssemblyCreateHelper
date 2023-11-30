using System;
using UnityEngine.Serialization;

namespace Editor.Utils.AssemblyCreateHelper
{
	[Serializable]
	internal class AssemblyDefinitionSchema
	{
		public string name;
		public string[] references;

		public AssemblyDefinitionSchema(string name, string[] references)
		{
			this.name = name;
			this.references = references;
		}
	}
}