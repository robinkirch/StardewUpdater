using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
