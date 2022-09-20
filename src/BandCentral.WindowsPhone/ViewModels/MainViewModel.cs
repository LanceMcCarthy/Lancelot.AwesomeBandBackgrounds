using Windows.UI;
using FlickrNet;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.ApplicationModel;
using Windows.Graphics.Display;
using Windows.Networking.Connectivity;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;
using BandCentral.Commands;
using BandCentral.Common;
using BandCentral.WindowsBase.Common;
using BandCentralBase.ViewModels;
using BandCentralBase.Common;
using Size = Windows.Foundation.Size;
using Microsoft.Band;
using Microsoft.Band.Notifications;
using Photo = FlickrNet.Photo;

namespace BandCentral.ViewModels
{
    [DataContract]
    public class MainViewModel : ViewModelCore
    {
        //band items
        private string firmwareVersion, hardwareVersion = "[Band not connected]";
        private IBandClient selectedBand;
        private int remainingTileCapacity = 1;
        private BandColorTheme currentBandTheme;
        private IBandInfo currentBand;
        private string preferredBandName;

        private readonly ApplicationDataContainer localSettings;
        private readonly StorageFolder localFolder = ApplicationData.Current.LocalFolder;
        private CoreDispatcher dispatcher;
        private string isBusyMessage = "";
        private bool isBusy;
        private bool hasBeenRated;
        private bool isBandConnected;
        private bool areFavoritesLoaded;
        private bool areThemeHistoryItemsLoaded;
        private bool isNetworkConnected;
        private bool? connectOnLaunch;
        private bool? loadCurrentBandImage = true;
        private int applicationRuns;
        private double displayScaleFactor = 1;        
        private Photo selectedFlickrPhoto;
        private FlickrFav selectedFav;
        private WriteableBitmap currentBandImage;
        private ObservableCollection<FilterListItem> filterEffects = new ObservableCollection<FilterListItem>();
        private ObservableCollection<FlickrFav> flickrFavs;
        private ObservableCollection<BandColorTheme> themeHistory;
        private ObservableCollection<IBandInfo> bands;
        private int bandModel = 1;
        private double tileAspectRatio;

        public MainViewModel()
        {
            AddToFavoritesCommand = new AddToFavoritesCommand();
            RemoveFromFavoritesCommand = new RemoveFavoriteCommand();
            SendToBandCommand = new SendToBandCommand();
            EditImageCommand = new EditImageCommand();

            if(!DesignMode.DesignModeEnabled)
            {
                dispatcher = Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher;
                //roamingSettings = ApplicationData.Current.RoamingSettings;
                localSettings = ApplicationData.Current.LocalSettings;
            }

            LoadData();
        }

        #region collections

        [DataMember]
        public ObservableCollection<FlickrFav> FlickrFavs
        {
            get { return flickrFavs ?? (flickrFavs = new ObservableCollection<FlickrFav>()); }
            set
            {
                if(flickrFavs == value) return;
                flickrFavs = value;
                OnPropertyChanged();
            }
        }

        [DataMember]
        public ObservableCollection<BandColorTheme> ThemeHistory
        {
            get { return themeHistory ?? (themeHistory = new ObservableCollection<BandColorTheme>()); }
            set
            {
                if (themeHistory == value) return;
                themeHistory = value;
                OnPropertyChanged();
            }
        }

        [DataMember]
        public ObservableCollection<FilterListItem> FilterEffects
        {
            get { return filterEffects; }
            set
            {
                if (filterEffects == value) return;
                filterEffects = value;
                OnPropertyChanged();
            }
        }

        [IgnoreDataMember]
        public ObservableCollection<IBandInfo> Bands
        {
            get { return bands ?? (bands = new ObservableCollection<IBandInfo>()); }
            set
            {
                bands = value;
                OnPropertyChanged();
            }
        }

        [IgnoreDataMember]
        public ObservableCollection<int> AvailableBandModels
        {
            get
            {
                return new ObservableCollection<int> {1,2};
            }
        } 
        
        #endregion

        #region commands

        public ICommand AddToFavoritesCommand { get; private set; }

        public ICommand RemoveFromFavoritesCommand { get; private set; }
        
        public ICommand SendToBandCommand { get; private set; }

        public ICommand EditImageCommand { get; private set; }

        #endregion

        #region runtime properties
        
        [DataMember]
        public Photo SelectedFlickrPhoto
        {
            get
            {
                return selectedFlickrPhoto;
            }
            set
            {
                if(selectedFlickrPhoto == value) return;
                selectedFlickrPhoto = value;
                OnPropertyChanged();
            }
        }

        [DataMember]
        public FlickrFav SelectedFav
        {
            get { return selectedFav; }
            set { selectedFav = value; OnPropertyChanged(); }
        }

        [IgnoreDataMember]
        public string AppVersion
        {
            get
            {
                if(DesignMode.DesignModeEnabled)return "D1.1.26";
                var nameHelper = Package.Current.Id;
                return nameHelper.Version.Major + "." + nameHelper.Version.Minor + "." + nameHelper.Version.Build;
            }
        }

        [IgnoreDataMember]
        public WriteableBitmap CurrentBandImage
        {
            get { return currentBandImage; }
            set
            {
                currentBandImage = value;
                OnPropertyChanged();
            }
        }

        [DataMember]
        public bool IsBusy
        {
            get { return isBusy; }
            set
            {
                isBusy = value;
                OnPropertyChanged();
            }
        }

        [DataMember]
        public string IsBusyMessage
        {
            get { return isBusyMessage; }
            set
            {
                isBusyMessage = value;
                OnPropertyChanged();
            }
        }

        [DataMember]
        public bool IsBandConnected
        {
            get { return isBandConnected; }
            set
            {
                if(isBandConnected == value) return;
                isBandConnected = value;
                OnPropertyChanged();
            }
        }

        [DataMember]
        public double DisplayScaleFactor
        {
            get { return displayScaleFactor; }
            set
            {
                if(Math.Abs(displayScaleFactor - value) < 0.1) return;
                displayScaleFactor = value;
                OnPropertyChanged();
            }
        }

        [DataMember]
        public double TileAspectRatio
        {
            get { return tileAspectRatio; }
            set
            {
                tileAspectRatio = value;
                OnPropertyChanged();
            }
        }

        [DataMember]
        public Size WindowBounds { get; private set; }

        [DataMember]
        public Size ListItemSize { get; set; }

        [DataMember]
        public bool HideReviewReminderForSession { get; set; }

        public bool IsNetworkConnected
        {
            get { return isNetworkConnected; }
            set { isNetworkConnected = value; OnPropertyChanged(); }
        }

        #endregion

        #region band specific properties

        public BandColorTheme CurrentBandTheme
        {
            get { return currentBandTheme ?? (currentBandTheme = new BandColorTheme(Colors.DimGray, Colors.DimGray, Colors.DimGray, Colors.DimGray, Colors.DimGray, Colors.DimGray)); }
            set { currentBandTheme = value; OnPropertyChanged(); }
        }

        [IgnoreDataMember]
        public IBandInfo CurrentBand
        {
            get { return currentBand; }
            set
            {
                currentBand = value;
                PreferredBandName = value.Name;
                OnPropertyChanged();
            }
        }

        [DataMember]
        public string FirmwareVersion
        {
            get { return firmwareVersion; }
            set { firmwareVersion = value; OnPropertyChanged(); }
        }

        [DataMember]
        public string HardwareVersion
        {
            get { return hardwareVersion; }
            set { hardwareVersion = value; OnPropertyChanged(); }
        }

        [DataMember]
        public int RemainingTileCapacity
        {
            get { return remainingTileCapacity; }
            set { remainingTileCapacity = value; OnPropertyChanged(); }
        }

        [DataMember]
        public int BandModel
        {
            get
            {
                object val;
                if (localSettings != null && localSettings.Values.TryGetValue(Constants.BandModelKey, out val))
                {
                    bandModel = (int)val;
                }

                return bandModel;
            }
            set
            {
                if (bandModel == value) return;
                if (localSettings != null) localSettings.Values[Constants.BandModelKey] = value;
                bandModel = value;
                OnPropertyChanged();
            }
        }

        [IgnoreDataMember]
        public Size MeTileSize
        {
            get
            {
                switch (BandModel)
                {
                    case 1:
                        return new Size(310, 102);
                    case 2:
                        return new Size(310, 128);
                    default:
                        return new Size(310, 128);
                }
            }
        }

        #endregion

        #region Local Settings

        [DataMember]
        public string PreferredBandName
        {
            get
            {
                object val;
                if (localSettings != null && localSettings.Values.TryGetValue(Constants.PreferredBandNameKey, out val))
                {
                    preferredBandName = (string)val;
                }

                return preferredBandName;
            }
            set
            {
                preferredBandName = value;
                if (localSettings != null) localSettings.Values[Constants.PreferredBandNameKey] = preferredBandName;
                OnPropertyChanged();
            }
        }

        [DataMember]
        public bool? ConnectOnLaunch
        {
            get
            {
                object val;
                if(localSettings != null && localSettings.Values.TryGetValue("ConnectOnLaunch", out val))
                {
                    connectOnLaunch = (bool?) val;
                }

                return connectOnLaunch;
            }
            set
            {
                if(connectOnLaunch == value) return;
                if(localSettings != null) localSettings.Values["ConnectOnLaunch"] = value;
                connectOnLaunch = value;
                OnPropertyChanged();
            }
        }

        [DataMember]
        public bool? LoadCurrentBandImage
        {
            get
            {
                object val;

                if(localSettings != null && localSettings.Values.TryGetValue("LoadCurrentBandImage", out val))
                {
                    loadCurrentBandImage = (bool?)val;
                }

                return loadCurrentBandImage;
            }
            set
            {
                if(loadCurrentBandImage == value) return;
                loadCurrentBandImage = value;
                if(localSettings != null) localSettings.Values["LoadCurrentBandImage"] = value;
                OnPropertyChanged();
            }
        }

        [DataMember]
        public bool HasBeenRated
        {
            get
            {
                object val;
                if(localSettings != null && localSettings.Values.TryGetValue("HasBeenRated", out val))
                {
                    hasBeenRated = (bool)val;
                }
                return hasBeenRated;
            }
            set
            {
                if(value == hasBeenRated) return;
                hasBeenRated = value;
                if(localSettings != null) localSettings.Values["HasBeenRated"] = value;
                OnPropertyChanged();
            }
        }

        [DataMember]
        public int ApplicationRuns
        {
            get
            {
                object val;
                if(localSettings != null && localSettings.Values.TryGetValue("ApplicationRuns", out val))
                {
                    applicationRuns = (int)val;

                }
                return applicationRuns;
            }
            set
            {
                if(applicationRuns == value) return;
                applicationRuns = value;
                if(localSettings != null) localSettings.Values["ApplicationRuns"] = value;
                OnPropertyChanged();
            }
        }

        #endregion
        
        private async void LoadData()
        {
            if (DesignMode.DesignModeEnabled) return;

            //listen for nework changes
            Network.InternetConnectionChanged += async (s, e) =>
            {
                await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    IsNetworkConnected = e.IsConnected;

                    //var statusBar = StatusBar.GetForCurrentView();

                    //if (e.IsConnected)
                    //{
                    //    statusBar.ProgressIndicator.Text = "";
                    //    await statusBar.ProgressIndicator.HideAsync();
                    //}
                    //else
                    //{
                    //    await statusBar.ProgressIndicator.ShowAsync();
                    //    statusBar.BackgroundColor = Colors.Red;
                    //    statusBar.ForegroundColor = Colors.White;
                    //    statusBar.ProgressIndicator.Text = "Warning: Internet is not available!";
                    //}
                });
            };

            ApplicationRuns++;
            
            //NOTE code above has been replace by a public facing method here
            UpdateAspectRatio();

            if (!DesignMode.DesignModeEnabled)
                DisplayScaleFactor = DisplayInformation.GetForCurrentView().RawPixelsPerViewPixel;

            if (!areFavoritesLoaded)
            {
                //await LoadFavoritesAsync();
                FlickrFavs = await LoadFavortiesJsonAsync();
            }

            if (!areThemeHistoryItemsLoaded)
            {
                ThemeHistory = await LoadThemeHistoryAsync();
            }

            //filter effects
            FilterEffects = await FilterEffectsHelper.GetAvailableFiltersAsync();
        }

        public void UpdateAspectRatio()
        {
            WindowBounds = new Size((int)Window.Current.Bounds.Width, (int)Window.Current.Bounds.Height);
            
            TileAspectRatio = BandModel == 1 ? 0.329 : 0.412; //0.303 (if widht is 320);

            ListItemSize = new Size { Width = WindowBounds.Width, Height = Math.Floor(WindowBounds.Width * TileAspectRatio) };
        }

        public bool GetCurrentInternetConnection()
        {
            IsNetworkConnected = NetworkInformation.GetInternetConnectionProfile()?.GetNetworkConnectivityLevel() == NetworkConnectivityLevel.InternetAccess;
            return IsNetworkConnected;
        }

        #region  Data Saving/Loading Tasks

        public async Task<bool> SaveFavoritesJsonAsync()
        {
            if(DesignMode.DesignModeEnabled) return true;
            Exception exception = null;

            try
            {
                var file = await localFolder.CreateFileAsync("FavoritesJson.txt", CreationCollisionOption.ReplaceExisting);

                var settings = new DataContractJsonSerializerSettings
                {
                    KnownTypes = new List<Type>() { typeof(FlickrFav), typeof(ObservableCollection<FlickrFav>) }
                };

                using(var stream = await file.OpenStreamForWriteAsync())
                {
                    var serializer = new DataContractJsonSerializer(typeof(ObservableCollection<FlickrFav>), settings);
                    serializer.WriteObject(stream, FlickrFavs);

                    Debug.WriteLine("---------{0} Favorites Saved---------", FlickrFavs.Count);
                }
            }
            catch(FileNotFoundException fnfEx)
            {
                Debug.WriteLine($"SaveFavoritesJsonAsync FNF-EX: {fnfEx.Message}");
                return false;
            }
            catch(Exception ex)
            {
                exception = ex;
            }

            if(exception == null) return false;

            Debug.WriteLine("Favorites JSON Save Exception: {0}", exception.Message);
            await new MessageDialog("There was a problem loading your favorites file").ShowAsync();

            return false;
        }

        private async Task<ObservableCollection<FlickrFav>> LoadFavortiesJsonAsync()
        {
            if(DesignMode.DesignModeEnabled) return null;
            Exception exception = null;

            try
            {
                //Using datacontractserializer
                var file = await localFolder.GetFileAsync("FavoritesJson.txt");

                var settings = new DataContractJsonSerializerSettings
                {
                    KnownTypes = new List<Type>() { typeof(FlickrFav), typeof(ObservableCollection<FlickrFav>) }
                };

                using(var stream = await file.OpenStreamForReadAsync())
                {
                    var serializer = new DataContractJsonSerializer(typeof(ObservableCollection<FlickrFav>), settings);

                    var favs = serializer.ReadObject(stream) as ObservableCollection<FlickrFav>;
                    var list = new ObservableCollection<FlickrFav>();

                    if(favs == null)
                    {
                        areFavoritesLoaded = false;
                        return list;
                    }

                    foreach(var photo in favs)
                    {
                        list.Add(photo);
                    }

                    Debug.WriteLine("--- {0} favorites loaded ---", list.Count);

                    areFavoritesLoaded = true;
                    return list;
                }
            }
            catch(FileNotFoundException fnfException)
            {
                Debug.WriteLine("No favorties json file present");
                return new ObservableCollection<FlickrFav>();
            }
            catch(Exception ex)
            {
                exception = ex;
            }

            if(!string.IsNullOrEmpty(exception.Message))
            {
                Debug.WriteLine("Favorites Json Load Exception: {0}", exception.Message);
                await new MessageDialog("There was a problem loading your favorites file").ShowAsync();
            }

            return new ObservableCollection<FlickrFav>();
        }

        public async Task<bool> SaveThemeHistoryAsync()
        {
            if(DesignMode.DesignModeEnabled) return true;
            Exception exception = null;

            var themeList = new List<List<Color>>();

            foreach (var bandColorTheme in ThemeHistory)
            {
                //using the order of the BandColorTheme object's overloaded ctor parameters (used when loading colors into app)
                var item = new List<Color>
                {
                    bandColorTheme.BaseColor,
                    bandColorTheme.HighlightColor,
                    bandColorTheme.LowLightColor,
                    bandColorTheme.SecondaryColor,
                    bandColorTheme.HighContrastColor,
                    bandColorTheme.MutedColor
                };

                themeList.Add(item);
            }

            try
            {
                //Using datacontract JSON serializer
                var file = await localFolder.CreateFileAsync("ThemeHistoryJson.txt", CreationCollisionOption.ReplaceExisting);

                var settings = new DataContractJsonSerializerSettings
                {
                    KnownTypes = new List<Type>() { typeof(List<Color>), typeof(List<List<Color>>) }
                };

                using(var stream = await file.OpenStreamForWriteAsync())
                {
                    var serializer = new DataContractJsonSerializer(typeof(List<List<Color>>), settings);
                    serializer.WriteObject(stream, themeList);

                    Debug.WriteLine("---------{0} ThemeHistory Saved---------", themeList.Count);
                }
            }
            catch(FileNotFoundException fnfException)
            {
                Debug.WriteLine($"No ThemeHistory JSON file present: {fnfException.Message}");
                return false;
            }
            catch(Exception ex)
            {
                exception = ex;
            }

            if(exception == null) return false;

            Debug.WriteLine($"ThemeHistory JSON Save Exception: {exception.Message}");
            await new MessageDialog("There was a problem saving your ThemeHistory file").ShowAsync();

            return false;
        }

        private async Task<ObservableCollection<BandColorTheme>> LoadThemeHistoryAsync()
        {
            if(DesignMode.DesignModeEnabled) return null;
            Exception exception = null;

            try
            {
                //Using datacontractserializer
                var file = await localFolder.GetFileAsync("ThemeHistoryJson.txt");

                var settings = new DataContractJsonSerializerSettings
                {
                    KnownTypes = new List<Type>() { typeof(List<Color>), typeof(List<List<Color>>) }
                };

                using(var stream = await file.OpenStreamForReadAsync())
                {
                    var serializer = new DataContractJsonSerializer(typeof(List<List<Color>>), settings);

                    var themes = serializer.ReadObject(stream) as List<List<Color>>;
                    var list = new ObservableCollection<BandColorTheme>();

                    if(themes == null)
                    {
                        areThemeHistoryItemsLoaded = false;
                        return list;
                    }

                    foreach(var bandTheme in themes)
                    {
                        var item = new BandColorTheme(
                            bandTheme[0], bandTheme[1], 
                            bandTheme[2], bandTheme[3],
                            bandTheme[4], bandTheme[5]);

                        list.Add(item);
                    }

                    Debug.WriteLine("--- {0} ThemeHistory loaded ---", list.Count);

                    areThemeHistoryItemsLoaded = true;
                    return list;
                }
            }
            catch(FileNotFoundException fnfException)
            {
                Debug.WriteLine($"No ThemeHistory json file present: {fnfException.Message}");
                return new ObservableCollection<BandColorTheme>();
            }
            catch(Exception ex)
            {
                exception = ex;
            }

            if(!string.IsNullOrEmpty(exception.Message))
            {
                Debug.WriteLine("Favorites ThemeHistory Load Exception: {0}", exception.Message);
                await new MessageDialog("There was a problem loading your ThemeHistory file").ShowAsync();
            }

            return new ObservableCollection<BandColorTheme>();
        }

        public async Task ClearThemeHistoryAsync()
        {
            ThemeHistory.Clear();
            await SaveThemeHistoryAsync();
        }

        #endregion

        #region Band Connection Tasks

        public async Task<bool> InitializeBandInformationAsync()
        {
            IsBusy = true;
            IsBusyMessage = $"connecting to band...";

            try
            {
                var band = await GetCurrentBandInfoAsync();
                IsBusyMessage = $"connecting to {band.Name}...";

                using (var activeBandClient = await BandClientManager.Instance.ConnectAsync(band))
                {
                    IsBusyMessage = "getting theme...";

                    //-------------Get Theme---------------//
                    var theme = await activeBandClient.PersonalizationManager.GetThemeAsync();
                    var colorTheme = new BandColorTheme(
                        theme.Base.ToColor(),
                        theme.Highlight.ToColor(),
                        theme.Lowlight.ToColor(),
                        theme.SecondaryText.ToColor(),
                        theme.HighContrast.ToColor(),
                        theme.Muted.ToColor());

                    CurrentBandTheme = colorTheme;

                    //if user want to load current image //TODO
                    if (LoadCurrentBandImage == true)
                    {
                        //-------------Get MeTile---------------//
                        IsBusyMessage = "getting current background...";

                        var bandImage = await activeBandClient.PersonalizationManager.GetMeTileImageAsync();
                        var wb = bandImage.ToWriteableBitmap();
                        wb.Invalidate();
                        Debug.WriteLine($"InitializeBandInformationAsync - BandImage WB: PixelWidth {wb.PixelWidth} x PixelHeight {wb.PixelHeight}");
                        CurrentBandImage = wb;
                    }

                    //-------------Get Specs---------------//

                    IsBusyMessage = "getting specs...";

                    FirmwareVersion = await activeBandClient.GetFirmwareVersionAsync();
                    Debug.WriteLine($"Firmware Version {FirmwareVersion}");

                    HardwareVersion = await activeBandClient.GetHardwareVersionAsync();
                    Debug.WriteLine($"Hardware Version {HardwareVersion}");

                    int mainVersion;

                    if (int.TryParse(HardwareVersion, out mainVersion))
                        BandModel = mainVersion < 20 ? 1 : 2;

                    Debug.WriteLine($"Band Model {BandModel}");

                    //always update dimensions after getting specs
                    UpdateAspectRatio();

                    IsBandConnected = true;
                    IsBusyMessage = "connected...";

                    return true;
                }
            }
            catch (BandException ex)
            {
                await new MessageDialog($"InitializeBandInformationAsync() Error connecting to Band, make sure it is:" +
                                        $"\r\n-Powered ON" +
                                        $"\r\n-Is within range" +
                                        $"\r\n-You are not currently in an activity like Sleep tracking or Workout " +
                                        $"\r\n-Is paired " +
                                        $"\r\n\nNOTE: This can happen if you no longer have the paired Band. Go to your Bluetooth settings and delete any old Bands." +
                                        $"\r\n\nError: {ex.Message}").ShowAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("InitializeBandInformationAsync() Exception: {0}", ex.Message);
                await new MessageDialog($"InitializeBandInformationAsync() Exception:\r\n\nError Details: {ex.Message}").ShowAsync();
            }
            finally
            {
                IsBusyMessage = "";
                IsBusy = false;
            }

            return IsBandConnected = false; //if all fails
        }

        public async Task LoadHardwareSpecsAsync()
        {
            if (DesignMode.DesignModeEnabled) return;

            try
            {
                IsBusy = true;

                var band = await GetCurrentBandInfoAsync();
                IsBusyMessage = $"connecting to {band.Name}...";

                using (var activeBandClient = await BandClientManager.Instance.ConnectAsync(band))
                {
                    IsBusyMessage = "getting specs...";

                    FirmwareVersion = await activeBandClient.GetFirmwareVersionAsync();
                    Debug.WriteLine($"Firmware Version {FirmwareVersion}");

                    HardwareVersion = await activeBandClient.GetHardwareVersionAsync();
                    Debug.WriteLine($"Hardware Version {HardwareVersion}");

                    int mainVersion;

                    if (int.TryParse(HardwareVersion, out mainVersion))
                        BandModel = mainVersion < 20 ? 1 : 2;

                    Debug.WriteLine($"Band Model {BandModel}");
                }
            }
            catch (BandException ex)
            {
                Debug.WriteLine("Get FW-HW Version Exception: {0}", ex.Message);
            }
            catch (Exception ex)
            {
                await new MessageDialog("Error getting Band specifications: " + ex.Message).ShowAsync();
            }
            finally
            {
                IsBusyMessage = "";
                IsBusy = false;
            }
        }

        public async Task<bool> ResumeConnectionsAsync()
        {
            try
            {
                IsBusy = true;
                IsBusyMessage = "Reconnecting your Band because the app was suspended";

                CurrentBand = await GetCurrentBandInfoAsync();

                IsBandConnected = true;
                return true;
            }
            catch (Exception ex)
            {
                await new MessageDialog("Something went wrong resuming Band connection, restarting the app will fix this. \r\n\nError Details: " + ex.Message).ShowAsync();
                return IsBandConnected = false;
            }
            finally
            {
                IsBusy = false;
                IsBusyMessage = "";
            }
        }

        #endregion

        #region Band Tasks

        //one stop shop to get the CurrendBandInfo ALWAYS USE TO GET THE USERS BAND
        public async Task<IBandInfo> GetCurrentBandInfoAsync()
        {
            //if the Bands collection needs refresing, refresh it
            if (this.Bands == null || !this.Bands.Any())
                Bands = await BandConnectionHelper.FindPairedBandsAsync();

            //if the CurrentBand is populated, just return it
            if (CurrentBand != null)
                return CurrentBand;

            CurrentBand = await BandConnectionHelper.RefreshCurrentBandInfo(PreferredBandName, Bands.ToArray());

            return CurrentBand;
        }

        public async Task SetMeTileAsync(WriteableBitmap wb)
        {
            if (DesignMode.DesignModeEnabled) return;
            if (wb == null) return;

            try
            {
                IsBusy = true;
                IsBusyMessage = "setting band background image...";

                wb.Invalidate();

                var meTileImage = wb.ToBandImage();

                using (var activeBandClient = await BandClientManager.Instance.ConnectAsync(await GetCurrentBandInfoAsync()))
                {
                    await activeBandClient.PersonalizationManager.SetMeTileImageAsync(meTileImage);

                    CurrentBandImage = null;
                    CurrentBandImage = wb;

                    await activeBandClient.NotificationManager.VibrateAsync(VibrationType.OneToneHigh);
                }
            }
            catch (BandException ex)
            {
                await new MessageDialog($"SetMeTileAsync() Error connecting to Band, make sure it is:" +
                                        $"\r\n-Powered ON" +
                                        $"\r\n-Is within range" +
                                        $"\r\n-You are not currently in an activity like Sleep tracking or Workout " +
                                        $"\r\n-Is paired " +
                                        $"\r\n\nNOTE: This can happen if you no longer have the paired Band. Go to your Bluetooth settings and delete any old Bands." +
                                        $"\r\n\nError: {ex.Message}").ShowAsync();
            }
            catch (Exception ex)
            {
                await new MessageDialog("Set Current Me Tile Exception: " + ex.Message).ShowAsync();
            }
            finally
            {
                IsBusy = false;
                IsBusyMessage = "";
            }
        }

        public async Task<WriteableBitmap> GetMeTileAsync()
        {
            if (DesignMode.DesignModeEnabled)
                return null;

            //in case user doesnt want to load band image
            if (LoadCurrentBandImage == false)
                return null;

            try
            {
                IsBusy = true;
                var band = await GetCurrentBandInfoAsync();
                IsBusyMessage = $"connecting to {band.Name}...";

                using (var activeBandClient = await BandClientManager.Instance.ConnectAsync(band))
                {
                    IsBusyMessage = "retrieving your current image...";
                    // grab the Me Tile image from the device 
                    var bandImage = await activeBandClient.PersonalizationManager.GetMeTileImageAsync();
                    var wb = bandImage.ToWriteableBitmap();
                    wb.Invalidate();

                    IsBusy = false;
                    IsBusyMessage = "";
                    return wb;
                }
            }
            catch (BandException ex)
            {
                await new MessageDialog($"GetMeTileAsync() Error connecting to Band, make sure it is:" +
                                        $"\r\n-Powered ON" +
                                        $"\r\n-Is within range" +
                                        $"\r\n-You are not currently in an activity like Sleep tracking or Workout " +
                                        $"\r\n-Is paired " +
                                        $"\r\n\nNOTE: This can happen if you no longer have the paired Band. Go to your Bluetooth settings and delete any old Bands." +
                                        $"\r\n\nError: {ex.Message}").ShowAsync();
                return null;
            }
            catch (Exception ex)
            {
                await new MessageDialog($"Get Background error. If this continues to happen after you've set a new background image, please contact support./r/n/nError: {ex.Message}").ShowAsync();
                return null;
            }
            finally
            {
                IsBusy = false;
                IsBusyMessage = "";
            }
        }

        public async Task<bool> GetBandThemeAsync()
        {
            if (DesignMode.DesignModeEnabled) return false;

            try
            {
                IsBusy = true;

                var band = await GetCurrentBandInfoAsync();

                IsBusyMessage = $"connecting to {band.Name}...";

                using (var activeBandClient = await BandClientManager.Instance.ConnectAsync(band))
                {
                    IsBusyMessage = "getting theme...";

                    // get the current theme from the device     
                    BandTheme theme = await activeBandClient.PersonalizationManager.GetThemeAsync();

                    var colorTheme = new BandColorTheme(
                        theme.Base.ToColor(),
                        theme.Highlight.ToColor(),
                        theme.Lowlight.ToColor(),
                        theme.SecondaryText.ToColor(),
                        theme.HighContrast.ToColor(),
                        theme.Muted.ToColor());

                    CurrentBandTheme = colorTheme;

                    return true;
                }
            }
            catch (BandException ex)
            {
                await new MessageDialog($"GetBandThemeAsync() Error connecting to Band, make sure it is:" +
                                        $"\r\n-Powered ON" +
                                        $"\r\n-Is within range" +
                                        $"\r\n-You are not currently in an activity like Sleep tracking or Workout " +
                                        $"\r\n-Is paired " +
                                        $"\r\n\nNOTE: This can happen if you no longer have the paired Band. Go to your Bluetooth settings and delete any old Bands." +
                                        $"\r\n\nError: {ex.Message}").ShowAsync();
                return false;
            }
            catch (Exception ex)
            {
                await new MessageDialog("Exception getting Band theme: " + ex.Message).ShowAsync();
                return false;
            }
            finally
            {
                IsBusyMessage = "";
                IsBusy = false;
            }
        }

        public async Task<bool> SetBandThemeAsync(BandColorTheme colorTheme)
        {
            if (DesignMode.DesignModeEnabled) return false;

            try
            {
                IsBusy = true;

                var band = await GetCurrentBandInfoAsync();
                IsBusyMessage = $"connecting to {band.Name}...";

                using (var activeBandClient = await BandClientManager.Instance.ConnectAsync(band))
                {
                    IsBusyMessage = "setting theme...";

                    //convert my theme object to a band theme object and send it
                    await activeBandClient.PersonalizationManager.SetThemeAsync(new BandTheme()
                    {
                        Base = colorTheme.BaseBrush.Color.ToBandColor(),
                        Highlight = colorTheme.HighlightBrush.Color.ToBandColor(),
                        HighContrast = colorTheme.HighContrastBrush.Color.ToBandColor(),
                        Lowlight = colorTheme.LowLightBrush.Color.ToBandColor(),
                        Muted = colorTheme.MutedBrush.Color.ToBandColor(),
                        SecondaryText = colorTheme.SecondaryBrush.Color.ToBandColor()
                    });

                    await activeBandClient.NotificationManager.VibrateAsync(VibrationType.OneToneHigh);
                }

                //keep the list to the recent 8 themes
                if (ThemeHistory.Count > 8)
                {
                    ThemeHistory.RemoveAt(7);
                }

                //for some reason, if I add it directly to the HX, the HX item is changed when the user makes a change to the originating item
                var separatedThemeItem = new BandColorTheme(
                    colorTheme.BaseColor,
                    colorTheme.HighlightColor,
                    colorTheme.LowLightColor,
                    colorTheme.SecondaryColor,
                    colorTheme.HighContrastColor,
                    colorTheme.MutedColor);

                ThemeHistory.Insert(0, separatedThemeItem);

                await SaveThemeHistoryAsync();

                return true;
            }
            catch (BandException ex)
            {
                await new MessageDialog($"SetBandThemeAsync() Error connecting to Band, make sure it is:" +
                                        $"\r\n-Powered ON" +
                                        $"\r\n-Is within range" +
                                        $"\r\n-You are not currently in an activity like Sleep tracking or Workout " +
                                        $"\r\n-Is paired " +
                                        $"\r\n\nNOTE: This can happen if you no longer have the paired Band. Go to your Bluetooth settings and delete any old Bands." +
                                        $"\r\n\nError: {ex.Message}").ShowAsync();
                return false;
            }
            catch (Exception ex)
            {
                await new MessageDialog($"MainViewModel -> SetBandThemeAsync() Exception \r\n\nMessage: {ex.Message}").ShowAsync();
                return false;
            }
            finally
            {
                IsBusyMessage = "";
                IsBusy = false;
            }
        }

        #endregion
        
    }
}
