using System;
using System.Collections.Generic;

namespace StardewUpdater
{
    public class Mods
    {
        public string Name { get; set; }

		public string Author { get; set; }

		public Version Version { get; set; }

		public Version latestVersion { get; set; }

		public string Description { get; set; }

		public string UniqueID { get; set; }

		public Version MinimumApiVersion { get; set; }

		public List<Depedencies> Dependencies { get; set; } = new List<Depedencies>();

		public string[] UpdateKeys { get; set; }
	}

	public class Depedencies
	{
		public string UniqueID { get; set; }

		public bool IsRequired { get; set; } = true;
	}
}
