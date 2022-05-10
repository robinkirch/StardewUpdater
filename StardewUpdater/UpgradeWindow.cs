using NLog;
using SevenZipExtractor;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StardewUpdater
{
    public partial class UpgradeWindow : Form
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public List<UpgradeMods> modList = new List<UpgradeMods>();

        private readonly StardewUpdater _stardewUpdater;
        private Dictionary<Action, string> functions = new Dictionary<Action, string>();

        public UpgradeWindow(StardewUpdater stardewUpdater)
        {
            InitializeComponent();
            _stardewUpdater = stardewUpdater;
        }

        private void UpgradeWindow_Load(object sender, EventArgs e)
        {
            insertModList();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            functions.Add(_stardewUpdater.CreateBackUpZip, "Creating BackUp...");

            functions.Add(() =>
            {
                List<UpgradeMods> selectedMods = new List<UpgradeMods>();
                
                //here it is neccessary, but in StardewUpdater not... microsoft is a shithole for this Invoker-bullshit
                listView1.BeginInvoke(new MethodInvoker(delegate
                {
                    foreach (ListViewItem item in listView1.Items)
                    {
                        if (item.Checked)
                        {
                            _logger.Debug($"Item selected : {item.SubItems[1].Text}");
                            selectedMods.Add(modList.Where(m => m.Name == item.SubItems[1].Text).Single());
                        }
                    }

                    foreach (UpgradeMods mod in selectedMods)
                    {
                        _logger.Trace($"Extracting : {mod.Name}({mod.UniqueID})");
                        string downloadedFiles = _stardewUpdater.DownloadFiles(mod.UpdateKeys[0].Replace("Nexus:", ""), mod.Name.SkipWhitespaces());
                        if (downloadedFiles == string.Empty)
                        {
                            // TODO: Error on downloading
                        }

                        if (mod.Type == UpgradeType.New)
                        {
                            string nameWithOutSpaces = mod.Name.SkipWhitespaces();
                            File.Move($"{nameWithOutSpaces}.rar", $@"{_stardewUpdater.configuration.installationFolder}\{nameWithOutSpaces}.rar");

                            using (ArchiveFile archiveFile = new ArchiveFile($@"{_stardewUpdater.configuration.installationFolder}\{nameWithOutSpaces}.rar"))
                            {
                                archiveFile.Extract($@"{_stardewUpdater.configuration.installationFolder}\Mods");
                            }
                            File.Delete($@"{_stardewUpdater.configuration.installationFolder}\{nameWithOutSpaces}.rar");
                        }
                        else if (mod.Type == UpgradeType.Upgrade || mod.Type == UpgradeType.Downgrade)
                        {
                            string nameWithOutSpaces = mod.Name.SkipWhitespaces();
                            File.Move($"{nameWithOutSpaces}.rar", $@"{_stardewUpdater.configuration.installationFolder}\{nameWithOutSpaces}.rar");
                            try
                            {
                                ExtensionMethods.DeleteFileAndWait($@"{_stardewUpdater.configuration.installationFolder}\Mods\{mod.Name}");
                                using (ArchiveFile archiveFile = new ArchiveFile($@"{_stardewUpdater.configuration.installationFolder}\{nameWithOutSpaces}.rar"))
                                {
                                    archiveFile.Extract($@"{_stardewUpdater.configuration.installationFolder}\Mods");
                                }
                                File.Delete($@"{_stardewUpdater.configuration.installationFolder}\{nameWithOutSpaces}.rar");
                            }
                            catch (Exception exSpace)
                            {
                                try
                                {
                                    ExtensionMethods.DeleteFileAndWait($@"{_stardewUpdater.configuration.installationFolder}\Mods\{nameWithOutSpaces}");
                                    using (ArchiveFile archiveFile = new ArchiveFile($@"{_stardewUpdater.configuration.installationFolder}\{nameWithOutSpaces}.rar"))
                                    {
                                        archiveFile.Extract($@"{_stardewUpdater.configuration.installationFolder}\Mods");
                                    }
                                    File.Delete($@"{_stardewUpdater.configuration.installationFolder}\{nameWithOutSpaces}.rar");
                                }
                                catch (Exception exName)
                                {
                                    try
                                    {
                                        //TODO : Rigeside hat noch probleme mit - im Namen. Einfach grundsätzlich alle Ordner ab . oder - trennen
                                        string part = mod.UniqueID.Substring(mod.UniqueID.LastIndexOf('.') + 1);
                                        ExtensionMethods.DeleteFileAndWait($@"{_stardewUpdater.configuration.installationFolder}\Mods\{part}");
                                        using (ArchiveFile archiveFile = new ArchiveFile($@"{_stardewUpdater.configuration.installationFolder}\{nameWithOutSpaces}.rar"))
                                        {
                                            archiveFile.Extract($@"{_stardewUpdater.configuration.installationFolder}\Mods");
                                        }
                                        File.Delete($@"{_stardewUpdater.configuration.installationFolder}\{nameWithOutSpaces}.rar");
                                    }
                                    catch (Exception exUnique)
                                    {
                                        _logger.Error($"Access to folder not granted. User could solve it. Exceptions: {exSpace}, {exName}, {exUnique}");
                                        //TODO: Can i fixe the access denied on SVE and Ridgeside ?
                                        Process.Start(_stardewUpdater.configuration.installationFolder + @"\Mods\");
                                        string text = $"This folder ({mod.Name}) cant be deleted by the system cause someone fucked up the directory... Please delete it yourself and click ok to continue the process.";
                                        string caption = "Manual deletion";
                                        MessageBoxButtons buttons = MessageBoxButtons.OKCancel;
                                        MessageBoxIcon icon = MessageBoxIcon.Exclamation;
                                        DialogResult result = MessageBox.Show(text, caption, buttons, icon);
                                        if (result == DialogResult.OK)
                                        {
                                            try
                                            {
                                                using (ArchiveFile archiveFile = new ArchiveFile($@"{_stardewUpdater.configuration.installationFolder}\{nameWithOutSpaces}.rar"))
                                                {
                                                    archiveFile.Extract($@"{_stardewUpdater.configuration.installationFolder}\Mods");
                                                }
                                                File.Delete($@"{_stardewUpdater.configuration.installationFolder}\{nameWithOutSpaces}.rar");
                                            }
                                            catch (Exception exManual)
                                            {
                                                _logger.Error($"Access to folder not granted. User couldn' solve it. Explicit Exception: {exManual}");
                                                MessageBox.Show($"Something still doesnt work like intended : {exManual}");
                                                File.Delete($@"{_stardewUpdater.configuration.installationFolder}\{nameWithOutSpaces}.rar");
                                            }
                                        }
                                        else if (result == DialogResult.Cancel)
                                        {
                                            File.Delete($@"{_stardewUpdater.configuration.installationFolder}\{nameWithOutSpaces}.rar");
                                        }
                                    }
                                }
                            }
                        }
                    }
                })); 
            }, "Installing Mods...");

            //TODO: Check if something went wrong

            functions.Add(() =>
            {
                _stardewUpdater.updateForm();
                this.BeginInvoke((MethodInvoker)delegate () { this.Close(); });
            }, "Post Clear...");

            backgroundWorker1.RunWorkerAsync();
        }

        private void button2_Click_1(object sender, EventArgs e) => Close();

        public void insertModList()
        {
            _logger.Trace($"Insert Mod List for Upgrade");
            listView1.BeginInvoke(new MethodInvoker(delegate
            {

                //remove all Items first
                foreach (ListViewItem item in listView1.Items)
                {
                    listView1.Items.Remove(item);
                }

                listView1.View = View.Details;
                listView1.AllowColumnReorder = true;
                listView1.CheckBoxes = true;
                listView1.FullRowSelect = true;
                listView1.GridLines = true;

                // Width of -2 indicates auto-size, -1 will resize to the longest item
                listView1.Columns.Add("", -1, HorizontalAlignment.Left);
                listView1.Columns.Add("Mod-Name", -2, HorizontalAlignment.Left);
                listView1.Columns.Add("Type", -2, HorizontalAlignment.Left);
                listView1.Columns.Add("Version", -2, HorizontalAlignment.Left);

                foreach (UpgradeMods mod in modList)
                {
                    ListViewItem item = new ListViewItem(mod.Name, 0);
                    if (mod.Type == UpgradeType.New)
                        item.ForeColor = Color.Blue;
                    else if (mod.Type == UpgradeType.Upgrade)
                        item.ForeColor = Color.DarkGreen;
                    else if (mod.Type == UpgradeType.Downgrade)
                        item.ForeColor = Color.DarkRed;


                    item.Checked = false;
                    item.Text = "";
                    item.SubItems.Add(mod.Name);
                    item.SubItems.Add(mod.Type.ToString());
                    item.SubItems.Add(mod.Version.VersionWithoutRevisions().ToString());
                    listView1.Items.Add(item);
                }

            }));
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
                this.label2.BeginInvoke((MethodInvoker)delegate () { this.label2.Text = ((percentage / functions.Count) * 100).ToString("00.##") + " %"; });
            else
                this.label2.Text = ((percentage / functions.Count) * 100).ToString("00.##") + " %";
        }
    }
}
