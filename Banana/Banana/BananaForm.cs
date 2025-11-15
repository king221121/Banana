using Microsoft.Win32;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.IO.Compression;
using System.Net;
using System.Text.RegularExpressions;

namespace Banana
{
    public partial class Banana : Form
    {
        public Banana()
        {
            InitializeComponent();
        }

        //vars and shit rahh
        public static string baseUrl = "https://raw.githubusercontent.com/ShibaGT/Banana/main/";

        static string gtaglocation = getgtpath();
        string bananaDir = Path.Combine(gtaglocation, "Gorilla Tag_Data", "Banana");
        string currentVersion = "1.1.1";
        static string getgtpath() //YES this is chatgpt YES im lazy YES the rest is coded by me fuck OFF!
        {
            string steam = Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam")?.GetValue("SteamPath")?.ToString().Replace("/", "\\");
            if (steam == null) return "Steam not found";

            string vdf = File.ReadAllText(Path.Combine(steam, "steamapps", "libraryfolders.vdf"));
            var libs = Regex.Matches(vdf, "\"path\"\\s*\"(.*?)\"");

            foreach (Match m in libs)
                if (File.Exists(Path.Combine(m.Groups[1].Value.Replace("\\\\", "\\"), "steamapps", "appmanifest_1533390.acf")))
                    return Path.Combine(m.Groups[1].Value, "steamapps", "common", "Gorilla Tag");

            if (File.Exists(Path.Combine(steam, "steamapps", "appmanifest_1533390.acf")))
                return Path.Combine(steam, "steamapps", "common", "Gorilla Tag");

            return "Not installed";
        }

        WebClient w = new WebClient();

        string githubDownload;
        string githubVersion;

        private async Task GetDownloadFromGithub(string repo)
        {
            //iiDk-the-actual/iis.Stupid.Menu example dingus
            string url = $"https://api.github.com/repos/{repo}/releases/latest";
            using HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.UserAgent.ParseAdd("CSharpApp");
            string response = await client.GetStringAsync(url);
            Newtonsoft.Json.Linq.JObject release = Newtonsoft.Json.Linq.JObject.Parse(response);
            githubDownload = release["assets"][0]["browser_download_url"]?.ToString() ?? "(no download)";
        }

        private async Task GetVersionFromGithub(string repo)
        {
            //iiDk-the-actual/iis.Stupid.Menu example dingus
            string url = $"https://api.github.com/repos/{repo}/releases/latest";
            using HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.UserAgent.ParseAdd("CSharpApp");
            string response = await client.GetStringAsync(url);
            Newtonsoft.Json.Linq.JObject release = Newtonsoft.Json.Linq.JObject.Parse(response);
            githubVersion = release["tag_name"]?.ToString() ?? "(no download)";
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string versionPath = Path.Combine(bananaDir, "banana_version.txt");
            version.Text = "Banana Version: " + currentVersion;
            File.WriteAllText(versionPath, currentVersion);

            label1.Text = gtaglocation;
            status.Text = "init path";

            disableenableupdate();
            UpdateVersions();
        }

        private void game_Click(object sender, EventArgs e)
        {
            string cleanPath = gtaglocation.Replace(@"\\", @"\");
            Process.Start("explorer.exe", cleanPath);
        }

        private void mods_Click(object sender, EventArgs e)
        {
            string cleanPath = gtaglocation.Replace(@"\\", @"\");
            Process.Start("explorer.exe", cleanPath + "\\BepInEx\\plugins");
        }

        public static void bepinexshit()
        {
            string downloadUrl = "https://github.com/BepInEx/BepInEx/releases/download/v5.4.23.4/BepInEx_win_x64_5.4.23.4.zip";
            string downloadPath = Path.Combine(Path.GetTempPath(), "BepInEx_win_x64_5.4.23.2.zip");
            string extractTempPath = Path.Combine(Path.GetTempPath(), "BepInExExtract");
            string targetPath = gtaglocation.Replace(@"\\", @"\");
            try
            {
                using (WebClient client = new WebClient())
                {
                    client.DownloadFile(downloadUrl, downloadPath);
                }

                if (Directory.Exists(extractTempPath))
                    Directory.Delete(extractTempPath, true);

                ZipFile.ExtractToDirectory(downloadPath, extractTempPath);

                CopyFilesRecursively(new DirectoryInfo(extractTempPath), new DirectoryInfo(targetPath));

                File.Delete(downloadPath);
                Directory.Delete(extractTempPath, true);
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message);
            }
            try
            {
                Directory.CreateDirectory(targetPath + "\\BepInEx\\plugins");
                Directory.CreateDirectory(targetPath + "\\BepInEx\\config");
                using (WebClient client = new WebClient())
                    File.WriteAllText(targetPath + "\\BepInEx\\config\\BepInEx.cfg", client.DownloadString($"{baseUrl}config.txt"));
            }
            catch ( Exception ex )
            {
                MessageBox.Show("An error occurred while creating the plugins directory or writing the config file: " + ex.Message);
            }
        }


        public static void ueZip() //chatgpt bepinex shit yes yes ik wha ta skid fuck you
        {
            string downloadUrl = $"{baseUrl}Banana/ModFiles/UnityFixV3_LTS.zip";
            string downloadPath = Path.Combine(Path.GetTempPath(), "UnityFixV3_LTS.zip");
            string extractTempPath = Path.Combine(Path.GetTempPath(), "UEExtract");
            string targetPath = gtaglocation.Replace(@"\\", @"\") + "\\BepInEx\\plugins";
            try
            {
                using (WebClient client = new WebClient())
                {
                    client.DownloadFile(downloadUrl, downloadPath);
                }

                if (Directory.Exists(extractTempPath))
                    Directory.Delete(extractTempPath, true);

                ZipFile.ExtractToDirectory(downloadPath, extractTempPath);

                CopyFilesRecursively(new DirectoryInfo(extractTempPath), new DirectoryInfo(targetPath));

                File.Delete(downloadPath);
                Directory.Delete(extractTempPath, true);
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message);
            }
        }

        private static void CopyFilesRecursively(DirectoryInfo source, DirectoryInfo target) //chatgpt too pal
        {
            foreach (var directory in source.GetDirectories())
            {
                var targetSubDir = target.CreateSubdirectory(directory.Name);
                CopyFilesRecursively(directory, targetSubDir);
            }

            foreach (var file in source.GetFiles())
            {
                var targetFilePath = Path.Combine(target.FullName, file.Name);
                file.CopyTo(targetFilePath, true);
            }
        }

        public static void DownloadFromRepo(string mod, string to)
        {
            new WebClient().DownloadFile($"{baseUrl}Banana/ModFiles/{mod}", to);
        }

        public (CheckBox checkBox, string repo, string outputFile, string statusText, Label versionlabel)[] githubMods
        {
            get
            {
                return new (CheckBox checkBox, string repo, string outputFile, string statusText, Label versionlabel)[]
                {
                       (utilla, "iiDk-the-actual/Utilla-Public", "Utilla.dll", "utilla", utillav),
                       (iidk, "iiDk-the-actual/iis.Stupid.Menu", "iis Stupid Menu.dll", "iidk menu sigma", iiv),
                       (sodium, "TAGMONKE/Sodium", "Sodium.dll", "sodium", sodiumv),
                       (forpreds, "iiDk-the-actual/ForeverPreds", "ForeverPreds.dll", "forever preds", predv),
                       (forhz, "iiDk-the-actual/ForeverHz", "ForeverHz.dll", "hz mod", hzv),
                       (cosm, "iiDk-the-actual/ForeverCosmetx", "ForeverCosmetx.dll", "cosmetx", cosmetxv),
                       (media, "iiDk-the-actual/GorillaMedia", "GorillaMedia.dll", "media", mediav),
                       (pokruk, "iiDk-the-actual/iiCamMod", "iiCamMod.dll", "iicam", pokrukv),
                       (toomuchinfo, "iiDk-the-actual/TooMuchInfo", "TooMuchInfo.dll", "too much info", toomuchinfov),
                       (walksim, "iiDk-the-actual/WalkSim", "WalkSim.dll", "walksim", walksimv),
                       (draw, "drowsiiii/MonkeDraw-Drawing-Pad", "MonkeDrawing.dll", "draw", drawv),
                       (astra, "ASTRA228b/ASTRA-CLIENT", "ASTRA.CLIENT.dll", "astra", astraversion),
                };
            }
        }

        public (CheckBox checkBox, string fileName, string statusText)[] discordMods
        {
            get
            {
                return new (CheckBox checkBox, string fileName, string statusText)[]
                {
                    (flick, "unknown's DC Flick Mod.dll", "flick"),
                    (automod, "NoAutoMod.dll", "automod"),
                    (bans, "DavukssModdingServers.dll", "banservers"),
                    (noleaves, "No Leaves.dll", "leaves !!!"),
                };
            }
        }

        public async void UpdateVersions()
        {
            foreach (var (checkBox, repo, outputFile, statusText, versionlabel) in githubMods)
            {
                await GetVersionFromGithub(repo);
                versionlabel.Text = githubVersion;
            }
        }

        private async void download_Click(object sender, EventArgs e)
        {
            string pluginsloc;
            if (Directory.Exists(Path.Combine(gtaglocation, "BepInEx")))
                pluginsloc = Path.Combine(gtaglocation.Replace(@"\\", @"\"), "BepInEx", "plugins\\");
            else
                pluginsloc = Path.Combine(gtaglocation.Replace(@"\\", @"\"));

            try
            {
                if (bepinex.Checked)
                {
                    bepinexshit();
                    status.Text = "bepinex";
                    pluginsloc = Path.Combine(gtaglocation.Replace(@"\\", @"\"), "BepInEx", "plugins\\");
                }

                if (ue.Checked)
                {
                    ueZip();
                    status.Text = "ue";
                }

                foreach (var (checkBox, repo, outputFile, statusText, versionlabel) in githubMods)
                {
                    if (checkBox.Checked)
                    {
                        status.Text = statusText;
                        await GetDownloadFromGithub(repo);
                        w.DownloadFile(githubDownload, Path.Combine(pluginsloc, outputFile));
                    }
                }

                foreach (var (checkBox, fileName, statusText) in discordMods)
                {
                    if (checkBox.Checked)
                    {
                        status.Text = statusText;
                        DownloadFromRepo(fileName, Path.Combine(pluginsloc, fileName));
                    }
                }

                MessageBox.Show("Finished installing mods!");
            }
            catch
            {
                MessageBox.Show("An error occurred while installing mods. Please check your Gorilla Tag directory and try again.");
            }
        }


        private void disableenable_Click(object sender, EventArgs e)
        {
            if (disableenable.BackColor == Color.Red)
                System.IO.File.Move($"{gtaglocation}\\winhttp.dll", $"{gtaglocation}\\winhttp.d");
            else
                System.IO.File.Move($"{gtaglocation}\\winhttp.d", $"{gtaglocation}\\winhttp.dll");

            disableenableupdate();
        }

        private void disableenableupdate()
        {
            if (File.Exists(gtaglocation + "\\winhttp.dll"))
            {
                disableenable.BackColor = Color.Red;
                disableenable.Text = "Disable Mods";
            }
            else
            {
                disableenable.BackColor = Color.Green;
                disableenable.Text = "Enable Mods";
            }
        }

        private void discord_Click(object sender, EventArgs e)
        {
            var ps = new ProcessStartInfo("https://discord.gg/NtgqZkwuPy")
            {
                UseShellExecute = true,
                Verb = "open"
            };
            Process.Start(ps);
        }

        private void changelocation_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
            {
                folderDialog.Description = "Select your Gorilla Tag directory.";
                folderDialog.ShowNewFolderButton = true;

                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    string selectedPath = folderDialog.SelectedPath;
                    MessageBox.Show("Selected Folder: " + selectedPath);
                    // You can also set this to a TextBox or use it in your code
                    label1.Text = selectedPath;
                    gtaglocation = selectedPath;
                }
            }
        }
    }
}
