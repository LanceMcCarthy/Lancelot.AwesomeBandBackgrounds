// Lance McCarthy 2013-2023 MIT
// Free to use, maintain attribution to original
// https://github.com/LanceMcCarthy/Lancelot.AwesomeBandBackgrounds

using BandCentral.Models.Extensions;
using BandCentral.Models.Favorites;
using BandCentral.Models.Helpers;
using BandCentral.Uwp.Common;
using BandCentral.Uwp.ViewModels;
using FlickrNet;
using Lumia.Imaging;
using Lumia.Imaging.Transforms;
using Microsoft.HockeyApp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
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

namespace BandCentral.Uwp.Views
{
    public sealed partial class TempSearchPage : Page
    {
        public static readonly DependencyProperty FlickrViewModelProperty = DependencyProperty.Register(
            nameof(FlickrViewModel), typeof(FlickrViewModel), typeof(TempSearchPage), new PropertyMetadata(new FlickrViewModel()));

        public FlickrViewModel FlickrViewModel
        {
            get => (FlickrViewModel) GetValue(FlickrViewModelProperty);
            set => SetValue(FlickrViewModelProperty, value);
        }

        public static readonly DependencyProperty IsPreviewGridVisibleProperty = DependencyProperty.Register(
            nameof(IsPreviewGridVisible), typeof(bool), typeof(TempSearchPage), new PropertyMetadata(default(bool)));

        public bool IsPreviewGridVisible
        {
            get => (bool) GetValue(IsPreviewGridVisibleProperty);
            set => SetValue(IsPreviewGridVisibleProperty, value);
        }

        private readonly List<string> autoCompleteList = new List<string>
        {
            "Eyes","Lips","Nails","Hair","Wallpaper","Backgrounds","Technology","Nature",
            "Art","Animals","Cars","Women","Men","Landscape",
            "Panorama","Trees","Water","Space","Planets","Forest",
            "Sunrise","Sunset","Geeky","Sci-Fi","TV", "Skylines",
            "Star Wars","Star Trek","Cats","Dogs","Airplane", "Windows",
            "Klingon","Flowers","","","","","",""
        };

        private ApplicationDataContainer localSettings;

        public TempSearchPage()
        {
            this.InitializeComponent();
            DataContext = this.FlickrViewModel;

            HockeyClient.Current.TrackPageView("TempSearchPage");

            if (DesignMode.DesignModeEnabled)
                return;
            
            localSettings = ApplicationData.Current.LocalSettings;
        }

        private async Task BringPhotoIntoViewAsync(Photo photo)
        {
            if (photo == null) return;

            await Window.Current.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                this.ResultsGridView.ScrollIntoView(photo);
            });
        }

        private void PreviewImage_ImageLoaded(object sender, RoutedEventArgs e)
        {
            TimeGrid.Visibility = Visibility.Visible;
        }

        private async void SearchButton_OnClick(object sender, RoutedEventArgs e)
        {
            SearchOptionsButton?.Flyout?.Hide();

            if (IsPreviewGridVisible)
            {
                HidePreviewStoryboard.Begin();
                IsPreviewGridVisible = false;
            }

            ResultsGridView.ItemsSource = null;
            FlickrViewModel.ResetSearch();
            ResultsGridView.ItemsSource = FlickrViewModel.Source;
        }

        #region Event handlers
        
        private void ShowHidePreviewButton_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            //in case the user pops out the preview grid before making a selection
            if (ResultsGridView.SelectedItem == null && App.ViewModel.SelectedFlickrPhoto == null)
            {
                var firstItem = FlickrViewModel.Source[0];
                ResultsGridView.SelectedItem = firstItem;
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

            var isPresent = App.ViewModel.FlickrFavs.FirstOrDefault(x => x.Photo.PhotoId == FlickrViewModel.SelectedPhoto.PhotoId);

            if (isPresent != null)
            {
                var md = new MessageDialog("This item is already in your favorties, are you sure you want to remove it?", "Remove favorite?");
                md.Commands.Add(new UICommand("remove", async (a) =>
                {
                    App.ViewModel.FlickrFavs.Remove(fav);
                    await App.ViewModel.SaveFavoritesJsonAsync();

                    FavsButton.Icon = new SymbolIcon(Symbol.Favorite);
                }));
                md.Commands.Add(new UICommand("cancel"));
                md.CancelCommandIndex = 1;
                md.DefaultCommandIndex = 0;
                await md.ShowAsync();
            }
            else
            {
                App.ViewModel.FlickrFavs.Add(fav);
                await App.ViewModel.SaveFavoritesJsonAsync();

                //FavsButton.Icon = isPresent == null ? new SymbolIcon(Symbol.Favorite) : new SymbolIcon(Symbol.UnFavorite);
                FavsButton.Icon = new SymbolIcon(Symbol.UnFavorite);
                
            }

            FavsButton.IsEnabled = true;
        }

        private async void SendToBandButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (FlickrViewModel.SelectedPhoto == null) return;
            var imageFormat = ImageFormat.Jpeg;

            if (string.IsNullOrEmpty(FlickrViewModel.SelectedPhoto.OriginalFormat))
            {
                imageFormat = FilterEffectsHelper.GetImageFormatType(FlickrViewModel.SelectedPhoto.OriginalFormat);
            }

            var url = FlickrHelpers.GetPhotoUrl(FlickrViewModel.SelectedPhoto);
            if (string.IsNullOrEmpty(url))
            {
                await new MessageDialog("This image has an invalid link and cannot be used, try a different photo")
                        .ShowAsync();
                return;
            }

            await SetBandImageAsync(await ApplyFilterAsync(url, imageFormat));
            
        }

        private void EditButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (FlickrViewModel.SelectedPhoto == null) return;

            App.ViewModel.SelectedFlickrPhoto = FlickrViewModel.SelectedPhoto;

            Frame.Navigate(typeof(PhotoDetailsPage));
        }

        #endregion

        #region Image processing

        private async Task<WriteableBitmap> ApplyFilterAsync(string url, ImageFormat imageFormat)
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

                var outputWriteableBitmap = new WriteableBitmap((int) tileSize.Width, (int) tileSize.Height);

                using (var source = new RandomAccessStreamImageSource(memStream.AsRandomAccessStream()))
                using (var effect = new CropEffect(source, new Rect(0, 0, (int) tileSize.Width, (int) tileSize.Height)))
                using (var renderer = new WriteableBitmapRenderer(effect, outputWriteableBitmap))
                {
                    await renderer.RenderAsync();
                }

                return outputWriteableBitmap;
            }
            catch (Exception exception)
            {
                await new MessageDialog($"Sorry, there was a problem cropping the image. It may be too small to put on the Band. Error: {exception}").ShowAsync();
                return null;
            }
            finally
            {
                App.ViewModel.IsBusyMessage = "";
                App.ViewModel.IsBusy = false;
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

        #region NavigationHelper registration


        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            //Window.Current.CoreWindow.SizeChanged += CoreWindow_SizeChanged;

            if (e.SourcePageType == typeof(PhotoDetailsPage))
            {
                LoadListPosition();
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            //Window.Current.CoreWindow.SizeChanged -= CoreWindow_SizeChanged;

            SaveListPosition();
        }

        private void CoreWindow_SizeChanged(CoreWindow sender, WindowSizeChangedEventArgs args)
        {
            //SetItemSize(args.Size.Width);
        }

        private void SaveListPosition()
        {
            try
            {
                var gridScrollViewer = ResultsGridView.GetChildOfType<ScrollViewer>();
                
                if (FlickrViewModel != null && gridScrollViewer != null)
                    FlickrViewModel.ScrollPosition = gridScrollViewer.VerticalOffset;

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SaveListPosition Exception: {ex.Message}");
            }
        }

        private void LoadListPosition()
        {
            try
            {
                var gridScrollViewer = ResultsGridView.GetChildOfType<ScrollViewer>();

                if (FlickrViewModel != null)
                    gridScrollViewer?.ScrollToHorizontalOffset(FlickrViewModel.ScrollPosition);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"LoadListPosition Exception: {ex.Message}");
            }
        }


        #endregion
    }
}
