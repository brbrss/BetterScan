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
    public partial class VerifyView : UserControl
    {
        private readonly BetterScan plugin;
        private readonly BetterScanSettingsViewModel settings;
        private static readonly ILogger logger = LogManager.GetLogger();
        public VerifyViewModel model = new VerifyViewModel();

        public VerifyView()
        {
            InitializeComponent();
        }
        public VerifyView(BetterScan plugin, BetterScanSettingsViewModel settings)
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




        private void ClickDo(object sender, RoutedEventArgs e)
        {

        }

        private void ClickCancel(object sender, RoutedEventArgs e)
        {
           
        }


    }


 
}
