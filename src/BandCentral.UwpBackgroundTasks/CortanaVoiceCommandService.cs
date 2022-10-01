using BandCentral.Models.Secrets;
using Microsoft.Band;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.VoiceCommands;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using BandCentral.Models.Bing;

namespace BandCentral.UwpBackgroundTasks
{
    public sealed class CortanaVoiceCommandService : XamlRenderingBackgroundTask
    {
        private static ApplicationDataContainer localSettings;
        private VoiceCommandServiceConnection voiceServiceConnection;
        private BackgroundTaskDeferral serviceDeferral;
        private IBackgroundTaskInstance taskInstance;

        protected override async void OnRun(IBackgroundTaskInstance taskInstance)
        {
            this.taskInstance = taskInstance;
            serviceDeferral = taskInstance.GetDeferral();
            
            taskInstance.Canceled += OnTaskCanceled;

            var triggerDetails = taskInstance.TriggerDetails as AppServiceTriggerDetails;
            
            if (triggerDetails != null && triggerDetails.Name == "CortanaVoiceCommandService")
            {
                try
                {
                    voiceServiceConnection =
                        VoiceCommandServiceConnection.FromAppServiceTriggerDetails(
                            triggerDetails);

                    voiceServiceConnection.VoiceCommandCompleted += OnVoiceCommandCompleted;

                    // must be called prior to any messages sent to Cortana. 
                    VoiceCommand voiceCommand = await voiceServiceConnection.GetVoiceCommandAsync();
                    var photoType = voiceCommand.Properties["photoType"][0];

                    switch (voiceCommand.CommandName)
                    {
                        case "updateBandBackground":
                            await SendCompletionForBackgroundUpdate(photoType);
                            break;
                        case "updateBandBackgroundAndTheme":
                            await SendCompletionForBackgroundAndThemeUpdate(photoType);
                            break;
                        default:
                            LaunchAppInForeground();
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Handling Voice Command failed {ex}" + ex);
                }
            }

        }

        private async Task SendCompletionForBackgroundUpdate(string photoType)
        {
            await ShowProgressScreen($"locating {photoType} photo");

            string imageFilePathUsed = "";

            var lowerType = photoType.ToLowerInvariant();

            if (lowerType == "bing" || lowerType == "bing image" || lowerType == "bing images")
            {
                imageFilePathUsed = await UpdateBackgroundWithBingAsync();
            }

            if (lowerType == "favorites" || lowerType == "favorite" || lowerType == "favs" || lowerType == "fav")
            {
                //imageFilePathUsed = await UpdateBackgroundWithFavoriteAsync();
            }

            //construct message
            var userMessage = new VoiceCommandUserMessage();
            var tiles = new List<VoiceCommandContentTile>();

            if (string.IsNullOrEmpty(imageFilePathUsed))
            {
                string foundNoTripToDestination = "Band was not updated :(";
                userMessage.DisplayMessage = foundNoTripToDestination;
                userMessage.SpokenMessage = foundNoTripToDestination;
            }
            else
            {
                // Set a title message for the page.
                string message = $"Your Band background has been updated!";
                userMessage.DisplayMessage = message;
                userMessage.SpokenMessage = message;

                var successTile = new VoiceCommandContentTile
                {
                    ContentTileType = VoiceCommandContentTileType.TitleWith68x68IconAndText,
                    Image = await StorageFile.GetFileFromPathAsync(imageFilePathUsed),
                    Title = "",
                    TextLine1 = ""
                };

                tiles.Add(successTile);

                var response = VoiceCommandResponse.CreateResponse(userMessage, tiles);
                
                await voiceServiceConnection.ReportSuccessAsync(response);
            }
        }

        private async Task SendCompletionForBackgroundAndThemeUpdate(string photoType)
        {
            await ShowProgressScreen($"beginning fav image update");
        }

        private async Task<string> UpdateBackgroundWithBingAsync()
        {
            

            localSettings = ApplicationData.Current.LocalSettings;
            if (localSettings == null)
                return "";

            //---------------- GET NEXT IMAGE TO USE ----------------//
            await ShowProgressScreen($"Getting today's Bing image");

            var todaysBingImage = await new BingImageHelper().GetTodaysBingImageAsync();

            if (todaysBingImage == null || string.IsNullOrEmpty($"http://www.bing.com{todaysBingImage.url}"))
            {
                Debug.WriteLine($"Cancelled. BingImage object was null or url was empty...");
                return "";
            }

            //---------------- GET BAND NAME ----------------//
            await ShowProgressScreen($"Finding your Band");

            string preferredBandName = "";
            object val;
            if (localSettings.Values.TryGetValue(GeneralConstants.PreferredBandNameKey, out val))
            {
                preferredBandName = (string) val;
                Debug.WriteLine($"{taskInstance.Task.Name}: Found Preferred Band {preferredBandName}...");
            }
            else
            {
                //user needs to have a preferred band name stored
                Debug.WriteLine($"{taskInstance.Task.Name}: ERROR!!! PreferredBandName not found");
                return "";
            }

            //---------------FIND PAIRED BANDS---------------------//

            var results = await BandClientManager.Instance.GetBandsAsync();
            if (results.Length == 0)
            {
                Debug.WriteLine($"{taskInstance.Task.Name} Cancelled. No Paired Bands found");
                return "";
            }
            else
            {
                Debug.WriteLine($"{taskInstance.Task.Name}: {results.Length} paired Bands found...");
            }

            //------------DOWNLOAD AND CROP THE IMAGE------------------//
            await ShowProgressScreen($"Downloading and cropping Bing image");

            var imageBitmap = await DownloadAndCropBingBitmapAsync($"http://www.bing.com{todaysBingImage.url}");

            if (imageBitmap == null)
            {
                Debug.WriteLine($"{taskInstance.Task.Name} Cancelled. Bing Image cropped bitmap was null");
                return "";
            }

            //----------------CONNECT TO BAND------------------//

            using (IBandClient selectedBandClient = await BandClientManager.Instance.ConnectAsync(results.FirstOrDefault(bandInfo => bandInfo.Name == preferredBandName)))
            {
                if (selectedBandClient == null)
                    return "";

                //------------CONVERT TO BANDIMAGE AND SEND TO BAND------------------//
                await ShowProgressScreen($"Setting band background now...");

                Debug.WriteLine($"{taskInstance.Task.Name}: SetMeTileImageAsync called...");

                await selectedBandClient.PersonalizationManager.SetMeTileImageAsync(imageBitmap.ToBandImage());

                //if successful, update last used and last completed
                localSettings.Values[BingConstants.BingTaskLastDayUpdatedKey] = todaysBingImage.startdate;
                localSettings.Values[BingConstants.BingTaskLastCompletedKey] = DateTime.Now.ToString("g", CultureInfo.InvariantCulture);
                
                Debug.WriteLine($"{taskInstance.Task.Name}: Completed. Image used http://www.bing.com{todaysBingImage.url}");
                
                //only if I have to save cropped image for toast notification
                var x =  await SaveToJpgFileAsync(imageBitmap, "BingImageOfTheDay.jpg", ApplicationData.Current.LocalFolder);
                
                return x.Path;
            }
        }

        //private async Task<string> UpdateBackgroundWithFavoriteAsync()
        //{

        //}
        
        
        private async Task ShowProgressScreen(string message)
        {
            var userProgressMessage = new VoiceCommandUserMessage();
            userProgressMessage.DisplayMessage = userProgressMessage.SpokenMessage = message;

            VoiceCommandResponse response = VoiceCommandResponse.CreateResponse(userProgressMessage);
            await voiceServiceConnection.ReportProgressAsync(response);
        }
        
        private async void LaunchAppInForeground()
        {
            var userMessage = new VoiceCommandUserMessage();
            userMessage.SpokenMessage = "Launching app...";
            userMessage.DisplayMessage = "Launching app...";

            var response = VoiceCommandResponse.CreateResponse(userMessage);

            response.AppLaunchArgument = "";

            await voiceServiceConnection.RequestAppLaunchAsync(response);
        }
        
        private void OnVoiceCommandCompleted(VoiceCommandServiceConnection sender, VoiceCommandCompletedEventArgs args)
        {
            this.serviceDeferral?.Complete();
        }
        
        private void OnTaskCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            Debug.WriteLine("Task cancelled, clean up");
            this.serviceDeferral?.Complete();
        }

        #region Bing image utilities

        private static async Task<WriteableBitmap> DownloadAndCropBingBitmapAsync(string imageUrl)
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
                    bandTileHeight = (int)bandModel == 1 ? 102 : 128;
                }

                //0.161458 is the expected value for the normal Bing Image size of 1920x1080
                var scaleRatio = (double)310 / (double)wbm.PixelWidth;
                Debug.WriteLine($"BingTask Image ScaleRatio: {scaleRatio}");

                var scaledHeight = wbm.PixelHeight * scaleRatio;
                Debug.WriteLine($"BingTask Image: Width: 310 x ScaledHeight: {scaledHeight}");

                var resizedBitmap = wbm.Resize((int)310, (int)scaledHeight, Windows.UI.Xaml.Media.Imaging.WriteableBitmapExtensions.Interpolation.Bilinear);

                Debug.WriteLine($"BingTask Image Resize Complete: Height: {resizedBitmap.PixelWidth} x Height: {resizedBitmap.PixelHeight}");

                return resizedBitmap.Crop(0, 0, (int)310, (int)bandTileHeight);
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

        #endregion

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
