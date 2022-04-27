using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Net;
using Newtonsoft.Json.Linq;
using SevenZipExtractor;
using Ionic.Zip;
using System.Threading.Tasks;
using System.Drawing;
using System.Text.Json;
using System.Configuration;
using Gameloop.Vdf;
using Gameloop.Vdf.Linq;
using Gameloop.Vdf.JsonConverter;

namespace StardewUpdater
{
    public partial class Form1 : Form
    {
        private bool hasConfig;
        private readonly string _configfile;
        private Configuration configuration;
        public Dictionary<Action, string> functions = new Dictionary<Action, string>();
        //gets true when a backgroundoperation fails
        private bool asnycHasFailed = false;
        private readonly string _apikey = "Q3FPNUUvVCtzWHltWFdKTDZhTklMRUdPOGRLUGlqaGh4dXhWd2laWXFZZ1RwREcxaDV1UlRzQ01SQk1LTnM5Ny0tMmNubVFvQnNYN3Rib0FWai9ucFN4QT09--119ab7b53305b8b1678b0007a3b28955f402e729";
        //private readonly string _apikey = "XXXXXXXXXX";
        private readonly string _gameName = "Stardew Valley";
        private readonly string _steamGameId = "413150";
        //TODO: Loose coupling with Updater
        //TODO: Better Form and Button Names

        public Form1()
        {
            InitializeComponent();
            _configfile = $@"{Application.StartupPath}\configSVU.json";
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            button1.Enabled = false;
            button2.Enabled = false;
            button3.Enabled = false;
            button4.Enabled = false;
            button5.Enabled = false;

            hasConfig = CheckConfigurationFile();
            if (hasConfig)
            {
                functions.Add(getLatestSmapiVersion, "Searching for latest SMAPI Version......");
                functions.Add(getLatestModVersions, "Searching for Mod Versions......");
                functions.Add(setConfiguration, "Savin Progress...");
                functions.Add(activateUI, "Making a splendid UI...");
            }
            else
            {
                functions.Add(getInstallationFolder, "Searching for Installations...");
                functions.Add(getSMAPIInstallation, "Searching for SMAPI Installation...");
                functions.Add(getAllMods, "Collecting Mod Data...");
                functions.Add(getLatestSmapiVersion, "Searching for latest SMAPI Version......");
                functions.Add(getLatestModVersions, "Searching for Mod Versions......");
                functions.Add(setConfiguration, "Savin Progress...");
                functions.Add(activateUI, "Making a splendid UI...");
            }

            backgroundWorker2.WorkerSupportsCancellation = true; // does it really run aync ?
            backgroundWorker1.RunWorkerAsync();
        }

        //just call it by backgroundworker
        private void UpdateInformationService(string currentProcess, float percentage)
        {
            //title
            if (this.label1.InvokeRequired)
                this.label1.BeginInvoke((MethodInvoker)delegate () { this.label1.Text = currentProcess; });
            else
                this.label1.Text = currentProcess;

            //percentage
            if (this.label2.InvokeRequired)
                this.label2.BeginInvoke((MethodInvoker)delegate () { this.label2.Text = ((percentage/functions.Count)*100).ToString("00.##") + " %"; });
            else
                this.label2.Text = ((percentage / functions.Count) * 100).ToString("00.##") + " %";
        }

        private bool CheckConfigurationFile()
        {
            if (File.Exists(_configfile))
            {
                using (StreamReader r = new StreamReader(_configfile))
                {
                    //TODO: find a better solutions to this Version mess
                    string json = r.ReadToEnd();
                    //Replace cause they set -1 when not set and json cant deal with it
                    json = json.Replace("Revision\": -1", "Revision\": 0");
                    json = json.Replace("MajorRevision\": -1", "MajorRevision\": 0");
                    json = json.Replace("MinorRevision\": -1", "MinorRevision\": 0");
                    json = json.Replace("Build\": -1", "Build\": 0");
                    configuration = JsonConvert.DeserializeObject<Configuration>(json);

                    //CleanUp so no .0 for all Versions
                    configuration.SMAPIVersion = configuration.SMAPIVersion.VersionWithoutRevisions();
                    configuration.latestSMAPIVersion = configuration.latestSMAPIVersion.VersionWithoutRevisions();
                    foreach(Mods mod in configuration.installedMods)
                    {
                        mod.Version = mod.Version.VersionWithoutRevisions();
                        mod.latestVersion = mod.latestVersion.VersionWithoutRevisions();
                        mod.MinimumApiVersion = mod.MinimumApiVersion.VersionWithoutRevisions();

                    }

                }
                return true;
            }

            configuration = new Configuration();
            return false;
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            this.panel1.BeginInvoke((MethodInvoker)delegate () { this.panel1.Visible = true; });

            int pos = 1;
            foreach (var function in functions)
            {
                UpdateInformationService(function.Value, pos);
                function.Key.Invoke();
                pos++;
            }

            panel1.Invoke(new MethodInvoker(delegate { this.panel1.Visible = false; }));
        }

        //Find Installfolder
        private void getInstallationFolder()
        {
            //Steam
            JProperty jProperty;
            string steampath = string.Empty;
            using (var reader = new StreamReader(@"C:\Program Files (x86)\Steam\steamapps\libraryfolders.vdf"))
            {
                VProperty volvos = VdfConvert.Deserialize(reader.ReadToEnd());
                jProperty = volvos.ToJson();
            }
            var listofPaths = jProperty.First().Where(j => j.Path != "libraryfolders.contentstatsid");
            foreach (var path in listofPaths)
            {
                var appIds = path.First().Last().KeysToList();

                if (appIds.Contains(_steamGameId))
                {
                    steampath = path.First().First().First().ToString();
                    steampath = steampath.Trim().Replace("\"", "");
                    configuration.installationFolder = steampath + $@"\steamapps\common\{_gameName}";
                    break;
                }
            }

            //fallback if file is empty
            if (string.IsNullOrEmpty(steampath))
            {

                DriveInfo[] allDrives = DriveInfo.GetDrives();
                //First Steam librarys %steam% -> steamapps -> common
                foreach (DriveInfo d in allDrives)
                {
                    if (d.IsReady)
                    {
                        //Guessing it is the main drive
                        if (d.Name == "C:\\")
                        {
                            //Search through Program Files and Program Files (x86)
                            string[] cdirs = Directory.GetDirectories(d.Name + "Program Files", "*steam*");
                            foreach (string dir in cdirs)
                            {
                                if (dir == "steamapps")
                                {
                                    configuration.knownSteamFolders.Add(dir);
                                    //hit and search in it
                                    if (Directory.Exists(dir + $@"\common\{_gameName}"))
                                    {
                                        configuration.installationFolder = dir + $@"\common\{_gameName}";
                                        break;
                                    }
                                }
                                else
                                {
                                    string innerDirs = Directory.GetDirectories(dir, "steamapps").SingleOrDefault();
                                    if (innerDirs != string.Empty)
                                    {
                                        configuration.knownSteamFolders.Add(innerDirs);
                                        //hit and search in it
                                        if (Directory.Exists(innerDirs + $@"\common\{_gameName}"))
                                        {
                                            configuration.installationFolder = innerDirs + $@"\common\{_gameName}";
                                            break;
                                        }
                                    }
                                }
                            }

                            cdirs = Directory.GetDirectories(d.Name + "Program Files (x86)", "*steam*");
                            foreach (string dir in cdirs)
                            {
                                if (dir == "steamapps")
                                {
                                    configuration.knownSteamFolders.Add(dir);
                                    //hit and search in it
                                    if (Directory.Exists(dir + $@"\common\{_gameName}"))
                                    {
                                        configuration.installationFolder = dir + $@"\common\{_gameName}";
                                        break;
                                    }
                                }
                                else
                                {
                                    string innerDirs = Directory.GetDirectories(dir, "steamapps").SingleOrDefault();
                                    if (innerDirs != string.Empty)
                                    {
                                        configuration.knownSteamFolders.Add(innerDirs);
                                        //hit and search in it
                                        if (Directory.Exists(innerDirs + $@"\common\{_gameName}"))
                                        {
                                            configuration.installationFolder = innerDirs + $@"\common\{_gameName}";
                                            break;
                                        }
                                    }
                                }
                            }
                        }

                        string[] dirs = Directory.GetDirectories(d.Name, "*steam*");
                        foreach (string dir in dirs)
                        {
                            if (dir == "steamapps")
                            {
                                configuration.knownSteamFolders.Add(dir);
                                //hit and search in it
                                if (Directory.Exists(dir + $@"\common\{_gameName}"))
                                {
                                    configuration.installationFolder = dir + $@"\common\{_gameName}";
                                    break;
                                }
                            }
                            else
                            {
                                string innerDirs = Directory.GetDirectories(dir, "steamapps").SingleOrDefault();
                                if (innerDirs != string.Empty)
                                {
                                    configuration.knownSteamFolders.Add(innerDirs);
                                    //hit and search in it
                                    if (Directory.Exists(innerDirs + $@"\common\{_gameName}"))
                                    {
                                        configuration.installationFolder = innerDirs + $@"\common\{_gameName}";
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }


            // TODO : Implement GOG Search

            //CleanUp
            configuration.installationFolder = configuration.installationFolder.Replace(@"\\", @"\");
        }

        private void getSMAPIInstallation()
        {
            configuration.isSMAPIInstalled = (File.Exists(Path.Combine(configuration.installationFolder, "StardewModdingAPI.exe")) && Directory.Exists(configuration.installationFolder + @"\Mods")) ? true : false;

            if (configuration.isSMAPIInstalled)
            {
                configuration.SMAPIVersion = new Version(FileVersionInfo.GetVersionInfo(Path.Combine(configuration.installationFolder, "StardewModdingAPI.exe")).ProductVersion); // FileVersion always appends a '.0' Product Versions Ignores it
            }

        }

        private void getAllMods()
        {
            if (configuration.isSMAPIInstalled)
            {
                string[] mods = Directory.GetDirectories(Path.Combine(configuration.installationFolder, "Mods"));
                foreach (string modFolder in mods)
                {
                    try
                    {
                        //Search for normal Mods, One Directory with manifest
                        if (File.Exists(Path.Combine(modFolder, "manifest.json")))
                        {
                            Mods moddata = JsonConvert.DeserializeObject<Mods>(File.ReadAllText(Path.Combine(modFolder, "manifest.json")));
                            if (moddata.Author == "SMAPI")
                                continue;
                            configuration.installedMods.Add(moddata);
                        }
                        //Search for bigger Mods, multiple Directorys with manifests
                        else if (File.Exists(Path.Combine(modFolder, modFolder.Substring(modFolder.LastIndexOf(@"\")+1).SkipWhitespaces(), "manifest.json")))
                        {
                            Mods moddata = JsonConvert.DeserializeObject<Mods>(File.ReadAllText(Path.Combine(modFolder, modFolder.Substring(modFolder.LastIndexOf(@"\") + 1).SkipWhitespaces(), "manifest.json")));
                            if (moddata.Author == "SMAPI")
                                continue;
                            configuration.installedMods.Add(moddata);
                        }
                        //Search through all directorys for a manifest with UpdateKeys
                        else
                        {
                            string[] files = Directory.GetFiles(modFolder, "manifest.json", SearchOption.AllDirectories);
                            bool oneFileIsValid = false;
                            foreach (string file in files)
                            {
                                try
                                {
                                    Mods moddata = JsonConvert.DeserializeObject<Mods>(File.ReadAllText(file));
                                    if (!moddata.UpdateKeys.Contains("Nexus:???"))
                                    {
                                        configuration.installedMods.Add(moddata);
                                        oneFileIsValid = true;
                                        break;
                                    }
                                }
                                catch { }
                            }

                            if(!oneFileIsValid)
                                throw new FileNotFoundException($"No manifest found in this Mod: {modFolder}");
                        }
                    }
                    catch(Exception ex)
                    {
                        configuration.unknownInstalledMods.Add(new Mods
                        {
                            Name = modFolder,
                            Version = new Version("0.0.0"),
                            Description = $"missing manifest or invalid Version. Please check this Mod by yourself. Explicit Exception: {ex}"
                        });
                    }
                }
            }
        }

        private void getLatestSmapiVersion()
        {
            string strContent = "";
            //check on github
            var webRequest = WebRequest.Create("https://github.com/Pathoschild/SMAPI/releases");

            using (var response = webRequest.GetResponse())
            using (var content = response.GetResponseStream())
            using (var reader = new StreamReader(content))
            {
                strContent = reader.ReadToEnd();
            }

            var configstring = ExtensionMethods.ReadFromAppSettings("WebSearchStringForLatestVersion");
            string relevantContent = strContent.Substring(strContent.IndexOf(configstring), 200).Replace(configstring, string.Empty);
            configuration.latestSMAPIVersion = new Version(relevantContent.Substring(0, relevantContent.IndexOf("\"")));

            if (configuration.latestSMAPIVersion != configuration.SMAPIVersion)
            {
                string text = $"You have SMAPI Version {configuration.SMAPIVersion} installed. There is a newer Version {configuration.latestSMAPIVersion}. Do you want to update now ?";
                string caption = "Upgrade ?";
                MessageBoxButtons buttons = MessageBoxButtons.YesNo;
                MessageBoxIcon icon = MessageBoxIcon.Question;
                DialogResult result = MessageBox.Show(text,caption,buttons,icon);
                if(result == DialogResult.Yes)
                {
                    setNewestSmapiVersion();
                }
            }
        }

        private void setNewestSmapiVersion()
        {
            string zipfile = DownloadFiles("2400", "SMAPI");
            using (ArchiveFile archiveFile = new ArchiveFile(zipfile))
            {
                archiveFile.Extract("SMAPI");
            }

            //TODO: check if its windows or linux etc...
            //TODO: maybe do it automatically with arguments ?
            string filepathForInstallationBatch = zipfile.Replace("SMAPI.rar", $@"SMAPI\SMAPI {configuration.latestSMAPIVersion} installer\");
            Process process = new Process();
            process.StartInfo.WorkingDirectory = filepathForInstallationBatch;
            process.StartInfo.FileName = "install on Windows.bat";
            process.Start();
            process.WaitForExit();
            Directory.Delete(zipfile.Replace("SMAPI.rar", "SMAPI"), true);
            File.Delete(zipfile);
        }

        private void getLatestModVersions()
        {
            foreach (Mods mods in configuration.installedMods)
            {
                try
                {
                    int gameId = 0;
                    if (mods.UpdateKeys != null && Int32.TryParse(mods.UpdateKeys[0].Replace("Nexus:", ""), out gameId))
                    {
                        var webRequest = System.Net.WebRequest.Create($"https://api.nexusmods.com/v1/games/stardewvalley/mods/{gameId}.json");
                        if (webRequest != null)
                        {
                            webRequest.Method = "GET";
                            webRequest.Timeout = 12000;
                            webRequest.ContentType = "application/json";
                            webRequest.Headers.Add("apikey", _apikey);

                            using (System.IO.Stream s = webRequest.GetResponse().GetResponseStream())
                            {
                                using (System.IO.StreamReader sr = new System.IO.StreamReader(s))
                                {
                                    var jsonResponse = JObject.Parse((sr.ReadToEnd()));
                                    mods.latestVersion = new Version(jsonResponse["version"].ToString());
                                }
                            }
                        }
                    }
                }
                catch { }
            }
        }

        private void setConfiguration()
        {
            if (!hasConfig)
            {
                string configjson  = JsonConvert.SerializeObject(configuration,Formatting.Indented);
                if(!File.Exists(_configfile))
                    File.Create(_configfile).Dispose();

                using(StreamWriter sw = new StreamWriter(_configfile))
                {
                    sw.Write(configjson);
                }
            }

            //display all data
            UpdateListView();
        }

        public void UpdateListView()
        {
            if (this.listView1.InvokeRequired)
            {
                this.label1.BeginInvoke(new MethodInvoker(delegate { //TODO: Check label1

                    //remove all Items first
                    foreach (ListViewItem item in listView1.Items)
                    {
                        listView1.Items.Remove(item);
                    }

                    // Set the view to show details.
                    listView1.View = View.Details;
                    // Allow the user to rearrange columns.
                    listView1.AllowColumnReorder = true;
                    // Display check boxes.
                    listView1.CheckBoxes = true;
                    // Select the item and subitems when selection is made.
                    listView1.FullRowSelect = true;
                    // Display grid lines.
                    listView1.GridLines = true;

                    // Width of -2 indicates auto-size, -1 will resize to the longest item
                    listView1.Columns.Add("", -1, HorizontalAlignment.Left);
                    listView1.Columns.Add("Mod-Name", -2, HorizontalAlignment.Left);
                    listView1.Columns.Add("Author", -2, HorizontalAlignment.Left);
                    listView1.Columns.Add("Installed Version", -2, HorizontalAlignment.Left);
                    listView1.Columns.Add("Latest Version", -2, HorizontalAlignment.Left);


                    foreach (Mods mod in configuration.installedMods)
                    {
                        ListViewItem item = new ListViewItem(mod.Name, 0);
                        if (mod.Version == mod.latestVersion)
                            item.ForeColor = Color.DarkGreen;
                        else if (mod.Version < mod.latestVersion)
                            item.ForeColor = Color.DarkOrange;


                        item.Checked = false;
                        item.Text = "";
                        item.SubItems.Add(mod.Name);
                        item.SubItems.Add(mod.Author);
                        item.SubItems.Add(mod.Version.ToString());
                        item.SubItems.Add((mod.latestVersion != null) ? mod.latestVersion.ToString() : "?.?.?");
                        listView1.Items.Add(item);
                    }

                    foreach (Mods mod in configuration.unknownInstalledMods)
                    {
                        ListViewItem item = new ListViewItem(mod.Name, 0);
                        item.ForeColor = Color.Gold;
                        item.Checked = false;
                        item.Text = "";
                        item.SubItems.Add(mod.Name);
                        item.SubItems.Add(mod.Author);
                        item.SubItems.Add(mod.Version.ToString());
                        item.SubItems.Add((mod.latestVersion != null) ? mod.latestVersion.ToString() : "?.?.?");
                        listView1.Items.Add(item);
                    }
                }));
            }
        }

        private void activateUI()
        {
            button1.Invoke(new MethodInvoker(delegate { this.button1.Enabled = true; }));
            button2.Invoke(new MethodInvoker(delegate { this.button2.Enabled = true; }));
            button3.Invoke(new MethodInvoker(delegate { this.button3.Enabled = true; }));
            button4.Invoke(new MethodInvoker(delegate { this.button4.Enabled = true; }));
            button5.Invoke(new MethodInvoker(delegate { this.button5.Enabled = true; }));
            this.Invoke(new MethodInvoker(delegate { this.Text = $"Stardew Valley Mod Updater ({configuration.installedMods.Count}-Mods)"; }));
        }

        private string DownloadFiles(string modid, string modname)
        {
            try
            {
                string fileid = "", remoteUri = "";

                var webRequest = WebRequest.Create($"https://api.nexusmods.com/v1/games/stardewvalley/mods/{modid}/files.json");
                if (webRequest != null)
                {
                    webRequest.Method = "GET";
                    webRequest.Timeout = 12000;
                    webRequest.ContentType = "application/json";
                    webRequest.Headers.Add("apikey", _apikey);

                    using (Stream s = webRequest.GetResponse().GetResponseStream())
                    {
                        using (StreamReader sr = new StreamReader(s))
                        {
                            var jsonList = JObject.Parse((sr.ReadToEnd())).First.ToList();
                            var jsonResponse = JsonConvert.DeserializeObject<List<dynamic>>(jsonList[0].ToString());
                            Dictionary<string, string> mainFileIds = jsonResponse.Where(jr => jr.category_name == "MAIN").Select(jr => (jr.file_id, jr.name)).ToDictionary(jr => (string)jr.file_id, jr => (string)jr.name);//&& jr.name.ToString().contains(modname)
                            fileid = mainFileIds.Where(d => d.Value.SkipWhitespaces().Contains(modname)).Select(d => d.Key).SingleOrDefault();
                        }
                    }
                }

                webRequest = WebRequest.Create($"https://api.nexusmods.com/v1/games/stardewvalley/mods/{modid}/files/{fileid}/download_link.json");
                if (webRequest != null)
                {
                    webRequest.Method = "GET";
                    webRequest.Timeout = 12000;
                    webRequest.ContentType = "application/json";
                    webRequest.Headers.Add("apikey", _apikey);

                    using (Stream s = webRequest.GetResponse().GetResponseStream())
                    {
                        using (StreamReader sr = new StreamReader(s))
                        {
                            var jsonResponse = JsonConvert.DeserializeObject<List<dynamic>>(sr.ReadToEnd());
                            remoteUri = jsonResponse[0].URI;
                        }
                    }
                }

                string fileName = modname + ".rar", myStringWebResource = null;
                // Create a new WebClient instance.
                WebClient myWebClient = new WebClient();
                // Concatenate the domain with the Web resource filename.
                myStringWebResource = remoteUri + fileName;
                // Download the Web resource and save it into the current filesystem folder.
                myWebClient.DownloadFile(myStringWebResource, fileName);
                myWebClient.Dispose();

                return Path.Combine(Application.StartupPath.ToString(), fileName);
            }
            catch(Exception ex)
            {
                return string.Empty;
            }
        }

        //Download
        private void button1_Click(object sender, EventArgs e)
        {
            string importFile = "";
            try
            {
                var url = new Uri(textBox1.Text);
                string fileName = "SVU-ModConfigImport.json";
                WebClient myWebClient = new WebClient();
                myWebClient.DownloadFile(url, fileName);
                myWebClient.Dispose();

                importFile = Path.Combine(Application.StartupPath.ToString(), fileName);
                List<Mods> modsFromImport = ValidateDownloadConfig(importFile);
                File.Delete(importFile);

                List<Mods> modsYouAreMissing = new List<Mods>();
                List<Mods> modsThatAreNotRequired = new List<Mods>();
                foreach (Mods mod in modsFromImport)
                {
                    if (!configuration.installedMods.Select(m => m.UniqueID).Contains(mod.UniqueID))
                        modsYouAreMissing.Add(mod);
                    else
                        modsThatAreNotRequired.Add(mod);
                }

                string text = $"Following Mods will be new: \n {string.Join(", ", modsYouAreMissing.Select(m => m.Name))} \n\n Following Mods are not rewuired with this Import:  \n {string.Join(", ", modsThatAreNotRequired.Select(m => m.Name))} \n\n Do You wanna keep them and install the new ones?";
                string caption = "Please concider which Mods you want";
                MessageBoxButtons buttons = MessageBoxButtons.YesNoCancel;
                MessageBoxIcon icon = MessageBoxIcon.Question;
                DialogResult result = MessageBox.Show(text, caption, buttons, icon);
            }
            catch(Exception ex)
            {
                string extext = $"There was an error while downloading this file. Explicit error: {ex.Message}";
                string excaption = "Error on receiving the File !";
                MessageBoxButtons exbuttons = MessageBoxButtons.OK;
                MessageBoxIcon exicon = MessageBoxIcon.Error;
                MessageBox.Show(extext, excaption, exbuttons, exicon);
            }
        }

        public List<Mods> ValidateDownloadConfig(string filePath)
        {
            using (StreamReader r = new StreamReader(filePath))
            {
                try
                {
                    string json = r.ReadToEnd();
                    //TODO: What happens when one mod is not serializable??
                    return JsonConvert.DeserializeObject<List<Mods>>(json);
                }
                catch (Exception ex)
                {
                    string text = $"This File cannot be validated. Explicit error: {ex.Message}";
                    string caption = "Error on Validating the File !";
                    MessageBoxButtons buttons = MessageBoxButtons.OK;
                    MessageBoxIcon icon = MessageBoxIcon.Error;
                    MessageBox.Show(text, caption, buttons, icon);
                    return new List<Mods>();
                }
            }
        }

        //Update
        private void button2_Click(object sender, EventArgs e)
        { 
            //new functions for Backgroundworker
            functions = new Dictionary<Action, string>();
            
            //Backup
            //backgroundWorker2.RunWorkerAsync();
            functions.Add(CreateBackUpZip, "Creating BackUp...");

            List<Mods> selectedMods = new List<Mods>();
            foreach(ListViewItem item in this.listView1.Items)
            {
                if (item.Checked)
                    selectedMods.Add(configuration.installedMods.Where(im => im.Name == item.SubItems[1].Text).Single());
            }

            functions.Add(() =>
            {
                foreach (Mods mod in selectedMods)
                {
                    if (mod.UpdateKeys != null)
                    {
                        string downloadedFiles = DownloadFiles(mod.UpdateKeys[0].Replace("Nexus:", ""), mod.Name.SkipWhitespaces());
                        if (downloadedFiles == string.Empty)
                        {
                            // TODO: Error on downloading
                        }
                    }
                }
            }, "Downloading Files...");

            functions.Add(() =>
            {
                foreach (Mods mod in selectedMods)
                {
                    //First Check SMAPI Version
                    if(mod.MinimumApiVersion.CompareTo(configuration.SMAPIVersion) < 0)
                    {
                        //TODO: Implement Fehler mit SMAPI Version
                    }

                    foreach(Depedencies dep in mod.Dependencies)
                    {
                        if (dep.IsRequired && !configuration.installedMods.Where(m => m.UniqueID == dep.UniqueID).Any())
                        {
                            //TODO: Implement Fehler mit Depedencies
                        }
                    }
                    //TODO: Implement Second Check for Other Mods
                }
            }, "Check Dependencies...");

            //if (asnycHasFailed)
            //{
            //    asnycHasFailed = false;
            //    // TODO: interrupt process
            //}

            functions.Add(() =>
            {
                foreach (Mods mod in selectedMods)
                {
                    //TODO: get Name from Folder Inside ?
                    string nameWithOutSpaces = mod.Name.SkipWhitespaces();
                    File.Move($"{nameWithOutSpaces}.rar", $@"{configuration.installationFolder}\{nameWithOutSpaces}.rar");
                    try
                    {
                        Directory.Delete($@"{configuration.installationFolder}\Mods\{nameWithOutSpaces}", true);
                    }
                    catch
                    {
                        try
                        {
                            string part = mod.UniqueID.Substring(mod.UniqueID.LastIndexOf('.')+1);
                            Directory.Delete($@"{configuration.installationFolder}\Mods\{part}", true);
                        }
                        catch(Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                            break;
                        }
                    }
                    
                    using (ArchiveFile archiveFile = new ArchiveFile($@"{configuration.installationFolder}\{nameWithOutSpaces}.rar"))
                    {
                        archiveFile.Extract($@"{configuration.installationFolder}\Mods");
                    }
                    File.Delete($@"{configuration.installationFolder}\{nameWithOutSpaces}.rar");
                }
            }, "Extracting Files...");

            //TODO: SHould propably check again
            functions.Add(() => {
                foreach (Mods mod in selectedMods)
                {
                    configuration.installedMods.Where(im => im.UniqueID == mod.UniqueID).Single().Version = mod.latestVersion;
                }

                //Saving Updated Mods in File as well
                string configjson = JsonConvert.SerializeObject(configuration, Formatting.Indented);
                if (!File.Exists(_configfile))
                    File.Create(_configfile).Dispose();

                using (StreamWriter sw = new StreamWriter(_configfile))
                {
                    sw.Write(configjson);
                }
            }, "Updating Mod Data...");

            functions.Add(UpdateListView, "Updating UI...");

            backgroundWorker1.RunWorkerAsync();
        }

        public void CreateBackUpZip()
        {
            try
            {
                //create fallback directory for modfolder backups
                if (!Directory.Exists($@"{configuration.installationFolder}\SVU-Fallback"))
                    Directory.CreateDirectory($@"{configuration.installationFolder}\SVU-Fallback");


                string filename = DateTime.Now.ToString("d") + Guid.NewGuid().ToString();
                using (ZipFile zip = new ZipFile())
                {
                    zip.AddItem(configuration.installationFolder + @"\Mods");
                    zip.Save($@"{configuration.installationFolder}\SVU-Fallback\{filename}.zip");
                }
            } 
            catch(Exception ex)
            {
                string message = $"Following Error accured when making an BackUp: {ex.ToString()}";
                string caption = "Error on BackUp ";
                MessageBoxButtons buttons = MessageBoxButtons.OK;
                MessageBoxIcon icon = MessageBoxIcon.Error;
                DialogResult result = MessageBox.Show(message, caption, buttons, icon);
                asnycHasFailed = true;
            }
        }

        //Delete
        private void button3_Click(object sender, EventArgs e)
        {
            //TODO: Shows UI
            //TODO: Run Backup Async until real deletion process

            //Backup
            //backgroundWorker2.RunWorkerAsync();
            CreateBackUpZip();

            //check Dependencies
            List<Mods> selectedMods = new List<Mods>();
            foreach (ListViewItem item in this.listView1.Items)
            {
                if (item.Checked)
                    selectedMods.Add(configuration.installedMods.Where(im => im.Name == item.Text).Single());
            }

            List<Depedencies> requiredDependencies = new List<Depedencies>();
            foreach (Mods mod in configuration.installedMods)
            {
                foreach (Depedencies dep in mod.Dependencies)
                {
                    if (dep.IsRequired && selectedMods.Where(sm => dep.UniqueID == sm.UniqueID).Any())
                        requiredDependencies.Add(dep);
                }
            }

            List<Mods> criticalMods = new List<Mods>();
            foreach (Depedencies dep in requiredDependencies)
            {
                Mods requiredMod = selectedMods.Where(sm => sm.UniqueID == dep.UniqueID).SingleOrDefault();
                if (requiredMod != null)
                    criticalMods.Add(requiredMod);
            }

            bool deleteMods = true;
            if(criticalMods.Count != 0)
            {
                string modlist = "";
                criticalMods.ForEach(mod => modlist += mod.Name + ", ");
                string message = $"Following Mods are required: {modlist}";
                string caption = "Mods are certantly required ";
                MessageBoxButtons buttons = MessageBoxButtons.RetryCancel;
                MessageBoxIcon icon = MessageBoxIcon.Error;
                DialogResult result = MessageBox.Show(message, caption, buttons, icon);
                if (result == System.Windows.Forms.DialogResult.Cancel)
                    deleteMods = false;
            }

            //while (backgroundWorker2.IsBusy) { 
            //    // we will wait
            //}

            //if (asnycHasFailed)
            //{
            //    asnycHasFailed = false;
            //    deleteMods = false;
            //}


            if (deleteMods)
            {
                foreach (Mods mod in selectedMods)
                    Directory.Delete($@"{configuration.installationFolder}\Mods\{mod.Name.SkipWhitespaces()}");
            }
        }

        private void button4_Click(object sender, EventArgs e) => Process.Start(configuration.installationFolder);

        //Settings
        private void button5_Click(object sender, EventArgs e)
        {
            Settings settings = new Settings(configuration, _configfile);
            settings.Show();
        }

        private void textBox1_Enter(object sender, EventArgs e)
        {
            if (textBox1.Text == "Custom Mod Collection URL...")
                textBox1.Text = string.Empty;
        }

        private void textBox1_Leave(object sender, EventArgs e)
        {
            if (textBox1.Text == string.Empty)
                textBox1.Text = "Custom Mod Collection URL...";
        }

        //TODO: currently in Progress
        private void backgroundWorker2_DoWork(object sender, DoWorkEventArgs e)
        {
            CreateBackUpZip();
            backgroundWorker2.CancelAsync();
        }
    }
}
