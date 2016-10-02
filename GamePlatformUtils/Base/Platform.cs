using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamePlatformUtils
{
    public class GameEventArgs : EventArgs {
        public Game Game { get; set; }
    }

    public class Platform
    {
        public event EventHandler<GameEventArgs> GameAdded;

        public string InstallPath { get; set; }

        protected Dictionary<string, Game> _Games = new Dictionary<string, Game>();
        public Dictionary<string, Game> Games { get { return this._Games; } set { this._Games = value; } }

        public Platform()
        {
            this.LoadData();
        }

        protected void GameAddedTrigger(GameEventArgs e)
        {
            this.GameAdded?.Invoke(this, e);
        }

        public virtual void LoadData()
        {
            this.LoadGames();
        }

        public virtual void LoadGames()
        {

        }

    }
}
