﻿using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Foundation;
using Windows.UI.Popups;
using Windows.UI.Xaml.Media.Imaging;
using BandCentral.Uwp.Common;
using BandCentral.WindowsBase.Common;
using FlickrNet;
using Lumia.Imaging;
using Lumia.Imaging.Transforms;
using Microsoft.HockeyApp;

namespace BandCentral.Uwp.Commands
{
    public class SendToBandCommand : ICommand
    {
        public bool CanExecute(object parameter)
        {
            return true;
        }

        public async void Execute(object parameter)
        {
            //band detection and reconnection is within SetBandImage()
            //if (!App.ViewModel.IsBandConnected)
            //{
            //    await new MessageDialog("Reconnect to your Band, then try again.", "Band not connected").ShowAsync();
            //}

            if (parameter is Photo)
            {
                var selectedPhoto = (Photo)parameter;
                await SetBandImageAsync(await DownloadAndCropAsync(selectedPhoto));
            }

            if (parameter is FlickrFav)
            {
                var selectedPhoto = (FlickrFav)parameter;

                await SetBandImageAsync(await DownloadAndCropAsync(selectedPhoto.Photo));
            }
        }

        public event EventHandler CanExecuteChanged;

        private async Task<WriteableBitmap> DownloadAndCropAsync(Photo photo)
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

                var start = DateTimeOffset.Now;

                //http get for image stream
                var handler = new HttpClientHandler();
                if (handler.SupportsAutomaticDecompression)
                    handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

                var client = new HttpClient(handler);
                var stream = await client.GetStreamAsync(imageInfo.Url);

                //analytics
                //App.TelemetryClient.TrackRequest("SendToBandCommand GetStreamAsync", start, DateTimeOffset.Now - start, "200", true);

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
                
                HockeyClient.Current.TrackEvent($"SendToBandCommand");

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

        private static async Task SetBandImageAsync(WriteableBitmap outputWb)
        {
            if (outputWb == null) return;

            try
            {
                App.ViewModel.IsBusy = true;
                await App.ViewModel.SetMeTileAsync(outputWb);
            }
            catch (Exception ex)
            {
                HockeyClient.Current.TrackException(ex);
                await new MessageDialog("Sorry, there was a problem cropping the image: " + ex).ShowAsync();
            }
            finally
            {
                App.ViewModel.IsBusyMessage = "";
                App.ViewModel.IsBusy = false;
            }
        }

    }
}
