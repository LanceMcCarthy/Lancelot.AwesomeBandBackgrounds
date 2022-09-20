using BandCentral.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Email;
using Windows.ApplicationModel.Store;
using Windows.Graphics.Display;
using Windows.Storage;
using Windows.System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using BandCentral.WindowsBase.Common;
using BandCentralBase.Common;
using Microsoft.Advertising.WinRT.UI;

namespace BandCentral
{
    public sealed partial class SettingsPage : Page
    {
        private NavigationHelper navigationHelper;
        public NavigationHelper NavigationHelper => this.navigationHelper;

        //private bool selectionMute;
        private InterstitialAd myVideoAd;
        string MyAppId = "d64751a9-6d30-4d83-9048-fff78f328f9c";
        string MyAdUnitId = "279592";
        //private bool isLoading;
        private int errorCount;

        public SettingsPage()
        {
            this.InitializeComponent();
            DataContext = App.ViewModel;

            DisplayInformation.AutoRotationPreferences = DisplayOrientations.Portrait;
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
        }

        private async Task EmailSupportAsync(string context)
        {
            string subjectName = "Band Background " + context;
            string emailAddress = "awesome.apps@outlook.com";
#if WINDOWS_PHONE_APP
            string emailName = "Awesome Band Background";
            var email = new EmailMessage()
            {
                Subject = subjectName
            };
            email.To.Add(new EmailRecipient(emailAddress, emailName));
            await EmailManager.ShowComposeNewEmailAsync(email);
#else
            var mailto = new Uri("mailto:?to=" + emailAddress + "&subject=" + subjectName);
            await Launcher.LaunchUriAsync(mailto);
#endif
        }

        private async Task ReviewAppAsync()
        {
            await Launcher.LaunchUriAsync(new Uri("ms-windows-store:reviewapp?appid=" + CurrentApp.AppId));
        }

        private async void RatingTapped(object sender, TappedRoutedEventArgs e)
        {
            var selectedRating = Rating.Value;

            if (selectedRating >= 3)
            {
                await ReviewAppAsync();
            }
            else
            {
                var md = new MessageDialog("Leaving a negative review won't alert us of the problem. If you send an email instead, we will be able to fix it and push out an update.\r\n\nLeave a negative review or send us an email?", "Help us fix it");
                md.Commands.Add(new UICommand("send feedback", async (a) =>
                {
                    await EmailSupportAsync("Feedback");
                }));
                md.Commands.Add(new UICommand("leave review", async (a) =>
                {
                    await ReviewAppAsync();
                }));
                await md.ShowAsync();
            }
        }

        private async void SendEmailButton_OnClick(object sender, RoutedEventArgs e)
        {
            await EmailSupportAsync("Feedback");
        }

        private void SelectBandButton_OnClick(object sender, RoutedEventArgs e)
        {
            BandConnectionDialog.Visibility = Visibility.Visible;
        }

        #region NavigationHelper registration

        private async void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            if (e.NavigationParameter is int)
            {
                var index = (int) e.NavigationParameter;
                MainPivot.SelectedIndex = index;
            }

            if (string.IsNullOrEmpty(App.ViewModel.HardwareVersion))
            {
                await App.ViewModel.LoadHardwareSpecsAsync(); //get the version number
            }

            //------------BG Task related-----------//

            //selectionMute = true;

            //if (await CheckBackgroundTasksAsync(Constants.BingTaskName))
            //{
            //    UpdateStatus($"Currently Active");
            //    BingImageTaskButton.IsOn = true;
            //}
            //else
            //{
            //    UpdateStatus($"Inactive", false);
            //    BingImageTaskButton.IsOn = false;
            //}

            //LoadTaskInfo();

            //selectionMute = false;
            

            //------------Ads related-----------//
            myVideoAd = new InterstitialAd();
            myVideoAd.RequestAd(AdType.Video, MyAppId, MyAdUnitId);

            myVideoAd.AdReady += MyVideoAd_AdReady;
            myVideoAd.ErrorOccurred += MyVideoAd_ErrorOccurred;
            myVideoAd.Completed += MyVideoAd_Completed;
            myVideoAd.Cancelled += MyVideoAd_Cancelled;


        }

        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            myVideoAd.AdReady -= MyVideoAd_AdReady;
            myVideoAd.ErrorOccurred -= MyVideoAd_ErrorOccurred;
            myVideoAd.Completed -= MyVideoAd_Completed;
            myVideoAd.Cancelled -= MyVideoAd_Cancelled;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        #region favorites backup

        private async void BackupFavoritesButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (!App.ViewModel.FlickrFavs.Any())
            {
                await new MessageDialog("You do not have any favorites to backup. Go explore and add some favorites", "No Favorites").ShowAsync();
                return;
            }

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

            if (await BackupFavoritesToRoamingFolderAsync())
                await new MessageDialog("You have successfully backed up your favorties. You can now retrieve them on any device. \r\n\nIMPORTANT NOTE: \r\nIt take a couple minutes for the latest backup to be available for all your devices.", "Success").ShowAsync();
        }

        private async void LoadFavortiesButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                bool performLoad = false;
                bool overwrite = false;

                var md = new MessageDialog("Load favorties from your synced storage backup?\r\n\nNOTE: Choose to either to REPLACE or ADD to your current favorties list. (click the back button to cancel)", "Load favorties from backup?");
                md.Commands.Add(new UICommand("ADD", (args) =>
                {
                    performLoad = true;
                }));
                md.Commands.Add(new UICommand("REPLACE", (args) =>
                {
                    performLoad = true;
                    overwrite = true;
                }));

                await md.ShowAsync();

                if (!performLoad)
                    return;
                
                //-------overwrite or add--------// moved from VM method

                var backupList = await LoadFavoritesFromRoamingFolderAsync(overwrite);

                //will catch if the file wasnt found, etc
                if (backupList == null)
                    return;

                if (overwrite)
                {
                    App.ViewModel.FlickrFavs = backupList;
                }
                else
                {
                    foreach (var backedUpFav in backupList)
                    {
                        if (App.ViewModel.FlickrFavs.Contains(backedUpFav))
                            continue;

                        backedUpFav.LocalImageFilePath = "";
                        //need to clear the local file path if it accidentally got persisted
                        App.ViewModel.FlickrFavs.Add(backedUpFav);
                    }
                }

                await new MessageDialog("You have successfully loaded your favorties.", "Success").ShowAsync();
            }
            catch (Exception ex)
            {
                await new MessageDialog($"Something went wrong. Error: {ex.Message}", "Error").ShowAsync();
            }
        }

        public async Task<bool> BackupFavoritesToRoamingFolderAsync()
        {
            if (DesignMode.DesignModeEnabled) return true;

            try
            {
                App.ViewModel.IsBusy = true;
                App.ViewModel.IsBusyMessage = "backing up favorites...";
                
                Debug.WriteLine("Backup Favorties To Roaming Folder");

                var file = await ApplicationData.Current.RoamingFolder.CreateFileAsync(Constants.FlickrFavoritesBackupFileName, CreationCollisionOption.ReplaceExisting);

                var settings = new DataContractJsonSerializerSettings
                {
                    KnownTypes = new List<Type>() { typeof(FlickrFav), typeof(ObservableCollection<FlickrFav>) }
                };

                //need to make a copy of th elist so I can clear the LocalImageFilePath (this should also make the data smaller)
                var backupList = new ObservableCollection<FlickrFav>();

                foreach (var flickrFav in App.ViewModel.FlickrFavs)
                {
                    if (!string.IsNullOrEmpty(flickrFav.LocalImageFilePath))
                        flickrFav.LocalImageFilePath = "";

                    backupList.Add(flickrFav);
                }

                using (var stream = await file.OpenStreamForWriteAsync())
                {
                    //use the copy to save
                    var serializer = new DataContractJsonSerializer(typeof(ObservableCollection<FlickrFav>), settings);
                    serializer.WriteObject(stream, backupList);

                    Debug.WriteLine("---------{0} ROAMING Favorites Saved---------", App.ViewModel.FlickrFavs.Count);
                }

                return true;
            }
            catch (FileNotFoundException)
            {
                Debug.WriteLine("Could not create ROAMING FavoritesJson.txt, saving favorites failed");
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ROAMING Favorites JSON Save Exception: {0}", ex.Message);
                await new MessageDialog("There was a problem saving your synced backup favorites file").ShowAsync();
                return false;
            }
            finally
            {
                App.ViewModel.IsBusy = false;
                App.ViewModel.IsBusyMessage = "";
            }
        }

        public async Task<ObservableCollection<FlickrFav>> LoadFavoritesFromRoamingFolderAsync(bool overwriteCurrent)
        {
            if (DesignMode.DesignModeEnabled) return null;

            try
            {
                App.ViewModel.IsBusy = true;
                App.ViewModel.IsBusyMessage = "loading favorites...";

                Debug.WriteLine("Load Favorties From Roaming Folder");
                var storageItem = await ApplicationData.Current.RoamingFolder.GetFileAsync(Constants.FlickrFavoritesBackupFileName);

                if (storageItem == null)
                {
                    Debug.WriteLine("No ROAMING favorties json file present");
                    await new MessageDialog("You do not have currently have a synced file. This sometimes can take a few moments to sync if you've just backed up on another device. Try again in a little while.")
                            .ShowAsync();
                    return null;
                }

                var file = storageItem as StorageFile;

                //Using datacontractserializer
                var settings = new DataContractJsonSerializerSettings
                {
                    KnownTypes = new List<Type> {typeof (FlickrFav), typeof (ObservableCollection<FlickrFav>)}
                };

                using (var stream = await file.OpenStreamForReadAsync())
                {
                    var serializer = new DataContractJsonSerializer(typeof (ObservableCollection<FlickrFav>), settings);

                    var favs = serializer.ReadObject(stream) as ObservableCollection<FlickrFav>;

                    if (favs == null)
                    {
                        Debug.WriteLine($"Could not deserialize ROAMING FavoritesJson.txt, returning empty list");
                        return null;
                    }

                    var backupList = new ObservableCollection<FlickrFav>();

                    foreach (var photo in favs)
                    {
                        backupList.Add(photo);
                    }

                    Debug.WriteLine("--- {0} ROAMING backup favorites loaded ---", backupList.Count);
                    
                    return backupList;
                }
            }
            catch (FileNotFoundException fnfEx)
            {
                Debug.WriteLine("ROAMING Favorites Json Load Exception: {0}", fnfEx.Message);
                await new MessageDialog("You do not have a favorites backup file to load. Try again another time after you've backed up.").ShowAsync();
                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ROAMING Favorites Json Load Exception: {0}", ex.Message);
                await
                    new MessageDialog(
                        "There was a problem loading your synced favorites file, memory may be corrupt or missing. You could also have a bad connection. \r\n\nTry again later")
                        .ShowAsync();
                return null;
            }
            finally
            {
                App.ViewModel.IsBusy = false;
                App.ViewModel.IsBusyMessage = "";
            }
        }

        #endregion

        private void FivehundredPixButton_OnClick(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof (PhotoProvidersPage), "500px");
        }

        private void BingImagesButton_OnClick(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(PhotoProvidersPage), "Bing");
        }

        //#region background task test

        //private async void BingImageTaskButton_OnToggled(object sender, RoutedEventArgs e)
        //{
        //    if(selectionMute)
        //        return;

        //    var button = sender as ToggleSwitch;
        //    if (button == null)
        //        return;

        //    try
        //    {
        //        App.ViewModel.IsBusy = true;
        //        App.ViewModel.IsBusyMessage = "configuring Background Task";

        //        var accessStatus = await BackgroundExecutionManager.RequestAccessAsync();

        //        if (button.IsOn)
        //        {
        //            switch (accessStatus)
        //            {
        //                case BackgroundAccessStatus.AllowedWithAlwaysOnRealTimeConnectivity:
        //                    await RegisterTaskAsync(Constants.BingTaskName, typeof(BingImageBackgroundTask).FullName, (uint) 360);
        //                    UpdateStatus($"Currently Active");
        //                    break;
        //                case BackgroundAccessStatus.AllowedMayUseActiveRealTimeConnectivity:
        //                    await RegisterTaskAsync(Constants.BingTaskName, typeof(BingImageBackgroundTask).FullName, (uint) 360);
        //                    UpdateStatus($"Currently Active");
        //                    break;
        //                case BackgroundAccessStatus.Unspecified:
        //                    UpdateStatus($"Background task is currently NOT running", false);
        //                    await new MessageDialog("You did not make a choice. If you want to update your Band in the background, please try again.").ShowAsync();
        //                    break;
        //                case BackgroundAccessStatus.Denied:
        //                    UpdateStatus($"Background task was DENIED access", false);
        //                    await
        //                        new MessageDialog("You've denied the app from updating your Band in the background or you have too many background tasks already. " +
        //                                          "r\n\nGo to Phone Settings > Background Apps \r\n\nFind this app in the list and re-enable background tasks.").ShowAsync();
        //                    break;
        //            }
        //        }
        //        else
        //        {
        //            await UnregisterTaskAsync(Constants.BingTaskName);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        await new MessageDialog($"BingImageTaskButton_OnToggled Exception\r\n\nError: {ex.Message}").ShowAsync();
        //    }
        //    finally
        //    {
        //        App.ViewModel.IsBusy = false;
        //        App.ViewModel.IsBusyMessage = "";
        //    }
        //}

        //private async Task RegisterTaskAsync(string taskFriendlyName, string taskEntryPoint, uint taskRunFrequency)
        //{
        //    try
        //    {
        //        //if task already exists, unregister it before adding it
        //        foreach (var task in BackgroundTaskRegistration.AllTasks.Where(cur => cur.Value.Name == taskFriendlyName))
        //        {
        //            task.Value.Unregister(true);
        //        }

        //        var builder = new BackgroundTaskBuilder();
        //        builder.Name = taskFriendlyName;
        //        builder.TaskEntryPoint = taskEntryPoint;
        //        builder.SetTrigger(new TimeTrigger(taskRunFrequency, false));
        //        builder.AddCondition(new SystemCondition(SystemConditionType.InternetAvailable));
        //        builder.Register();
        //    }
        //    catch (Exception ex)
        //    {
        //        await new MessageDialog($"RegisterTaskAsync Exception\r\n\nError: {ex.Message}").ShowAsync();
        //        UpdateStatus($"RegisterTaskAsync Exception", false);
        //    }
        //}

        //private async Task<bool> CheckBackgroundTasksAsync(string taskFriendlyName)
        //{
        //    try
        //    {
        //        App.ViewModel.IsBusy = true;
        //        App.ViewModel.IsBusyMessage = "checking background task...";

        //        await BackgroundExecutionManager.RequestAccessAsync();

        //        return BackgroundTaskRegistration.AllTasks.Any(task => task.Value.Name == taskFriendlyName);
        //    }
        //    catch (Exception ex)
        //    {
        //        await new MessageDialog($"CheckBackgroundTasksAsync Exception. Error: {ex.Message}").ShowAsync();
        //        UpdateStatus($"CheckBackgroundTasksAsync Exception", false);
        //        return false;
        //    }
        //    finally
        //    {
        //        App.ViewModel.IsBusy = false;
        //        App.ViewModel.IsBusyMessage = "";
        //    }
        //}

        //private async Task<bool> UnregisterTaskAsync(string taskFriendlyName)
        //{
        //    try
        //    {
        //        await BackgroundExecutionManager.RequestAccessAsync();

        //        foreach (var task in BackgroundTaskRegistration.AllTasks.Where(cur => cur.Value.Name == taskFriendlyName))
        //        {
        //            task.Value.Unregister(true);
        //            return true;
        //        }

        //        return false;
        //    }
        //    catch (Exception ex)
        //    {
        //        await new MessageDialog($"UnregisterTaskAsync Exception\r\n\nError: {ex.Message}").ShowAsync();
        //        UpdateStatus($"UnregisterTaskAsync Exception", false);
        //        return false;
        //    }
        //}
        
        //private void UpdateStatus(string message, bool isActive = true)
        //{
        //    StatusTextBlock.Text = message;
        //    StatusBorder.Background = isActive ? new SolidColorBrush(Colors.Green) : new SolidColorBrush(Colors.Red);
        //}


        //private void LoadTaskInfo()
        //{
        //    var localSettings = ApplicationData.Current.LocalSettings;
        //    if(localSettings==null)
        //        return;

        //    object val;
        //    if (localSettings.Values.TryGetValue(Constants.BingTaskStatusKey, out val))
        //    {
        //        LastStatusTextBlock.Text = (string) val;
        //    }

        //}

        //#endregion


        private async void IapFlyoutItem_OnClick(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            var tag = ((MenuFlyoutItem) sender).Tag as string;

            bool success = false;

            switch (tag)
            {
                case "small":
                    success = await Constants.SmallDonationIapKey.PurchaseProductAsync();
                    break;
                case "medium":
                    success = await Constants.MediumDonationIapKey.PurchaseProductAsync();
                    break;
                case "large":
                    success = await Constants.LargeDonationIapKey.PurchaseProductAsync();
                    break;
            }

            if (success)
            {
                await new MessageDialog("If you have any suggestions for features or feedback, please let me know at awesome.apps@outlook.com", "Thank You!").ShowAsync();
            }
        }

        private async void AdButton_OnClick(object sender, RoutedEventArgs e)
        {
            switch (myVideoAd.State)
            {
                case InterstitialAdState.NotReady:
                    await new MessageDialog("The video is not ready yet, try again in a few seconds.").ShowAsync();
                    break;
                case InterstitialAdState.Ready:
                    myVideoAd.Show();
                    break;
                case InterstitialAdState.Showing:
                    break;
                case InterstitialAdState.Closed:
                    await new MessageDialog("You've already seen a video this time. Come back to this page in a little bit and try again.").ShowAsync();
                    break;
                default:
                    break;
            }
        }

        #region ad events

        private void MyVideoAd_AdReady(object sender, object e)
        {
            AdButton.IsEnabled = true;
            VideoAdLoadingProgressRing.IsActive = false;
            VideoAdLoadingProgressRing.Visibility = Visibility.Collapsed;
        }

        private void MyVideoAd_ErrorOccurred(object sender, AdErrorEventArgs e)
        {
            AdButton.IsEnabled = false;
            VideoAdLoadingProgressRing.IsActive = false;
            VideoAdLoadingProgressRing.Visibility = Visibility.Collapsed;

            //to prevent a continuous loop of bad attempts
            if (++errorCount < 4)
                myVideoAd.RequestAd(AdType.Video, MyAppId, MyAdUnitId);
        }

        private void MyVideoAd_Completed(object sender, object e)
        {
            AdButton.IsEnabled = false;
            VideoAdLoadingProgressRing.IsActive = true;
            VideoAdLoadingProgressRing.Visibility = Visibility.Visible;

            myVideoAd.RequestAd(AdType.Video, MyAppId, MyAdUnitId);
        }

        private void MyVideoAd_Cancelled(object sender, object e)
        {
            AdButton.IsEnabled = false;
            VideoAdLoadingProgressRing.IsActive = true;
            VideoAdLoadingProgressRing.Visibility = Visibility.Visible;

            myVideoAd.RequestAd(AdType.Video, MyAppId, MyAdUnitId);
        }

        #endregion
    }
}
