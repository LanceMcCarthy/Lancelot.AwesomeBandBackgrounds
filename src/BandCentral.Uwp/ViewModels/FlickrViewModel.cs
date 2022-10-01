using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.ApplicationModel;
using Windows.Networking.Connectivity;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Popups;
using BandCentral.Models.Enums;
using BandCentral.Models.Helpers;
using BandCentral.Uwp.Commands;
using CommonHelpers.Common;
using FlickrNet;
using Photo = FlickrNet.Photo;

namespace BandCentral.Uwp.ViewModels
{
    public class FlickrViewModel : ViewModelBase
    {
        private readonly ApplicationDataContainer localSettings;
        private readonly ApplicationDataContainer roamingSettings;
        private CoreDispatcher dispatcher;
        private const string NoConnectionString = "No Internet Connection. Tap to try again.";
        private const string DataLoadingString = "loading photos...";
        
        private int loadedPagesCount = 0;

        private string status;
        private ListLayoutMode layoutMode;
        private bool isInSelectionMode;

        private string searchTerm = "Nature";
        private bool isBusy;
        private string isBusyMessage;
        private uint itemsPerPage = 20;
        private int photosReceived;
        private int totalPhotos;
        private int currentPage;
        private int totalPages;
        private Photo selectedPhoto;
        private Size scaledSize;
        private Size bandPhotoSize;
        private List<PopularitySort> photoPopularitySortOrdersList;
        private PopularitySort selectedPhotoSearchPopularitySortOrder = PopularitySort.Favorites;
        private List<PhotoSearchExtras> photoSearchExtrasList;
        private PhotoSearchExtras selectedPhotoSearchExtras = PhotoSearchExtras.All;
        private List<SafetyLevel> photoSearchSafetyLevelsList;
        private SafetyLevel selectedSafetyLevel = SafetyLevel.Moderate;
        private List<PhotoSearchSortOrder> photoSearchSortOrdersList;
        private PhotoSearchSortOrder selectedPhotoSearchSortOrder = PhotoSearchSortOrder.Relevance;
        private Photo listPositionPhoto;
        private bool isGettingItems;
        private Telerik.Core.Data.IncrementalLoadingCollection<Photo> source;

        public FlickrViewModel()
        {
            if (!DesignMode.DesignModeEnabled)
            {
                localSettings = ApplicationData.Current.LocalSettings;
                roamingSettings = ApplicationData.Current.RoamingSettings;
                dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;
            }

            Status = DataLoadingString;
            AddToFavoritesCommand = new AddToFavoritesCommand();
            SendToBandCommand = new SendToBandCommand();
            EditImageCommand = new EditImageCommand();
        }

        #region commands

        public ICommand AddToFavoritesCommand { get; set; }

        public ICommand SendToBandCommand { get; set; }

        public ICommand EditImageCommand { get; set; }

        #endregion

        #region fetching and list management

        public string Status
        {
            get
            {
                return status;
            }
            set
            {
                if (status != value)
                {
                    status = value;
                    OnPropertyChanged();
                }
            }
        }

        public Telerik.Core.Data.IncrementalLoadingCollection<Photo> Source
        {
            get { return source??( source = new Telerik.Core.Data.IncrementalLoadingCollection<Photo>(GetItemsAsync) { BatchSize = ItemsPerPage }); }
            set { source = value; OnPropertyChanged();}
        }

        private async Task<IEnumerable<Photo>> GetItemsAsync(uint count)
        {
            Status = DataLoadingString;
            Exception exception = null;
            PhotoCollection photos = null;

            try
            {
                var profile = NetworkInformation.GetInternetConnectionProfile();
                var isConnected = profile != null && NetworkInformation.GetInternetConnectionProfile()?.GetNetworkConnectivityLevel() == NetworkConnectivityLevel.InternetAccess;

                if (!isConnected)
                {
                    await Task.Delay(1000);
                    Status = NoConnectionString;
                    return null;
                }

                await TaskHelpers.CallOnUiThreadAsync(() =>
                {
                    IsGettingItems = true;
                });
                
                photos = await PortableHelpers.GetPhotosAsync(++loadedPagesCount, (int) ItemsPerPage, SelectedSafetyLevel,
                    SearchTerm, SelectedPhotoSearchSortOrder);
                
                PhotosReceived = photos.Count + PhotosReceived;
            }
            catch (Exception ex)
            {
                exception = ex;
            }
            finally
            {
                await TaskHelpers.CallOnUiThreadAsync(() =>
                {
                    IsGettingItems = false;
                });
            }

            if (exception != null)
            {
                await new MessageDialog("There was a problem fetching photos: " + exception.Message).ShowAsync();
            }

            return photos;
        }

        public void ResetSearch()
        {
            Source.Clear();
            loadedPagesCount = 0;
            PhotosReceived = 0;
        }

        #endregion

        #region Search preferences and roaming settings

        public List<PopularitySort> PhotoPopularitySortOrdersList
        {
            get { return photoPopularitySortOrdersList ?? (photoPopularitySortOrdersList = Helpers.GetEnumAsList<PopularitySort>()); }
        }

        public PopularitySort SelectedPhotoSearchPopularitySortOrder
        {
            get
            {
                if (DesignMode.DesignModeEnabled) return PopularitySort.Favorites;

                object val;
                if (roamingSettings.Values.TryGetValue("SelectedPhotoSearchPopularitySortOrder", out val))
                {
                    selectedPhotoSearchPopularitySortOrder = (PopularitySort)(long)val;
                }

                return selectedPhotoSearchPopularitySortOrder;
            }
            set
            {
                if (DesignMode.DesignModeEnabled) return;

                if (selectedPhotoSearchPopularitySortOrder == value) return;
                roamingSettings.Values["SelectedPhotoSearchPopularitySortOrder"] = (long)value;
                selectedPhotoSearchPopularitySortOrder = value;
                OnPropertyChanged();
            }
        }

        public List<SafetyLevel> PhotoSearchSafetyLevelsList
        {
            get { return photoSearchSafetyLevelsList ?? (photoSearchSafetyLevelsList = Helpers.GetEnumAsList<SafetyLevel>()); }
        }

        public SafetyLevel SelectedSafetyLevel
        {
            get
            {
                if (DesignMode.DesignModeEnabled) return SafetyLevel.Moderate;

                object val;
                if (roamingSettings.Values.TryGetValue("SelectedSafetyLevel", out val))
                {
                    selectedSafetyLevel = (SafetyLevel)(int)val;
                }

                return selectedSafetyLevel;
            }
            set
            {
                if (DesignMode.DesignModeEnabled) return;
                if (selectedSafetyLevel == value) return;
                selectedSafetyLevel = value;
                roamingSettings.Values["SelectedSafetyLevel"] = (int)value;
                OnPropertyChanged();
            }
        }

        public List<PhotoSearchSortOrder> PhotoSearchSortOrdersList
        {
            get { return photoSearchSortOrdersList ?? (photoSearchSortOrdersList = Helpers.GetEnumAsList<PhotoSearchSortOrder>()); }
        }

        public PhotoSearchSortOrder SelectedPhotoSearchSortOrder
        {
            get
            {
                if (DesignMode.DesignModeEnabled) return PhotoSearchSortOrder.Relevance;

                object val;
                if (roamingSettings.Values.TryGetValue("SelectedPhotoSearchSortOrder", out val))
                {
                    selectedPhotoSearchSortOrder = (PhotoSearchSortOrder)(int)val;
                }

                return selectedPhotoSearchSortOrder;
            }
            set
            {
                if (DesignMode.DesignModeEnabled) return;

                if (selectedPhotoSearchSortOrder == value) return;
                selectedPhotoSearchSortOrder = value;
                roamingSettings.Values["SelectedPhotoSearchSortOrder"] = (int)value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region public properties

        public Photo ListPositionPhoto
        {
            get { return listPositionPhoto; }
            set { listPositionPhoto = value; OnPropertyChanged(); }
        }

        public string SearchTerm
        {
            get { return searchTerm; }
            set
            {
                if (searchTerm == value) return;
                searchTerm = value; OnPropertyChanged();
            }
        }

        public bool IsBusy
        {
            get { return isBusy; }
            set
            {
                if (isBusy == value) return;
                isBusy = value; OnPropertyChanged();
            }
        }

        public string IsBusyMessage
        {
            get { return isBusyMessage; }
            set
            {
                if (isBusyMessage == value) return;
                isBusyMessage = value; OnPropertyChanged();
            }
        }

        public bool IsGettingItems
        {
            get { return isGettingItems; }
            set { isGettingItems = value; OnPropertyChanged();}
        }

        public uint ItemsPerPage
        {
            get { return itemsPerPage; }
            set
            {
                if (itemsPerPage == value) return;
                itemsPerPage = value; OnPropertyChanged();
            }
        }

        public int PhotosReceived
        {
            get { return photosReceived; }
            set
            {
                if (photosReceived == value) return;
                photosReceived = value; OnPropertyChanged();
            }
        }

        public int TotalPhotos
        {
            get { return totalPhotos; }
            set
            {
                if (totalPhotos == value) return;
                totalPhotos = value; OnPropertyChanged();
            }
        }

        public int CurrentPage
        {
            get { return currentPage; }
            set { currentPage = value; OnPropertyChanged(); }
        }

        public int TotalPages
        {
            get { return totalPages; }
            set
            {
                if (totalPages == value) return;
                totalPages = value; OnPropertyChanged();
            }
        }

        public Photo SelectedPhoto
        {
            get { return selectedPhoto; }
            set
            {
                if (selectedPhoto == value) return;
                selectedPhoto = value; OnPropertyChanged();
            }
        }

        public Size ScaledSize
        {
            get
            {
                return scaledSize;
            }
            set
            {
                if (scaledSize == value) return;
                scaledSize = value;
                OnPropertyChanged();
            }
        }

        public Size BandPhotoSize
        {
            get { return bandPhotoSize; }
            set
            {
                if (bandPhotoSize == value) return;
                bandPhotoSize = value; OnPropertyChanged();
            }
        }

        public Windows.Foundation.Size ListItemSize { get; set; }

        public Windows.Foundation.Size MeTileSize
        {
            get
            {
                switch (App.ViewModel.BandModel)
                {
                    case 1:
                        return new Windows.Foundation.Size(310, 102);
                    case 2:
                        return new Windows.Foundation.Size(310, 128);
                    default:
                        return new Windows.Foundation.Size(310, 128);
                }
            }
        }

        public double ScrollPosition { get; set; }

        #endregion
    }
}
