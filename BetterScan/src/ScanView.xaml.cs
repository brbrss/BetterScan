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
        private void ClickScan(object sender, RoutedEventArgs e)
        {
            string path = model.TargetFolder;
            List<string> plist = new List<string> { "*.exe", "*.bat" };
            try
            {
                List<System.IO.FileInfo> res = Helper.Search(path, plist);
                var arr = new List<Candidate>();
                foreach (var item in res)
                {
                    Candidate c = new Candidate
                    {
                        Selected = false,
                        FilePath = item.FullName
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
        private Game GenGame(string fp)
        {
            const string placeholder = "{PlayniteDir}";
            string folder = GetFolderPath(fp);
            string fpname = new FileInfo(fp).Name;
            string root = plugin.PlayniteApi.Paths.ApplicationPath;

            string installDir = Helper.ToRelPath(folder, root, placeholder);
            Game g = new Game
            {
                Name = GetFolderName(fp),
                InstallDirectory = installDir,
                IsInstalled = true,
            };
            GameAction action = new GameAction
            {
                IsPlayAction = true,
                Type = GameActionType.File,
                Name = fpname,
                Path = Helper.ConcatPath(installDir, fpname),
                WorkingDir = installDir
            };
            g.GameActions = new ObservableCollection<GameAction> { action };

            return g;
        }

        private void ClickAdd(object sender, RoutedEventArgs e)
        {
            List<Candidate> leftover = new List<Candidate>();
            foreach (var item in model.CandidateList)
            {
                if (item.Selected)
                {
                    Game g = GenGame(item.FilePath);
                    plugin.PlayniteApi.Database.Games.Add(g);
                }
                else
                {
                    leftover.Add(item);
                }
            }

            model.CandidateList = leftover;
        }

        private string toRelFolder(string absFolder)
        {
            string s = plugin.PlayniteApi.Paths.ApplicationPath;
            plugin.PlayniteApi.Dialogs.ShowMessage(s + '\n' + absFolder);
            return absFolder;
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
