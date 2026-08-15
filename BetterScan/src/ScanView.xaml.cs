using System;
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
                this.model.TargetFolder = settings.Settings.TargetFolder;
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
            IEnumerable<string> plist
                = from x in settings.Settings.MatchPattern.Split(',')
                  select x.Trim();
            IEnumerable<string> slist
                = from x in settings.Settings.SkipPattern.Split(',')
                  select x.Trim();
            //List<string> plist = new List<string> { "*.exe", "*.bat" };
            var exFolder = Impl.GetExcludedDir(plugin.PlayniteApi);
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
                        IconData = Helper.GetIconData(item.FullName)///
                    };
                    arr.Add(c);
                }
                model.CandidateList = arr;
                this.SavePath();
            }
            catch (Exception E)
            {
                logger.Error(E.Message);
                plugin.PlayniteApi.Dialogs.ShowErrorMessage(E.Message, " Error");
            }
        }
        private void SavePath()
        {
            settings.BeginEdit();
            settings.Settings.TargetFolder = this.model.TargetFolder;
            settings.EndEdit();
        }







        private void ClickAdd(object sender, RoutedEventArgs e)
        {
            List<Candidate> leftover = new List<Candidate>();
            foreach (Candidate item in model.CandidateList)
            {
                if (item.Selected)
                {
                    Game g = Impl.GenGame(item, plugin.PlayniteApi, settings.Settings);
                    try
                    {
                        g.Icon = plugin.PlayniteApi.Database.AddFile(g.Icon, g.Id);
                    }
                    catch (Exception)
                    {
                    }
                    plugin.PlayniteApi.Database.Games.Add(g);
                }
                else
                {
                    leftover.Add(item);
                }
            }

            model.CandidateList = leftover;
        }

        private void ClickSelectAll(object sender, RoutedEventArgs e)
        {
            bool allChecked = model.CandidateList.All(_c => _c.Selected);
            if (allChecked)
            {
                foreach (Candidate c in model.CandidateList)
                {
                    c.Selected = false;
                }
            }
            else
            {
                foreach (Candidate c in model.CandidateList)
                {
                    c.Selected = true;
                }
            }
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
