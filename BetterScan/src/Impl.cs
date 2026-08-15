using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Playnite.SDK;
using Playnite.SDK.Models;

using System.Collections.ObjectModel;


namespace BetterScan
{

    public struct VerifyRes
    {
        // number of games with invalid paths
        public int ndir;
        public int nrom;
        public int nact;
    };

    public class Impl
    {
        static public string TagInvalidDir = "__INVALID_DIR";
        static public string TagInvalidRom = "__INVALID_ROM";
        static public string TagInvalidAction = "__INVALID_ACTION";

        private static void addTag(IPlayniteAPI api, Game game, Guid tid)
        {
            if (game.TagIds is null)
            {
                game.TagIds = new List<Guid>();
            }
            if (!game.TagIds.Contains(tid))
            {
                game.TagIds.Add(tid);
            }
            api.Database.Games.Update(game);
        }

        private static void removeTag(IPlayniteAPI api, Game game, Guid tid)
        {
            if (game.TagIds is null)
            {
                return;
            }
            game.TagIds.Remove(tid);
            api.Database.Games.Update(game);
        }

        private static bool VerifyFolder(IPlayniteAPI api, Game game)
        {
            string dir = game.InstallDirectory;
            string path = api.ExpandGameVariables(game, dir);
            Tag tag = api.Database.Tags.Add(TagInvalidDir);


            if (!Directory.Exists(path))
            {
                addTag(api, game, tag.Id);
                return false;
            }
            removeTag(api, game, tag.Id);
            return true;
        }

        private static bool VerifyRom(IPlayniteAPI api, Game game)
        {
            var romList = game.Roms;
            if (romList == null)
            {
                return true;
            }

            Tag tag = api.Database.Tags.Add(TagInvalidRom);
            foreach (var rom in romList)
            {
                string path = rom.Path;
                path = api.ExpandGameVariables(game, path);

                if (!File.Exists(path))
                {
                    addTag(api, game, tag.Id);
                    return false;
                }
            }
            removeTag(api, game, tag.Id);

            return true;
        }

        private static bool VerifyAction(IPlayniteAPI api, Game game)
        {
            var actionList = game.GameActions;
            if (actionList == null)
            {
                return true;
            }

            Tag tag = api.Database.Tags.Add(TagInvalidAction);
            foreach (var action in actionList)
            {
                string path = action.Path;
                path = api.ExpandGameVariables(game, path);

                if (!File.Exists(path))
                {
                    addTag(api, game, tag.Id);
                    return false;
                }
            }
            removeTag(api, game, tag.Id);

            return true;
        }

        public static VerifyRes VerifyPath(IPlayniteAPI api)
        {
            IEnumerable<Playnite.SDK.Models.Game> gameList
               = from g in api.Database.Games
                 where g.IsInstalled
                 select g;
            int ndir = 0;
            int nrom = 0;
            int nact = 0;
            foreach (Game game in gameList)
            {
                ndir += !VerifyFolder(api, game) ? 1 : 0;
                nrom += !VerifyRom(api, game) ? 1 : 0;
                nact += !VerifyAction(api, game) ? 1 : 0;
            }
            VerifyRes res;
            res.ndir = ndir;
            res.nrom = nrom;
            res.nact = nact;
            return res;
            //api.Dialogs.ShowMessage("text", "caption");
        }

        public static Game GenGame(Candidate candi, IPlayniteAPI api, BetterScanSettings settings)
        {
            const string placeholder = "{PlayniteDir}";
            string folder = Helper.GetFolderPath(candi.EntryFilePath);
            string fpname = new FileInfo(candi.EntryFilePath).Name;
            string root = api.Paths.ApplicationPath;

            string installDir =
                settings.OptionRelPath ?
                Helper.ToRelPath(folder, root, placeholder)
                : folder;
            string name =
                settings.OptionUseFolder ?
                 Helper.GetFolderName(candi.EntryFilePath) :
                 Path.GetFileNameWithoutExtension(candi.EntryFilePath);
            Game g = new Game
            {
                Name = name,
                InstallDirectory = installDir,
                IsInstalled = true,
                Icon = Helper.GetIconTempPath(candi.EntryFilePath, settings)
            };
            GameAction action = new GameAction
            {
                IsPlayAction = true,
                Type = GameActionType.File,
                Name = fpname,
                Path = "{InstallDir}\\" + fpname,
                WorkingDir = "{InstallDir}"
            };
            g.GameActions = new ObservableCollection<GameAction> { action };

            return g;
        }


        public static IEnumerable<string> GetExcludedDir(IPlayniteAPI api)
        {
            const string varname = "{PlayniteDir}";
            string basePath = api.Paths.ApplicationPath;

            IEnumerable<string> rawDirList
                = from g in api.Database.Games
                  where !string.IsNullOrEmpty(g.InstallDirectory)
                  select g.InstallDirectory.Replace(varname, basePath);

            return from fp in rawDirList
                   select Helper.ResolveRelPath(fp);
        }
    }
};

