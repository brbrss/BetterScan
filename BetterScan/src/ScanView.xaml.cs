﻿using System;
using System.Collections.Generic;
using System.Linq;
using Playnite.SDK.Models;
using System.Windows;
using System.Windows.Controls;
using Playnite.SDK;
using System.Windows.Data;
using System.IO;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;

namespace BetterScan
{
    /// <summary>
    /// Interaction logic for ScanView.xaml
    /// </summary>
    public partial class ScanView : UserControl
    {
        private readonly BetterScan plugin;
        private readonly BetterScanSettingsViewModel settings;
        private static readonly ILogger logger = LogManager.GetLogger();
        public ScanViewModel model = new ScanViewModel();

        public ScanView()
        {
            InitializeComponent();
        }
        public ScanView(BetterScan plugin, BetterScanSettingsViewModel settings)
        {
            try
            {
                this.plugin = plugin;
                this.settings = settings;
                DataContext = model;
                InitializeComponent();
            }
            catch (Exception E)
            {
                logger.Error(E, "Error during initializing GameImportView");
                plugin.PlayniteApi.Dialogs.ShowErrorMessage(E.Message, "Error during initializing GameImportView");
            }
        }
        private void ClickSelectFolder(object sender, RoutedEventArgs e)
        {
            string folder = plugin.PlayniteApi.Dialogs.SelectFolder();
            model.TargetFolder = folder;
            //if (settings.Settings.OptionRelPath)
            //{
            //    folder = toRelFolder(folder);
            //}
            Window.GetWindow(this).Title = "Scan Result of " + folder;
            //plugin.PlayniteApi.Dialogs.ShowMessage(folder);
        }

        private IEnumerable<string> GetExcludedDir()
        {
            const string varname = "{PlayniteDir}";
            string basePath = plugin.PlayniteApi.Paths.ApplicationPath;

            IEnumerable<string> rawDirList
                = from g in plugin.PlayniteApi.Database.Games
                  where !string.IsNullOrEmpty(g.InstallDirectory)
                  select g.InstallDirectory.Replace(varname, basePath);

            return from fp in rawDirList
                   select Helper.ResolveRelPath(fp);
        }
        private void ClickScan(object sender, RoutedEventArgs e)
        {
            string path = model.TargetFolder;
            IEnumerable<string> plist
                = from x in settings.Settings.MatchPattern.Split(',')
                  select x.Trim();
            IEnumerable<string> slist
                = from x in settings.Settings.SkipPattern.Split(',')
                  select x.Trim();
            //List<string> plist = new List<string> { "*.exe", "*.bat" };
            var exFolder = GetExcludedDir();
            try
            {
                List<FileInfo> res = Helper.Search(path, exFolder, plist, slist);
                var arr = new List<Candidate>();
                foreach (var item in res)
                {
                    Candidate c = new Candidate
                    {
                        Selected = false,
                        EntryFilePath = item.FullName,
                        IconData = GetIconData(item.FullName)///
                    };
                    arr.Add(c);
                }
                model.CandidateList = arr;
            }
            catch (Exception E)
            {
                logger.Error(E.Message);
                plugin.PlayniteApi.Dialogs.ShowErrorMessage(E.Message, " Error");
            }

        }

        static private string GetFolderPath(string fp)
        {
            return new FileInfo(fp).Directory.ToString();
        }
        static private string GetFolderName(string fp)
        {
            return new FileInfo(fp).Directory.Name;
        }
        private static string SearchIcoFile(string fp)
        {
            string folderName = Path.GetDirectoryName(fp);
            DirectoryInfo di = new DirectoryInfo(folderName);
            foreach (FileInfo fi in di.EnumerateFiles())
            {
                if (fi.Extension.ToLower() == ".ico")
                {
                    return fi.FullName;
                }
            }
            return null;
        }
        private string GetIconTempPath(string fp)
        {
            string res = null;
            try
            {
                var iconex = new TsudaKageyu.IconExtractor(fp);
                if (iconex.Count > 0)
                {
                    string tempfp = Path.GetTempPath() + Guid.NewGuid().ToString() + ".png";
                    var f = new FileStream(tempfp, FileMode.CreateNew);
                    iconex.Save(0, f);
                    res = tempfp;
                }

            }
            catch (Exception)
            {

            }
            if (res == null && settings.Settings.OptionSearchAdjacentIco)
            {
                string iconPath = SearchIcoFile(fp);
                if (iconPath == null) {
                    return res;
                }
                string tempfp = Path.GetTempPath() + Guid.NewGuid().ToString() + ".png";
                File.Copy(iconPath, tempfp);
                res = tempfp;
            }
            return res;
        }

        static private System.Windows.Media.ImageSource GetIconData(string fp)
        {
            try
            {
                TsudaKageyu.IconExtractor iconex = new TsudaKageyu.IconExtractor(fp);
                System.Drawing.Icon icon = iconex.Count > 0 ? iconex.GetIcon(0) : null;
                return System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
                 icon.Handle,
                 new Int32Rect(0, 0, icon.Width, icon.Height),
                 BitmapSizeOptions.FromEmptyOptions());
            }
            catch (Exception)
            {
                return null;
            }
        }


        private Game GenGame(Candidate candi)
        {
            const string placeholder = "{PlayniteDir}";
            string folder = GetFolderPath(candi.EntryFilePath);
            string fpname = new FileInfo(candi.EntryFilePath).Name;
            string root = plugin.PlayniteApi.Paths.ApplicationPath;

            string installDir =
                settings.Settings.OptionRelPath ?
                Helper.ToRelPath(folder, root, placeholder)
                : folder;
            string name =
                settings.Settings.OptionUseFolder ?
                 GetFolderName(candi.EntryFilePath) :
                 Path.GetFileNameWithoutExtension(candi.EntryFilePath);
            Game g = new Game
            {
                Name = name,
                InstallDirectory = installDir,
                IsInstalled = true,
                Icon = GetIconTempPath(candi.EntryFilePath)
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

        private void ClickAdd(object sender, RoutedEventArgs e)
        {
            List<Candidate> leftover = new List<Candidate>();
            foreach (Candidate item in model.CandidateList)
            {
                if (item.Selected)
                {
                    Game g = GenGame(item);
                    plugin.PlayniteApi.Database.Games.Add(g);
                }
                else
                {
                    leftover.Add(item);
                }
            }

            model.CandidateList = leftover;
        }


    }


    public class EmptyPathConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (value is "") ? "(No Folder Selected)" : value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
