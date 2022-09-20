using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using BandCentral.Common;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using BandCentral.WindowsBase.Common;
using BandCentralBase.Common;
using Lumia.Imaging;
using Lumia.Imaging.Transforms;
using Lumia.InteropServices.WindowsRuntime;
using WriteableBitmapExtensions = Windows.UI.Xaml.Media.Imaging.WriteableBitmapExtensions;

namespace BandCentral
{
    public sealed partial class PhotoProvidersPage : Page
    {
        private NavigationHelper navigationHelper;
        public NavigationHelper NavigationHelper => this.navigationHelper;

        public static readonly DependencyProperty IsPreviewGridVisibleProperty = DependencyProperty.Register(
            "IsPreviewGridVisible", 
            typeof(bool), 
            typeof(PhotoProvidersPage), 
            new PropertyMetadata(default(bool), (o, e) =>
            {
                if ((bool) e.NewValue)
                {
                    ((PhotoProvidersPage) o).ShowPreviewStoryboard.Begin();
                }
                else
                {
                    ((PhotoProvidersPage) o).HidePreviewStoryboard.Begin();
                }
                
            }));

        public bool IsPreviewGridVisible
        {
            get { return (bool) GetValue(IsPreviewGridVisibleProperty); }
            set { SetValue(IsPreviewGridVisibleProperty, value); }
        }

        private BingImage selectedBingPhoto;
        private PixPhoto selectedPixPhoto;
        private WriteableBitmap processedBitmap;
        
        public PhotoProvidersPage()
        {
            this.InitializeComponent();
            DataContext = App.ViewModel;

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
            
        }


        #region Band overlay events
        
        private async void SendToBandButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (processedBitmap == null)
                {
                    Debug.WriteLine("processedBitmap was null");
                    return;
                }

                await App.ViewModel.SetMeTileAsync(processedBitmap);

                await new MessageDialog("You've successfully updated your Band background", "Updated!").ShowAsync();

                IsPreviewGridVisible = false;
                selectedPixPhoto = null;
                selectedBingPhoto = null;
            }
            catch (Exception ex)
            {
                await new MessageDialog("Sorry, there was a problem sending image to the band: " + ex).ShowAsync();
            }
            finally
            {
                App.ViewModel.IsBusyMessage = "";
                App.ViewModel.IsBusy = false;
            }
        }

        private void CancelButton_OnClick(object sender, RoutedEventArgs e)
        {
            IsPreviewGridVisible = false;
            selectedBingPhoto = null;
            selectedPixPhoto = null;

            DownloadAndCropButton.IsEnabled = false;
        }

        private void ShowHidePreviewButton_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            IsPreviewGridVisible = !IsPreviewGridVisible;
        }

        #endregion

        #region Bing images

        private async void Images_OnSelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            try
            {
                if (args.AddedItems.Any())
                {
                    if (args.AddedItems[0] is BingImage)
                    {
                        Debug.WriteLine($"Bing Image Selected: {args.AddedItems[0]}");
                        selectedBingPhoto = (BingImage) args.AddedItems[0];

                        var url = $"http://www.bing.com{selectedBingPhoto?.url}";
                        ChosenImage.Source = new BitmapImage(new Uri(url));

                        DownloadAndCropButton.IsEnabled = true;

                        if (!IsPreviewGridVisible)
                            IsPreviewGridVisible = true;
                    }
                    if (args.AddedItems[0] is PixPhoto)
                    {
                        Debug.WriteLine($"Pix Image Selected: {args.AddedItems[0]}");
                        selectedPixPhoto = (PixPhoto) args.AddedItems[0];

                        ChosenImage.Source = new BitmapImage(new Uri(selectedPixPhoto.image_url));

                        DownloadAndCropButton.IsEnabled = true;

                        if (!IsPreviewGridVisible)
                            IsPreviewGridVisible = true;
                    }
                }
                else
                {
                    selectedBingPhoto = null;
                    selectedPixPhoto = null;
                    DownloadAndCropButton.IsEnabled = false;

                    if (IsPreviewGridVisible)
                        IsPreviewGridVisible = false;
                }

                DownloadAndCropBorder.Visibility = Visibility.Visible;
                SendToBandButton.IsEnabled = false;
            }
            catch (Exception ex)
            {
                await new MessageDialog("If this keeps happening, take a screenshot of this error and email it to awesome.apps@outlook.com" +
                                        $"\r\n\nError: {ex.Message}", "Selection Error").ShowAsync();
            }
            
        }

        private async void DownloadAndCropButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (selectedBingPhoto != null)
            {
                try
                {
                    App.ViewModel.IsBusy = true;
                    App.ViewModel.IsBusyMessage = "downloading and cropping image..";

                    var url = $"http://www.bing.com{selectedBingPhoto?.url}";
                    var bitmap = await GetBingBitmapAsync(url); //returns a 1920x1080 image everytime

                    if (bitmap == null)
                    {
                        Debug.WriteLine("Bitmap was null");
                        return;
                    }

                    processedBitmap = await CropBingBitmapAsync(bitmap);
                    ChosenImage.Source = processedBitmap;
                    SendToBandButton.IsEnabled = true;
                    DownloadAndCropBorder.Visibility = Visibility.Collapsed;
                }
                catch (Exception ex)
                {
                    await new MessageDialog("Sorry, there was a problem sending image to the band: " + ex).ShowAsync();
                }
                finally
                {
                    App.ViewModel.IsBusyMessage = "";
                    App.ViewModel.IsBusy = false;
                }
            }

            if (selectedPixPhoto != null)
            {
                try
                {
                    App.ViewModel.IsBusy = true;
                    App.ViewModel.IsBusyMessage = "downloading and cropping image..";

                    var result = await new FiveHundredPix(Constants.FiveHundredPixConsumerKey).GetPhotoAsync(selectedPixPhoto.id.ToString());
                    
                    var fullImageUrl = result.photo.image_url;

                    Debug.WriteLine($"---------Pix photo url {fullImageUrl}");

                    var bitmap = await GetPixBitmapAsync(fullImageUrl, new Size(440, 440));

                    if (bitmap == null)
                    {
                        Debug.WriteLine("Pix Bitmap was null");
                        return;
                    }

                    processedBitmap = await CropPixBitmapAsync(bitmap);
                    ChosenImage.Source = processedBitmap;
                    SendToBandButton.IsEnabled = true;
                    DownloadAndCropBorder.Visibility = Visibility.Collapsed;
                }
                catch (Exception ex)
                {
                    await new MessageDialog("Sorry, there was a problem sending image to the band: " + ex).ShowAsync();
                }
                finally
                {
                    App.ViewModel.IsBusyMessage = "";
                    App.ViewModel.IsBusy = false;
                }
            }

            if (selectedBingPhoto == null && selectedPixPhoto == null)
            {
                Debug.WriteLine("SelectedImage was null");
                await new MessageDialog("You need to select a photo before continuing.", "No Selection").ShowAsync();
                return;
            }
        }
        
        #endregion

        #region download and crop methods
        
        private async Task<WriteableBitmap> GetBingBitmapAsync(string imageUrl)
        {
            try
            {
                App.ViewModel.IsBusy = true;
                App.ViewModel.IsBusyMessage = "downloading image...";

                var handler = new HttpClientHandler();
                if (handler.SupportsAutomaticDecompression)
                    handler.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;

                using (var client = new HttpClient(handler))
                using (var stream = await client.GetStreamAsync(imageUrl))
                using (var memStream = new MemoryStream())
                {
                    stream.CopyTo(memStream);
                    memStream.Position = 0;

                    //Bing images throught the API are always this size
                    var wbm = new WriteableBitmap(1920, 1080);
                    await wbm.SetSourceAsync(memStream.AsRandomAccessStream());
                    wbm.Invalidate();
                    return wbm;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GetCroppedBitmapAsync Exception: {ex.Message}");
                return null;
            }
            finally
            {
                App.ViewModel.IsBusy = false;
                App.ViewModel.IsBusyMessage = "";
            }
        }

        private async Task<WriteableBitmap> CropBingBitmapAsync(WriteableBitmap inputBitmap)
        {
            try
            {
                App.ViewModel.IsBusy = true;
                App.ViewModel.IsBusyMessage = "cropping image...";
                //NOTE: App.ViewModel.MeTileSize (310 x 128 [or 102 for Band 1])

                //0.161458 is the expected value for the normal Bing Image size of 1920x1080
                var scaleRatio = (double) App.ViewModel.MeTileSize.Width / (double) inputBitmap.PixelWidth;
                Debug.WriteLine($"ScaleRatio: {scaleRatio}");

                var scaledHeight = inputBitmap.PixelHeight * scaleRatio;
                Debug.WriteLine($"ScaledHeight: {scaledHeight}");

                var resizedBitmap = inputBitmap.Resize((int) App.ViewModel.MeTileSize.Width, (int) scaledHeight, WriteableBitmapExtensions.Interpolation.Bilinear);

                var outputWriteableBitmap = new WriteableBitmap((int) App.ViewModel.MeTileSize.Width, (int) App.ViewModel.MeTileSize.Height);

                using (var source = new BitmapImageSource(resizedBitmap.AsBitmap()))
                using (var filters = new FilterEffect(source))
                {
                    filters.Filters = new IFilter[]
                    {
                        new CropFilter(new Rect(0, 0, (int) App.ViewModel.MeTileSize.Width, (int) App.ViewModel.MeTileSize.Height))
                    };

                    using (var renderer = new WriteableBitmapRenderer(filters, outputWriteableBitmap))
                    {
                        await renderer.RenderAsync();
                        return outputWriteableBitmap;
                    }
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GetCroppedBitmapAsync Exception: {ex.Message}");
                return null;
            }
            finally
            {
                App.ViewModel.IsBusy = false;
                App.ViewModel.IsBusyMessage = "";
            }
        }

        private async Task<WriteableBitmap> GetPixBitmapAsync(string imageUrl, Size imageSize)
        {
            try
            {
                App.ViewModel.IsBusy = true;
                App.ViewModel.IsBusyMessage = "downloading image...";

                var handler = new HttpClientHandler();
                if (handler.SupportsAutomaticDecompression)
                    handler.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;

                using (var client = new HttpClient(handler))
                using (var stream = await client.GetStreamAsync(imageUrl))
                using (var memStream = new MemoryStream())
                {
                    stream.CopyTo(memStream);
                    memStream.Position = 0;
                    
                    var wbm = new WriteableBitmap((int)imageSize.Width, (int)imageSize.Height);
                    await wbm.SetSourceAsync(memStream.AsRandomAccessStream());
                    wbm.Invalidate();
                    return wbm;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GetCroppedBitmapAsync Exception: {ex.Message}");
                return null;
            }
            finally
            {
                App.ViewModel.IsBusy = false;
                App.ViewModel.IsBusyMessage = "";
            }
        }

        private async Task<WriteableBitmap> CropPixBitmapAsync(WriteableBitmap inputBitmap)
        {
            try
            {
                App.ViewModel.IsBusy = true;
                App.ViewModel.IsBusyMessage = "cropping image...";
                //NOTE: App.ViewModel.MeTileSize (310 x 128 [or 102 for Band 1])

                //pix bitmap range widely
                var scaleRatio = (double) App.ViewModel.MeTileSize.Width / (double) inputBitmap.PixelWidth;
                Debug.WriteLine($"ScaleRatio: {scaleRatio}");

                var scaledHeight = inputBitmap.PixelHeight * scaleRatio;
                Debug.WriteLine($"ScaledHeight: {scaledHeight}");

                var resizedBitmap = inputBitmap.Resize((int) App.ViewModel.MeTileSize.Width, (int) scaledHeight, WriteableBitmapExtensions.Interpolation.Bilinear);

                var outputWriteableBitmap = new WriteableBitmap((int) App.ViewModel.MeTileSize.Width, (int) App.ViewModel.MeTileSize.Height);

                using (var source = new BitmapImageSource(resizedBitmap.AsBitmap()))
                using (var filters = new FilterEffect(source))
                {
                    filters.Filters = new IFilter[]
                    {
                        new CropFilter(new Rect(0, 0, (int) App.ViewModel.MeTileSize.Width, (int) App.ViewModel.MeTileSize.Height))
                    };

                    using (var renderer = new WriteableBitmapRenderer(filters, outputWriteableBitmap))
                    {
                        await renderer.RenderAsync();
                        return outputWriteableBitmap;
                    }
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GetCroppedBitmapAsync Exception: {ex.Message}");
                return null;
            }
            finally
            {
                App.ViewModel.IsBusy = false;
                App.ViewModel.IsBusyMessage = "";
            }
        }

        #endregion

        #region navigation

        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            if (!(e.NavigationParameter is string)) return;

            switch (e.NavigationParameter.ToString())
            {
                case "Bing":
                    ProvidersPivot.SelectedIndex = 0;
                    break;
                case "500px":
                    ProvidersPivot.SelectedIndex = 1;
                    break;
            }
        }

        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private void ProvidersPivot_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsPreviewGridVisible)
                IsPreviewGridVisible = false;

            selectedBingPhoto = null;
            selectedPixPhoto = null;
            SendToBandButton.IsEnabled = false;
            DownloadAndCropBorder.Visibility = Visibility.Visible;
        }

        
    }
}

