using BandCentral.Models.Helpers;
using BandCentral.Uwp.Dialogs;
using System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace BandCentral.Uwp.Views
{
    public sealed partial class SettingsPage : Page
    {
        public NavigationHelper NavigationHelper { get; }

        public SettingsPage()
        {
            this.InitializeComponent();
            DataContext = App.ViewModel;

            Microsoft.HockeyApp.HockeyClient.Current.TrackPageView("SettingsPage");

            this.NavigationHelper = new NavigationHelper(this);
            this.NavigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.NavigationHelper.SaveState += this.NavigationHelper_SaveState;
        }

        #region NavigationHelper registration

        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
        }

        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.NavigationHelper.OnNavigatedTo(e);
            
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.NavigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private async void OpenBandSettingsButton_OnClick(object sender, RoutedEventArgs e)
        {
            await new ConnectToBandDialog().ShowAsync();
        }

        private async void BackupFavoritesButton_OnClick(object sender, RoutedEventArgs e)
        {
            bool performBackup = false;
            var md = new MessageDialog("Backup your favorties list to synced storage?\r\n\nWARNING: This will replace your last favorites backup.", "Backup current favorties?");
            md.Commands.Add(new UICommand("backup", (args) =>
            {
                performBackup = true;
            }));
            md.Commands.Add(new UICommand("cancel"));

            await md.ShowAsync();

            if (!performBackup)
                return;

            if (await App.ViewModel.BackupFavoritesToRoamingFolderAsync())
                await new MessageDialog("You have successfully backed up your favorties. You can now retrieve them on any device. \r\n\nIMPORTANT NOTE: \r\nIt take a couple minutes for the latest backup to be available for all your devices.", "Success").ShowAsync();
        }

        private async void LoadFavortiesButton_OnClick(object sender, RoutedEventArgs e)
        {
            bool loadBackup = false;
            MessageDialog md = null;
            
            md = new MessageDialog("Choosing 'load' will add your backed up favs to your current favs list." +
                                       "\r\n\nNOTE: It can sometimes take a little while to sync your roaming storage. If you recently backed up on another machine, it may not be immediately available.", "Restore Backup?");

            md.Commands.Add(new UICommand("load", (args) =>
            {
                loadBackup = true;
            }));
            md.Commands.Add(new UICommand("cancel"));

            await md.ShowAsync();

            if (loadBackup)
            {
                var backupList = await App.ViewModel.LoadFavoritesFromRoamingFolderAsync();

                foreach (var backedUpFav in backupList)
                {
                    //skip if fav is already present
                    if (App.ViewModel.FlickrFavs.Contains(backedUpFav))
                        continue;

                    //need to clear the local file path if it accidentally got persisted
                    backedUpFav.LocalImageFilePath = "";

                    App.ViewModel.FlickrFavs.Add(backedUpFav);
                }
                
                await App.ViewModel.SaveFavoritesJsonAsync();

                await new MessageDialog("You have successfully loaded your favorties.", "Success").ShowAsync();
            }
        }

        //private void ThemeChangeButton_OnClick(object sender, RoutedEventArgs e)
        //{
            
        //}
    }
}
