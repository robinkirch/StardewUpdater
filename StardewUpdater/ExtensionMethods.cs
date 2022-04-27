using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StardewUpdater
{
    public static class ExtensionMethods
    {
        private static readonly string appsettings = $@"{Application.StartupPath}\appsettings.json";

        /// <summary> 
        /// Converts version number to a three-digit version number.
        /// </summary>
        /// <returns>Returns either the version number without revisions or null</returns> 
        public static Version VersionWithoutRevisions(this Version version)
        {
            if (version == null)
                return null;

            string[] versionArray = version.ToString().Split('.');
            if (versionArray.Count() < 3)
                return null;

            return new Version(versionArray[0] + "." + versionArray[1] + "." + versionArray[2]);
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

        public static string ReadFromAppSettings(string key)
        {
            using (StreamReader r = new StreamReader(appsettings))
            {
                string json = r.ReadToEnd();
                var data = (JObject)JsonConvert.DeserializeObject(json);
                string value = data[key].Value<string>();

                if(value == null)
                {
                    throw new Exception($"No key like '{key}' in appsettings.json was found.");
                }
                return value;
            }
        }

        public static List<string> KeysToList(this JToken jToken)
        {
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
                return tokenList;
            }
            catch(Exception ex)
            {
                return new List<string>() { ex.ToString() };
            }
        }
    }
}
