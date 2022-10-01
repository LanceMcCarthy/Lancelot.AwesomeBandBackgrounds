using Microsoft.Band;
using NotificationsExtensions;
using NotificationsExtensions.Toasts;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Data.Xml.Dom;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Notifications;
using Windows.UI.Xaml.Media.Imaging;
using BandCentral.Models.Bing;
using BandCentral.Models.Secrets;

namespace BandCentral.UwpBackgroundTasks
{
    public sealed class BingImageTask : XamlRenderingBackgroundTask
    {
        private static ApplicationDataContainer localSettings;

        protected override async void OnRun(IBackgroundTaskInstance taskInstance)
        {
            var deferral = taskInstance.GetDeferral();

            try
            {
                localSettings = ApplicationData.Current.LocalSettings;
                if (localSettings == null)
                    return;

                localSettings.Values[BingConstants.BingTaskLastAttemptedKey] = DateTime.Now.ToString("g", CultureInfo.InvariantCulture);
                localSettings.Values[BingConstants.BingTaskStatusKey] = $"Starting...";
                Debug.WriteLine($"{taskInstance.Task.Name} Starting...");
                Debug.WriteLine($"Background Cost: {BackgroundWorkCost.CurrentBackgroundWorkCost}");

                //---------------- GET NEXT IMAGE TO USE ----------------//
                
                var todaysBingImage = await new BingImageHelper().GetTodaysBingImageAsync();
                
                if (todaysBingImage == null || string.IsNullOrEmpty($"http://www.bing.com{todaysBingImage.url}"))
                {
                    localSettings.Values[BingConstants.BingTaskStatusKey] = $"Cancelled. BingImage object was null or url was empty...";
                    return;
                }

                //---------------- CHECK TO SEE IF WE'VE ALREADY UPDATED ----------------//

                object lastDayUpdatedString;
                if (localSettings.Values.TryGetValue(BingConstants.BingTaskLastDayUpdatedKey, out lastDayUpdatedString))
                {
                    if (todaysBingImage.startdate == (string) lastDayUpdatedString)
                    {
                        localSettings.Values[BingConstants.BingTaskStatusKey] = $"Cancelled. Image already set for the day...";
                        return;
                    }

                    //TODO - Remove the DateTime parsing and comaprison if the string comparison works
                    //DateTime lastDayUpdated;
                    //if (DateTime.TryParse((string) lastDayUpdatedString, out lastDayUpdated))
                    //{
                    //    if (lastDayUpdated.Date == DateTime.Today.Date)
                    //    {
                    //        localSettings.Values[GeneralConstants.BingTaskStatusKey] = $"Cancelled. Image already set for the day...";
                    //        return;
                    //    }
                    //}

                }

                //---------------- GET BAND NAME ----------------//

                string preferredBandName = "";
                object val;
                if (localSettings.Values.TryGetValue(GeneralConstants.PreferredBandNameKey, out val))
                {
                    preferredBandName = (string) val;
                    Debug.WriteLine($"{taskInstance.Task.Name}: Found Preferred Band {preferredBandName}...");
                    localSettings.Values[BingConstants.BingTaskStatusKey] = $"{taskInstance.Task.Name}: Found Preferred Band {preferredBandName}...";
                }
                else
                {
                    //user needs to have a preferred band name stored
                    Debug.WriteLine($"{taskInstance.Task.Name}: ERROR!!! PreferredBandName not found");
                    localSettings.Values[BingConstants.BingTaskStatusKey] = $"PreferredBandName not found";
                    return;
                }

                //---------------FIND PAIRED BANDS---------------------//

                var results = await BandClientManager.Instance.GetBandsAsync();
                if (results.Length == 0)
                {
                    localSettings.Values[BingConstants.BingTaskStatusKey] = $"{taskInstance.Task.Name} Cancelled. No Paired Bands found";
                    return;
                }
                else
                {
                    localSettings.Values[BingConstants.BingTaskStatusKey] = $"{taskInstance.Task.Name}: {results.Length} paired Bands found...";
                    Debug.WriteLine($"{taskInstance.Task.Name}: {results.Length} paired Bands found...");
                }

                //------------DOWNLOAD AND CROP THE IMAGE------------------//

                var imageBitmap = await DownloadAndCropBitmapAsync($"http://www.bing.com{todaysBingImage.url}");

                if (imageBitmap == null)
                {
                    localSettings.Values[BingConstants.BingTaskStatusKey] = $"{taskInstance.Task.Name} Cancelled. Bing Image cropped bitmap was null";
                    return;
                }

                //----------------CONNECT TO BAND------------------//

                using (IBandClient selectedBandClient = await BandClientManager.Instance.ConnectAsync(results.FirstOrDefault(bandInfo => bandInfo.Name == preferredBandName)))
                {
                    if (selectedBandClient == null)
                        return;

                    //------------CONVERT TO BANDIMAGE AND SEND TO BAND------------------//
                    localSettings.Values[BingConstants.BingTaskStatusKey] = $"Sending to Band";
                    Debug.WriteLine($"{taskInstance.Task.Name}: SetMeTileImageAsync called...");

                    await selectedBandClient.PersonalizationManager.SetMeTileImageAsync(imageBitmap.ToBandImage());

                    //if successful, update last used and last completed
                    localSettings.Values[BingConstants.BingTaskLastDayUpdatedKey] = todaysBingImage.startdate;
                    localSettings.Values[BingConstants.BingTaskLastCompletedKey] = DateTime.Now.ToString("g", CultureInfo.InvariantCulture);

                    localSettings.Values[BingConstants.BingTaskStatusKey] = $"{taskInstance.Task.Name} Completed. Image used http://www.bing.com{todaysBingImage.url}";
                    Debug.WriteLine($"{taskInstance.Task.Name}: Completed. Image used http://www.bing.com{todaysBingImage.url}");

                    //------------------TOAST NOTIFICATION-------------------//
                    //only if I have to save cropped image for toast notification
                    await SaveToJpgFileAsync(imageBitmap, "BingImageOfTheDay.jpg", ApplicationData.Current.LocalFolder);

                    //This updates tile and sends toast using new adaptive
                    ShowNotification("BingImageOfTheDay.jpg", todaysBingImage.copyright);

                    //-----Check to see if the user needs to open the app to refresh the BG task--------//
                    CheckIfTaskRefreshNeeded();
                }
            }
            catch (BandException ex)
            {
                localSettings.Values[BingConstants.BingTaskStatusKey] = $"{taskInstance.Task.Name} Cancelled. Band Exception: {ex.Message}";
                Debug.WriteLine($"{taskInstance.Task.Name}: BandException: {ex}");
            }
            catch (Exception ex)
            {
                localSettings.Values[BingConstants.BingTaskStatusKey] = $"{taskInstance.Task.Name} Cancelled. General Exception: {ex.Message}";
                Debug.WriteLine($"{taskInstance.Task.Name}: General Exception: {ex}");
            }
            finally
            {
                deferral.Complete();
            }
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

                int bandTileHeight = 128;
                object bandModel;
                if (localSettings.Values.TryGetValue(GeneralConstants.BandModelKey, out bandModel))
                {
                    bandTileHeight = (int) bandModel == 1 ? 102 : 128;
                }

                //0.161458 is the expected value for the normal Bing Image size of 1920x1080
                var scaleRatio = (double) 310 / (double) wbm.PixelWidth;
                Debug.WriteLine($"BingTask Image ScaleRatio: {scaleRatio}");

                var scaledHeight = wbm.PixelHeight * scaleRatio;
                Debug.WriteLine($"BingTask Image: Width: 310 x ScaledHeight: {scaledHeight}");

                var resizedBitmap = wbm.Resize((int) 310, (int) scaledHeight, Windows.UI.Xaml.Media.Imaging.WriteableBitmapExtensions.Interpolation.Bilinear);

                Debug.WriteLine($"BingTask Image Resize Complete: Height: {resizedBitmap.PixelWidth} x Height: {resizedBitmap.PixelHeight}");

                return resizedBitmap.Crop(0, 0, (int) 310, (int) bandTileHeight);
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"BingTask HttpRequestException: {ex}");
                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"DownloadAndCropGetBitmapAsync Exception: {ex}");
                return null;
            }
        }

        private static void ShowNotification(string imageFileName, string attribution)
        {
            try
            {
                //var payload = $@"
                //<toast scenario='reminder'>
                //    <visual>
                //        <binding template='ToastGeneric'>
                //            <image placement='hero' src='ms-appdata:///local/{imageFileName}'/>
                //            <text>Your Band's background was updated!</text>
                //            <text>Today's Bing Image of the day is your Band wallpaper</text>
                //            <image src='ms-appdata:///local/{imageFileName}'/>
                //        </binding>
                //    </visual>
                //</toast>";

                //var doc = new XmlDocument();
                //doc.LoadXml(payload);

                var content = new ToastContent
                {
                    Visual = new ToastVisual
                    {
                        BindingGeneric = new ToastBindingGeneric
                        {
                            Children =
                            {
                                new AdaptiveText
                                {
                                    Text = $"Your Band's background was updated!", 
                                    HintMaxLines = 2,
                                    HintWrap = true
                                },
                            },
                            HeroImage = new ToastGenericHeroImage
                            {
                                Source = $"ms-appdata:///local/{imageFileName}"
                            },
                            Attribution = new ToastGenericAttributionText
                            {
                                Text = attribution
                            }
                        }
                    }
                };

                var doc = content.GetXml();
                
                TileUpdateManager.CreateTileUpdaterForApplication().Update(new TileNotification(doc));

                //check to see if toast is muted
                object notificationMute;
                if (localSettings.Values.TryGetValue(GeneralConstants.BackgroundRotatorNotificationMuteKey, out notificationMute))
                {
                    if (!(bool) notificationMute)
                    {
                        ToastNotificationManager.CreateToastNotifier().Show(new ToastNotification(doc) { Tag = "BingTask" });
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"BG Task ShowNotification Exception: {ex.Message}");
            }
        }

//        private void UpdateTile(string imageFileName)
//        {
////            var payload = $@"
////<tile>
////    <visual branding='name'>
////        < binding template = 'TileMedium' > 
////            < text hint - style = 'caption' >Your Band background was updated!</ text >        
////            < text hint - style = 'captionSubtle' hint - wrap = 'true' >Today's Bing Image of the Day is your Band's wallpaper</ text >                   
////        </ binding >
////        < binding template = 'TileWide' >
////            < group > 
////                < subgroup hint - weight = '33' >  
////                    < image src='ms-appdata:///local/{imageFileName}' /> 
////                </ subgroup >   
////                < subgroup >    
////                    < text hint - style = 'caption' >Your Band background was updated!</ text >
////                    < text hint - style = 'captionSubtle' hint - wrap = 'true' hint - maxLines = '3' > Today's Bing Image of the Day is your Band's wallpaper</ text >
////                </ subgroup > 
////            </ group >
////        </ binding >
////        < binding template = 'TileLarge' >  
////            < group > 
////                < subgroup hint - weight = '33' > 
////                    < image src='ms-appdata:///local/{imageFileName}' />                                              
////                </ subgroup > 
////                < subgroup > 
////                    < text hint - style = 'caption' >Your Band background was updated!</ text >                                                      
////                    < text hint - style = 'captionSubtle' hint - wrap = 'true' hint - maxLines = '3' > Today's Bing Image of the Day is your Band's wallpaper </ text >
////                </ subgroup >
////            </ group >
////            < image src='ms-appdata:///local/{imageFileName}' />
////        </ binding >
////    </ visual >
////</ tile > ";
            
//            var imageOnlyPayload = $@"
//<tile>
//  <visual>
//    <binding template='TileSmall'>
//            < image src='ms-appdata:///local/{imageFileName}' placement = 'peek' hint - overlay = '20' />     
//            < image src = 'ms-appx:///Assets/Square71x71Logo.scale-100.png' placement = 'background' />        
//    </ binding >
//    < binding template = 'TileMedium' >
//        < image src='ms-appdata:///local/{imageFileName}' placement = 'peek' hint - overlay = '20' />
//        < image src = 'ms-appx:///Assets/Square310x310Logo.scale-100.png' placement = 'background' />
//    </ binding >
//    < binding template = 'TileWide' >
//        < image src='ms-appdata:///local/{imageFileName}' placement = 'peek' hint - overlay = '20' />
//        < image src = 'ms-appx:///Assets/Wide310x150Logo.scale-100.png' placement = 'background' />
//    </ binding >
//    < binding template = 'TileLarge' >
//        < image src='ms-appdata:///local/{imageFileName}' placement = 'peek' hint - overlay = '20' />
//        < image src = 'ms-appx:///Assets/Square310x310Logo.scale-100.png' placement = 'background' />
//    </ binding >
//  </ visual >
//</ tile >";
            
//            var doc = new XmlDocument();
//            doc.LoadXml(imageOnlyPayload);

//            TileUpdateManager.CreateTileUpdaterForApplication().Update(new TileNotification(doc));

//        }

        private static void CheckIfTaskRefreshNeeded()
        {
            try
            {
                //check to see if toast is muted
                object lastSet;
                if (localSettings.Values.TryGetValue(BingConstants.BingTaskLastSetKey, out lastSet))
                {
                    DateTime lastSetDate;
                    if (DateTime.TryParse((string) lastSet, out lastSetDate))
                    {
                        var timeSinceLastSet = DateTime.Now - lastSetDate;
                        if (timeSinceLastSet.Days < 29)
                            return;

                        var payload = $@"
                                <toast launch='app-defined-string'>
                                  <visual>
                                    <binding template='ToastGeneric'>
                                      <text>Your Bing task is getting stale and needs to be refreshed</text>
                                      <text>Your Bing Image of the Day task has been running for {timeSinceLastSet.Days}, it will stop working afer 30 days. </text>
                                    </binding>
                                  </visual>
                                  <actions>
                                    <action activationType='foreground' content='refresh now' arguments='refreshBingTask' />
                                  </actions>
                                </toast>";

                        var doc = new XmlDocument();
                        doc.LoadXml(payload);

                        ToastNotificationManager.CreateToastNotifier().Show(new ToastNotification(doc) { Tag = "TaskRefresh" });
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"BG Task CheckIfTaskRefreshNeeded Exception: {ex.Message}");
            }
        }

        /// <summary>
        /// Saves a WriteableBitmap to a file
        /// </summary>
        /// <param name="writeableBitmap">Bitmap to save</param>
        /// <param name="fileName">Full file name WITH extension</param>
        /// <param name="folder">Folder to save the file to</param>
        /// <returns>StorageFile object of the saved image</returns>
        private async Task<StorageFile> SaveToJpgFileAsync(WriteableBitmap writeableBitmap, string fileName, StorageFolder folder)
        {
            Guid bitmapEncoderGuid = BitmapEncoder.JpegEncoderId;

            var file = await folder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);

            using (IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.ReadWrite))
            {
                BitmapEncoder encoder = await BitmapEncoder.CreateAsync(bitmapEncoderGuid, stream);
                Stream pixelStream = writeableBitmap.PixelBuffer.AsStream();
                byte[] pixels = new byte[pixelStream.Length];
                await pixelStream.ReadAsync(pixels, 0, pixels.Length);
                encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore, (uint) writeableBitmap.PixelWidth, (uint) writeableBitmap.PixelHeight, 96.0, 96.0, pixels);

                await encoder.FlushAsync();
            }

            Debug.WriteLine($"---ImageHelpers.SaveToJpgFileAsync {fileName} saved to {file.Path}");

            return file;
        }

    }
}
