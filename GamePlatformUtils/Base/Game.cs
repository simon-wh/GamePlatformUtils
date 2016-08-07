using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamePlatformUtils
{
    public enum GameStatus
    {
        NotInstalled,
        Installed,
        RequiresUpdate
    }

    public class Game
    {
        public string ID { get; set; }
        public string Title { get; set; }
        public GameStatus Status { get; set; }

        public string InstallDirectory { get; set; }

        public ulong Size { get; set; }

        public virtual void Reload()
        {

        }

    }
}
