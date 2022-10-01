using Microsoft.Band;
using Newtonsoft.Json;
using NotificationsExtensions;
using NotificationsExtensions.Toasts;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Storage;
using Windows.UI.Notifications;
using Windows.UI.Xaml.Media.Imaging;
using BandCentral.Models.Favorites;
using BandCentral.Models.Secrets;

namespace BandCentral.UwpBackgroundTasks
{
    public sealed class BackgroundRotatorTask : XamlRenderingBackgroundTask
    {
        private static ApplicationDataContainer localSettings;

        protected override async void OnRun(IBackgroundTaskInstance taskInstance)
        {
            var deferral = taskInstance.GetDeferral();

            try
            {
                localSettings = ApplicationData.Current.LocalSettings;
                if (localSettings == null)
                {
                    Debug.WriteLine("Cancelled. LocalSettings was null...");
                    return;
                }

                localSettings.Values[GeneralConstants.BackgroundRotatorLastAttemptedKey] = DateTime.Now.ToString("g", CultureInfo.InvariantCulture);
                localSettings.Values[GeneralConstants.BackgroundRotatorStatusKey] = $"Starting...";
                Debug.WriteLine($"{taskInstance.Task.Name} Starting...");
                Debug.WriteLine($"Background Cost: {BackgroundWorkCost.CurrentBackgroundWorkCost}");

                //---------------- QUIET HOURS CHECK ----------------//
                if (IsDuringQuietHours())
                    return;

                //---------------- GET NEXT FAVORITE TO USE ----------------//
                var favorite = await GetNextFavoriteAsync(taskInstance.Task.Name);

                if (string.IsNullOrEmpty(favorite.LocalImageFileName))
                {
                    localSettings.Values[GeneralConstants.BackgroundRotatorStatusKey] = $"Cancelled. Image filename was empty...";
                    return;
                }

                //---------------- GET BAND NAME ----------------//

                string preferredBandName = "";
                object val;
                if (localSettings.Values.TryGetValue(GeneralConstants.PreferredBandNameKey, out val))
                {
                    preferredBandName = (string) val;
                    Debug.WriteLine($"{taskInstance.Task.Name}: Found Preferred Band {preferredBandName}...");
                    localSettings.Values[GeneralConstants.BackgroundRotatorStatusKey] = $"{taskInstance.Task.Name}: Found Preferred Band {preferredBandName}...";
                }
                else
                {
                    //user needs to have a preferred band name stored
                    Debug.WriteLine($"{taskInstance.Task.Name}: ERROR!!! PreferredBandName not found");
                    localSettings.Values[GeneralConstants.BackgroundRotatorStatusKey] = $"PreferredBandName not found";
                    return;
                }

                //---------------FIND PAIRED BANDS---------------------//

                var results = await BandClientManager.Instance.GetBandsAsync();
                if (results.Length == 0)
                {
                    localSettings.Values[GeneralConstants.BackgroundRotatorStatusKey] = $"No Paired Bands found";
                    return;
                }
                else
                {
                    localSettings.Values[GeneralConstants.BackgroundRotatorStatusKey] = $"{ taskInstance.Task.Name}: { results.Length} paired Bands found...";
                    Debug.WriteLine($"{taskInstance.Task.Name}: {results.Length} paired Bands found...");
                }

                //----------------CONNECT TO BAND------------------//

                using (IBandClient selectedBandClient = await BandClientManager.Instance.ConnectAsync(results.FirstOrDefault(bandInfo => bandInfo.Name == preferredBandName)))
                {
                    if (selectedBandClient == null)
                        return;

                    //------------GET WRITEABLEBITMAP FROM THE PHOTO FILE------------------//

                    var imageBitmap = await GetFavBitmapAsync(favorite.LocalImageFileName, taskInstance.Task.Name);

                    //------------CONVERT TO BANDIMAGE AND SEND TO BAND------------------//
                    localSettings.Values[GeneralConstants.BackgroundRotatorStatusKey] = $"Sending to Band";
                    Debug.WriteLine($"{taskInstance.Task.Name}: SetMeTileImageAsync called...");

                    await selectedBandClient.PersonalizationManager.SetMeTileImageAsync(imageBitmap.ToBandImage());

                    //------------SET BAND THEME------------------//
                    bool themeSuccessfullyUpdated = false;

                    if (favorite.UseCustomTheme)
                    {
                        localSettings.Values[GeneralConstants.BackgroundRotatorStatusKey] = $"Updating Band Theme...";
                        Debug.WriteLine($"{taskInstance.Task.Name}: UsePairedTheme is True, SetThemeAsync Called...");

                        await selectedBandClient.PersonalizationManager.SetThemeAsync(new BandTheme
                        {
                            Base = favorite.Theme.BaseColor.ToBandColor(),
                            Highlight = favorite.Theme.HighlightColor.ToBandColor(),
                            HighContrast = favorite.Theme.HighContrastColor.ToBandColor(),
                            Lowlight = favorite.Theme.LowLightColor.ToBandColor(),
                            Muted = favorite.Theme.MutedColor.ToBandColor(),
                            SecondaryText = favorite.Theme.SecondaryColor.ToBandColor()
                        });

                        themeSuccessfullyUpdated = true;
                    }
                    
                    localSettings.Values[GeneralConstants.BackgroundRotatorLastCompletedKey] = DateTime.Now.ToString("g", CultureInfo.InvariantCulture);
                    localSettings.Values[GeneralConstants.BackgroundRotatorStatusKey] = $"{taskInstance.Task.Name} Completed. Image used {favorite.LocalImageFileName}";
                    Debug.WriteLine($"{taskInstance.Task.Name}: Completed. Image used {favorite.LocalImageFileName}");

                    //------------------TOAST NOTIFICATION-------------------//

                    //also updates tile using adaptive template
                    ShowNotification(favorite, themeSuccessfullyUpdated);
                }
            }
            catch (BandException ex)
            {
                if (localSettings != null)
                    localSettings.Values[GeneralConstants.BackgroundRotatorStatusKey] = $"Band Exception: {ex.Message}";
                Debug.WriteLine($"{taskInstance.Task.Name}: BandException: {ex}");
            }
            catch (Exception ex)
            {
                if (localSettings != null)
                    localSettings.Values[GeneralConstants.BackgroundRotatorStatusKey] = $"General Exception: {ex.Message}";
                Debug.WriteLine($"{taskInstance.Task.Name}: General Exception: {ex}");
            }
            finally
            {
                deferral.Complete();
            }
        }

        private static bool IsDuringQuietHours()
        {
            try
            {
                object quietHoursObj;
                if (localSettings.Values.TryGetValue(GeneralConstants.BackgroundRotatorUpdateQuietHoursEnabledKey, out quietHoursObj))
                {
                    if ((bool) quietHoursObj)
                    {
                        //--------------quiet hours is enabled-------------//
                        localSettings.Values[GeneralConstants.BackgroundRotatorStatusKey] = $"Checking quiet hours...";

                        //--------------get begin and end time---------------//
                        object startTimeObj;
                        if (localSettings.Values.TryGetValue(GeneralConstants.BackgroundRotatorUpdateQuietHoursStartKey,
                            out startTimeObj))
                        {
                            var start = TimeSpan.Parse((string) startTimeObj);

                            object endTimeObj;
                            if (localSettings.Values.TryGetValue(GeneralConstants.BackgroundRotatorUpdateQuietHoursEndKey,
                                out endTimeObj))
                            {
                                var end = TimeSpan.Parse((string) endTimeObj);

                                var now = DateTime.Now.TimeOfDay;

                                if (now > start && now < end)
                                {
                                    localSettings.Values[GeneralConstants.BackgroundRotatorStatusKey] = $"Cancelled Update! Reason: During Quiet Hours";
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"IsDuringQuietHours Exception: {ex}");
                return false;
            }

            return false;
        }

        private static async Task<FlickrFav> GetNextFavoriteAsync(string taskName)
        {
            int favoriteIndex = 0;

            try
            {
                //get index of next photo to use

                object lastIndexUsed;
                if (localSettings.Values.TryGetValue(GeneralConstants.BackgroundRotatorFavoriteIndexKey, out lastIndexUsed))
                {
                    Debug.WriteLine($"{taskName} - LastIndex: {lastIndexUsed}");
                    favoriteIndex = (int) lastIndexUsed + 1;
                }
                else
                {
                    Debug.WriteLine($"{taskName} - LastUsedIndex key not found, using default: 0");
                }

                var file = await ApplicationData.Current.LocalFolder.TryGetItemAsync(FlickrConstants.FlickrFavoritesFileName);
                if (file != null)
                {
                    var json = await FileIO.ReadTextAsync(file as StorageFile);
                    var favs = JsonConvert.DeserializeObject<ObservableCollection<FlickrFav>>(json);

                    var bgFavs = favs.Where(f => f.IsBackgroundFav).ToList();

                    if (favoriteIndex > bgFavs.Count)
                        favoriteIndex = 0;

                    Debug.WriteLine($"{taskName} - Favorites Loaded: {favs.Count} FavoriteIndex to use: {favoriteIndex}");

                    return bgFavs[favoriteIndex];
                }
                else
                {
                    localSettings.Values[GeneralConstants.BackgroundRotatorStatusKey] = $"Favorites file not present";
                    return null;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{taskName} GetNextFavoriteAsync Exception: {ex}");
                return null;
            }
            finally
            {
                //update setting to use the currentFavIndex value
                localSettings.Values[GeneralConstants.BackgroundRotatorFavoriteIndexKey] = favoriteIndex;
            }
        }
        
        private static async Task<WriteableBitmap> GetFavBitmapAsync(string fileName, string taskName)
        {
            //figure out image width to use
            int bandTileWidth = 128;
            object bandModel;
            if (localSettings.Values.TryGetValue(GeneralConstants.BandModelKey, out bandModel))
            {
                bandTileWidth = (int) bandModel == 1 ? 102 : 128;
            }

            //--------------------------OPEN FILE----------------------//

            var file = await ApplicationData.Current.LocalFolder.TryGetItemAsync(fileName);
            var imageFile = file as StorageFile;
            if (imageFile == null)
                return null;

            Debug.WriteLine($"{taskName}: File opened {file.Name}...");
            localSettings.Values[GeneralConstants.BackgroundRotatorStatusKey] = $"File opened {file.Name}...";

            //--------------------LOAD WRITEABLEBITMAP---------------//

            var wb = new WriteableBitmap(310, bandTileWidth);
            Debug.WriteLine($"{taskName} - WriteableBitmap created...");

            using (var stream = await imageFile.OpenAsync(FileAccessMode.Read))
            {
                Debug.WriteLine($"{taskName} - WriteableBitmap.SetSourceAsync using stream of {stream.Size} bytes...");
                localSettings.Values[GeneralConstants.BackgroundRotatorStatusKey] = $"WriteableBitmap.SetSourceAsync using stream of {stream.Size} bytes";
                await wb.SetSourceAsync(stream);
                wb.Invalidate();

                Debug.WriteLine($"{taskName} - WriteableBitmap final size - w: {wb.PixelWidth} x h:{wb.PixelHeight}");
                
                return wb;
            }

            //NOTE - let exception be caught up top
        }
        
        private static void ShowNotification(FlickrFav favorite, bool updatedTheme)
        {
            try
            {
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

                var themeMessage = updatedTheme ? " and theme " : " ";

                //WORKING
                //image based toast
                //var payload = $@"
                //<toast scenario='reminder'>
                //    <visual>
                //        <binding template='ToastGeneric'>
                //            <image placement='hero' src='ms-appdata:///local/{imagePath}' />
                //            <text>Band background{themeMessage}was updated!</text>
                //            <text>Below is your new Band background photo...</text>
                //            <image src='ms-appdata:///local/{imagePath}' />
                //        </binding>
                //    </visual>
                //</toast>";

                //var doc = new XmlDocument();
                //doc.LoadXml(payload);

                //Extensions
                var content = new ToastContent
                {
                    Visual = new ToastVisual
                    {
                        BindingGeneric = new ToastBindingGeneric()
                        {
                            Children =
                            {
                                new AdaptiveText
                                {
                                    Text = $"Your Band background{themeMessage}was updated!",
                                    HintMaxLines = 2,
                                    HintWrap = true
                                },
                                new AdaptiveText
                                {
                                    Text = $"{favorite.Photo.Description}",
                                    HintMaxLines = 2,
                                    HintWrap = true
                                }
                            },
                            HeroImage = new ToastGenericHeroImage
                            {
                                Source = $"ms-appdata:///local/{favorite.LocalImageFileName}"
                            },
                            Attribution = new ToastGenericAttributionText
                            {
                                Text = $"{favorite.Photo.OwnerName}"
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
                        //clear previous notifications? UPDATE taost shares same tag, so it's replaced
                        //ToastNotificationManager.History.Clear();

                        ToastNotificationManager.CreateToastNotifier().Show(new ToastNotification(doc) { Tag = "UpdateMeTile" });
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
        //            //            var payload = $@"
        //            //<tile>
        //            //    <visual branding='name'>
        //            //        < binding template = 'TileMedium' > 
        //            //            < text hint - style = 'caption' >Your Band background was updated!</ text >        
        //            //            < text hint - style = 'captionSubtle' hint - wrap = 'true' >Today's Bing Image of the Day is your Band's wallpaper</ text >                   
        //            //        </ binding >
        //            //        < binding template = 'TileWide' >
        //            //            < group > 
        //            //                < subgroup hint - weight = '33' >  
        //            //                    < image src='ms-appdata:///local/{imageFileName}' /> 
        //            //                </ subgroup >   
        //            //                < subgroup >    
        //            //                    < text hint - style = 'caption' >Your Band background was updated!</ text >
        //            //                    < text hint - style = 'captionSubtle' hint - wrap = 'true' hint - maxLines = '3' > Today's Bing Image of the Day is your Band's wallpaper</ text >
        //            //                </ subgroup > 
        //            //            </ group >
        //            //        </ binding >
        //            //        < binding template = 'TileLarge' >  
        //            //            < group > 
        //            //                < subgroup hint - weight = '33' > 
        //            //                    < image src='ms-appdata:///local/{imageFileName}' />                                              
        //            //                </ subgroup > 
        //            //                < subgroup > 
        //            //                    < text hint - style = 'caption' >Your Band background was updated!</ text >                                                      
        //            //                    < text hint - style = 'captionSubtle' hint - wrap = 'true' hint - maxLines = '3' > Today's Bing Image of the Day is your Band's wallpaper </ text >
        //            //                </ subgroup >
        //            //            </ group >
        //            //            < image src='ms-appdata:///local/{imageFileName}' />
        //            //        </ binding >
        //            //    </ visual >
        //            //</ tile > ";

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


        //private static async Task<BackgroundFav> GetNextFavoriteAsync(string taskName)
        //{
        //    int favoriteIndex = 0;

        //    try
        //    {
        //        //get index of next photo to use

        //        object lastIndexUsed;
        //        if (localSettings.Values.TryGetValue(GeneralConstants.BackgroundRotatorFavoriteIndexKey, out lastIndexUsed))
        //        {
        //            Debug.WriteLine($"{taskName} - LastIndex: {lastIndexUsed}");
        //            favoriteIndex = (int) lastIndexUsed + 1;
        //        }
        //        else
        //        {
        //            Debug.WriteLine($"{taskName} - LastUsedIndex key not found, using default: 0");
        //        }

        //        var file = await ApplicationData.Current.LocalFolder.TryGetItemAsync(GeneralConstants.BackgroundFavoritesFileName);
        //        if (file != null)
        //        {
        //            var json = await FileIO.ReadTextAsync(file as StorageFile);
        //            var favs = JsonConvert.DeserializeObject<ObservableCollection<BackgroundFav>>(json);

        //            if (favoriteIndex > favs.Count)
        //                favoriteIndex = 0;

        //            Debug.WriteLine($"{taskName} - Favorites Loaded: {favs.Count} FavoriteIndex to use: {favoriteIndex}");

        //            return favs[favoriteIndex];
        //        }
        //        else
        //        {
        //            localSettings.Values[GeneralConstants.BackgroundRotatorStatusKey] = $"BackgroundFav file not present";
        //            return null;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.WriteLine($"{taskName} GetFavoritePhotoFileName Exception: {ex}");
        //        return null;
        //    }
        //    finally
        //    {
        //        //update setting to use the currentFavIndex value
        //        localSettings.Values[GeneralConstants.BackgroundRotatorFavoriteIndexKey] = favoriteIndex;
        //    }
        //}

    }
}
