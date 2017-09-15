using GamePlatformUtils.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GamePlatformUtils
{
    public class GameEventArgs : EventArgs {
        public Game Game { get; set; }
    }

    public class Platform<TGame> : NotifyPropertyChangedEx
    {
        public event EventHandler<GameEventArgs> GameAdded;

        public string InstallPath { get; protected set; }

        private Process _ActiveProcess = null;

        public Process ActiveProcess { get { return _ActiveProcess; }
            protected set {
                bool changed = false;
                var previous = _ActiveProcess;
                if (value != previous)
                    changed = true;

                _ActiveProcess = value;
                if (changed)
                    InvokePropertyChanged(previous, value);
            }
        }

        public bool Running { get { return ActiveProcess != null; } }

        protected Dictionary<string, TGame> _Games = new Dictionary<string, TGame>();
        public Dictionary<string, TGame> Games { get { return this._Games; } set { this._Games = value; } }

        /// <summary>
        /// Indicates whether the specified platform is installed. This must be true if this instance is going to be used.
        /// </summary>
        public bool IsInstalled { get; protected set; }

        public Platform()
        {
            this.LoadData();
        }

        protected void GameAddedTrigger(GameEventArgs e)
        {
            this.GameAdded?.Invoke(this, e);
        }

        protected virtual void CheckIfInstalled()
        {
            IsInstalled = false;
        }

        protected virtual void LoadData()
        {
            this.CheckIfInstalled();

            if (!IsInstalled)
                return;

            this.LoadGames();
        }

        protected virtual void LoadGames()
        {

        }
    }
}
