using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Data.Xml.Dom;
using Windows.Storage;
using Windows.UI.Notifications;
using Windows.UI.Xaml.Media.Imaging;
using BandCentralBase.Common;
using Microsoft.Band;
using Newtonsoft.Json;

namespace BandCentral.WpBackgroundTasks
{
    public sealed class BingImageBackgroundTask : XamlRenderingBackgroundTask
    {
        private static ApplicationDataContainer localSettings;

        protected override async void OnRun(IBackgroundTaskInstance taskInstance)
        {
            var deferral = taskInstance.GetDeferral();
            localSettings = ApplicationData.Current.LocalSettings;
            if (localSettings == null)
                return;

            try
            {
                localSettings.Values[Constants.BingTaskLastAttemptedKey] = DateTime.Now.ToString("g", CultureInfo.InvariantCulture);
                localSettings.Values[Constants.BingTaskStatusKey] = $"Starting...";
                Debug.WriteLine($"Background {taskInstance.Task.Name} Starting...");
                Debug.WriteLine($"Background Cost: {BackgroundWorkCost.CurrentBackgroundWorkCost}");
                
                //---------------- GET NEXT IMAGE TO USE ----------------//
                var bingImagePath = await GetBingImagePathAsync();

                if (string.IsNullOrEmpty(bingImagePath))
                {
                    return;
                }

                //---------------- GET BAND NAME ----------------//

                string preferredBandName = "";
                object val;
                if (localSettings.Values.TryGetValue(Constants.PreferredBandNameKey, out val))
                {
                    preferredBandName = (string) val;
                    Debug.WriteLine($"BGTASK - {taskInstance.Task.Name}: Found Preferred Band {preferredBandName}...");
                    localSettings.Values[Constants.BingTaskStatusKey] = $"BGTASK - {taskInstance.Task.Name}: Found Preferred Band {preferredBandName}...";
                }
                else
                {
                    //user needs to have a preferred band name stored
                    Debug.WriteLine($"BGTASK - {taskInstance.Task.Name}: ERROR!!! PreferredBandName not found");
                    localSettings.Values[Constants.BingTaskStatusKey] = $"PreferredBandName not found";
                    return;
                }

                //---------------FIND PAIRED BANDS---------------------//

                var results = await BandClientManager.Instance.GetBandsAsync();
                if (results.Length == 0)
                {
                    localSettings.Values[Constants.BingTaskStatusKey] = $"No Paired Bands found";
                    return;
                }
                else
                {
                    localSettings.Values[Constants.BingTaskStatusKey] = $"BGTASK - { taskInstance.Task.Name}: { results.Length} paired Bands found...";
                    Debug.WriteLine($"BGTASK - {taskInstance.Task.Name}: {results.Length} paired Bands found...");
                }

                //----------------CONNECT TO BAND------------------//

                using (IBandClient selectedBand = await BandClientManager.Instance.ConnectAsync(results.FirstOrDefault(bandInfo => bandInfo.Name == preferredBandName)))
                {
                    if (selectedBand == null)
                        return;

                    //------------GET WRITEABLEBITMAP FROM THE PHOTO FILE------------------//

                    var imageBitmap = await GetCroppedBitmapAsync(bingImagePath);

                    if (imageBitmap == null)
                    {
                        localSettings.Values[Constants.BingTaskStatusKey] = $"Task Cancelled: bitmap was null";
                        return;
                    }

                    //------------CONVERT TO BANDIMAGE AND SEND TO BAND------------------//
                    localSettings.Values[Constants.BingTaskStatusKey] = $"Sending to Band";
                    Debug.WriteLine($"BGTASK - {taskInstance.Task.Name}: SetMeTileImageAsync called...");

                    await selectedBand.PersonalizationManager.SetMeTileImageAsync(imageBitmap.ToBandImage());

                    localSettings.Values[Constants.BingTaskLastAttemptedKey] = DateTime.Now.DayOfYear;
                    localSettings.Values[Constants.BingTaskStatusKey] = $"{taskInstance.Task.Name} Completed!";
                    Debug.WriteLine($"BGTASK - {taskInstance.Task.Name}: Completed!");

                    //------------------TOAST NOTIFICATION-------------------//
                    ShowNotification(bingImagePath);
                }
            }
            catch (BandException ex)
            {
                localSettings.Values[Constants.BingTaskStatusKey] = $"Band Exception: {ex.Message}";
                Debug.WriteLine($"UpdateMeTileTask BandException: {ex.Message}");
            }
            catch (Exception ex)
            {
                localSettings.Values[Constants.BingTaskStatusKey] = $"General Exception: {ex.Message}";
                Debug.WriteLine($"UpdateMeTileTask General Exception: {ex.Message}");
            }
            finally
            {
                deferral.Complete();
            }
        }

        private async Task<string> GetBingImagePathAsync()
        {
            try
            {
                object lastDayUsed;
                if (localSettings.Values.TryGetValue("", out lastDayUsed))
                {
                    if (DateTime.Now.DayOfYear <= (int) lastDayUsed)
                    {
                        localSettings.Values[Constants.BingTaskStatusKey] = $"Task cancelled: Already have today's (day #{lastDayUsed}) image on the Band";
                        return null;
                    }
                }
                
                var handler = new HttpClientHandler();
                if (handler.SupportsAutomaticDecompression)
                    handler.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;

                using (var client = new HttpClient(handler))
                {
                    var stringResponse = await client.GetStringAsync(@"http://www.bing.com/HPImageArchive.aspx?format=js&idx=0&n=1");

                    var result = JsonConvert.DeserializeObject<BingImagesResult>(stringResponse);

                    var url = $"http://www.bing.com{result.images[0].url}";

                    localSettings.Values[Constants.BingTaskStatusKey] = $"Retrieved Image Url: {url}";

                    return url;
                }
            }
            catch (HttpRequestException ex)
            {
                localSettings.Values[Constants.BingTaskStatusKey] = $"Task cancelled - HttpException: {ex.Message}";
                return null;
            }
            catch (Exception ex)
            {
                localSettings.Values[Constants.BingTaskStatusKey] = $"Task cancelled - Exception: {ex.Message}";
                return null;
            }
        }

        private async Task<WriteableBitmap> GetCroppedBitmapAsync(string imageUrl)
        {
            try
            {
                //figure out image width to use
                int bandTileWidth = 128;
                object bandModel;
                if (localSettings.Values.TryGetValue(Constants.BandModelKey, out bandModel))
                {
                    bandTileWidth = (int) bandModel == 1 ? 102 : 128;
                }

                //--------------------------OPEN FILE----------------------//

                var handler = new HttpClientHandler();
                if (handler.SupportsAutomaticDecompression)
                    handler.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;

                using (var client = new HttpClient(handler))
                using (var stream = await client.GetStreamAsync(imageUrl))
                using (var memStream = new MemoryStream())
                {
                    //--------------------LOAD WRITEABLEBITMAP---------------//
                    stream.CopyTo(memStream);
                    memStream.Position = 0;

                    var wb = new WriteableBitmap(310, bandTileWidth);
                    Debug.WriteLine($"{Constants.BingTaskName} - WriteableBitmap created...");

                    await wb.SetSourceAsync(memStream.AsRandomAccessStream());
                    Debug.WriteLine($"{Constants.BingTaskName} - WriteableBitmap.SetSourceAsync using stream of {memStream.Length} bytes...");

                    wb.Invalidate();
                    return wb;
                }


            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GetCroppedBitmapAsync Exception: {ex.Message}");
                return null;
            }
        }

        private static void ShowNotification(string imagePath)
        {
            try
            {
                //TODO uncommentonce AutoFavMute is back in the aopo
                //check to see if toast is muted
                //object notificationMute;
                //if (localSettings.Values.TryGetValue("AutoFavNotificationMute", out notificationMute))
                //{
                //    if ((bool) notificationMute) return;
                //}

                //clear previous notifications?
                ToastNotificationManager.History.Clear();

                //EXPERIMENTAL
                //var payload = $@"
                //<toast scenario='reminder'>
                //    <visual>
                //        <binding template='ToastGeneric'>
                //            <image placement='appLogoOverride' hint-crop='circle' src='ms-appx:///Assets/Square150x150Logo.scale-100.png' />
                //            <text>Band background was updated!</text>
                //            <text>Expand this notification to see the AutoFav image used</text>
                //            <image src='ms-appdata:///local/{imagePath}'/>
                //        </binding>
                //    </visual>
                //</toast>";

                //WORKING
                //image based toast
                var payload = $@"
                <toast scenario='reminder'>
                    <visual>
                        <binding template='ToastGeneric'>
                            <text>Band background was updated!</text>
                            <text>Below is your new Band background photo...</text>
                            <image src='ms-appdata:///local/{imagePath}'/>
                        </binding>
                    </visual>
                </toast>";

                var doc = new XmlDocument();
                doc.LoadXml(payload);
                
                ToastNotificationManager.CreateToastNotifier().Show(new ToastNotification(doc) { Tag = "BingTask" });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"BG Task ShowNotification Exception: {ex.Message}");
            }
        }
    }
}
