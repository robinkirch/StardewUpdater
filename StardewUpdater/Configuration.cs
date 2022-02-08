using System;
using System.Collections.Generic;

namespace StardewUpdater
{
    [Serializable]
    public class Configuration
    {
        public string installationFolder;

        public List<string> knownSteamFolders = new List<string>();

        public List<string> knownGoGFolders = new List<string>();

        public List<Mods> installedMods = new List<Mods>();

        public List<Mods> unknownInstalledMods = new List<Mods>();

        public bool isSMAPIInstalled;

        public string SMAPIVersion;

        public string latestSMAPIVersion;
    }
}
