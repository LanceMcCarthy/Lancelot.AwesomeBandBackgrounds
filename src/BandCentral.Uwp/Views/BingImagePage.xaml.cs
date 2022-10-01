// Lance McCarthy 2013-2023 MIT
// Free to use, maintain attribution to original
// https://github.com/LanceMcCarthy/Lancelot.AwesomeBandBackgrounds

using BandCentral.Models.Bing;
using BandCentral.Models.Helpers;
using BandCentral.Models.Secrets;
using BandCentral.Uwp.Common;
using BandCentral.Uwp.ViewModels;
using BandCentral.UwpBackgroundTasks;
using Lumia.Imaging;
using Lumia.Imaging.Transforms;
using Lumia.InteropServices.WindowsRuntime;
using Microsoft.HockeyApp;
using System;
using System.Diagnostics;
using System.Globalization;
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
using WriteableBitmapExtensions = Windows.UI.Xaml.Media.Imaging.WriteableBitmapExtensions;

namespace BandCentral.Uwp.Views
{
    public sealed partial class BingImagePage : Page
    {
        #region fields

        private readonly ApplicationDataContainer localSettings;
        private BingImage selectedImage;
        private bool selectionMute = true;
        private WriteableBitmap processedBitmap;

        #endregion

        public BingImagePage()
        {
            this.InitializeComponent();
            DataContext = App.ViewModel;

            if (DesignMode.DesignModeEnabled)
                return;

            localSettings = ApplicationData.Current.LocalSettings;

            HockeyClient.Current.TrackPageView("BingImagePage");
        }

        #region button and selection event handlers

        private async void BingImages_OnSelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            try
            {
                if (args.AddedItems.Any())
                {
                    //reset buttons and image sources
                    selectedImage = null;
                    ChosenImage.Source = null;
                    SendToBandButton.IsEnabled = false;

                    selectedImage = args.AddedItems[0] as BingImage;
                    ShowPreviewStoryboard.Begin();
                }
                else
                {
                    HidePreviewStoryboard.Begin();
                    return;
                }

                if (selectedImage == null)
                {
                    Debug.WriteLine("SelectedImage was null");
                    return;
                }

                //----------------download, crop and set-----------------//
                App.ViewModel.IsBusy = true;
                App.ViewModel.IsBusyMessage = "downloading and cropping image..";

                processedBitmap = await DownloadAndCropBitmapAsync($"http://www.bing.com{selectedImage?.url}"); //returns a 1920x1080 image everytime

                if (processedBitmap == null)
                {
                    Debug.WriteLine("Bitmap was null");
                    HidePreviewStoryboard.Begin();
                    SendToBandButton.IsEnabled = false;
                    return;
                }

                await processedBitmap.SaveToJpgFileAsync($"Bing_{selectedImage.startdate}.jpg", ApplicationData.Current.TemporaryFolder);

                ChosenImage.Source = processedBitmap;
                SendToBandButton.IsEnabled = true;
            }
            catch (Exception ex)
            {
                selectedImage = null;
                HidePreviewStoryboard.Begin();
                SendToBandButton.IsEnabled = false;
                await new MessageDialog("Sorry, there was a problem downloading and cropping the image: " + ex).ShowAsync();
            }
            finally
            {
                App.ViewModel.IsBusyMessage = "";
                App.ViewModel.IsBusy = false;
            }
        }

        private async void SendToBandButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (processedBitmap == null)
                {
                    Debug.WriteLine("processedBitmap was null");
                    return;
                }

                await App.ViewModel.SetMeTileAsync(processedBitmap);

                HidePreviewStoryboard.Begin();
                SendToBandButton.IsEnabled = false;
                processedBitmap = null;
                ChosenImage.Source = null;
            }
            catch (Exception ex)
            {
                await new MessageDialog("Sorry, there was a problem sending image to the band: " + ex).ShowAsync();
            }
            finally
            {
                App.ViewModel.IsBusyMessage = "";
                App.ViewModel.IsBusy = false;
            }
        }

        private async void BingTaskEnabledToggle_OnToggled(object sender, RoutedEventArgs e)
        {
            if (selectionMute)
                return;

            if (BingTaskEnabledToggle?.IsOn == true)
            {
                var successful = await ConfigureBackgroundTaskAsync();

                HockeyClient.Current.TrackEvent(successful ? "BingBackgroundTaskEnabled" : "BingBackgroundTaskFailed");
            }
            else
            {
                await DisableTaskAsync();
            }
        }

        private async void UnlockTasksButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (await GeneralConstants.BackgroundTasksIapKey.PurchaseProductAsync())
            {
                ((MainViewModel) DataContext).IapBackgroundTasks = true;
            }
        }

        #endregion

        #region methods

        private void HideButton_OnClick(object sender, RoutedEventArgs e)
        {
            selectedImage = null;
            processedBitmap = null;
            ChosenImage.Source = null;
            HidePreviewStoryboard.Begin();
        }

        private void UpdateStatus(string status, bool isActive = true)
        {
            CurrentStatusTextBlock.Text = status;
            StatusBorder.Background = isActive ? new SolidColorBrush(Colors.Green) : new SolidColorBrush(Colors.Red);
        }

        private static async Task<WriteableBitmap> DownloadAndCropBitmapAsync(string imageUrl)
        {
            try
            {
                var handler = new HttpClientHandler();
                if (handler.SupportsAutomaticDecompression)
                    handler.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;

                //Bing images are usually this size (fingers crossed)
                var wbm = new WriteableBitmap(1920, 1080);

                using (var client = new HttpClient(handler))
                using (var stream = await client.GetStreamAsync(imageUrl))
                using (var memStream = new MemoryStream())
                {
                    stream.CopyTo(memStream);
                    memStream.Position = 0;

                    await wbm.SetSourceAsync(memStream.AsRandomAccessStream());
                    wbm.Invalidate();
                }

                //NOTE: App.ViewModel.MeTileSize (310 x 128 [or 102 for Band 1])

                //0.161458 is the expected value for the normal Bing Image size of 1920x1080
                var scaleRatio = (double) App.ViewModel.MeTileSize.Width / (double) wbm.PixelWidth;
                Debug.WriteLine($"ScaleRatio: {scaleRatio}");

                var scaledHeight = wbm.PixelHeight * scaleRatio;
                Debug.WriteLine($"ScaledHeight: {scaledHeight}");

                var resizedBitmap = wbm.Resize((int) App.ViewModel.MeTileSize.Width, (int) scaledHeight, WriteableBitmapExtensions.Interpolation.Bilinear);

                var outputWriteableBitmap = new WriteableBitmap((int) App.ViewModel.MeTileSize.Width, (int) App.ViewModel.MeTileSize.Height);

                using (var source = new BitmapImageSource(resizedBitmap.AsBitmap()))
                using (var filters = new CropEffect(source, new Rect(0, 0, (int) App.ViewModel.MeTileSize.Width, (int) App.ViewModel.MeTileSize.Height)))
                using (var renderer = new WriteableBitmapRenderer(filters, outputWriteableBitmap))
                {
                    await renderer.RenderAsync();
                    return outputWriteableBitmap;
                }
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"DownloadAndCropGetBitmapAsync HttpRequestException: {ex}");
                await new MessageDialog($"There was a problem downloading, check your internet connection.\r\nError: {ex.Message}").ShowAsync();
                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"DownloadAndCropGetBitmapAsync Exception: {ex}");
                await new MessageDialog($"There was a problem downloading or cropping the image.\r\nError: {ex.Message}").ShowAsync();
                return null;
            }
        }

        #endregion

        #region Background Task management

        private async Task<bool> ConfigureBackgroundTaskAsync()
        {
            try
            {
                App.ViewModel.IsBusy = true;
                App.ViewModel.IsBusyMessage = "configuring background task...";

                var accessStatus = await BackgroundExecutionManager.RequestAccessAsync();

                switch (accessStatus)
                {
                    case BackgroundAccessStatus.AlwaysAllowed:
                    case BackgroundAccessStatus.AllowedMayUseActiveRealTimeConnectivity:
                    case BackgroundAccessStatus.AllowedWithAlwaysOnRealTimeConnectivity:
                    case BackgroundAccessStatus.AllowedSubjectToSystemPolicy:
                        // TODO This needs to be re-implemented after riuntime component classes are fixed
                        await BackgroundTaskEngine.RegisterTaskRequiringInternetAsync(BingConstants.BingTaskName, typeof(BingImageTask).FullName, (uint) 240);
                        UpdateStatus($"Bing Image of the Day task is running");
                        localSettings.Values[BingConstants.BingTaskLastSetKey] = DateTime.Now.ToString(CultureInfo.InvariantCulture);
                        return true;
                    case BackgroundAccessStatus.DeniedBySystemPolicy:
                        UpdateStatus($"Bing Image task was DENIED access", false);
                        await new MessageDialog("The app has denied from adding a background task due to System Policy. This is usually because there are too many background tasks already. " + "r\n\nGo to Phone Settings > Background Apps and free up a couple slots.").ShowAsync();
                        break;
                    case BackgroundAccessStatus.Unspecified:
                        UpdateStatus($"Bing Image of the Day task is NOT running", false);
                        await new MessageDialog("You did not make a choice. If you want to update your Band in the background, please try again.").ShowAsync();
                        break;
                    case BackgroundAccessStatus.DeniedByUser:
                    case BackgroundAccessStatus.Denied:
                        UpdateStatus($"Bing Image task was DENIED access", false);
                        await new MessageDialog("You've denied the app from updating your Band in the background or you have too many background tasks already. " + "r\n\nGo to Phone Settings > Background Apps \r\n\nFind this app in the list and re-enable background tasks.").ShowAsync();
                        break;
                }

                return false;
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error setting task", false);
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
                if (await BackgroundTaskEngine.UnregisterTaskAsync(BingConstants.BingTaskName))
                {
                    UpdateStatus("Background task was removed!", false);
                    HockeyClient.Current.TrackEvent("BingBackgroundTaskDisabled");
                }

                selectionMute = false;
            }
            catch (Exception ex)
            {
                await new MessageDialog($"There was a problem removing the background task. Error: {ex.Message}").ShowAsync();
            }
            finally
            {
                App.ViewModel.IsBusyMessage = "";
                App.ViewModel.IsBusy = false;
            }
        }

        #endregion

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (await BackgroundTaskEngine.CheckBackgroundTasksAsync(BingConstants.BingTaskName))
            {
                BingTaskEnabledToggle.IsOn = true;
                UpdateStatus($"Bing Image of the Day task is running");
            }
            else
            {
                BingTaskEnabledToggle.IsOn = false;
                UpdateStatus($"Bing Image of the Day task is not running", false);
            }

            //update the UI with Task details

            object settingValue;

            if (localSettings.Values.TryGetValue(BingConstants.BingTaskStatusKey, out settingValue))
                LastStatusTextBlock.Text = (string) settingValue;

            if (localSettings.Values.TryGetValue(BingConstants.BingTaskLastAttemptedKey, out settingValue))
                LastAttemptTextBlock.Text = (string) settingValue;


            if (localSettings.Values.TryGetValue(BingConstants.BingTaskLastCompletedKey, out settingValue))
                LastSuccessTextBlock.Text = (string) settingValue;

            //handle reactivation of Task 
            if (e?.Parameter as string == "refreshBingTask")
            {
                Debug.WriteLine($"BingImagePage OnNavigatedTo Parameter: {e?.Parameter}");

                if (await ConfigureBackgroundTaskAsync())
                {
                    await new MessageDialog("Your Bing Image of the day task has been refreshed!\r\n\n", "refreshed!").ShowAsync();
                    HockeyClient.Current.TrackEvent("BingBackgroundTaskRefreshed");
                }
            }

            selectionMute = false;
        }
    }
}
