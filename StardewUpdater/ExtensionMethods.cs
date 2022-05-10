using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StardewUpdater
{
    public enum VersionCompareTypes
    {
        Unknown,
        Newer,
        Older,
        Equal,
    }

    public static class ExtensionMethods
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private static readonly string appsettings = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "StardewUpdater", "appsettings.json");

        /// <summary> 
        /// Converts version number to a three-digit version number.
        /// </summary>
        /// <returns>Returns either the version number without revisions or null</returns> 
        public static Version VersionWithoutRevisions(this Version version)
        {
            _logger.Trace($"Clear Versions: {version.ToString()}");
            if (version == null)
                return null;

            string[] versionArray = version.ToString().Split('.');
            if (versionArray.Count() < 3)
                return null;

            return new Version(versionArray[0] + "." + versionArray[1] + "." + versionArray[2]);
        }

        /// <summary> 
        /// Replaces all revisionnumbers with value -1 in a text literal
        /// </summary>
        /// <param name="replacement">Optional parameter setting the replacemnet value. Default: "0"</param>
        /// <returns>Returns an string without unused ( value -1) revisionnumbers</returns> 
        public static string ReplaceUnusedVersionRevisions(this string textWithVersions, string replacement = "0")
        {
            _logger.Trace($"Remove unused Revisions and build numbers");
            _logger.Trace($"Replacement: {replacement}");
            _logger.Trace($"Text: {textWithVersions}");

            textWithVersions = textWithVersions.Replace("\"Build\": -1", $"\"Build\": {replacement}");
            textWithVersions = textWithVersions.Replace("\"Revision\": -1", $"\"Revision\": {replacement}");
            textWithVersions = textWithVersions.Replace("\"MajorRevision\": -1", $"\"MajorRevision\": {replacement}");
            textWithVersions = textWithVersions.Replace("\"MinorRevision\": -1", $"\"MinorRevision\": {replacement}");
            return textWithVersions;
        }

        /// <summary> 
        /// Removes all spaces from a string
        /// </summary>
        /// <returns>Returns the string without whitespaces</returns> 
        public static string SkipWhitespaces(this string str) => str.Replace(" ", "");

        /// <summary> 
        /// Checks the mod for errors and removes them against serializable propertys
        /// </summary>
        /// <param name="allowNullable">If set, the return will be null if there is an error</param>
        /// <returns>Returns a serializable mod. Propertys may changed. Returns null if it has an error and allowNullable is set</returns> 
        public static Mods ValidateModToBeSerializable(this Mods mod, bool allowNullable = true)
        {
            //TODO: Implement
            return null;
        }

        /// <summary> 
        /// Checks a list of mods for errors and removes them against serializable propertys
        /// </summary>
        /// <param name="allowNullables">If set, the return will contain null if there is an error</param>
        /// <returns>Returns a serializable list of mods. Propertys may changed. Returns null for an mod if it has an error and allowNullable is set</returns> 
        public static List<Mods> ValidateModListToBeSerializable(this List<Mods> mod, bool allowNullables = true)
        {
            //TODO: Implement
            return null;
        }

        /// <summary> 
        /// Reads a value to the matching keys from the manually added appsettings.json
        /// </summary>
        /// <param name="key">Name of the key in the file</param>
        /// <returns>Returns a value or throws an exception if key is not found</returns> 
        public static string ReadFromAppSettings(string key)
        {
            _logger.Trace($"Receive value for key: {key}");
            using (StreamReader r = new StreamReader(appsettings))
            {
                string json = r.ReadToEnd();
                var data = (JObject)JsonConvert.DeserializeObject(json);
                string value = data[key].Value<string>();

                if(value == null)
                {
                    _logger.Error($"Key {key} not found");
                    throw new Exception($"No key like '{key}' in appsettings.json was found.");
                }
                return value;
            }
        }

        /// <summary> 
        /// Goes to the lowest level of a JToken list and returns the keys in a list
        /// </summary>
        /// <param name="jToken">A list of JTokens whose depth does not matter</param>
        /// <returns>Returns a list of keys of type string.  Returns null if the list is not JToken compliant</returns> 
        public static List<string> KeysToList(this JToken jToken)
        {
            _logger.Trace($"Going through JToken");
            
            try
            {
                JToken tempjToken = jToken;
                while (tempjToken.HasValues)
                {
                    tempjToken = tempjToken.First();
                    if (tempjToken.HasValues)
                    {
                        jToken = jToken.First();
                    }
                }

                List<string> tokenList = new List<string>();
                foreach (JToken token in jToken.Parent.Children())
                {

                    string name = token.ToString().Substring(0,token.ToString().IndexOf(":"));
                    name = name.Trim().Replace("\"", "");
                    tokenList.Add(name);
                }
                _logger.Debug($"{string.Join(",", tokenList)}");
                return tokenList;
            }
            catch(Exception ex)
            {
                _logger.Trace($"Can't go through JToken: {ex}");
                return new List<string>() { ex.ToString() };
            }
        }

        /// <summary> 
        /// Compares the version of two mods and returns if the first is newer, older or equal. 
        /// </summary>
        /// <param name="firstMod">The first Mod to be compared</param>
        /// <param name="secondMod">The second Mod to be compared</param>
        /// <returns>Returns if the first mod is newer or older. Returns type Unknown if the mods dont have the same author or name</returns> 
        public static VersionCompareTypes CompareModVersion(Mods firstMod, Mods secondMod)
        {
            _logger.Debug($"Compare Versions : {firstMod.Name}({firstMod.UniqueID}) vs {secondMod.Name}({secondMod.UniqueID})");
            if (firstMod.Author == secondMod.Author && firstMod.Name == secondMod.Name)
            {
                if(firstMod.Version.VersionWithoutRevisions() == secondMod.Version.VersionWithoutRevisions())
                    return VersionCompareTypes.Equal;

                if (firstMod.Version.VersionWithoutRevisions() > secondMod.Version.VersionWithoutRevisions())
                    return VersionCompareTypes.Newer;

                if (firstMod.Version.VersionWithoutRevisions() < secondMod.Version.VersionWithoutRevisions())
                    return VersionCompareTypes.Older;
            }

            _logger.Error($"No Compare possible");
            return VersionCompareTypes.Unknown;
        }

        /// <summary> 
        /// Deletes a Directory and Wait for the process to end. 
        /// https://stackoverflow.com/questions/9370012/waiting-for-system-to-delete-file
        /// </summary>
        /// <param name="filepath">Path of the directory to be deleted</param>
        /// <param name="timeout">Optional parameter to limit waiting time</param>
        public static void DeleteFileAndWait(string filepath, int timeout = 3000)
        {
            _logger.Trace($"DeleteAndWait on: {filepath}");
            using (var fw = new FileSystemWatcher(Path.GetDirectoryName(filepath), Path.GetFileName(filepath)))
            using (var mre = new ManualResetEventSlim())
            {
                fw.EnableRaisingEvents = true;
                fw.Deleted += (object sender, FileSystemEventArgs e) =>
                {
                    mre.Set();
                };
                File.Delete(filepath);
                mre.Wait(timeout);
            }
        }
    }
}
