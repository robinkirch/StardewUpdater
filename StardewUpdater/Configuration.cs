using System;
using System.Collections.Generic;

namespace StardewUpdater
{
    [Serializable]
    public class Configuration
    {
        public string installationFolder { get; set; }

        public List<string> knownSteamFolders { get; set; } = new List<string>();

        public List<string> knownGoGFolders { get; set; } = new List<string>();

        public List<Mods> installedMods { get; set; } = new List<Mods>();

        public List<Mods> unknownInstalledMods { get; set; } = new List<Mods>();

        public bool isSMAPIInstalled { get; set; }

        public Version SMAPIVersion { get; set; }

        public Version latestSMAPIVersion { get; set; }
    }
}
