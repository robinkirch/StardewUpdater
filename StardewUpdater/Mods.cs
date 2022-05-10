using System;
using System.Collections.Generic;

namespace StardewUpdater
{
    public class Mods
    {
        public string Name { get; set; }

		public string Author { get; set; }

		public Version Version { get; set; }

		public Version LatestVersion { get; set; }

		public string Description { get; set; }

		public string UniqueID { get; set; }

		public Version MinimumApiVersion { get; set; }

		public List<Depedencies> Dependencies { get; set; } = new List<Depedencies>();

		public string[] UpdateKeys { get; set; }

		public static explicit operator Mods(UpgradeMods mod)
			=> new Mods
			{
				Name = mod.Name,
				Author = mod.Author,
				Version = mod.Version,
				LatestVersion = mod.LatestVersion,
				MinimumApiVersion = mod.MinimumApiVersion,
				Dependencies = mod.Dependencies,
				UpdateKeys = mod.UpdateKeys,
			};
	}

	public class Depedencies
	{
		public string UniqueID { get; set; }

		public bool IsRequired { get; set; } = true;
	}

	public class UpgradeMods
    {
		public string Name { get; set; }

		public string Author { get; set; }

		public Version Version { get; set; }

		public Version LatestVersion { get; set; }

		public string Description { get; set; }

		public string UniqueID { get; set; }

		public Version MinimumApiVersion { get; set; }

		public List<Depedencies> Dependencies { get; set; } = new List<Depedencies>();

		public string[] UpdateKeys { get; set; }

		public UpgradeType Type { get; set; } = UpgradeType.NotDefined;

		public static explicit operator UpgradeMods(Mods mod)
			=> new UpgradeMods
            {
				Name = mod.Name,
				Author = mod.Author,
				Version = mod.Version,
				LatestVersion = mod.LatestVersion,
				MinimumApiVersion = mod.MinimumApiVersion,
				Dependencies = mod.Dependencies,
				UpdateKeys = mod.UpdateKeys,
            };
	}

	public enum UpgradeType
	{
		NotDefined,
		Upgrade,
		Downgrade,
		New,
	}
}
