// Lance McCarthy 2013-2023 MIT
// Free to use, maintain attribution to original
// https://github.com/LanceMcCarthy/Lancelot.AwesomeBandBackgrounds

using BandCentral.Models.Common;
using BandCentral.Models.Extensions;
using BandCentral.Models.Favorites;
using BandCentral.Models.Helpers;
using BandCentral.Models.Pictalicious;
using BandCentral.Uwp.Controls.ImageCropper.Helpers;
using Lumia.Imaging;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;
using Microsoft.HockeyApp;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using CropEffect = Lumia.Imaging.Transforms.CropEffect;
using Photo = FlickrNet.Photo;

namespace BandCentral.Uwp.Views
{
    public sealed partial class PairedThemePage : Page
    {
        public static readonly DependencyProperty SelectedPaletteBrushProperty = DependencyProperty.Register(
            nameof(SelectedPaletteBrush), typeof(SolidColorBrush), typeof(PairedThemePage), new PropertyMetadata(default(SolidColorBrush)));

        public SolidColorBrush SelectedPaletteBrush
        {
            get => (SolidColorBrush)GetValue(SelectedPaletteBrushProperty);
            set => SetValue(SelectedPaletteBrushProperty, value);
        }

        #region fields

        private StorageFile imageFile;
        private string selectedBandThemeColor;
        private bool hasThreeShown;

        #endregion

        public PairedThemePage()
        {
            this.InitializeComponent();
            DataContext = App.ViewModel;

            HockeyClient.Current.TrackPageView(nameof(PairedThemePage));
        }

        #region Event handlers

        private async void ThemeColorGrid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (SelectedPaletteBrush == null)
            {
                await new MessageDialog("You need to select a palette color below first").ShowAsync();
                return;
            }

            var tag = ((Grid)sender).Tag as string;
            selectedBandThemeColor = tag;

            switch (selectedBandThemeColor)
            {
                case "Base":
                    App.ViewModel.SelectedFav.Theme.BaseBrush = SelectedPaletteBrush;
                    break;
                case "Highlight":
                    App.ViewModel.SelectedFav.Theme.HighlightBrush = SelectedPaletteBrush;
                    break;
                case "LowLight":
                    App.ViewModel.SelectedFav.Theme.LowLightBrush = SelectedPaletteBrush;
                    break;
                case "Secondary":
                    App.ViewModel.SelectedFav.Theme.SecondaryBrush = SelectedPaletteBrush;
                    break;
                case "HighContrast":
                    App.ViewModel.SelectedFav.Theme.HighContrastBrush = SelectedPaletteBrush;
                    break;
                case "Muted":
                    App.ViewModel.SelectedFav.Theme.MutedBrush = SelectedPaletteBrush;
                    break;
                default:
                    break;
            }
        }

        private void PaletteColor_OnItemClick(object sender, ItemClickEventArgs e)
        {
            var selectedColor = e.ClickedItem as string;
            if (string.IsNullOrEmpty(selectedColor))
                return;

            if (!hasThreeShown)
            {
                StepThreeStoryboard.Begin();
                hasThreeShown = true;
            }

            SelectedPaletteBrush = Helpers.HexToBrush(selectedColor);
            Debug.WriteLine($"Selected Palette Color {selectedColor}");
        }

        private async void GeneratePalettesButton_OnClick(object sender, RoutedEventArgs e)
        {
            await GenerateSwatchesAsync(App.ViewModel.SelectedFav?.Photo);

            StepOneGrid.Visibility = Visibility.Collapsed;
            StepTwoStoryboard.Begin();
        }

        private async void RegeneratePalettesButton_OnClick(object sender, RoutedEventArgs e)
        {
            HockeyClient.Current.TrackEvent("RegeneratePalettes");

            await GenerateSwatchesAsync(App.ViewModel.SelectedFav?.Photo);
        }

        private async void SaveButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                App.ViewModel.IsBusy = true;
                App.ViewModel.IsBusyMessage = "saving Paired Theme";

                //update the property the paired theme flag
                App.ViewModel.SelectedFav.HasCustomTheme = true;

                ////NOTE If there are any BGfavs
                //if (App.ViewModel.BackgroundFavs.Any())
                //{
                //    var bgFav = App.ViewModel.BackgroundFavs.FirstOrDefault(b => b.PhotoId == App.ViewModel.SelectedFav.Photo.PhotoId);
                //    if (bgFav != null) bgFav.HasCustomTheme = true;
                //}

                if (await App.ViewModel.SaveFavoritesJsonAsync())
                {
                    await new MessageDialog("Your custom theme has been saved. If you want to apply it now, click the SET THEME button", "Saved!").ShowAsync();
                }
            }
            catch (Exception ex)
            {
                await new MessageDialog($"There was a problem saving, try again.\r\n\nIf this keeps happening, take a screenshot and contact us at awesomeapps@outlook.com.\r\n\nSaveThemeException: {ex.Message}", "Error").ShowAsync();
            }
            finally
            {
                App.ViewModel.IsBusy = false;
                App.ViewModel.IsBusyMessage = "";

                HockeyClient.Current.TrackEvent("FavThemeSaved");
            }
        }

        private async void SetThemeButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (await App.ViewModel.SetBandThemeAsync(App.ViewModel.SelectedFav.Theme))
            {
                App.ViewModel.CurrentBandTheme = App.ViewModel.SelectedFav.Theme;

                var properties = new Dictionary<string, string>();
                properties.Add("PageUsedOn", "ImageSwatchTest");
                HockeyClient.Current.TrackEvent("SetTheme", properties);
            }
        }

        #endregion

        #region Methods

        private async Task GenerateSwatchesAsync(Photo photo)
        {
            if (photo == null)
                return;

            try
            {
                //call api with file and get swatches as json
                var json = await GetSwatchJsonAsync(imageFile);

                var pictaliciousResult = JsonConvert.DeserializeObject<PictaliciousRoot>(json);

                if (pictaliciousResult == null)
                    return;

                var list = new List<FavPalette>();

                foreach (var item in pictaliciousResult.Cl_themes)
                {
                    list.Add(new FavPalette
                    {
                        Id = item.Id.ToString(),
                        Title = item.Title,
                        Author = item.Author,
                        Rating = item.Rating,
                        Thumb = item.Thumb,
                        Colors = item.Colors
                    });
                }

                foreach (var item in pictaliciousResult.Kuler_themes)
                {
                    list.Add(new FavPalette
                    {
                        Id = item.Id,
                        Title = item.Title,
                        Author = item.Author,
                        Rating = item.Rating,
                        Thumb = item.Thumb,
                        Colors = item.Colors
                    });
                }

                ThemesListView.ItemsSource = list;

                //send telemetry

                var properties = new Dictionary<string, string>();
                properties.Add("FlickrPhotoMediumUrl", photo.PhotoId);

                var metrics = new Dictionary<string, double>();
                metrics.Add("Kuler_themes", pictaliciousResult.Kuler_themes.Count);
                metrics.Add("Cl_themes", pictaliciousResult.Cl_themes.Count);

                HockeyClient.Current.TrackEvent("GenerateSwatchesAsync", properties, metrics);

                //save results to a local json file using the photoID so we wont have to do another API call again
                await SaveGeneratedPalettesAsync(list, photo);
            }
            catch (Exception ex)
            {
                HockeyClient.Current.TrackException(ex);
            }
        }

        private async Task<string> GetSwatchJsonAsync(StorageFile file)
        {
            try
            {
                App.ViewModel.IsBusy = true;
                App.ViewModel.IsBusyMessage = "generating palettes in the cloud...";

                var progressReporter = new Progress<DownloadProgressArgs>();
                progressReporter.ProgressChanged += ProgressReporterOnProgressChanged;
                var json = await HttpClientExtensions.SendImageDataWithDownloadProgressAsync(file, "http://pictaculous.com/api/1.0/", "image", progressReporter);
                progressReporter.ProgressChanged -= ProgressReporterOnProgressChanged;
                return json;

            }
            catch (Exception ex)
            {
                await new MessageDialog($"Sorry, there was a problem getting the color swatches. Please try again. Error: {ex}").ShowAsync();
                return null;
            }
            finally
            {
                App.ViewModel.IsBusyMessage = "";
                App.ViewModel.IsBusy = false;
            }
        }

        private void ProgressReporterOnProgressChanged(object sender, DownloadProgressArgs e)
        {
            App.ViewModel.IsBusyMessage = $"downloading {e.PercentComplete}%";
        }

        private async Task<WriteableBitmap> DownloadAndCropImageAsync(Photo photo)
        {
            try
            {
                var start = DateTimeOffset.Now; //analytics

                App.ViewModel.IsBusy = true;
                App.ViewModel.IsBusyMessage = "downloading image data...";

                var imageInfo = FlickrHelpers.GetPhotoInfo(photo, (int)App.ViewModel.WindowBounds.Width);

                if (imageInfo == null)
                {
                    await new MessageDialog("Something went wrong getting the photo. Try again.").ShowAsync();
                    return null;
                }
                else if (string.IsNullOrEmpty(imageInfo.Url))
                {
                    await new MessageDialog("The photo's links are invalid, try a different photo.\r\n\n" +
                                            "Tip: you can add the photo to your Favorites and try at another time.").ShowAsync();
                    return null;
                }

                //http get for image stream
                var handler = new HttpClientHandler();
                if (handler.SupportsAutomaticDecompression)
                    handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

                var client = new HttpClient(handler);
                var stream = await client.GetStreamAsync(imageInfo.Url);

                //seekable stream
                var memStream = new MemoryStream();
                await stream.CopyToAsync(memStream);
                memStream.Position = 0;

                App.ViewModel.IsBusyMessage = "cropping photo...";

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
                await new MessageDialog($"Sorry, there was a problem cropping the image. It may be too small to put on the Band. Error: {ex}").ShowAsync();
                return null;
            }
            finally
            {
                App.ViewModel.IsBusyMessage = "";
                App.ViewModel.IsBusy = false;

                var properties = new Dictionary<string, string>();
                properties.Add("PageUsedOn", "ImageSwatchTest");
                HockeyClient.Current.TrackEvent("DownloadAndCropAsync", properties);
            }
        }

        private static async Task ApplyBackgroundBlurAsync(UIElement sourceElement, Grid targetGrid, float blurAmount = 8.0f)
        {
            if (sourceElement == null || targetGrid == null)
                return;

            var rtb = new RenderTargetBitmap();
            await rtb.RenderAsync(sourceElement);

            var pixelBuffer = await rtb.GetPixelsAsync();
            var pixels = pixelBuffer.ToArray();

            var displayInformation = DisplayInformation.GetForCurrentView();

            using (var stream = new InMemoryRandomAccessStream())
            {
                var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
                encoder.SetPixelData(BitmapPixelFormat.Bgra8,
                                     BitmapAlphaMode.Premultiplied,
                                     (uint)rtb.PixelWidth,
                                     (uint)rtb.PixelHeight,
                                     displayInformation.RawDpiX,
                                     displayInformation.RawDpiY,
                                     pixels);

                await encoder.FlushAsync();
                stream.Seek(0);

                var device = new CanvasDevice();
                var bitmap = await CanvasBitmap.LoadAsync(device, stream);

                var renderer = new CanvasRenderTarget(device,
                                                      bitmap.SizeInPixels.Width,
                                                      bitmap.SizeInPixels.Height, bitmap.Dpi);

                using (var ds = renderer.CreateDrawingSession())
                {
                    var blur = new GaussianBlurEffect
                    {
                        BlurAmount = blurAmount,
                        Source = bitmap
                    };
                    ds.DrawImage(blur);
                }

                stream.Seek(0);
                await renderer.SaveAsync(stream, CanvasBitmapFileFormat.Png);

                var image = new BitmapImage();
                await image.SetSourceAsync(stream);
                targetGrid.Background = new ImageBrush
                {
                    ImageSource = image,
                    Stretch = Stretch.UniformToFill
                };
            }
        }

        #endregion

        #region File operations

        private async Task<WriteableBitmap> SaveImageLocallyAsync(FlickrFav fav)
        {
            Debug.WriteLine($"---------------SaveImageLocally() Called-----------------");
            try
            {
                App.ViewModel.IsBusy = true;
                App.ViewModel.IsBusyMessage = "saving image locally...";

                imageFile = await ApplicationData.Current.LocalFolder.CreateFileAsync($"{App.ViewModel.SelectedFav.Photo.PhotoId}.jpg", CreationCollisionOption.ReplaceExisting);
                var bitmap = await DownloadAndCropImageAsync(fav.Photo);
                await bitmap.SaveAsync(imageFile);

                App.ViewModel.SelectedFav.LocalImageFilePath = imageFile.Path;

                return bitmap;
            }
            catch (Exception ex)
            {
                await new MessageDialog($"SaveImageLocallyAsync() Exception. Error: {ex.Message}").ShowAsync();
                return null;
            }
            finally
            {
                App.ViewModel.IsBusyMessage = "";
                App.ViewModel.IsBusy = false;
            }
        }

        private async Task<WriteableBitmap> LoadLocalImageIntoBitmapAsync(FlickrFav fav)
        {
            Debug.WriteLine($"-----------------LoadLocalImageIntoBitmap() Called------------------");
            try
            {
                App.ViewModel.IsBusy = true;
                App.ViewModel.IsBusyMessage = "loading image from storage...";

                var file = await ApplicationData.Current.LocalFolder.TryGetItemAsync($"{App.ViewModel.SelectedFav.Photo.PhotoId}.jpg");

                imageFile = file as StorageFile;

                //if the file path does not return a valid file, save it and return the resulting bitmap
                if (imageFile == null)
                {
                    return await SaveImageLocallyAsync(fav);
                }

                using (var stream = await imageFile.OpenAsync(FileAccessMode.Read))
                {
                    var bitmap = new WriteableBitmap(1, 1);
                    await bitmap.SetSourceAsync(stream);
                    return bitmap;
                }
            }
            catch (Exception ex)
            {
                await new MessageDialog($"LoadLocalImageIntoBitmapAsync() Exception. Error: {ex.Message}").ShowAsync();
                return null;
            }
            finally
            {
                App.ViewModel.IsBusyMessage = "";
                App.ViewModel.IsBusy = false;
            }
        }

        private static async Task SaveGeneratedPalettesAsync(List<FavPalette> palettes, Photo photo)
        {
            Debug.WriteLine($"-----------------SaveGeneratedPalettesAsync() Called------------------");

            try
            {
                App.ViewModel.IsBusy = true;
                App.ViewModel.IsBusyMessage = "saving generated palettes...";

                var json = JsonConvert.SerializeObject(palettes);

                var file = await ApplicationData.Current.LocalFolder.CreateFileAsync($"{photo.PhotoId}_GeneratedPalettes.json", CreationCollisionOption.ReplaceExisting);

                using (var fileStream = await file.OpenStreamForWriteAsync())
                using (var writer = new StreamWriter(fileStream))
                {
                    await writer.WriteAsync(json);
                }
            }
            catch (Exception ex)
            {
                HockeyClient.Current.TrackException(ex);
            }
            finally
            {
                App.ViewModel.IsBusyMessage = "";
                App.ViewModel.IsBusy = false;
            }
        }

        private static async Task<List<FavPalette>> LoadGeneratedPalettesAsync(Photo photo)
        {
            Debug.WriteLine($"-----------------LoadGeneratedPalettesAsync() Called------------------");

            try
            {
                App.ViewModel.IsBusy = true;
                App.ViewModel.IsBusyMessage = "checking for previously generated palettes...";

                var themesFile = await ApplicationData.Current.LocalFolder.TryGetItemAsync($"{photo.PhotoId}_GeneratedPalettes.json");
                if (themesFile == null)
                    return null;

                var file = themesFile as StorageFile;

                using (var fileStream = await file.OpenStreamForReadAsync())
                using (var reader = new StreamReader(fileStream))
                {
                    var json = await reader.ReadToEndAsync();
                    return JsonConvert.DeserializeObject<List<FavPalette>>(json);
                }
            }
            catch (Exception ex)
            {
                HockeyClient.Current.TrackException(ex);
                return null;
            }
            finally
            {
                App.ViewModel.IsBusyMessage = "";
                App.ViewModel.IsBusy = false;
            }
        }

        #endregion

        #region navigation

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (App.ViewModel.SelectedFav == null)
            {
                await new MessageDialog("You do not have a Favorite selected, please try again.").ShowAsync();
                Frame.GoBack();
            }

            if (string.IsNullOrEmpty(App.ViewModel.SelectedFav?.LocalImageFilePath))
            {
                SourceImage.Source = await SaveImageLocallyAsync(App.ViewModel.SelectedFav);

                //transition test
                //await SaveImageLocallyAsync(App.ViewModel.SelectedFav);
                //SourceImage.Source = new Uri(App.ViewModel.SelectedFav?.LocalImageFilePath);

                //if we had to download the image, then save Favorties to persist the  new value for fav.LocalImageFilePath
                await App.ViewModel.SaveFavoritesJsonAsync();
            }
            else
            {
                SourceImage.Source = await LoadLocalImageIntoBitmapAsync(App.ViewModel.SelectedFav);

                //transition test
                //SourceImage.Source = new Uri(App.ViewModel.SelectedFav?.LocalImageFilePath);
            }

            //----------- check and load any previously generated palettes ---------//

            var savedPalettes = await LoadGeneratedPalettesAsync(App.ViewModel.SelectedFav.Photo);

            if (savedPalettes == null)
            {
                StepOneGrid.Visibility = Visibility.Visible;
            }
            else
            {
                //set the list if there were results
                ThemesListView.ItemsSource = savedPalettes;

                //clear the tutorials
                StepOneGrid.Visibility = Visibility.Collapsed;
                hasThreeShown = true;
            }

            //------------------------------------------------------------------------//

            //transition test
            // ((ContinuityTransition) e.Parameter)?.Start(PhotoGrid, SourceImage, null, PhotoGrid);

            //if (!AnalyticsInfo.VersionInfo.DeviceFamily.Equals("Windows.Mobile", StringComparison.CurrentCultureIgnoreCase))
            //{
            //    AppBarHelpButton.Visibility = Visibility.Collapsed;
            //}

            //----------- APPLY BLUR -------------//

            await ApplyBackgroundBlurAsync(SourceImage, ContentRoot);
        }

        #endregion
    }
}
