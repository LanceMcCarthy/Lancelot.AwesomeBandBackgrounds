using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using BandCentral.Common;
using BandCentral.ViewModels;
using BandCentral.WindowsBase.Common;
using Lumia.Imaging;
using Lumia.Imaging.Transforms;
using Telerik.Core;
using Telerik.UI.Xaml.Controls.Data;
using Size = Windows.Foundation.Size;
using Photo = FlickrNet.Photo;

namespace BandCentral
{
    public sealed partial class TestingPage : Page
    {
        private readonly NavigationHelper navigationHelper;
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        public static readonly DependencyProperty ListItemSizeProperty = DependencyProperty.Register(
            "ListItemSize", typeof(Size), typeof(TestingPage), new PropertyMetadata(default(double)));

        public Size ListItemSize
        {
            get { return (Size)GetValue(ListItemSizeProperty); }
            private set { SetValue(ListItemSizeProperty, value); }
        }

        public static readonly DependencyProperty FlickrViewModelProperty = DependencyProperty.Register(
            "FlickrViewModel", typeof(FlickrViewModel), typeof(TestingPage), new PropertyMetadata(new FlickrViewModel()));

        public FlickrViewModel FlickrViewModel
        {
            get { return (FlickrViewModel)GetValue(FlickrViewModelProperty); }
            set { SetValue(FlickrViewModelProperty, value); }
        }

        public static readonly DependencyProperty IsPreviewGridVisibleProperty = DependencyProperty.Register(
            "IsPreviewGridVisible", typeof(bool), typeof(TestingPage), new PropertyMetadata(default(bool)));

        public bool IsPreviewGridVisible
        {
            get { return (bool)GetValue(IsPreviewGridVisibleProperty); }
            set { SetValue(IsPreviewGridVisibleProperty, value); }
        }

        //readonly List<string> autoCompleteList = new List<string>
        //    {
        //        "Eyes","Lips","Nails","Hair","Wallpaper","Backgrounds","Technology","Nature",
        //        "Art","Animals","Cars","Women","Men","Landscape",
        //        "Panorama","Trees","Water","Space","Planets","Forest",
        //        "Sunrise","Sunset","Geeky","Sci-Fi","TV", "Skylines", "Windows"
        //    };

        public TestingPage()
        {
            this.InitializeComponent();
            DataContext = this.FlickrViewModel;
            ListItemSize = new Size((App.ViewModel.ListItemSize.Width / 2) - 4, App.ViewModel.ListItemSize.Height);
            
            var animation = new RadMoveAndFadeAnimation();
            animation.FadeAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(1000));
            animation.FadeAnimation.StartOpacity = 0;
            animation.FadeAnimation.EndOpacity = 1;
            animation.MoveAnimation.StartPoint = new Point(0, 30);
            animation.MoveAnimation.EndPoint = new Point(0, 0);
            animation.MoveAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(1000));
            this.DynamicLayoutListBox.ItemAddedAnimation = animation;

            DisplayInformation.AutoRotationPreferences = DisplayOrientations.Portrait;
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
            
        }

        #region List visibility and item management code

        private async void TryLoadMoreData(object sender, RoutedEventArgs e)
        {
            await FlickrViewModel.Source.LoadMoreItemsAsync(20);
        }

        private async Task BringPhotoIntoViewAsync(Photo photo)
        {
            if (photo == null) return;

            await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                this.DynamicLayoutListBox.BringIntoView(photo);
            });
        }

        #endregion

        #region events

        private void DualListEvent_ItemTap(object sender, ListBoxItemTapEventArgs e)
        {
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

            //hide the TimeGrid overlay while the image is loading and the BusyIndiocator is visible
            TimeGrid.Visibility = Visibility.Collapsed;
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
            var photo = this.FlickrViewModel.SelectedPhoto;
            if (photo == null) return;

            if (App.ViewModel.FlickrFavs == null) return;

            FavsButton.IsEnabled = false;

            var fav = new FlickrFav
            {
                DateFavorited = DateTime.UtcNow,
                Photo = photo
            };

            if (App.ViewModel.FlickrFavs.Contains(fav))
            {
                await new MessageDialog("This item is already in your favorties").ShowAsync();
                return;
            }

            App.ViewModel.FlickrFavs.Add(fav);

            await App.ViewModel.SaveFavoritesJsonAsync();

            var isPresent = App.ViewModel.FlickrFavs.FirstOrDefault(x => x.Photo.PhotoId == FlickrViewModel.SelectedPhoto.PhotoId);
            FavsButton.Icon = isPresent == null ? new SymbolIcon(Symbol.Favorite) : new SymbolIcon(Symbol.UnFavorite);

            FavsButton.IsEnabled = true;
        }

        private async void SendToBandButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (FlickrViewModel.SelectedPhoto == null) return;

                //IMPORTANT - need to set the busy using the FlickViewModel 
                FlickrViewModel.IsBusy = true;
                FlickrViewModel.IsBusyMessage = "getting photo details...";

                var url = FlickrHelpers.GetPhotoUrl(FlickrViewModel.SelectedPhoto);

                if (string.IsNullOrEmpty(url))
                {
                    await new MessageDialog("This image has an invalid link and cannot be used, try a different photo").ShowAsync();
                    return;
                }
                
                //busy message updated in the method
                var croppedImage = await DownloadAndCropAsync(url);
                
                FlickrViewModel.IsBusyMessage = "sending image to your Band...";
                await App.ViewModel.SetMeTileAsync(croppedImage);
                
            }
            catch (Exception ex)
            {
                await new MessageDialog($"There was a problem setting your Band's background. Try again.\r\n\nError: {ex.Message}").ShowAsync();
            }
            finally
            {
                FlickrViewModel.IsBusy = false;
                FlickrViewModel.IsBusyMessage = "";
            }
        }

        private void EditButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (FlickrViewModel.SelectedPhoto == null) return;

            App.ViewModel.SelectedFlickrPhoto = FlickrViewModel.SelectedPhoto;

            Frame.Navigate(typeof(PhotoDetailsPage));
        }

        private void SearchButton_OnClick(object sender, RoutedEventArgs e)
        {
            DynamicLayoutListBox.ItemsSource = null;
            FlickrViewModel.ResetSearch();
            DynamicLayoutListBox.ItemsSource = FlickrViewModel.Source;

            MainPivot.SelectedIndex = 0;
        }

        private void MainPivot_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MainPivot.SelectedIndex != 1 || !IsPreviewGridVisible) return;
            HidePreviewStoryboard.Begin();
            IsPreviewGridVisible = false;
        }

        private void PreviewImage_ImageLoaded(object sender, RoutedEventArgs e)
        {
            TimeGrid.Visibility = Visibility.Visible;
        }

        #endregion

        #region image processing

        private async Task<WriteableBitmap> DownloadAndCropAsync(string url)
        {
            try
            {
                FlickrViewModel.IsBusyMessage = "downloading image...";

                //http get for image stream
                var handler = new HttpClientHandler();
                if (handler.SupportsAutomaticDecompression)
                    handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

                var client = new HttpClient(handler);

                using (var stream = await client.GetStreamAsync(url))
                using (var memStream = new MemoryStream())
                {
                    await stream.CopyToAsync(memStream);
                    memStream.Position = 0;

                    FlickrViewModel.IsBusyMessage = "cropping photo...";
                    
                    var outputWriteableBitmap = new WriteableBitmap((int) App.ViewModel.MeTileSize.Width, (int) App.ViewModel.MeTileSize.Height);

                    using (var source = new RandomAccessStreamImageSource(memStream.AsRandomAccessStream(), Lumia.Imaging.ImageFormat.Jpeg))
                    {
                        using (var filters = new FilterEffect(source))
                        {
                            filters.Filters = new IFilter[] { new CropFilter(new Rect(0, 0, (int) App.ViewModel.MeTileSize.Width, (int) App.ViewModel.MeTileSize.Height)) };

                            using (var renderer = new WriteableBitmapRenderer(filters, outputWriteableBitmap))
                            {
                                await renderer.RenderAsync();
                                return outputWriteableBitmap;
                            }
                        }
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                await new MessageDialog($"There was a problem downloading the image for processing. Make sure you are either:\r\n\n" +
                                        "Connected WiFi \r\n" +
                                        "Connected to cellular data \r\n\n" +
                                        $"Error Message: {ex.Message}", "Internet Error").ShowAsync();
                return null;
            }
            catch (Exception ex)
            {
                await new MessageDialog($"Sorry, there was a problem downloading and cropping the image: {ex.Message}").ShowAsync();
                return null;
            }
        }
        

        #endregion

        #region NavigationHelper registration

        private async void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            //TagAutoCompleteBox.ItemsSource = autoCompleteList;

            await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                await FlickrViewModel.Source.LoadMoreItemsAsync(20);
            });
        }

        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);

            try
            {
                if (e.SourcePageType == (typeof(PhotoDetailsPage)) && FlickrViewModel.ListPositionPhoto != null)
                {
                    if (FlickrViewModel.ListPositionPhoto != null)
                    {
                        await BringPhotoIntoViewAsync(FlickrViewModel.ListPositionPhoto);
                    }
                }
            }
            catch
            {
                // ignored
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);

            if (this.FlickrViewModel == null) return;

            try
            {
                var topContainer = DynamicLayoutListBox.ViewportItems.FirstOrDefault();

                if (topContainer != null)
                    this.FlickrViewModel.ListPositionPhoto = topContainer.DataContext as Photo;
            }
            catch
            {
                // ignored
            }
        }

        #endregion

        
    }
}
