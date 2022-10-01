// Lance McCarthy 2013-2023 MIT
// Free to use, maintain attribution to original
// https://github.com/LanceMcCarthy/Lancelot.AwesomeBandBackgrounds

using BandCentral.Models.Common;
using BandCentral.Models.Extensions;
using BandCentral.Models.Favorites;
using BandCentral.Models.Helpers;
using BandCentral.Uwp.Common;
using BandCentral.Uwp.ViewModels;
using Lumia.Imaging;
using Lumia.Imaging.Transforms;
using Microsoft.HockeyApp;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Telerik.Core;
using Telerik.UI.Xaml.Controls.Data;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Photo = FlickrNet.Photo;
using Size = Windows.Foundation.Size;

namespace BandCentral.Uwp.Views
{
    public sealed partial class PhotoSearchPage : Page
    {
        public static readonly DependencyProperty FlickrViewModelProperty = DependencyProperty.Register(
            nameof(FlickrViewModel), typeof(FlickrViewModel), typeof(PhotoSearchPage), new PropertyMetadata(new FlickrViewModel()));

        public FlickrViewModel FlickrViewModel
        {
            get => (FlickrViewModel)GetValue(FlickrViewModelProperty);
            set => SetValue(FlickrViewModelProperty, value);
        }

        public static readonly DependencyProperty IsPreviewGridVisibleProperty = DependencyProperty.Register(
            nameof(IsPreviewGridVisible), typeof(bool), typeof(PhotoSearchPage), new PropertyMetadata(default(bool)));

        public bool IsPreviewGridVisible
        {
            get => (bool)GetValue(IsPreviewGridVisibleProperty);
            set => SetValue(IsPreviewGridVisibleProperty, value);
        }

        public static readonly DependencyProperty SearchHistoryProperty = DependencyProperty.Register(
            nameof(SearchHistory), typeof(List<string>), typeof(PhotoSearchPage), new PropertyMetadata(default(List<string>)));

        public List<string> SearchHistory
        {
            get => (List<string>) GetValue(SearchHistoryProperty);
            set => SetValue(SearchHistoryProperty, value);
        }

        readonly ApplicationDataContainer localSettings;
        
        public PhotoSearchPage()
        {
            this.InitializeComponent();
            DataContext = this.FlickrViewModel;
            
            if (DesignMode.DesignModeEnabled)
                return;

            HockeyClient.Current.TrackPageView("PhotoSearchPage");

            localSettings = ApplicationData.Current.LocalSettings;

            SearchHistory = new List<string>();
            
            SetItemSize(this.Width);

            var animation = new RadMoveAndFadeAnimation();
            animation.FadeAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(1000));
            animation.FadeAnimation.StartOpacity = 0;
            animation.FadeAnimation.EndOpacity = 1;
            animation.MoveAnimation.StartPoint = new Point(0, 30);
            animation.MoveAnimation.EndPoint = new Point(0, 0);
            animation.MoveAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(1000));
            this.DynamicLayoutListBox.ItemAddedAnimation = animation;
            
            this.NavigationCacheMode = NavigationCacheMode.Enabled;

            Loaded += PhotoSearchPage_Loaded;
        }

        private void CoreWindow_SizeChanged(CoreWindow sender, WindowSizeChangedEventArgs args)
        {
            SetItemSize(args.Size.Width);
        }

        private void SetItemSize(double viewWidth)
        {
            //ListItemSize = new Size((App.ViewModel.ListItemSize.Width / 2) - 4, App.ViewModel.ListItemSize.Height);

            if (viewWidth < 500)
            {
                FlickrViewModel.ListItemSize = new Size((viewWidth / 2) - 4, viewWidth * App.ViewModel.TileAspectRatio);
            }
            else
            {
                //ListItemSize = new Size((viewWidth / 3) - 6, viewWidth * App.ViewModel.TileAspectRatio);
                //FlickrViewModel.ListItemSize = App.ViewModel.MeTileSize;
                FlickrViewModel.ListItemSize = App.ViewModel.ListItemSize;
            }
        }

        #region List visibility and item management code

        private async void TryLoadMoreData(object sender, RoutedEventArgs e)
        {
            await FlickrViewModel.Source.LoadMoreItemsAsync(20);
        }

        private async Task BringPhotoIntoViewAsync(Photo photo)
        {
            if (photo == null) return;

            await Window.Current.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                this.DynamicLayoutListBox.BringIntoView(photo);
            });
        }

        #endregion

        #region event handlers

        private void DualListEvent_ItemTap(object sender, ListBoxItemTapEventArgs e)
        {
            if (DynamicLayoutListBox.SelectedItem == null) return;

            var selectedItem = DynamicLayoutListBox.SelectedItem as FlickrNet.Photo;
            if (selectedItem == null) return;

            var isPresent = App.ViewModel.FlickrFavs.FirstOrDefault(x => x.Photo.PhotoId == selectedItem.PhotoId);
            FavsButton.Icon = isPresent == null ? new SymbolIcon(Symbol.Favorite) : new SymbolIcon(Symbol.UnFavorite);

            if (selectedItem == FlickrViewModel.SelectedPhoto && IsPreviewGridVisible)
            {
                HidePreviewStoryboard.Begin();
                IsPreviewGridVisible = false;
                return;
            }

            if (!IsPreviewGridVisible)
            {
                ShowPreviewStoryboard.Begin();
                IsPreviewGridVisible = true;
            }

            FlickrViewModel.SelectedPhoto = selectedItem;
        }

        private void ShowHidePreviewButton_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            //in case the user pops out the preview grid before making a selection
            if (DynamicLayoutListBox.SelectedItem == null && App.ViewModel.SelectedFlickrPhoto == null)
            {
                var firstItem = FlickrViewModel.Source[0];
                DynamicLayoutListBox.SelectedItem = firstItem;
                App.ViewModel.SelectedFlickrPhoto = firstItem;
            }

            if (IsPreviewGridVisible)
            {
                HidePreviewStoryboard.Begin();
                IsPreviewGridVisible = false;
            }
            else if (!IsPreviewGridVisible)
            {
                ShowPreviewStoryboard.Begin();
                IsPreviewGridVisible = true;
            }
        }

        private async void FavsButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var photo = this.FlickrViewModel.SelectedPhoto;
                if (photo == null) return;

                if (App.ViewModel.FlickrFavs == null) return;

                FavsButton.IsEnabled = false;

                var fav = new FlickrFav
                {
                    DateFavorited = DateTime.UtcNow,
                    Photo = photo
                };

                var isPresent = App.ViewModel.FlickrFavs.FirstOrDefault(x => x.Photo.PhotoId == FlickrViewModel.SelectedPhoto.PhotoId);

                if (isPresent != null)
                {
                    var md = new MessageDialog("This item is already in your favorties, are you sure you want to remove it?", "Remove favorite?");
                    md.Commands.Add(new UICommand("remove", async (a) =>
                    {
                        App.ViewModel.FlickrFavs.Remove(fav);
                        await App.ViewModel.SaveFavoritesJsonAsync();
                        HockeyClient.Current.TrackEvent("FavoriteRemoved");

                        FavsButton.Icon = new SymbolIcon(Symbol.Favorite);
                    }));
                    md.Commands.Add(new UICommand("cancel"));
                    md.CancelCommandIndex = 1;
                    md.DefaultCommandIndex = 0;
                    await md.ShowAsync();
                }
                else
                {
                    //---------new download progress code------------//

                    FlickrViewModel.IsBusy = true;

                    var reporter = new Progress<DownloadProgressArgs>();
                    reporter.ProgressChanged += Reporter_ProgressChanged;

                    using (var stream = await HttpClientExtensions.DownloadStreamWithProgressAsync(fav.Photo.MediumUrl, reporter))
                    {
                        //seekable stream
                        var memStream = new MemoryStream();
                        await stream.CopyToAsync(memStream);
                        memStream.Position = 0;

                        App.ViewModel.IsBusyMessage = "cropping photo...";
                        
                        var tileSize = App.ViewModel.MeTileSize;

                        var outputWriteableBitmap = new WriteableBitmap((int) tileSize.Width, (int) tileSize.Height);

                        using (var source = new RandomAccessStreamImageSource(memStream.AsRandomAccessStream()))
                        using (var effect = new CropEffect(source, new Rect(0, 0, (int) tileSize.Width, (int) tileSize.Height)))
                        using (var renderer = new WriteableBitmapRenderer(effect, outputWriteableBitmap))
                        {
                            await renderer.RenderAsync();
                        }
                        
                        var file = await outputWriteableBitmap.SaveToJpgFileAsync($"{fav.Photo.PhotoId}.jpg", ApplicationData.Current.LocalFolder);
                        
                        fav.LocalImageFilePath = file?.Path;
                        fav.LocalImageFileName = file?.Name;
                    }

                    reporter.ProgressChanged -= Reporter_ProgressChanged;

                    //-----------------------------------------------//

                    App.ViewModel.FlickrFavs.Add(fav);
                    await App.ViewModel.SaveFavoritesJsonAsync();

                    //FavsButton.Icon = isPresent == null ? new SymbolIcon(Symbol.Favorite) : new SymbolIcon(Symbol.UnFavorite);
                    FavsButton.Icon = new SymbolIcon(Symbol.UnFavorite);

                    HockeyClient.Current.TrackEvent("FavoriteAdded");
                }

                FavsButton.IsEnabled = true;
            }
            catch (Exception ex)
            {
                HockeyClient.Current.TrackException(ex);
            }
            finally
            {
                FlickrViewModel.IsBusy = false;
                FlickrViewModel.IsBusyMessage = "";
            }
        }

        private void Reporter_ProgressChanged(object sender, DownloadProgressArgs e)
        {
            FlickrViewModel.IsBusyMessage = $"downloading {e.PercentComplete}%";
        }

        private async void SendToBandButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (FlickrViewModel.SelectedPhoto == null) return;
            var imageFormat = Lumia.Imaging.ImageFormat.Jpeg;

            if (string.IsNullOrEmpty(FlickrViewModel.SelectedPhoto.OriginalFormat))
            {
                imageFormat = FilterEffectsHelper.GetImageFormatType(FlickrViewModel.SelectedPhoto.OriginalFormat);
            }

            var url = FlickrHelpers.GetPhotoUrl(FlickrViewModel.SelectedPhoto);

            if (string.IsNullOrEmpty(url))
            {
                await new MessageDialog("This image has an invalid link and cannot be used, try a different photo").ShowAsync();
                return;
            }

            await SetBandImageAsync(await DownloadAndCropAsync(url, imageFormat));
            
            var metrics = new Dictionary<string, string>();
            metrics.Add("PageUsedOn", "PhotoSearchPage");

            HockeyClient.Current.TrackEvent("SendImageToBand", metrics);
        }

        private void EditButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (FlickrViewModel.SelectedPhoto == null) return;

            App.ViewModel.SelectedFlickrPhoto = FlickrViewModel.SelectedPhoto;

            Frame.Navigate(typeof(PhotoDetailsPage));
        }

        private void PreviewImage_ImageLoaded(object sender, RoutedEventArgs e)
        {
            TimeGrid.Visibility = Visibility.Visible;
        }

        private async void SearchButton_OnClick(object sender, RoutedEventArgs e)
        {
            SearchOptionsButton?.Flyout?.Hide();

            var metrics = new Dictionary<string, string>();
            metrics.Add("SearchTerm", FlickrViewModel.SearchTerm.ToLowerInvariant());
            HockeyClient.Current.TrackEvent("FlickrSearchPerformed", metrics);

            if (IsPreviewGridVisible)
            {
                HidePreviewStoryboard.Begin();
                IsPreviewGridVisible = false;
            }

            DynamicLayoutListBox.ItemsSource = null;
            FlickrViewModel.ResetSearch();
            DynamicLayoutListBox.ItemsSource = FlickrViewModel.Source;

            //if user doesnt want tracking, backout now
            if (TrackHistoryCheckBox.IsChecked == false)
                return;

            if (!SearchHistory.Contains(FlickrViewModel.SearchTerm.ToLowerInvariant()))
            {
                SearchHistory.Add(FlickrViewModel.SearchTerm.ToLowerInvariant());
                SaveSearchHistory();
            }
        }

        private async void ClearSearchHistoryButton_OnClick(object sender, RoutedEventArgs e)
        {
            var md = new MessageDialog("Are you sure you want to clear your previous search terms?", "Delete History?");
            md.Commands.Add(new UICommand("delete"));
            md.Commands.Add(new UICommand("cancel"));
            var result =  await md.ShowAsync();

            if(result.Label == "delete")
                ClearSearchHistory();
        }

        private void UseLastTermCheckBox_OnChecked(object sender, RoutedEventArgs e)
        {
            if (localSettings == null) return;
            localSettings.Values["UseLastSearchTerm"] = true;
        }

        private void UseLastTermCheckBox_OnUnchecked(object sender, RoutedEventArgs e)
        {
            if (localSettings == null) return;
            localSettings.Values["UseLastSearchTerm"] = false;
        }

        private void TrackHistoryCheckBox_OnChecked(object sender, RoutedEventArgs e)
        {
            if (localSettings == null) return;
            localSettings.Values["TrackSearchHistory"] = true;
        }

        private void TrackHistoryCheckBox_OnUnchecked(object sender, RoutedEventArgs e)
        {
            if (localSettings == null) return;
            localSettings.Values["TrackSearchHistory"] = false;
        }

        #endregion

        #region image processing

        private async Task<WriteableBitmap> DownloadAndCropAsync(string url, ImageFormat imageFormat)
        {

            try
            {
                FlickrViewModel.IsBusy = true;
                FlickrViewModel.IsBusyMessage = "downloading image...";

                //http get for image stream
                var handler = new HttpClientHandler();
                if (handler.SupportsAutomaticDecompression)
                    handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

                var client = new HttpClient(handler);
                var stream = await client.GetStreamAsync(url);

                //seekable stream
                var memStream = new MemoryStream();
                await stream.CopyToAsync(memStream);
                memStream.Position = 0;

                FlickrViewModel.IsBusyMessage = "cropping photo...";

                //NOTE - getting size form VM now to support Band 2
                var tileSize = App.ViewModel.MeTileSize;

                var outputWriteableBitmap = new WriteableBitmap((int)tileSize.Width, (int)tileSize.Height);

                using (var source = new RandomAccessStreamImageSource(memStream.AsRandomAccessStream()))
                using (var effect = new CropEffect(source, new Rect(0, 0, (int)tileSize.Width, (int)tileSize.Height)))
                using (var renderer = new WriteableBitmapRenderer(effect, outputWriteableBitmap))
                {
                    await renderer.RenderAsync();
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
                
                var properties = new Dictionary<string, string>();
                properties.Add("PageUsedOn", "PhotoSearchPage");
                HockeyClient.Current.TrackEvent("DownloadAndCropAsync", properties);
            }
        }

        private async Task SetBandImageAsync(WriteableBitmap outputWb)
        {
            if (outputWb == null) return;

            try
            {
                FlickrViewModel.IsBusy = true;
                FlickrViewModel.IsBusyMessage = "connecting to band...";
                
                FlickrViewModel.IsBusyMessage = "connected, sending image...";

                await App.ViewModel.SetMeTileAsync(outputWb);
            }
            catch (Exception ex)
            {
                await new MessageDialog($"Sorry, there was a problem cropping the image: {ex.Message}").ShowAsync();
            }
            finally
            {
                FlickrViewModel.IsBusyMessage = "";
                FlickrViewModel.IsBusy = false;
            }
        }

        #endregion

        #region Navigation

        private async void PhotoSearchPage_Loaded(object sender, RoutedEventArgs e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                await FlickrViewModel.Source.LoadMoreItemsAsync(20);
            });

            //see if user turned off history tracking
            bool trackHistory = true;
            object trackHistorySetting;
            if (localSettings.Values.TryGetValue("TrackSearchHistory", out trackHistorySetting))
                trackHistory = (bool) trackHistorySetting;

            TrackHistoryCheckBox.IsChecked = trackHistory;

            if (trackHistory)
            {
                SearchHistory = await GetSearchHistory();

                //see if user wants to use last term
                CheckLastTermSetting();
            }
        }
        
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            if (DesignMode.DesignModeEnabled)
                return;

            Window.Current.CoreWindow.SizeChanged += CoreWindow_SizeChanged;

            if (e.SourcePageType == typeof(PhotoDetailsPage))
            {
                await LoadListPosition();
            }
        }
        
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            Window.Current.CoreWindow.SizeChanged -= CoreWindow_SizeChanged;

            SaveListPosition();
        }

        private void SaveListPosition()
        {
            try
            {
                var topContainer = DynamicLayoutListBox.ViewportItems.FirstOrDefault();
                if (FlickrViewModel != null && topContainer != null)
                    FlickrViewModel.ListPositionPhoto = topContainer.DataContext as Photo;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SaveListPosition Exception: {ex.Message}");
            }
        }

        private async Task LoadListPosition()
        {
            try
            {
                if (FlickrViewModel?.ListPositionPhoto != null)
                {
                    await BringPhotoIntoViewAsync(FlickrViewModel.ListPositionPhoto);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"LoadListPosition Exception: {ex.Message}");
            }
        }

        #endregion

        #region Search History management

        private void CheckLastTermSetting()
        {
            bool useLastTerm = true;

            object useLastTermSetting;
            if (localSettings != null && localSettings.Values.TryGetValue("UseLastSearchTerm", out useLastTermSetting))
            {
                //if the setting exists AND it's true, go ahead and update the search term before the first search
                useLastTerm = (bool) useLastTermSetting;
                if (useLastTerm)
                {
                    var lastTerm = SearchHistory.LastOrDefault();

                    if (!string.IsNullOrEmpty(lastTerm))
                        FlickrViewModel.SearchTerm = lastTerm;
                }
            }

            UseLastTermCheckBox.IsChecked = useLastTerm;
        }

        private async Task<List<string>> GetSearchHistory()
        {
            return await Task.Run(() =>
            {
                try
                {
                    if (localSettings == null)
                        return new List<string>();

                    object jsonInLocalSettings;
                    if (localSettings.Values.TryGetValue("SearchHistory", out jsonInLocalSettings))
                    {
                        var json = (string)jsonInLocalSettings;

                        var list = JsonConvert.DeserializeObject<List<string>>(json);

                        if (list != null)
                            return list;
                    }

                    return new List<string>();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"GetSearchHistory Exception: {ex}");
                    return new List<string>();
                }
            });
        }

        private bool SaveSearchHistory()
        {
            try
            {
                if (localSettings == null)
                    return false;

                localSettings.Values["SearchHistory"] = JsonConvert.SerializeObject(this.SearchHistory);

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SaveSearchHistory Exception: {ex}");
                return false;
            }
        }

        private bool ClearSearchHistory()
        {
            try
            {
                if (localSettings == null)
                    return false;

                SearchHistory.Clear();

                localSettings.Values["SearchHistory"] = JsonConvert.SerializeObject(SearchHistory);

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ClearSearchHistory Exception: {ex}");
                return false;
            }
        }

        #endregion
    }
}
