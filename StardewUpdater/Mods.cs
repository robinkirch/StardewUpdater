using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StardewUpdater
{
    public class Mods
    {
        public string Name;

		public string Author;

		public string Version;

		public string latestVersion;

		public string Description;

		public string UniqueID;

		public string MinimumApiVersion;

		public List<Depedencies> Dependencies = new List<Depedencies>();

		public string[] UpdateKeys;
	}

	public class Depedencies
	{
		public string UniqueID;

		public bool IsRequired;
	}
}
