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

namespace StardewUpdater
{
    public partial class Form1 : Form
    {
        private bool hasConfig;
        private Configuration configuration;
        public List<Func<string>> functions = new List<Func<string>>();
        private readonly string _apikey = "XXXXXXXXXX";
        private readonly string gameName = "Stardew Valley";

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            button1.Enabled = false;
            button2.Enabled = false;
            button3.Enabled = false;
            button4.Enabled = false;
            button5.Enabled = false;

            hasConfig = CheckConfigurationFile();
            functions.Add(getInstallationFolder);
            functions.Add(getSMAPIInstallation);
            functions.Add(getAllMods);
            functions.Add(getLatestSmapiVersion);
            functions.Add(getLatestModVersions);
            functions.Add(setConfiguration);
            functions.Add(activateUI);

            backgroundWorker1.RunWorkerAsync();
        }

        private void UpdateInformationService(string currentProcess, float percentage)
        {
            //title
            if (this.label1.InvokeRequired)
            {
                this.label1.BeginInvoke((MethodInvoker)delegate () { this.label1.Text = currentProcess; });
            }
            else
            {
                this.label1.Text = currentProcess;
            }

            //percentage
            if (this.label2.InvokeRequired)
            {
                this.label2.BeginInvoke((MethodInvoker)delegate () { this.label2.Text = ((percentage/functions.Count)*100).ToString("00.##") + " %"; });
            }
            else
            {
                this.label2.Text = ((percentage / functions.Count) * 100).ToString("00.##") + " %";
            }
        }

        private bool CheckConfigurationFile()
        {
            configuration = new Configuration();
            return false;
        }

        //Find Installfolder Steam/GoG or ask for it
        //Get Version SMAPI
        //Get all Mods
        //Check latest SMAPI Version
        //Check latest Mod Versions
        //Display
        //Save Configuration in File
        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            for(int i = 1; i <= functions.Count; i++)
                UpdateInformationService(functions[i-1].Invoke(), i);
        }

        //Find Installfolder
        private string getInstallationFolder()
        {

            //Ignoring Registry Information, cause i dont have a plan at all :D

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
                                if (Directory.Exists(dir + $@"\common\{gameName}"))
                                {
                                    configuration.installationFolder = dir + $@"\common\{gameName}";
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
                                    if (Directory.Exists(innerDirs + $@"\common\{gameName}"))
                                    {
                                        configuration.installationFolder = innerDirs + $@"\common\{gameName}";
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
                                if (Directory.Exists(dir + $@"\common\{gameName}"))
                                {
                                    configuration.installationFolder = dir + $@"\common\{gameName}";
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
                                    if (Directory.Exists(innerDirs + $@"\common\{gameName}"))
                                    {
                                        configuration.installationFolder = innerDirs + $@"\common\{gameName}";
                                        break;
                                    }
                                }
                            }
                        }
                    }

                    string[] dirs = Directory.GetDirectories(d.Name, "*steam*");
                    foreach(string dir in dirs)
                    {
                        if(dir == "steamapps")
                        {
                            configuration.knownSteamFolders.Add(dir);
                            //hit and search in it
                            if (Directory.Exists(dir + $@"\common\{gameName}"))
                            {
                                configuration.installationFolder = dir + $@"\common\{gameName}";
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
                                if (Directory.Exists(innerDirs + $@"\common\{gameName}"))
                                {
                                    configuration.installationFolder = innerDirs + $@"\common\{gameName}";
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            //Second GOG

            //CleanUp
            configuration.installationFolder = configuration.installationFolder.Replace(@"\\", @"\");


            return "Searching for SMAPI Installation...";
        }

        private string getSMAPIInstallation()
        {
            configuration.isSMAPIInstalled = (File.Exists(Path.Combine(configuration.installationFolder, "StardewModdingAPI.exe")) && Directory.Exists(configuration.installationFolder + @"\Mods")) ? true : false;

            if (configuration.isSMAPIInstalled)
                configuration.SMAPIVersion = FileVersionInfo.GetVersionInfo(Path.Combine(configuration.installationFolder, "StardewModdingAPI.exe")).FileVersion;

            return "Collecting Mod Data...";
        }

        private string getAllMods()
        {
            if (configuration.isSMAPIInstalled)
            {
                string[] mods = Directory.GetDirectories(Path.Combine(configuration.installationFolder, "Mods"));
                foreach (string modFolder in mods)
                {
                    try
                    {
                        Mods moddata = JsonConvert.DeserializeObject<Mods>(File.ReadAllText(Path.Combine(modFolder, "manifest.json")));
                        configuration.installedMods.Add(moddata);
                    }
                    catch(Exception ex)
                    {
                        configuration.unknownInstalledMods.Add(new Mods
                        {
                            Name = modFolder,
                            Version = "?.?.?",
                            Description = "missing manifest. Please check this Mod by yourself."
                        });
                    }
                }
            }
            return "Searching for latest SMAPI Version...";
        }

        private string getLatestSmapiVersion()
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

            string maincontent = strContent.Substring(0, strContent.IndexOf("<a href=\"/Pathoschild/SMAPI/releases/latest\" data-view-component=\"true\" class=\"v-align-text-bottom d-none d-md-inline-block\"><span data-view-component=\"true\" class=\"Label Label--success Label--large\">Latest</span></a>") - 14);//14 for 2 closing tags
            configuration.latestSMAPIVersion = maincontent.Substring(maincontent.LastIndexOf('>')+1);
            return "Searching for Mod Versions...";
        }

        private string getLatestModVersions()
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
                                    mods.latestVersion = jsonResponse["version"].ToString();
                                }
                            }
                        }
                    }
                }
                catch { }
            }
            return "Savin Progress...";
        }

        private string setConfiguration()
        {
            //save configuration
            if (hasConfig)
            {

            }
            else
            {

            }

            //display all data
            if (this.listView1.InvokeRequired)
            {
                this.label1.BeginInvoke(new MethodInvoker(delegate {
                    // Set the view to show details.
                    listView1.View = View.Details;
                    // Allow the user to edit item text.
                    listView1.LabelEdit = true;
                    // Allow the user to rearrange columns.
                    listView1.AllowColumnReorder = true;
                    // Display check boxes.
                    listView1.CheckBoxes = true;
                    // Select the item and subitems when selection is made.
                    listView1.FullRowSelect = true;
                    // Display grid lines.
                    listView1.GridLines = true;
                    // Sort the items in the list in ascending order.
                    listView1.Sorting = SortOrder.Ascending;

                    // Width of -2 indicates auto-size, -1 will resize to the longest item
                    listView1.Columns.Add("Mod-Name", -2, HorizontalAlignment.Left);
                    listView1.Columns.Add("Author", -2, HorizontalAlignment.Left);
                    listView1.Columns.Add("Installed Version", -2, HorizontalAlignment.Left);
                    listView1.Columns.Add("Latest Version", -2, HorizontalAlignment.Left);

                    foreach (Mods mod in configuration.installedMods)
                    {
                        ListViewItem item = new ListViewItem(mod.Name, 0);
                        item.Checked = false;
                        item.SubItems.Add(mod.Author);
                        item.SubItems.Add(mod.Version);
                        item.SubItems.Add(mod.latestVersion);
                        listView1.Items.Add(item);
                    }

                    foreach (Mods mod in configuration.unknownInstalledMods)
                    {
                        ListViewItem item = new ListViewItem(mod.Name, 0);
                        item.Checked = false;
                        item.SubItems.Add(mod.Author);
                        item.SubItems.Add(mod.Version);
                        item.SubItems.Add(mod.latestVersion);
                        listView1.Items.Add(item);
                    }
                }));
            }

            //close loading panel
            panel1.Invoke(new MethodInvoker(delegate { this.panel1.Visible = false; }));
            return "Cleaning up the mess...";
        }

        private string activateUI()
        {
            button1.Invoke(new MethodInvoker(delegate { this.button1.Enabled = true; }));
            button2.Invoke(new MethodInvoker(delegate { this.button2.Enabled = true; }));
            button3.Invoke(new MethodInvoker(delegate { this.button3.Enabled = true; }));
            button4.Invoke(new MethodInvoker(delegate { this.button4.Enabled = true; }));
            button5.Invoke(new MethodInvoker(delegate { this.button5.Enabled = true; }));

            return "Making a splendid UI";
        }

        //Download
        private void button1_Click(object sender, EventArgs e)
        {

        }

        //Update
        private void button2_Click(object sender, EventArgs e)
        {
            List<Mods> selectedMods = new List<Mods>();
            foreach(ListViewItem item in this.listView1.Items)
            {
                if (item.Checked)
                    selectedMods.Add(configuration.installedMods.Where(im => im.Name == item.Text).Single());
            }

            foreach (Mods mod in selectedMods)
            {
                if (mod.UpdateKeys != null)
                {
                    string modid = mod.UpdateKeys[0].Replace("Nexus:", "");
                    string modname = mod.Name.Replace(" ", "");//replace spaces
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
                                fileid = mainFileIds.Where(d => d.Value.Replace(" ", "").Contains(modname)).Select(d => d.Key).SingleOrDefault();
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
                }
            }

            foreach(Mods modfile in selectedMods)
            {
                using (ArchiveFile archiveFile = new ArchiveFile($"{modfile.Name.Replace(" ", "")}.rar"))
                {
                    archiveFile.Extract("Mods");
                }
            }

            using (ZipFile zip = new ZipFile()) //brauche das für verzeichnisse
            {
                zip.AddFile(configuration.installationFolder+@"\Mods");
                zip.Save($"Mods-backup-{DateTime.Now}.zip");
            }

            //Move it to installFolder
            //befor that make sure to have a backup of the old installation
        }

        //Delete
        private void button3_Click(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e) => Process.Start(configuration.installationFolder);

        //Settings
        private void button5_Click(object sender, EventArgs e)
        {

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
    }
}
