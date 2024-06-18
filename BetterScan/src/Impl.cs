using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Playnite.SDK;
using Playnite.SDK.Models;


namespace BetterScan
{
    public class Impl
    {
        static public string TagInvalidDir = "__INVALID_DIR";
        static public string TagInvalidRom = "__INVALID_ROM";
        static public string TagInvalidAction = "__INVALID_ACTION";

        private static void VerifyFolder(IPlayniteAPI api, Game game)
        {
            string dir = game.InstallDirectory;
            api.ExpandGameVariables(game, dir);
            Tag tag = api.Database.Tags.Add(TagInvalidDir);

            if (!Directory.Exists(dir))
            {
                var tagList = game.TagIds;
                tagList.Add(tag.Id);
                api.Database.Games.Update(game);
            }

        }

        private static void VerifyRom(IPlayniteAPI api, Game game)
        {
            var romList = game.Roms;
            if (romList == null) {
                return;
            }
            foreach (var rom in romList)
            {
                string path = rom.Path;
                api.ExpandGameVariables(game, path);

                if (!File.Exists(path))
                {

                }
            }


        }

        public static void VerifyPath(IPlayniteAPI api)
        {
            IEnumerable<Playnite.SDK.Models.Game> gameList
               = from g in api.Database.Games
                 where g.IsInstalled
                 select g;
            foreach (Game game in gameList)
            {
                VerifyFolder(api, game);
                VerifyRom(api, game);
            }
            //api.Dialogs.ShowMessage("text", "caption");
        }
    }
};

