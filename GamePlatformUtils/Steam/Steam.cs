using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using GamePlatformUtils.Steam.Utils;

namespace GamePlatformUtils.Steam
{
    public class Steam : Platform<SteamGame>
    {
        private SteamUser _LoggedInUser;

        /// <summary>
        /// SteamUser instance for the user that is logged into the current Steam instance
        /// </summary>
        public SteamUser LoggedInUser {
            get
            {
                return this._LoggedInUser;
            }
            protected set
            {
                bool changed = false;
                var previous = this._LoggedInUser;
                if (!value.Equals(previous))
                    changed = true;

                this._LoggedInUser = value;

                if (changed)
                    InvokePropertyChanged(previous, value);
            }
        }

        private List<string> _LibraryFolders = new List<string>();


        public List<string> LibraryFolders { get { return this._LibraryFolders; } set { this._LibraryFolders = value; } }

        private bool _BigPictureOpen = false;

        /// <summary>
        /// Bool that indicates whether Steam Big Picture is open and in Focus
        /// </summary>
        public bool BigPictureOpen { get { return this._BigPictureOpen; }
            set
            {
                bool changed = false;
                bool previous = this._BigPictureOpen;
                if (value != previous)
                    changed = true;

                this._BigPictureOpen = value;

                if (changed)
                    InvokePropertyChanged(previous, value);
            }
        }

        private string _Language = null;

        public string Language {
            get { return _Language; }
            set {
                bool changed = false;
                var previous = this._Language;
                if (value != previous)
                    changed = true;
                _Language = value;
                if (changed)
                    InvokePropertyChanged(previous, value);
            }
        }

        public Steam() : base()
        {
            this.SetupUpdateListeners();
        }

        protected override void CheckIfInstalled()
        {
            string path = null;

            //TODO: Implement alternative discovery method for linux
#if LINUX
#else
            string[] reg_values = { @"HKEY_LOCAL_MACHINE\SOFTWARE\Valve\Steam", @"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Valve\Steam" };
            foreach (string reg in reg_values)
            {
                try
                {
                    string install_path = (string)Registry.GetValue(reg, "InstallPath", null);
                    if (!string.IsNullOrWhiteSpace(install_path) && File.Exists(Path.Combine(install_path, "Steam.exe")))
                    {
                        path = install_path;
                        break;
                    }
                }
                catch (Exception exc) { }
            }
#endif
            InstallPath = path;
            this.IsInstalled = !string.IsNullOrWhiteSpace(path);
        }

        protected override void LoadData()
        {
            base.LoadData();
            this.LoadRegistryValues();
        }

        private void LoadRegistryValues()
        {
            this.CheckMainRegistryValues();
        }

        private void CheckMainRegistryValues()
        {
            int big_picture = (int)Registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\Valve\Steam", "BigPictureInForeground", null);
            this.BigPictureOpen = big_picture != 0;
            this.Language = (string)Registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\Valve\Steam", "Language", null);
        }

        private void CheckActiveProcessRegistryValues()
        {
            int pid = (int)Registry.GetValue(@"HKEY_CURRENT_USER\Software\Valve\Steam\ActiveProcess", "pid", 0);
            ActiveProcess = pid != 0 ? Process.GetProcessById(pid) : null;
            int activeUserID = (int)Registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\Valve\Steam\ActiveProcess", "ActiveUser", 0);
            this.LoggedInUser = new SteamUser(this, activeUserID);
        }

        private List<FileSystemWatcher> Watchers = new List<FileSystemWatcher>();
        private List<RegistryUtils.RegistryMonitor> RegWatchers = new List<RegistryUtils.RegistryMonitor>();
        protected virtual void SetupUpdateListeners()
        {
            //TODO: Implement alternative method for monitoring linux values. ~Monitor changes to files stored in /home/.steam/ (AFAIK) as they are the equivalent of the registry entries.
#if LINUX
#else
            RegistryUtils.RegistryMonitor reg_watcher = new RegistryUtils.RegistryMonitor(RegistryHive.CurrentUser, @"SOFTWARE\Valve\Steam");
            //reg_watcher.RegChangeNotifyFilter = RegistryUtils.RegChangeNotifyFilter.Value;
            reg_watcher.RegChanged += (s,e) => CheckMainRegistryValues();
            reg_watcher.Start();
            RegWatchers.Add(reg_watcher);

            reg_watcher = new RegistryUtils.RegistryMonitor(RegistryHive.CurrentUser, @"SOFTWARE\Valve\Steam\ActiveProcess");
            //reg_watcher.RegChangeNotifyFilter = RegistryUtils.RegChangeNotifyFilter.Value;
            reg_watcher.RegChanged += (s, e) => CheckActiveProcessRegistryValues();
            reg_watcher.Start();
            RegWatchers.Add(reg_watcher);
#endif

            foreach (string library in this.LibraryFolders)
            {
                FileSystemWatcher appwatcher = new FileSystemWatcher(library, "appmanifest_*.acf")
                { EnableRaisingEvents = true, NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.CreationTime | NotifyFilters.Size };
                appwatcher.Changed += this.LibraryAppFileChanged;
                appwatcher.Created += this.LibraryAppFileChanged;
                this.Watchers.Add(appwatcher);
            }

            string main_path = Path.Combine(this.InstallPath, SteamConstants.SteamAppsDirectory);
            FileSystemWatcher lib_watcher = new FileSystemWatcher(main_path, "libraryfolders.vdf")
            { EnableRaisingEvents = true, NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.CreationTime | NotifyFilters.Size };
            lib_watcher.Changed += this.LibraryFileChanged;
            lib_watcher.Created += this.LibraryFileChanged;
            this.Watchers.Add(lib_watcher);

        }

        private void LibraryAppFileChanged(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType == WatcherChangeTypes.Changed)
            {
                string ID = Path.GetFileNameWithoutExtension(e.Name).Substring("appmanifest_".Length);
                if (this.Games.ContainsKey(ID))
                    this.Games[ID].Reload();
            }
            else if (e.ChangeType == WatcherChangeTypes.Created)
            {
                SteamGame game = new SteamGame(e.FullPath);
                if (!this.Games.ContainsKey(game.ID))
                {
                    this.Games.Add(game.ID, game);
                    this.GameAddedTrigger(new GameEventArgs { Game = game });
                }
            }
        }

        private void LibraryFileChanged(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType.HasFlag(WatcherChangeTypes.Changed) || e.ChangeType.HasFlag(WatcherChangeTypes.Created))
                this.LoadAdditionalLibraries(e.FullPath);
        }

        protected override void LoadGames()
        {
            this.Games.Clear();

            string main_path = Path.Combine(this.InstallPath, SteamConstants.SteamAppsDirectory);
            this.LoadGames(main_path);
            string lib_path = Path.Combine(main_path, "libraryfolders.vdf");
            if (File.Exists(lib_path))
                this.LoadAdditionalLibraries(lib_path);
        }

        protected void LoadAdditionalLibraries(string lib_path)
        {
            KeyValueTable libraries = new KeyValue(new FileStream(lib_path, FileMode.Open, FileAccess.Read)).RootNode.SubTables["libraryfolders"];

            for (int i = 1; libraries?.Attributes?.ContainsKey(i.ToString()) ?? false; i++)
            {
                string add_lib_path = Path.Combine(libraries.Attributes[i.ToString()].Value, SteamConstants.SteamAppsDirectory);
                if (!this.LibraryFolders.Contains(add_lib_path))
                    this.LoadGames(add_lib_path);
            }
        }

        protected void LoadGames(string path)
        {
            if (path == null || !Directory.Exists(path))
                return;

            this.LibraryFolders.Add(path);

            foreach(string file in Directory.EnumerateFiles(path, "appmanifest_*.acf"))
            {
                SteamGame game = new SteamGame(file);
                if (!this.Games.ContainsKey(game.ID))
                    this.Games.Add(game.ID, game);
                else
                    Console.WriteLine("Game with ID '{0}' already exists in the Games dictionary!", game.ID);
            }
        }
    }
}
