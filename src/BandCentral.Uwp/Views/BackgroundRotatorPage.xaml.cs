// Lance McCarthy 2013-2023 MIT
// Free to use, maintain attribution to original
// https://github.com/LanceMcCarthy/Lancelot.AwesomeBandBackgrounds

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Background;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using BandCentral.Models.Common;
using BandCentral.Models.Favorites;
using BandCentral.Models.Helpers;
using BandCentral.Models.Secrets;
using BandCentral.Uwp.Common;
using Lumia.Imaging;
using Lumia.Imaging.Transforms;
using Microsoft.HockeyApp;
using Photo = FlickrNet.Photo;

namespace BandCentral.Uwp.Views
{
    public sealed partial class BackgroundRotatorPage : Page
    {
        private readonly StorageFolder localFolder = ApplicationData.Current.LocalFolder;
        private readonly ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        private int updateFrequency = 240;
        private bool selectionMute;
        private bool isDirty;

        private GroupedObservableCollection<bool, FlickrFav> groupedFavs;
        
        public BackgroundRotatorPage()
        {
            this.InitializeComponent();
            DataContext = App.ViewModel;
            
            this.CollectionViewSource.Source = App.ViewModel.FlickrFavs.GroupBy(f => f.IsBackgroundFav).OrderByDescending(g => g.Key);

            Loaded += BackgroundRotatorPage_Loaded;
        }

        #region settings items (time pickers and toggles)

        //Loaded handlers

        private void QuietHourStartPicker_Loaded(object sender, RoutedEventArgs e)
        {
            object startTimeObj;
            if (localSettings.Values.TryGetValue(GeneralConstants.BackgroundRotatorUpdateQuietHoursStartKey, out startTimeObj))
            {
                QuietHourStartPicker.Time = TimeSpan.Parse((string) startTimeObj);
            }
        }

        private void QuietHourEndPicker_Loaded(object sender, RoutedEventArgs e)
        {
            object endTimeObj;
            if (localSettings.Values.TryGetValue(GeneralConstants.BackgroundRotatorUpdateQuietHoursEndKey, out endTimeObj))
            {
                QuietHourEndPicker.Time = TimeSpan.Parse((string) endTimeObj);
            }
        }

        private void QuietHoursToggle_Loaded(object sender, RoutedEventArgs e)
        {
            object quietHoursObj;
            if (localSettings.Values.TryGetValue(GeneralConstants.BackgroundRotatorUpdateQuietHoursEnabledKey, out quietHoursObj))
            {
                QuietHoursToggle.IsOn = (bool) quietHoursObj;
            }
        }

        //Toggled and Changed handlers

        private async void AutoFavsEnabledToggle_OnToggled(object sender, RoutedEventArgs e)
        {
            if (selectionMute)
                return;

            //clear cache if disabled (NOTE:The binding handles UI elements' IsEnabled)
            if (AutoFavsEnabledToggle?.IsOn == true)
            {
                await ConfigureBackgroundTaskAsync();
                AutoFavsDisabledBorder.Visibility = Visibility.Collapsed;
            }
            else
            {
                await DisableTaskAsync();
                AutoFavsDisabledBorder.Visibility = Visibility.Visible;
            }
        }
        
        private async void FrequencyComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (selectionMute)
                return;

            foreach (int addedItem in e.AddedItems)
            {
                updateFrequency = addedItem;

                if (localSettings != null)
                    localSettings.Values[GeneralConstants.BackgroundRotatorUpdateFrequencyKey] = updateFrequency;

                Debug.WriteLine($"UpdateFrequency updated to: {updateFrequency}");
            }

            await ConfigureBackgroundTaskAsync();
        }

        private void BeginTimeChanged(object sender, TimePickerValueChangedEventArgs e)
        {
            if (selectionMute)
                return;
            if (localSettings != null)
                localSettings.Values[GeneralConstants.BackgroundRotatorUpdateQuietHoursStartKey] = e.NewTime.ToString();
        }

        private void EndTimeChanged(object sender, TimePickerValueChangedEventArgs e)
        {
            if (selectionMute)
                return;
            if (localSettings != null)
                localSettings.Values[GeneralConstants.BackgroundRotatorUpdateQuietHoursEndKey] = e.NewTime.ToString();
        }

        private void QuietHoursToggled(object sender, RoutedEventArgs e)
        {
            if (selectionMute)
                return;
            if (localSettings != null)
                localSettings.Values[GeneralConstants.BackgroundRotatorUpdateQuietHoursEnabledKey] = QuietHoursToggle?.IsOn;
        }
        
        private void PairedThemeToggleSwitch_OnToggled(object sender, RoutedEventArgs e)
        {
            if (selectionMute)
                return;

            isDirty = true;
        }

        #endregion

        #region Background Task management

        private async Task<bool> ConfigureBackgroundTaskAsync()
        {
            try
            {
                App.ViewModel.IsBusy = true;
                App.ViewModel.IsBusyMessage = "configuring Background Task";

                var accessStatus = await BackgroundExecutionManager.RequestAccessAsync();

                switch (accessStatus)
                {
                    case BackgroundAccessStatus.AlwaysAllowed:
                    case BackgroundAccessStatus.AllowedMayUseActiveRealTimeConnectivity:
                    case BackgroundAccessStatus.AllowedWithAlwaysOnRealTimeConnectivity:
                    case BackgroundAccessStatus.AllowedSubjectToSystemPolicy:
                        // TODO re-enable the background task after fixing runtime component classes
                        //await BackgroundTaskEngine.RegisterTaskRequiringInternetAsync(GeneralConstants.BackgroundRotatorTaskName, typeof(BackgroundRotatorTask).FullName, (uint) updateFrequency);
                        var bgCount = App.ViewModel.FlickrFavs.Count(f => f.IsBackgroundFav);
                        UpdateStatus($"Band updates every {updateFrequency} minutes using {bgCount} favs");
                        //UpdateStatus($"Band updates every {updateFrequency} minutes using {((MainViewModel) DataContext)?.BackgroundFavs?.Count} favs");
                        return true;
                    case BackgroundAccessStatus.DeniedBySystemPolicy:
                        UpdateStatus($"Background Rotator task was DENIED access", false);
                        await new MessageDialog("The app has denied from adding a background task due to System Policy. This is usually because there are too many background tasks already. " + "r\n\nGo to Phone Settings > Background Apps and free up a couple slots.").ShowAsync();
                        break;
                    case BackgroundAccessStatus.DeniedByUser:
                    case BackgroundAccessStatus.Denied:
                        UpdateStatus($"Background Rotator task was DENIED access", false);
                        await new MessageDialog("You have previously denied the app from adding a background task." + "r\n\nGo to Phone Settings > Background Apps \r\n\nFind this app in the list and re-enable background tasks.").ShowAsync();
                        break;
                    case BackgroundAccessStatus.Unspecified:
                        UpdateStatus($"Background Rotator is currently NOT running", false);
                        await new MessageDialog(content: "You did not make a choice. If you want to update your Band in the background, please try again.").ShowAsync();
                        break;
                }

                return false;
            }
            catch (Exception ex)
            {
                StatusRectangle.Fill = new SolidColorBrush(Colors.Red);
                await new MessageDialog($"Something went wrong configuring the background task. Error: {ex.Message}").ShowAsync();
                return false;
            }
            finally
            {
                App.ViewModel.IsBusy = false;
                App.ViewModel.IsBusyMessage = "";
            }
        }

        private async Task DisableTaskAsync()
        {
            try
            {
                Debug.WriteLine($"DisableTaskAsync() called");
                App.ViewModel.IsBusy = true;
                App.ViewModel.IsBusyMessage = "unregistering background task...";
                selectionMute = true;
                
                //unregister the task and confirm to user it was successful
                if (await BackgroundTaskEngine.UnregisterTaskAsync(GeneralConstants.BackgroundRotatorTaskName))
                {
                    UpdateStatus($"Background Rotator is currently NOT running", false);

                    //MAKE SURE task is unregistered before removing bFavs
                    Debug.WriteLine($"BackgroundFavs.Clear() called and updating file");
                    //((MainViewModel) DataContext)?.BackgroundFavs?.Clear();

                    //saving an empty list will delete the file
                    //await SaveBackgroundFavoritesAsync(((MainViewModel) DataContext)?.BackgroundFavs);

                    //set lastUsedIndex back to zero
                    localSettings.Values[GeneralConstants.BackgroundRotatorFavoriteIndexKey] = 0;
                }

                selectionMute = false;
            }
            catch (Exception ex)
            {
                await new MessageDialog($"There was a problem clearing the cache. Error: {ex.Message}").ShowAsync();
            }
            finally
            {
                App.ViewModel.IsBusyMessage = "";
                App.ViewModel.IsBusy = false;
            }
        }

        #endregion

        #region selection and click handlers

        private void MainPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MainPivot.SelectedIndex == 0)
            {

            }
            else if (MainPivot.SelectedIndex == 1)
            {
                object settingValue = "---";

                if (localSettings.Values.TryGetValue(GeneralConstants.BackgroundRotatorStatusKey, out settingValue))
                    LastStatusTextBlock.Text = (string) settingValue;

                settingValue = "---";

                if (localSettings.Values.TryGetValue(GeneralConstants.BackgroundRotatorLastAttemptedKey, out settingValue))
                    LastAttemptTextBlock.Text = (string) settingValue;

                settingValue = "---";

                if (localSettings.Values.TryGetValue(GeneralConstants.BackgroundRotatorLastCompletedKey, out settingValue))
                    LastSuccessTextBlock.Text = (string) settingValue;
            }
        }

        private void FavoritesGridView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //DeleteAppBarButton.Visibility = e.AddedItems.Any() ? Visibility.Visible : Visibility.Collapsed;
        }

        //private void MultiSelectAppbarButton_OnClick(object sender, RoutedEventArgs e)
        //{
        //    FavoritesGridView.SelectionMode = MultiSelectAppbarButton.IsChecked == true
        //        ? ListViewSelectionMode.Multiple
        //        : ListViewSelectionMode.None;
        //}

        //private async void PickFavoriteMenuItem_OnClick(object sender, RoutedEventArgs e)
        //{
        //    await AddSelectedFavToBackgroundFavsAsync();
        //}

        //private async void AddNewAppBarButton_OnClick(object sender, RoutedEventArgs e)
        //{
        //    var favPicker = new PickFlickrFavDialog();
        //    var result = await favPicker.ShowAsync();

        //    if (result == ContentDialogResult.Primary)
        //        await AddSelectedFavToBackgroundFavsAsync();
        //}

        //private async void DeleteAppBarButton_OnClick(object sender, RoutedEventArgs e)
        //{
        //    if (FavoritesGridView.SelectedItems.Any())
        //    {
        //        var md = new MessageDialog("Remove this photo from the background rotator?", "Remove Photo(s)?");

        //        md.Commands.Add(new UICommand("yes"));
        //        md.Commands.Add(new UICommand("cancel"));
        //        md.DefaultCommandIndex = 1;
        //        md.CancelCommandIndex = 1;
        //        var result = await md.ShowAsync();

        //        if (result.Label == "yes")
        //        {
        //            foreach (var selectedItem in FavoritesGridView.SelectedItems)
        //            {
        //                var fav = selectedItem as BackgroundFav;
        //                if (fav == null)
        //                    continue;

        //                //remove from list (dont need to delete local image file as it's already used for the main fav
        //                App.ViewModel.BackgroundFavs.Remove(fav); //remove
        //            }

        //            if (await SaveBackgroundFavoritesAsync(App.ViewModel.BackgroundFavs))
        //                await ConfigureBackgroundTaskAsync();

        //            if (!App.ViewModel.BackgroundFavs.Any())
        //            {
        //                MultiSelectAppbarButton.IsChecked = false;
        //            }
        //        }
        //    }
        //}

        private async void UnlockTasksButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (await GeneralConstants.BackgroundTasksIapKey.PurchaseProductAsync())
            {
                App.ViewModel.IapBackgroundTasks = true;
            }
        }
        
        #endregion

        #region file loading, downloading and saving

        private async Task<WriteableBitmap> DownloadAndCropImageAsync(Photo photo)
        {
            try
            {
                App.ViewModel.IsBusy = true;
                App.ViewModel.IsBusyMessage = "downloading image...";

                var imageInfo = FlickrHelpers.GetPhotoInfo(photo, (int) App.ViewModel.WindowBounds.Width);

                if (imageInfo == null)
                {
                    await new MessageDialog("Something went wrong getting the photo. Try again.").ShowAsync();
                    return null;
                }
                else if (string.IsNullOrEmpty(imageInfo.Url))
                {
                    await new MessageDialog("The photo's links are invalid, try a different photo.\r\n\n" + "Tip: you can add the photo to your Favorites and try at another time.").ShowAsync();
                    return null;
                }

                //http get for image stream
                var handler = new HttpClientHandler();
                if (handler.SupportsAutomaticDecompression)
                    handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

                var client = new HttpClient(handler);
                var stream = await client.GetStreamAsync(imageInfo.Url);

                Debug.WriteLine($"DOWNLOADED {photo.PhotoId}");

                //seekable stream
                var memStream = new MemoryStream();
                await stream.CopyToAsync(memStream);
                memStream.Position = 0;

                App.ViewModel.IsBusyMessage = "cropping photo...";

                //NOTE - getting size form VM now to support Band 2
                var tileSize = App.ViewModel.MeTileSize;

                var outputWriteableBitmap = new WriteableBitmap((int) tileSize.Width, (int) tileSize.Height);

                using (var source = new RandomAccessStreamImageSource(memStream.AsRandomAccessStream()))
                using (var effect = new CropEffect(source, new Rect(0, 0, (int) tileSize.Width, (int) tileSize.Height)))
                using (var renderer = new WriteableBitmapRenderer(effect, outputWriteableBitmap))
                {
                    await renderer.RenderAsync();
                    Debug.WriteLine($"CROPPED: {photo.PhotoId}");
                }

                return outputWriteableBitmap;
            }
            catch (Exception ex)
            {
                HockeyClient.Current.TrackException(ex);
                await new MessageDialog($"Sorry, there was a problem cropping the image. It may be too small to put on the Band. Error: {ex.Message}").ShowAsync();
                return null;
            }
            finally
            {
                App.ViewModel.IsBusyMessage = "";
                App.ViewModel.IsBusy = false;
            }
        }

        //MOVED to ImageHelpers.cs
        //private async Task<string> SaveToStorageFile1(WriteableBitmap writeableBitmap, string fileName)
        //{
            
        //    Guid bitmapEncoderGuid = BitmapEncoder.JpegEncoderId;

        //    var file = await ApplicationData.Current.LocalFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);

        //    using (IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.ReadWrite))
        //    {
        //        var encoder = await BitmapEncoder.CreateAsync(bitmapEncoderGuid, stream);
        //        var pixelStream = writeableBitmap.PixelBuffer.AsStream();
        //        byte[] pixels = new byte[pixelStream.Length];
        //        await pixelStream.ReadAsync(pixels, 0, pixels.Length);
        //        encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore, (uint) writeableBitmap.PixelWidth, (uint) writeableBitmap.PixelHeight, 96.0, 96.0, pixels);

        //        await encoder.FlushAsync();
        //    }

        //    Debug.WriteLine($"--- {fileName} saved to {file.Path}");

        //    return file.Path;
        //}

        //private async Task<bool> SaveBackgroundFavoritesAsync(ObservableCollection<BackgroundFav> favs)
        //{
        //    if (DesignMode.DesignModeEnabled) return true;

        //    try
        //    {
        //        var file = await localFolder.CreateFileAsync(GeneralConstants.BackgroundFavoritesFileName, CreationCollisionOption.ReplaceExisting);

        //        if (favs.Count == 0)
        //        {
        //            await file.DeleteAsync(StorageDeleteOption.PermanentDelete);
        //            Debug.WriteLine($"--------- No BackgroundFavs in the list - File Deleted ---------");
        //        }
        //        else
        //        {
        //            var json = JsonConvert.SerializeObject(favs);
        //            await FileIO.WriteTextAsync(file, json);
        //            Debug.WriteLine($"--------- {((MainViewModel) DataContext)?.BackgroundFavs.Count} BackgroundFavorites Saved ---------");
        //        }

        //        return true;
        //    }
        //    catch (FileNotFoundException)
        //    {
        //        Debug.WriteLine("Could not create BackgroundFavoritesJson.txt, saving favorites failed");
        //        return false;
        //    }
        //    catch (Exception ex)
        //    {
        //        HockeyClient.Current.TrackException(ex);
        //        Debug.WriteLine("BackgroundFavorites Save Exception: {0}", ex.Message);
        //        await new MessageDialog("There was a problem saving your favorites file").ShowAsync();
        //        return false;
        //    }
        //}

        //private async Task<ObservableCollection<BackgroundFav>> LoadBackgroundFavortiesAsync()
        //{
        //    if (DesignMode.DesignModeEnabled) return null;
        //    var list = new ObservableCollection<BackgroundFav>();

        //    try
        //    {
        //        var storageItem = await localFolder.TryGetItemAsync(GeneralConstants.BackgroundFavoritesFileName);
        //        if (storageItem == null)
        //        {
        //            Debug.WriteLine("No BackgroundFavorites json file present");
        //            return list; //return empty list
        //        }

        //        var file = storageItem as StorageFile;

        //        var json = await FileIO.ReadTextAsync(file);
        //        list = JsonConvert.DeserializeObject<ObservableCollection<BackgroundFav>>(json);
        //        if (list != null)
        //        {
        //            Debug.WriteLine($"--- {list.Count} BackgroundFavorites loaded ---");
        //            return list;
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        HockeyClient.Current.TrackException(ex);
        //        Debug.WriteLine("BackgroundFavorites Load Exception: {0}", ex.Message);
        //        await new MessageDialog("There was a problem loading your Background Favorites file, memory may be corrupt or missing. ").ShowAsync();
        //    }

        //    return list;
        //}

        #endregion

        #region ui methods

        //private async Task AddSelectedFavToBackgroundFavsAsync()
        //{
        //    try
        //    {
        //        App.ViewModel.IsBusy = true;
                
        //        if (App.ViewModel.SelectedFav == null)
        //            return;

        //        if (((MainViewModel) DataContext).BackgroundFavs.Any(f => f.PhotoId == App.ViewModel.SelectedFav.Photo.PhotoId))
        //        {
        //            await new MessageDialog("You already have this photo in your Background Rotator", "Duplicate!").ShowAsync();
        //            return;
        //        }

        //        //if the selected photo has not been saved locally yet, download and save
        //        if (string.IsNullOrEmpty(App.ViewModel.SelectedFav.LocalImageFilePath))
        //        {
        //            var wb = await DownloadAndCropImageAsync(App.ViewModel.SelectedFav.Photo);
        //            var file = await wb.SaveToJpgFileAsync($"{App.ViewModel.SelectedFav.Photo.PhotoId}.jpg", ApplicationData.Current.LocalFolder);
        //            App.ViewModel.SelectedFav.LocalImageFilePath = file.Path;
        //            App.ViewModel.FlickrFavs.FirstOrDefault(f => f.Photo.PhotoId == App.ViewModel.SelectedFav.Photo.PhotoId).LocalImageFilePath = App.ViewModel.SelectedFav.LocalImageFilePath;
        //            await App.ViewModel.SaveFavoritesJsonAsync();
        //        }
                
        //        //create BackgroundFav
        //        var selectedBackgroundFav = new BackgroundFav
        //        {
        //            HasCustomTheme = App.ViewModel.SelectedFav.HasCustomTheme,
        //            UseCustomTheme = App.ViewModel.SelectedFav.HasCustomTheme,
        //            IsUserPhoto = false,
        //            LocalImageFileName = $"{App.ViewModel.SelectedFav.Photo.PhotoId}.jpg",
        //            LocalImageFilePath = App.ViewModel.SelectedFav.LocalImageFilePath,
        //            PhotoId = App.ViewModel.SelectedFav.Photo.PhotoId,
        //            Theme = App.ViewModel.SelectedFav.Theme
        //        };

        //        App.ViewModel.BackgroundFavs.Add(selectedBackgroundFav);

        //        if (await SaveBackgroundFavoritesAsync(App.ViewModel.BackgroundFavs))
        //            await ConfigureBackgroundTaskAsync();
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.WriteLine($"PickFavoriteMenuItem_OnClick Exception:\r\n{ex}");
        //    }
        //    finally
        //    {
        //        App.ViewModel.IsBusy = false;
        //    }
        //}

        private void UpdateStatus(string status, bool isActive = true)
        {
            CurrentStatusTextBlock.Text = status;
            StatusRectangle.Fill = isActive ? new SolidColorBrush(Colors.Green) : new SolidColorBrush(Colors.Red);
            StatusUpdateStory.Begin();
        }

        #endregion

        #region navigation and lifecycle

        private async void BackgroundRotatorPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (DesignMode.DesignModeEnabled)
                return;

            App.ViewModel.IsBusy = true;
            App.ViewModel.IsBusyMessage = "loading...";

            selectionMute = true;

            //((MainViewModel) DataContext).BackgroundFavs = await LoadBackgroundFavortiesAsync();

            //get the frequency out of settings
            object obj;
            if (localSettings.Values.TryGetValue(GeneralConstants.BackgroundRotatorUpdateFrequencyKey, out obj))
            {
                updateFrequency = (int) obj;
            }
            else
            {
                //first time loading, need to set this now
                localSettings.Values[GeneralConstants.BackgroundRotatorUpdateFrequencyKey] = updateFrequency;
            }

            //setup frequency combobox
            FrequencyComboBox.ItemsSource = new List<int> { 15, 30, 60, 120, 240, 360, 720 };
            FrequencyComboBox.SelectedItem = updateFrequency;

            if (App.ViewModel.AutoFavsEnabled == true)
            {
                //load BG task details
                if (await BackgroundTaskEngine.CheckBackgroundTasksAsync(GeneralConstants.BackgroundRotatorTaskName))
                {
                    var bgCount = App.ViewModel.FlickrFavs.Count(f => f.IsBackgroundFav);
                    UpdateStatus($"Updating every {updateFrequency} minutes with {bgCount} photos");
                    //UpdateStatus($"Updating every {updateFrequency} minutes with {((MainViewModel) DataContext)?.BackgroundFavs?.Count} photos");
                }
                else
                {
                    await ConfigureBackgroundTaskAsync();
                }

                AutoFavsDisabledBorder.Visibility = Visibility.Collapsed;
            }
            else
            {
                UpdateStatus($"Background Rotator is currently NOT running", false);

                //TODO Make sure AutoFavsDisabledBorder overlay successfully replaces this dialog. I'd rather not have a blocking dialog.
                //await new MessageDialog("Enable Background Rotator on the settings tab.", "Background Rotator is turned off").ShowAsync();
                //MainPivot.SelectedIndex = 1;

                AutoFavsDisabledBorder.Visibility = Visibility.Visible;
            }

            selectionMute = false;

            App.ViewModel.IsBusyMessage = "";
            App.ViewModel.IsBusy = false;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (DesignMode.DesignModeEnabled)
                return;
            
            //if we're returning from the Custom theme page, get the BackgroundFav and set it's HasCustomTheme value
            //if (e.SourcePageType == typeof(PairedThemePage))
            //{
            //    var matchingBgFav = App.ViewModel.BackgroundFavs.FirstOrDefault(bf => bf.PhotoId == App.ViewModel.SelectedFav.Photo.PhotoId);

            //    if (matchingBgFav != null)
            //    {
            //        matchingBgFav.HasCustomTheme = App.ViewModel.SelectedFav.HasCustomTheme;
            //    }
            //}
        }

        protected override async void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (isDirty)
            {
                await App.ViewModel.SaveFavoritesJsonAsync();
                //await SaveBackgroundFavoritesAsync(App.ViewModel.BackgroundFavs);
            }

            base.OnNavigatingFrom(e);
        }

        #endregion

        private async void FavoritesGridView_OnItemClick(object sender, ItemClickEventArgs e)
        {
            var fav = e.ClickedItem as FlickrFav;
            if (fav == null)
                return;

            App.ViewModel.IsBusy = true;
            App.ViewModel.IsBusyMessage = "regrouping items...";
            
            fav.IsBackgroundFav = !fav.IsBackgroundFav;
            
            //TODO - work on an alternate approach to this as it causes the list to scroll to top
            this.CollectionViewSource.Source = App.ViewModel.FlickrFavs.GroupBy(f => f.IsBackgroundFav).OrderByDescending(g => g.Key);

            await App.ViewModel.SaveFavoritesJsonAsync();
            await ConfigureBackgroundTaskAsync();

            App.ViewModel.IsBusy = false;
            App.ViewModel.IsBusyMessage = "";
        }
    }
}
