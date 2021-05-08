using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordTokenProtector.INISystem
{
    class StartUpChecker
    {
       public static void main()
        {
            if (!Directory.Exists(PATHFILES.MAIN))
            {
                Directory.CreateDirectory(PATHFILES.MAIN);
            }
            if (!File.Exists(PATHFILES.CONFIGFILE))
            {
                var MyIni = new INIFile(PATHFILES.CONFIGFILE);
                //General Infos
                MyIni.Write("SAFEPATH", "false", "SETTINGS");

                //Saved Discordpath
                MyIni.Write("DISCORD", "[NOT ADDED]", "DISCORDPATHSAVE");
            }
        }
    }
}
