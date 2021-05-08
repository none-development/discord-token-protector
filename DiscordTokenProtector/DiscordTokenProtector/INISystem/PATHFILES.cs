using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordTokenProtector.INISystem
{
    class PATHFILES
    {
        public static string LOCALROW = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        public static string CONFIGFILE = LOCALROW + @"\DiscordSaver\config.ini";
        public static string MAIN = Path.Combine(LOCALROW, @"DiscordSaver\");
    }
}
