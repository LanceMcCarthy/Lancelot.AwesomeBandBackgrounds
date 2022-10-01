// Lance McCarthy 2013-2023 MIT
// Free to use, maintain attribution to original
// https://github.com/LanceMcCarthy/Lancelot.AwesomeBandBackgrounds

using BandCentral.Models.Extensions;
using BandCentral.Models.Helpers;
using BandCentral.Models.Pictalicious;
using BandCentral.Uwp.Controls.ImageCropper.Helpers;
using Lumia.Imaging;
using Lumia.Imaging.Transforms;
using Microsoft.HockeyApp;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace BandCentral.Uwp.Dialogs
{
    public sealed partial class ImageThemeDialog : ContentDialog
    {
        public static readonly DependencyProperty ThemesRootProperty = DependencyProperty.Register(
            nameof(ThemesRoot), typeof(PictaliciousRoot), typeof(ImageThemeDialog), new PropertyMetadata(default(PictaliciousRoot)));

        public PictaliciousRoot ThemesRoot
        {
            get => (PictaliciousRoot) GetValue(ThemesRootProperty);
            set => SetValue(ThemesRootProperty, value);
        }

        public static readonly DependencyProperty PhotoProperty = DependencyProperty.Register(
            "PixPhoto", typeof(FlickrNet.Photo), typeof(ImageThemeDialog), new PropertyMetadata(default(FlickrNet.Photo)));

        public FlickrNet.Photo Photo
        {
            get => (FlickrNet.Photo) GetValue(PhotoProperty);
            set => SetValue(PhotoProperty, value);
        }

        public ImageThemeDialog(FlickrNet.Photo photo)
        {
            this.InitializeComponent();
            this.Photo = photo;
        }

        private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            App.ViewModel.IsBusy = true;

            if (Photo != null)
                await GetThemeAsync(this.Photo);

            //OutputTextBlock.Text = json;
            App.ViewModel.IsBusy = false;
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private async Task<string> GetSwatchJsonAsync(StorageFile file)
        {
            using (var client = new HttpClient())
            {
                var result = await client.SendImageDataAsync(file, "http://pictaculous.com/api/1.0/", "image");
                return await result.Content.ReadAsStringAsync();
            }
        }


        private async Task GetThemeAsync(FlickrNet.Photo photo)
        {
            //analytics
            //var start = DateTimeOffset.Now;
            //App.TelemetryClient.TrackRequest("ImageThemeDialog SaveImageAsync", start, DateTimeOffset.Now - start, "200", true);

            //download and save imahe to temp file
            var tempImagefile = await ApplicationData.Current.TemporaryFolder.CreateFileAsync("ImageThemeTemp.jpg", CreationCollisionOption.ReplaceExisting);
            var bitmap = await DownloadAndCropImageAsync(photo);
            await bitmap.SaveAsync(tempImagefile);

            //call api with file and get swatches as json
            var json = await GetSwatchJsonAsync(tempImagefile);

            //desrialize json
            this.ThemesRoot = JsonConvert.DeserializeObject<PictaliciousRoot>(json);

            //App.TelemetryClient.TrackEvent("GetImageBasedTheme");
            HockeyClient.Current.TrackEvent("SendToBandCommand");
        }

        private async Task<WriteableBitmap> DownloadAndCropImageAsync(FlickrNet.Photo photo)
        {
            try
            {
                App.ViewModel.IsBusy = true;
                App.ViewModel.IsBusyMessage = "downloading image...";

                var imageInfo = FlickrHelpers.GetPhotoInfo(photo, (int) App.ViewModel.WindowBounds.Width);

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

                var outputWriteableBitmap = new WriteableBitmap((int) tileSize.Width, (int) tileSize.Height);

                using (var source = new RandomAccessStreamImageSource(memStream.AsRandomAccessStream()))
                using (var effect = new CropEffect(source, new Rect(0, 0, (int) tileSize.Width, (int) tileSize.Height)))
                using (var renderer = new WriteableBitmapRenderer(effect, outputWriteableBitmap))
                {
                    await renderer.RenderAsync();
                }

                return outputWriteableBitmap;
            }
            catch (Exception ex)
            {
                HockeyClient.Current.TrackException(ex);
                await new MessageDialog($"Sorry, there was a problem cropping the image. It may be too small to put on the Band. Error: {ex}").ShowAsync();
                return null;
            }
            finally
            {
                App.ViewModel.IsBusyMessage = "";
                App.ViewModel.IsBusy = false;
            }
        }
    }
}
