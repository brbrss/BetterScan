using System;
using System.Collections.Generic;
using System.Linq;
using Playnite.SDK.Models;
using System.Windows;
using System.Windows.Controls;
using Playnite.SDK;


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
                InitializeComponent();
                DataContext = this;
            }
            catch (Exception E)
            {
                logger.Error(E, "Error during initializing GameImportView");
                plugin.PlayniteApi.Dialogs.ShowErrorMessage(E.Message, "Error during initializing GameImportView");
            }
        }
        private void ClickScanFolder(object sender, RoutedEventArgs e)
        {
            string folder = plugin.PlayniteApi.Dialogs.SelectFolder();
            if (settings.Settings.OptionRelPath)
            {
                folder = toRelFolder(folder);
            }
            Window.GetWindow(this).Title = "Scan Result of " + folder;
            //plugin.PlayniteApi.Dialogs.ShowMessage(folder);
        }
        private string toRelFolder(string absFolder)
        {
            string s = plugin.PlayniteApi.Paths.ApplicationPath;
            plugin.PlayniteApi.Dialogs.ShowMessage(s + '\n' + absFolder);
            return absFolder;
        }
    }
}
