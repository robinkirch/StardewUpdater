using Newtonsoft.Json;
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
    public enum GameLauncherItemIndex
    {
        Steam = 0,
        GOG = 1,
        Epic = 2,
        Other = 3,
    }
    public partial class Settings : Form
    {
        private Configuration configuration;
        private readonly string _configfile;
        public Settings(Configuration c, string configfile)
        {
            InitializeComponent();
            configuration = c;
            _configfile = configfile;
        }

        private void Settings_Load(object sender, EventArgs e)
        {
            comboBox1.SelectedItem = comboBox1.Items[(int)getGame(configuration.installationFolder)];
            textBox1.Text = configuration.installationFolder;
            label3.Text = "0.0.4";
            label5.Text = configuration.SMAPIVersion.ToString();
            label7.Text = GetGameVersion();
        }

        ///<summary>
        ///This method returns the current Stardew Valley Version from the Game exe
        ///</summary>
        private string GetGameVersion() => FileVersionInfo.GetVersionInfo(Path.Combine(configuration.installationFolder, "Stardew Valley.exe")).FileVersion;

        ///<summary>
        ///This method returns the index for the ComboBox based on the Game Launcher
        ///</summary>
        private GameLauncherItemIndex getGame(string gamelauncherpath)
        {
            if(gamelauncherpath.Contains("steam"))
                return GameLauncherItemIndex.Steam;

            if (gamelauncherpath.Contains("gog"))
                return GameLauncherItemIndex.GOG;

            if (gamelauncherpath.Contains("epic"))
                return GameLauncherItemIndex.Steam;

            return GameLauncherItemIndex.Other;
        }

        ///<summary>
        ///This method Exports the Mod List from the configuration to a Json File, 
        ///saved by the User with a SaveFileDialog
        ///</summary>
        private void button2_Click(object sender, EventArgs e)
        {
            Configuration modConfiguration = new Configuration();
            string selectedFullPath = string.Empty;

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.DefaultExt = "json";
            saveFileDialog.Title = "Save Config";
            saveFileDialog.InitialDirectory = Environment.SpecialFolder.MyDocuments.ToString();
            saveFileDialog.FileName = "SVU-ModConfigExport";
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                if (File.Exists(_configfile))
                {
                    using (StreamReader r = new StreamReader(_configfile))
                    {
                        string json = r.ReadToEnd();
                        modConfiguration = JsonConvert.DeserializeObject<Configuration>(json);
                    }
                }

                selectedFullPath = Path.GetFullPath(saveFileDialog.FileName);
                if (!File.Exists(selectedFullPath))
                    File.Create(selectedFullPath).Dispose();

                using (StreamWriter sw = new StreamWriter(selectedFullPath))
                {
                    sw.Write(JsonConvert.SerializeObject(modConfiguration.installedMods, Formatting.Indented));
                }
            }  
        }

        ///<summary>
        ///This method deletes the configuration File
        ///</summary>
        private void button3_Click(object sender, EventArgs e)
        {
            string text = "Are you sure you want to delete this file? A new one will be created on StartUp";
            string caption = "Delete Config ?";
            MessageBoxButtons buttons = MessageBoxButtons.YesNo;
            MessageBoxIcon icon = MessageBoxIcon.Question;
            DialogResult result = MessageBox.Show(text, caption, buttons, icon);
            if(result == DialogResult.Yes)
                File.Delete(_configfile);
        }
    }
}
