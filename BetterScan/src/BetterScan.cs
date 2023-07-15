using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;


using System.Windows;



namespace BetterScan
{
    public class BetterScan : GenericPlugin
    {
        private static readonly ILogger logger = LogManager.GetLogger();

        private BetterScanSettingsViewModel Settings { get; set; }

        public override Guid Id { get; } = Guid.Parse("cfa84ecf-cb29-4aa2-bbac-c9aa5003cc22");

        public BetterScan(IPlayniteAPI api) : base(api)
        {
            Settings = new BetterScanSettingsViewModel(this);
            Properties = new GenericPluginProperties
            {
                HasSettings = true
            };
        }

        public override IEnumerable<MainMenuItem> GetMainMenuItems(GetMainMenuItemsArgs args)
        {
            yield return new MainMenuItem
            {
                MenuSection = "@Better Scan",
                Description = "Scan folder...",
                Action = CreateView
            };
        }

        private void CreateView(MainMenuItemActionArgs args)
        {
            try
            {
                ScanView view = new ScanView(this,Settings);
                var window = PlayniteApi.Dialogs.CreateWindow(new WindowCreationOptions
                {
                    ShowMinimizeButton = false,
                });

                window.Height = 350;
                window.Width = 650;
                window.Title = "Better Scan Folder";

                window.Content = view;

                window.Owner = PlayniteApi.Dialogs.GetCurrentAppWindow();
                window.WindowStartupLocation = WindowStartupLocation.CenterOwner;

                // Use Show or ShowDialog to show the window
                window.ShowDialog();
            }
            catch (Exception E)
            {
                logger.Error(E, "Error during initializing GameImportView");
                PlayniteApi.Dialogs.ShowErrorMessage(E.Message, "Error during DoImportGames");
            }
        }
        private void Foo(MainMenuItemActionArgs args)
        {
            Game newGame = new Game("New Game")
            {
                InstallDirectory = "{PlayniteDir}/../"
            };

            //PlayniteApi.Database.Games.Add(newGame);
        }

        public override void OnGameInstalled(OnGameInstalledEventArgs args)
        {
            // Add code to be executed when game is finished installing.
        }

        public override void OnGameStarted(OnGameStartedEventArgs args)
        {
            // Add code to be executed when game is started running.
        }

        public override void OnGameStarting(OnGameStartingEventArgs args)
        {
            // Add code to be executed when game is preparing to be started.
        }

        public override void OnGameStopped(OnGameStoppedEventArgs args)
        {
            // Add code to be executed when game is preparing to be started.
        }

        public override void OnGameUninstalled(OnGameUninstalledEventArgs args)
        {
            // Add code to be executed when game is uninstalled.
        }

        public override void OnApplicationStarted(OnApplicationStartedEventArgs args)
        {
            // Add code to be executed when Playnite is initialized.
        }

        public override void OnApplicationStopped(OnApplicationStoppedEventArgs args)
        {
            // Add code to be executed when Playnite is shutting down.
        }

        public override void OnLibraryUpdated(OnLibraryUpdatedEventArgs args)
        {
            // Add code to be executed when library is updated.
        }

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return Settings;
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new BetterScanSettingsView();
        }
    }
}