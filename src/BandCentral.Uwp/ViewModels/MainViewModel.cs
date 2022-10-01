using BandCentral.Models.Common;
using BandCentral.Models.Favorites;
using BandCentral.Models.Secrets;
using BandCentral.Uwp.Commands;
using BandCentral.Uwp.Common;
using CommonHelpers.Common;
using CommonHelpers.Mvvm;
using Microsoft.Band;
using Microsoft.Band.Notifications;
using Microsoft.HockeyApp;
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
using Windows.Storage;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;
using Photo = FlickrNet.Photo;
using Size = Windows.Foundation.Size;

namespace BandCentral.Uwp.ViewModels
{
    [DataContract]
    public class MainViewModel : ViewModelBase
    {
        #region Fields

        private readonly ApplicationDataContainer localSettings;
        private readonly ApplicationDataContainer roamingSettings;
        private readonly StorageFolder localFolder;
        private readonly StorageFolder roamingFolder;
        private CoreDispatcher dispatcher;
        private ObservableCollection<FilterListItem> filterEffects;
        private ObservableCollection<FlickrFav> flickrFavs;
        //private ObservableCollection<BackgroundFav> backgroundFavs;
        private string isBusyMessage = "";
        private bool hasFavs;
        private bool isBusy;
        private bool hasBeenRated;
        private bool iapBackgroundTasks;
        private bool isBandConnected;
        private bool areFavoritesLoaded;
        private bool areThemeHistoryItemsLoaded;
        private bool preventNavigation;
        private bool? autoFavsEnabled;
        private bool? autoFavNotificationMute;
        private int applicationRuns;
        private double displayScaleFactor = 1;
        private Photo selectedFlickrPhoto;
        private FlickrFav selectedFav;

        private ICommand getCurrentMeTileCommand;
        private ICommand addToFavoritesCommand;
        private ICommand removeFromFavoritesCommand;
        private ICommand sendToBandCommand;
        private ICommand editImageCommand;
        private ICommand editPaletteCommand;

        //Band related
        private string firmwareVersion, hardwareVersion = "[Band not connected]";
        private ObservableCollection<BandColorTheme> themeHistory;
        private ObservableCollection<IBandInfo> bands;
        private IBandInfo currentBand;
        private BandColorTheme currentBandTheme;
        private WriteableBitmap currentBandImage;
        private double tileAspectRatio;
        private int bandModel = 2;
        private string preferredBandName;
        private bool? loadCurrentBandImage = true;
        private bool? showTimeOverlay = false;
        private bool? connectOnLaunch = true;
        private bool disconnectedMode;

        //private bool hasFavorites;

        #endregion

        public MainViewModel()
        {
            if (!DesignMode.DesignModeEnabled)
            {
                localSettings = ApplicationData.Current.LocalSettings;
                localFolder = ApplicationData.Current.LocalFolder;
                roamingSettings = ApplicationData.Current.RoamingSettings;
                roamingFolder = ApplicationData.Current.RoamingFolder;

                dispatcher = Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher;
            }

            //FlickrFavs.CollectionChanged += FlickrFavs_CollectionChanged;

            LoadData();
        }

        //private void FlickrFavs_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        //{
        //    HasFavorites = FlickrFavs.Count > 0;
        //}

        #region Properties - Collections

        [DataMember]
        public ObservableCollection<FlickrFav> FlickrFavs
        {
            get { return flickrFavs ?? (flickrFavs = new ObservableCollection<FlickrFav>()); }
            set
            {
                if (flickrFavs == value) return;
                flickrFavs = value;
                OnPropertyChanged();
            }
        }

        //[IgnoreDataMember]
        //public ObservableCollection<BackgroundFav> BackgroundFavs
        //{
        //    get { return backgroundFavs ?? (backgroundFavs = new ObservableCollection<BackgroundFav>()); }
        //    set { backgroundFavs = value; OnPropertyChanged(); }
        //}

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
            get { return filterEffects ?? (filterEffects = new ObservableCollection<FilterListItem>()); }
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
        public ObservableCollection<int> AvailableBandModels => new ObservableCollection<int> { 1, 2 };

        #endregion

        #region Properties - Main

        //public bool HasFavorites
        //{
        //    get { return hasFavorites; }
        //    set { hasFavorites = value; OnPropertyChanged();}
        //}

        [DataMember]
        public bool PreventNavigation
        {
            get { return preventNavigation; }
            set { preventNavigation = value; OnPropertyChanged(); }
        }

        [DataMember]
        public Photo SelectedFlickrPhoto
        {
            get
            {
                return selectedFlickrPhoto;
            }
            set
            {
                if (selectedFlickrPhoto == value) return;
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
                if (DesignMode.DesignModeEnabled) return "D1.1.26";
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
                if (isBandConnected == value) return;
                if (value) DisconnectedMode = false;
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
                if (Math.Abs(displayScaleFactor - value) < 0.1) return;
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

        public bool DisconnectedMode
        {
            get { return disconnectedMode; }
            set { disconnectedMode = value; OnPropertyChanged(); }
        }

        #endregion

        #region Properties - Band specific

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
                if (value != null) PreferredBandName = value.Name;
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
        public int BandModel
        {
            get
            {
                object val;
                if (localSettings != null && localSettings.Values.TryGetValue(GeneralConstants.BandModelKey, out val))
                {
                    bandModel = (int) val;
                }

                return bandModel;
            }
            set
            {
                if (bandModel == value) return;
                if (localSettings != null) localSettings.Values[GeneralConstants.BandModelKey] = value;
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

        #region Properties persisted in Local Settings

        [DataMember]
        public bool? ShowTimeOverlay
        {
            get
            {
                object val;
                if (localSettings != null && localSettings.Values.TryGetValue("ShowTimeOverlay", out val))
                {
                    showTimeOverlay = (bool?) val;
                }

                return showTimeOverlay;
            }
            set
            {
                showTimeOverlay = value;
                if (localSettings != null) localSettings.Values["ShowTimeOverlay"] = showTimeOverlay;
                OnPropertyChanged();
            }
        }

        [DataMember]
        public string PreferredBandName
        {
            get
            {
                object val;
                if (localSettings != null && localSettings.Values.TryGetValue(GeneralConstants.PreferredBandNameKey, out val))
                {
                    preferredBandName = (string) val;
                }

                return preferredBandName;
            }
            private set
            {
                preferredBandName = value;
                if (localSettings != null) localSettings.Values[GeneralConstants.PreferredBandNameKey] = preferredBandName;
                OnPropertyChanged();
            }
        }

        [DataMember]
        public bool? AutoFavsEnabled
        {
            get
            {
                object val;
                if (localSettings != null && localSettings.Values.TryGetValue(GeneralConstants.BackgroundRotatorEnabledKey, out val))
                {
                    autoFavsEnabled = (bool?) val;
                }

                return autoFavsEnabled;
            }
            set
            {
                autoFavsEnabled = value;
                if (localSettings != null) localSettings.Values[GeneralConstants.BackgroundRotatorEnabledKey] = autoFavsEnabled;
                OnPropertyChanged();
            }
        }

        [DataMember]
        public bool? AutoFavNotificationMute
        {
            get
            {
                object val;
                if (localSettings != null && localSettings.Values.TryGetValue(GeneralConstants.BackgroundRotatorNotificationMuteKey, out val))
                {
                    autoFavNotificationMute = (bool?) val;
                }

                return autoFavNotificationMute;
            }
            set
            {
                autoFavNotificationMute = value;
                if (localSettings != null) localSettings.Values[GeneralConstants.BackgroundRotatorNotificationMuteKey] = autoFavNotificationMute;
                OnPropertyChanged();
            }
        }

        [DataMember]
        public bool? ConnectOnLaunch
        {
            get
            {
                object val;
                if (localSettings != null && localSettings.Values.TryGetValue("ConnectOnLaunch", out val))
                {
                    connectOnLaunch = (bool?) val;
                }

                return connectOnLaunch;
            }
            set
            {
                if (connectOnLaunch == value) return;
                if (localSettings != null) localSettings.Values["ConnectOnLaunch"] = value;
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

                if (localSettings != null && localSettings.Values.TryGetValue("LoadCurrentBandImage", out val))
                {
                    loadCurrentBandImage = (bool?) val;
                }

                return loadCurrentBandImage;
            }
            set
            {
                if (loadCurrentBandImage == value) return;
                loadCurrentBandImage = value;
                if (localSettings != null) localSettings.Values["LoadCurrentBandImage"] = value;
                OnPropertyChanged();
            }
        }

        [DataMember]
        public bool HasBeenRated
        {
            get
            {
                object val;
                if (localSettings != null && localSettings.Values.TryGetValue("HasBeenRated", out val))
                {
                    hasBeenRated = (bool) val;
                }
                return hasBeenRated;
            }
            set
            {
                if (value == hasBeenRated) return;
                hasBeenRated = value;
                if (localSettings != null) localSettings.Values["HasBeenRated"] = value;
                OnPropertyChanged();
            }
        }

        [DataMember]
        public int ApplicationRuns
        {
            get
            {
                object val;
                if (localSettings != null && localSettings.Values.TryGetValue("ApplicationRuns", out val))
                {
                    applicationRuns = (int) val;

                }
                return applicationRuns;
            }
            set
            {
                if (applicationRuns == value) return;
                applicationRuns = value;
                if (localSettings != null) localSettings.Values["ApplicationRuns"] = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region In App Purchases

        [DataMember]
        public bool IapBackgroundTasks
        {
            get
            {
#if DEBUG
                return true;
#endif
                object val;
                if (localSettings != null && localSettings.Values.TryGetValue("IapBackgroundTasks", out val))
                {
                    iapBackgroundTasks = (bool) val;
                }
                return iapBackgroundTasks;
            }
            set
            {
                if (value == iapBackgroundTasks) return;
                iapBackgroundTasks = value;
                if (localSettings != null) localSettings.Values["IapBackgroundTasks"] = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Commands

        public ICommand GetCurrentMeTileCommand => getCurrentMeTileCommand ?? (getCurrentMeTileCommand = new DelegateCommand(async () =>
        {
            this.CurrentBandImage = await GetMeTileAsync();
        }));

        public ICommand AddToFavoritesCommand => addToFavoritesCommand ?? (addToFavoritesCommand = new AddToFavoritesCommand());

        public ICommand RemoveFromFavoritesCommand => removeFromFavoritesCommand ?? (removeFromFavoritesCommand = new RemoveFavoriteCommand());

        public ICommand SendToBandCommand => sendToBandCommand ?? (sendToBandCommand = new SendToBandCommand());

        public ICommand EditImageCommand => editImageCommand ?? (editImageCommand = new EditImageCommand());

        public ICommand EditPaletteCommand => editPaletteCommand ?? (editPaletteCommand = new EditPaletteCommand());

        #endregion

        #region Methods

        private async void LoadData()
        {
            if (DesignMode.DesignModeEnabled) return;

            ApplicationRuns++;

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

            // always do this at launch, then update after Band connection
            UpdateAspectRatio();
        }

        public void UpdateAspectRatio()
        {
            Debug.WriteLine("************** UpdateAspectRatio **************");

            WindowBounds = new Size((int) Window.Current.Bounds.Width, (int) Window.Current.Bounds.Height);
            Debug.WriteLine($"WindowBounds set: {WindowBounds.Width}w x {WindowBounds.Height}h");

            TileAspectRatio = BandModel == 1 ? 0.329 : 0.412; //0.303 (if widht is 320);
            Debug.WriteLine($"TileAspectRatio set: {TileAspectRatio}");

            //if we're on a PC we dont want a huge item sizes, so check width and set the Size for list items accordingly
            ListItemSize = WindowBounds.Width < 500
                ? new Size { Width = WindowBounds.Width, Height = Math.Floor(WindowBounds.Width * TileAspectRatio) }
                : new Size { Width = 500, Height = Math.Floor(500 * TileAspectRatio) };

            Debug.WriteLine($"ListItemSize set: {ListItemSize.Width}w x {ListItemSize.Height}h");
            Debug.WriteLine("**************************************************");
        }

        #endregion

        #region  Data Saving/Loading Tasks

        public async Task<bool> SaveFavoritesJsonAsync()
        {
            if (DesignMode.DesignModeEnabled) return true;

            try
            {
                //Using datacontract JSON serializer
                var file = await localFolder.CreateFileAsync(FlickrConstants.FlickrFavoritesFileName, CreationCollisionOption.ReplaceExisting);

                var settings = new DataContractJsonSerializerSettings
                {
                    KnownTypes = new List<Type>() { typeof(FlickrFav), typeof(ObservableCollection<FlickrFav>) }
                };

                using (var stream = await file.OpenStreamForWriteAsync())
                {
                    var serializer = new DataContractJsonSerializer(typeof(ObservableCollection<FlickrFav>), settings);
                    serializer.WriteObject(stream, FlickrFavs);

                    Debug.WriteLine("---------{0} Favorites Saved---------", FlickrFavs.Count);
                }

                return true;
            }
            catch (FileNotFoundException)
            {
                Debug.WriteLine("Could not create FavoritesJson.txt, saving favorites failed");
                return false;
            }
            catch (Exception ex)
            {
                HockeyClient.Current.TrackException(ex);
                Debug.WriteLine("Favorites JSON Save Exception: {0}", ex.Message);
                await new MessageDialog("There was a problem saving your favorites file").ShowAsync();
                return false;
            }
        }

        private async Task<ObservableCollection<FlickrFav>> LoadFavortiesJsonAsync()
        {
            if (DesignMode.DesignModeEnabled) return null;
            var list = new ObservableCollection<FlickrFav>();

            try
            {
                IsBusy = true;
                IsBusyMessage = "loading favorites...";

                var storageItem = await localFolder.TryGetItemAsync(FlickrConstants.FlickrFavoritesFileName);
                if (storageItem == null)
                {
                    Debug.WriteLine("No favorties json file present");
                    return list; //return empty list
                }

                var file = storageItem as StorageFile;

                //Using datacontractserializer
                var settings = new DataContractJsonSerializerSettings
                {
                    KnownTypes = new List<Type> { typeof(FlickrFav), typeof(ObservableCollection<FlickrFav>) }
                };

                using (var stream = await file.OpenStreamForReadAsync())
                {
                    var serializer = new DataContractJsonSerializer(typeof(ObservableCollection<FlickrFav>), settings);

                    var favs = serializer.ReadObject(stream) as ObservableCollection<FlickrFav>;

                    if (favs == null)
                    {
                        areFavoritesLoaded = false;
                        Debug.WriteLine($"Could not deserialize FavoritesJson.txt, returning empty list");
                        return list;
                    }

                    foreach (var photo in favs)
                    {
                        list.Add(photo);
                    }

                    Debug.WriteLine("--- {0} favorites loaded ---", list.Count);

                    areFavoritesLoaded = true;
                    return list;
                }
            }
            catch (Exception ex)
            {
                HockeyClient.Current.TrackException(ex);
                Debug.WriteLine("Favorites Json Load Exception: {0}", ex.Message);
                await new MessageDialog("There was a problem loading your favorites file, memory may be corrupt or missing. ").ShowAsync();
            }
            finally
            {
                IsBusy = false;
                IsBusyMessage = "";
            }

            return list;
        }

        public async Task<bool> SaveThemeHistoryAsync()
        {
            if (DesignMode.DesignModeEnabled) return true;

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
                var file = await localFolder.CreateFileAsync(FlickrConstants.ThemeHistoryFileName, CreationCollisionOption.ReplaceExisting);

                var settings = new DataContractJsonSerializerSettings
                {
                    KnownTypes = new List<Type>() { typeof(List<Color>), typeof(List<List<Color>>) }
                };

                using (var stream = await file.OpenStreamForWriteAsync())
                {
                    var serializer = new DataContractJsonSerializer(typeof(List<List<Color>>), settings);
                    serializer.WriteObject(stream, themeList);

                    Debug.WriteLine("---------{0} ThemeHistory Saved---------", themeList.Count);
                }

                return true;
            }
            catch (Exception ex)
            {
                HockeyClient.Current.TrackException(ex);
                Debug.WriteLine($"ThemeHistory JSON Save Exception: {ex.Message}");
                await new MessageDialog("There was a problem saving your ThemeHistory file").ShowAsync();
            }

            return false;
        }

        private async Task<ObservableCollection<BandColorTheme>> LoadThemeHistoryAsync()
        {
            if (DesignMode.DesignModeEnabled) return null;
            var list = new ObservableCollection<BandColorTheme>();

            try
            {
                var storageItem = await localFolder.TryGetItemAsync(FlickrConstants.ThemeHistoryFileName);
                if (storageItem == null)
                {
                    Debug.WriteLine($"No ThemeHistory json file present, returning empty list");
                    return list;
                }

                //Using datacontractserializer
                var file = storageItem as StorageFile;

                var settings = new DataContractJsonSerializerSettings
                {
                    KnownTypes = new List<Type>() { typeof(List<Color>), typeof(List<List<Color>>) }
                };

                using (var stream = await file.OpenStreamForReadAsync())
                {
                    var serializer = new DataContractJsonSerializer(typeof(List<List<Color>>), settings);

                    var themes = serializer.ReadObject(stream) as List<List<Color>>;

                    if (themes == null)
                    {
                        areThemeHistoryItemsLoaded = false;
                        Debug.WriteLine($"Could not deserialize ThemeHistoryJson.txt, returning empty list");
                        return list;
                    }

                    foreach (var bandTheme in themes)
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
            catch (Exception ex)
            {
                HockeyClient.Current.TrackException(ex);
                Debug.WriteLine("Favorites ThemeHistory Load Exception: {0}", ex.Message);
                await new MessageDialog("There was a problem loading your ThemeHistory file").ShowAsync();
                return list;
            }
        }

        public async Task ClearThemeHistoryAsync()
        {
            ThemeHistory.Clear();
            await SaveThemeHistoryAsync();
        }

        public async Task<bool> BackupFavoritesToRoamingFolderAsync()
        {
            if (DesignMode.DesignModeEnabled) return true;

            try
            {
                IsBusy = true;
                IsBusyMessage = "backing up favorites...";

                HockeyClient.Current.TrackEvent("FavoritesBackedUp");

                var file = await roamingFolder.CreateFileAsync(FlickrConstants.FlickrFavoritesBackupFileName, CreationCollisionOption.ReplaceExisting);

                var settings = new DataContractJsonSerializerSettings
                {
                    KnownTypes = new List<Type>() { typeof(FlickrFav), typeof(ObservableCollection<FlickrFav>) }
                };

                //need to make a copy of th elist so I can clear the LocalImageFilePath (this should also make the data smaller)
                var backupList = new ObservableCollection<FlickrFav>();

                foreach (var flickrFav in FlickrFavs)
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

                    Debug.WriteLine("---------{0} ROAMING Favorites Saved---------", FlickrFavs.Count);
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
                HockeyClient.Current.TrackException(ex);
                Debug.WriteLine("ROAMING Favorites JSON Save Exception: {0}", ex.Message);
                await new MessageDialog("There was a problem saving your synced backup favorites file").ShowAsync();
                return false;
            }
            finally
            {
                IsBusy = false;
                IsBusyMessage = "";
            }
        }

        public async Task<ObservableCollection<FlickrFav>> LoadFavoritesFromRoamingFolderAsync()
        {
            if (DesignMode.DesignModeEnabled) return null;

            try
            {
                IsBusy = true;
                IsBusyMessage = "loading favorites from backup...";

                var storageItem = await roamingFolder.TryGetItemAsync(FlickrConstants.FlickrFavoritesBackupFileName);
                if (storageItem == null)
                {
                    Debug.WriteLine("No ROAMING favorties json file present");
                    await
                        new MessageDialog(
                            "You do not have currently have a synced file. This sometimes can take a few moments to sync if you've just backed up on another device. Try again in a little while.")
                            .ShowAsync();
                    return null;
                }

                var file = storageItem as StorageFile;

                //Using datacontractserializer
                var settings = new DataContractJsonSerializerSettings
                {
                    KnownTypes = new List<Type> { typeof(FlickrFav), typeof(ObservableCollection<FlickrFav>) }
                };

                using (var stream = await file.OpenStreamForReadAsync())
                {
                    var serializer = new DataContractJsonSerializer(typeof(ObservableCollection<FlickrFav>), settings);

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
            catch (Exception ex)
            {
                HockeyClient.Current.TrackException(ex);
                Debug.WriteLine("ROAMING Favorites Json Load Exception: {0}", ex.Message);
                await
                    new MessageDialog(
                        "There was a problem loading your synced favorites file, memory may be corrupt or missing. You could also have a bad connection. \r\n\nTry again later")
                        .ShowAsync();
                return null;
            }
            finally
            {
                IsBusy = false;
                IsBusyMessage = "";
            }
        }

        public async Task<bool> DeleteFavAsync(object favOrPhoto)
        {
            try
            {
                App.ViewModel.IsBusy = true;
                App.ViewModel.IsBusyMessage = "removing fav...";

                if (favOrPhoto == null)
                    return false;

                FlickrFav fav = null;

                //Validate the FlickrFav to delete
                if (favOrPhoto is FlickrFav)
                {
                    fav = (FlickrFav) favOrPhoto;
                }
                else if (favOrPhoto is Photo)
                {
                    var photo = (Photo) favOrPhoto;
                    fav = App.ViewModel.FlickrFavs.FirstOrDefault(x => x.Photo.PhotoId == photo.PhotoId);
                }

                if (fav == null)
                    return false;

                if (App.ViewModel.FlickrFavs == null)
                    return false;

                //Validate the list contains the FlickrFav
                if (!App.ViewModel.FlickrFavs.Contains(fav))
                    return false;

                //REMOVE FROM COLLECTION
                App.ViewModel.FlickrFavs.Remove(fav);

                //UPDATE SAVED FAVS LIST
                await App.ViewModel.SaveFavoritesJsonAsync();

                //CHECK AND REMOVE PHOTO FILE
                App.ViewModel.IsBusyMessage = "deleting photo file...";

                var imageFile = await ApplicationData.Current.LocalFolder.TryGetItemAsync($"{fav.Photo.PhotoId}.jpg");
                if (imageFile != null)
                    await imageFile.DeleteAsync(StorageDeleteOption.PermanentDelete);

                //CHECK AND REMOVE THE THEMES FILE
                App.ViewModel.IsBusyMessage = "deleting theme file...";

                var themesFile = await ApplicationData.Current.LocalFolder.TryGetItemAsync($"{fav.Photo.PhotoId}_GeneratedPalettes.json");
                if (themesFile != null)
                    await themesFile.DeleteAsync(StorageDeleteOption.PermanentDelete);

                return true;
            }
            catch (Exception ex)
            {
                HockeyClient.Current.TrackException(ex);
                return false;
            }
            finally
            {
                App.ViewModel.IsBusy = false;
                App.ViewModel.IsBusyMessage = "";
            }
        }

        #endregion

        #region Band Connection Tasks

        //one stop shop to get the CurrendBandInfo ONLY USE THIS to get the user's band (doesnt need disposal)
        public async Task<IBandInfo> GetCurrentBandInfoAsync()
        {
            //if the Bands collection needs refresing, refresh it
            if (this.Bands == null || !this.Bands.Any())
                Bands = await BandConnectionHelper.FindPairedBandsAsync();

            if (CurrentBand == null)
                CurrentBand = await BandConnectionHelper.RefreshCurrentBandInfo(PreferredBandName, Bands.ToArray());

            return CurrentBand;
        }

        public async Task<bool> RefreshBandInfoAsync()
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
                HockeyClient.Current.TrackException(ex);
                await new MessageDialog("Something went wrong resuming Band connection, restarting the app will fix this. \r\n\nError Details: " + ex.Message).ShowAsync();
                return IsBandConnected = false;
            }
            finally
            {
                IsBusy = false;
                IsBusyMessage = "";
            }
        }

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
                    //-------------Get hardware Specs---------------//
                    IsBusyMessage = "getting specs...";

                    HardwareVersion = await activeBandClient.GetHardwareVersionAsync();
                    Debug.WriteLine($"Hardware Version {HardwareVersion}");

                    int mainVersion;

                    if (int.TryParse(HardwareVersion, out mainVersion))
                        BandModel = mainVersion < 20 ? 1 : 2;

                    Debug.WriteLine($"Band Model {BandModel}");

                    //always update dimensions after getting specs
                    UpdateAspectRatio();

                    //-------------Get software Specs---------------//

                    FirmwareVersion = await activeBandClient.GetFirmwareVersionAsync();
                    Debug.WriteLine($"Firmware Version {FirmwareVersion}");

                    //-------------Get Theme---------------//
                    IsBusyMessage = "getting theme...";

                    var theme = await activeBandClient.PersonalizationManager.GetThemeAsync();

                    var colorTheme = new BandColorTheme(
                        theme.Base.ToColor(),
                        theme.Highlight.ToColor(),
                        theme.Lowlight.ToColor(),
                        theme.SecondaryText.ToColor(),
                        theme.HighContrast.ToColor(),
                        theme.Muted.ToColor());

                    CurrentBandTheme = colorTheme;

                    //-------------Get MeTile---------------//

                    if (LoadCurrentBandImage == true) //only get of the user wants to
                    {

                        IsBusyMessage = "getting current background...";

                        var bandImage = await activeBandClient.PersonalizationManager.GetMeTileImageAsync();
                        var wb = bandImage.ToWriteableBitmap();
                        wb.Invalidate();
                        Debug.WriteLine($"InitializeBandInformationAsync - BandImage WB: PixelWidth {wb.PixelWidth} x PixelHeight {wb.PixelHeight}");
                        CurrentBandImage = wb;
                    }

                    IsBandConnected = true;
                    IsBusyMessage = "connected...";

                    return true;
                }
            }
            catch (BandException ex)
            {
                HockeyClient.Current.TrackException(ex);
                await BandConnectionHelper.BandExceptionMessageDialog(ex, "InitializeBandInformationAsync").ShowAsync();
                //var message = $"This error can occur for several different reasons, check to make sure your Band:" +
                //              "\r\n\n-Is powered on" +
                //              "\r\n\n-Is within range" +
                //              "\r\n-Is paired " +
                //              "\r\n-You are not currently in an activity like Sleep tracking or Workout " +
                //              "\r\n\nNOTE: This can also happen if you have an old Band still paired to your device. Go to your Bluetooth settings and remove any old Bands.";

                //await new MessageDialog(message, $" InitializeBandInformationAsync Error: {ex.Message}").ShowAsync();

            }
            catch (Exception ex)
            {
                HockeyClient.Current.TrackException(ex);
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

        #endregion

        #region Band Content and Sensor Tasks

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
                await BandConnectionHelper.BandExceptionMessageDialog(ex, "LoadHardwareSpecsAsync").ShowAsync();
                //var message = $"This error can occur for several different reasons, check to make sure your Band:" +
                //              "\r\n\n-Is powered on" +
                //              "\r\n\n-Is within range" +
                //              "\r\n-Is paired " +
                //              "\r\n-You are not currently in an activity like Sleep tracking or Workout " +
                //              "\r\n\nNOTE: This can also happen if you have an old Band still paired to your device. Go to your Bluetooth settings and remove any old Bands.";

                //await new MessageDialog(message, $"LoadHardwareSpecsAsync Error: {ex.Message}").ShowAsync();

            }
            catch (Exception ex)
            {
                HockeyClient.Current.TrackException(ex);
                await new MessageDialog("Error getting Band specifications: " + ex.Message).ShowAsync();
            }
            finally
            {
                IsBusyMessage = "";
                IsBusy = false;
            }
        }

        public async Task SetMeTileAsync(WriteableBitmap wb)
        {
            if (DesignMode.DesignModeEnabled) return;

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
                HockeyClient.Current.TrackException(ex);

                await BandConnectionHelper.BandExceptionMessageDialog(ex, "SetMeTileAsync").ShowAsync();

                //var message = $"This error can occur for several different reasons, check to make sure your Band:" +
                //              "\r\n\n-Is powered on" +
                //              "\r\n\n-Is within range" +
                //              "\r\n-Is paired " +
                //              "\r\n-You are not currently in an activity like Sleep tracking or Workout " +
                //              "\r\n\nNOTE: This can also happen if you have an old Band still paired to your device. Go to your Bluetooth settings and remove any old Bands.";

                //await new MessageDialog(message, $"SetMeTileAsync Error: {ex.Message}").ShowAsync();
            }
            catch (Exception ex)
            {
                HockeyClient.Current.TrackException(ex);
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
                HockeyClient.Current.TrackException(ex);
                await BandConnectionHelper.BandExceptionMessageDialog(ex, "GetMeTileAsync").ShowAsync();
                //await new MessageDialog($"GetMeTileAsync() Error connecting to Band, make sure it is:" +
                //                        $"\r\n-Powered ON" +
                //                        $"\r\n-Is within range" +
                //                        $"\r\n-You are not currently in an activity like Sleep tracking or Workout " +
                //                        $"\r\n-Is paired " +
                //                        $"\r\n\nNOTE: This can happen if you no longer have the paired Band. Go to your Bluetooth settings and delete any old Bands." +
                //                        $"\r\n\nError: {ex.Message}").ShowAsync();
                return null;
            }
            catch (Exception ex)
            {
                HockeyClient.Current.TrackException(ex);
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
                HockeyClient.Current.TrackException(ex);
                await BandConnectionHelper.BandExceptionMessageDialog(ex, "GetBandThemeAsync").ShowAsync();
                //await new MessageDialog($"GetBandThemeAsync() Error connecting to Band, make sure it is:" +
                //                        $"\r\n-Powered ON" +
                //                        $"\r\n-Is within range" +
                //                        $"\r\n-You are not currently in an activity like Sleep tracking or Workout " +
                //                        $"\r\n-Is paired " +
                //                        $"\r\n\nNOTE: This can happen if you no longer have the paired Band. Go to your Bluetooth settings and delete any old Bands." +
                //                        $"\r\n\nError: {ex.Message}").ShowAsync();
                return false;
            }
            catch (Exception ex)
            {
                HockeyClient.Current.TrackException(ex);
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
                HockeyClient.Current.TrackException(ex);
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
                HockeyClient.Current.TrackException(ex);
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
