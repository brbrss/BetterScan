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
using System.Threading.Tasks;
using System.Threading;

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
                this.model.OptionEarlyStop = settings.Settings.OptionEarlyStop;
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
            Window.GetWindow(this).Title = "Better Scan Folder";
            //plugin.PlayniteApi.Dialogs.ShowMessage(folder);
        }



        private async void ClickScan(object sender, RoutedEventArgs e)
        {
            string path = model.TargetFolder;
            IEnumerable<string> exFolder = Impl.GetExcludedDir(plugin.PlayniteApi);
            IEnumerable<string> plist
                = from x in settings.Settings.MatchPattern.Split(',')
                  select x.Trim();
            IEnumerable<string> slist
                = from x in settings.Settings.SkipPattern.Split(',')
                  select x.Trim();
            bool earlyStop = model.OptionEarlyStop;

            Window.GetWindow(this).Title = "Scan in progress. This may take a while.";
            model.CandidateList = new List<Candidate>();
            var task = Task.Run(() =>
            {
                //Thread.Sleep(8000);
                var res = Helper.Search(path, exFolder, plist, slist, earlyStop);
                return res;
            });
            try
            {
                var res = await task;
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
                this.SaveSettings();
                Window.GetWindow(this).Title = "Scan Result of " + model.TargetFolder;
            }
            catch (Exception E)
            {
                logger.Error(E.Message);
                plugin.PlayniteApi.Dialogs.ShowErrorMessage(E.Message, " Error");
            }

        }
        private void SaveSettings()
        {
            settings.BeginEdit();
            settings.Settings.TargetFolder = this.model.TargetFolder;
            settings.Settings.OptionEarlyStop = model.OptionEarlyStop;
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
                        string iconTempPath = Helper.CreateIconTemp(item.EntryFilePath, settings.Settings);
                        g.Icon = plugin.PlayniteApi.Database.AddFile(iconTempPath, g.Id);
                        File.Delete(iconTempPath);
                    }
                    catch (Exception E)
                    {
                        logger.Error(E.Message);
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
