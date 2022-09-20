using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Popups;
using Windows.UI.Xaml.Media.Imaging;
using BandCentral.WindowsBase.Common;
using BandCentralBase.Common;
using FlickrNet;
using Lumia.Imaging;
using Lumia.Imaging.Adjustments;
using Lumia.Imaging.Artistic;
using Lumia.Imaging.Transforms;
using Lumia.InteropServices.WindowsRuntime;

namespace BandCentral.Common
{
    public static class FilterEffectsHelper
    {
        public static async Task<ObservableCollection<FilterListItem>> GetAvailableFiltersAsync()
        {
            return new ObservableCollection<FilterListItem>
                    {
                        new FilterListItem { Title = "Antique", ThumbnailPath = "Images/ImagingSDK/AntiqueFilter.jpg" },
                        new FilterListItem { Title = "Auto Levels", ThumbnailPath = "Images/ImagingSDK/AutoLevelsFilter.jpg" },
                        new FilterListItem { Title = "Blur", ThumbnailPath = "Images/ImagingSDK/Blur.jpg" },
                        new FilterListItem { Title = "Cartoon", ThumbnailPath = "Images/ImagingSDK/Cartoon.jpg" },
                        new FilterListItem { Title = "Color Boost", ThumbnailPath = "Images/ImagingSDK/ColorBoostFilter.jpg" },
                        new FilterListItem { Title = "Despeckle", ThumbnailPath = "Images/ImagingSDK/Despeckle.jpg" },
                        new FilterListItem { Title = "Emboss", ThumbnailPath = "Images/ImagingSDK/Emboss.jpg" },
                        new FilterListItem { Title = "Fog", ThumbnailPath = "Images/ImagingSDK/Fog.jpg" },
                        new FilterListItem { Title = "Gaussian Noise", ThumbnailPath = "Images/ImagingSDK/GaussianNoise.jpg" },
                        new FilterListItem { Title = "Grayscale", ThumbnailPath = "Images/ImagingSDK/Grayscale.jpg" },
                        new FilterListItem { Title = "Hue Saturation", ThumbnailPath = "Images/ImagingSDK/HueSaturation.jpg" },
                        new FilterListItem { Title = "Local Boost", ThumbnailPath = "Images/ImagingSDK/LocalBoostAutomatic.jpg" },
                        new FilterListItem { Title = "Lomo", ThumbnailPath = "Images/ImagingSDK/Lomo.jpg" },
                        new FilterListItem { Title = "Magic Pen", ThumbnailPath = "Images/ImagingSDK/Magicpen.jpg" },
                        new FilterListItem { Title = "Mirror", ThumbnailPath = "Images/ImagingSDK/Mirror.jpg" },
                        new FilterListItem { Title = "Moonlight", ThumbnailPath = "Images/ImagingSDK/Moonlight.jpg" },
                        new FilterListItem { Title = "Negative", ThumbnailPath = "Images/ImagingSDK/Negative.jpg" },
                        new FilterListItem { Title = "Posterize", ThumbnailPath = "Images/ImagingSDK/Posterize.jpg" },
                        new FilterListItem { Title = "Sepia", ThumbnailPath = "Images/ImagingSDK/Sepia.jpg" },
                        new FilterListItem { Title = "Sharpness", ThumbnailPath = "Images/ImagingSDK/Sharpness.jpg" },
                        new FilterListItem { Title = "Sketch", ThumbnailPath = "Images/ImagingSDK/Sketch.jpg" },
                        new FilterListItem { Title = "Solarize", ThumbnailPath = "Images/ImagingSDK/Solarize.jpg" },
                        new FilterListItem { Title = "Temp and Tint", ThumbnailPath = "Images/ImagingSDK/TemperatureAndTintFilter.jpg" },
                        new FilterListItem { Title = "Vibrance", ThumbnailPath = "Images/ImagingSDK/Vibrance.jpg" },
                        new FilterListItem { Title = "Watercolor", ThumbnailPath = "Images/ImagingSDK/Watercolor.jpg" },
                        new FilterListItem { Title = "White Balance", ThumbnailPath = "Images/ImagingSDK/WhiteBalance.jpg" }
                    };
        }

        //public static async Task<ImageFormat> GetImageFormatType(string format)
        //{
        //    switch(format)
        //    {
        //        case "png":
        //            return ImageFormat.Png;
        //        case "jpg":
        //            return ImageFormat.Jpeg;
        //        case "jpeg":
        //            return ImageFormat.Jpeg;
        //        case "bmp":
        //            return ImageFormat.Bmp;
        //        case "gif":
        //            return ImageFormat.Gif;
        //        case "tiff":
        //            return ImageFormat.Tiff;
        //        case "wbmp":
        //            return ImageFormat.Wbmp; //writeable bitmap
        //        default:
        //            return ImageFormat.Jpeg;
        //    }
        //}

        public static ImageFormat GetImageFormatType(string format)
        {
            switch (format)
            {
                case "png":
                    return ImageFormat.Png;
                case "jpg":
                    return ImageFormat.Jpeg;
                case "jpeg":
                    return ImageFormat.Jpeg;
                case "bmp":
                    return ImageFormat.Bmp;
                case "gif":
                    return ImageFormat.Gif;
                case "tiff":
                    return ImageFormat.Tiff;
                case "wbmp":
                    return ImageFormat.Wbmp; //writeable bitmap
                default:
                    return ImageFormat.Undefined;
            } 
        }

        public static async Task<BitmapImageSource> BitmapImageSourceConvertAsync(WriteableBitmap wb)
        {
            return new BitmapImageSource(wb.AsBitmap());
        }

        private static async Task<WriteableBitmap> ApplyCropAsync(WriteableBitmap wb, Rect cropRect)
        {
            Exception exception = null;

            try
            {
                App.ViewModel.IsBusy = true;
                App.ViewModel.IsBusyMessage = "cropping photo...";

                var bitmapSource = new BitmapImageSource(wb.AsBitmap());
                //var bitmapSource = await App.ViewModel.BitmapImageSourceConvertAsync(wb);
                var outputWB = new WriteableBitmap((int) App.ViewModel.MeTileSize.Width, (int) App.ViewModel.MeTileSize.Height);

                using(var source = bitmapSource)
                {
                    using(var filters = new FilterEffect(source))
                    {
                        filters.Filters = new IFilter[] { new CropFilter(cropRect) };

                        using(var renderer = new WriteableBitmapRenderer(filters, outputWB))
                        {
                            App.ViewModel.IsBusyMessage = "rendering...";
                            await renderer.RenderAsync();
                        }
                    }
                }

                App.ViewModel.IsBusy = false;
                App.ViewModel.IsBusyMessage = "";

                return outputWB;
            }
            catch(Exception ex)
            {
                exception = ex;
                App.ViewModel.IsBusy = false;
                App.ViewModel.IsBusyMessage = "";
            }

            if(exception != null)
            {
                await new MessageDialog("Cropping Error: " + exception.Message).ShowAsync();
                return null;
            }

            return null;
        }

        private static async Task<WriteableBitmap> ApplyCropAsync(ConsolidatedImageInfo info, Rect cropRect)
        {
            Exception exception = null;

            try
            {
                App.ViewModel.IsBusy = true;
                App.ViewModel.IsBusyMessage = "cropping photo...";

                MemoryStream memStream = null;

                var handler = new HttpClientHandler();
                if(handler.SupportsAutomaticDecompression)
                    handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

                var client = new HttpClient(handler);
                var stream = await client.GetStreamAsync(info.Url);

                //seekable stream
                memStream = new MemoryStream();
                await stream.CopyToAsync(memStream);
                memStream.Seek(0, SeekOrigin.Begin);

                var outputWB = new WriteableBitmap((int) App.ViewModel.MeTileSize.Width, (int) App.ViewModel.MeTileSize.Height);

                var stringImageformat = GetImageFormatType(App.ViewModel.SelectedFlickrPhoto.OriginalFormat);

                using(var source = new RandomAccessStreamImageSource(memStream.AsRandomAccessStream(), stringImageformat))
                {
                    using(var filters = new FilterEffect(source))
                    {
                        filters.Filters = new IFilter[] { new CropFilter(cropRect) };

                        using(var renderer = new WriteableBitmapRenderer(filters, outputWB))
                        {
                            await renderer.RenderAsync();
                        }
                    }
                }

                App.ViewModel.IsBusy = false;
                App.ViewModel.IsBusyMessage = "";

                return outputWB;
            }
            catch(Exception ex)
            {
                exception = ex;
                App.ViewModel.IsBusy = false;
                App.ViewModel.IsBusyMessage = "";
            }

            if(exception != null)
            {
                await new MessageDialog("Cropping Error: " + exception.Message).ShowAsync();
                return null;
            }

            return null;
        }

        public static async Task<WriteableBitmap> ApplyFilterAsync(IFilter[] filterArray, WriteableBitmap wb)
        {
            Exception exception = null;

            try
            {
                App.ViewModel.IsBusyMessage = "scanning bitmap...";

                var bitmapSource = new BitmapImageSource(wb.AsBitmap());

                using(var source = bitmapSource)
                {
                    using(var filters = new FilterEffect(source))
                    {
                        filters.Filters = filterArray;
                        var target = new WriteableBitmap((int)App.ViewModel.MeTileSize.Width, (int)App.ViewModel.MeTileSize.Height);

                        using(var renderer = new WriteableBitmapRenderer(filters, target))
                        {
                            App.ViewModel.IsBusyMessage = "rendering...";

                            await renderer.RenderAsync();

                            App.ViewModel.IsBusyMessage = "";
                            App.ViewModel.IsBusy = false;
                            return target;
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                exception = ex;
                App.ViewModel.IsBusy = false;
                App.ViewModel.IsBusyMessage = "";
            }

            if(exception != null)
            {
                await new MessageDialog("Apply filter task error: " + exception.Message).ShowAsync();
                return null;
            }

            return null;
        }

        public static async Task<WriteableBitmap> ApplySelectedFilterAsync(WriteableBitmap sourceBitmap, string selection)
        {
            if(string.IsNullOrEmpty(selection) || sourceBitmap == null) return null;

            WriteableBitmap filterAppliedBitmap = null;

            switch(selection)
            {
                case "Cartoon":
                    filterAppliedBitmap = await ApplyFilterAsync( new IFilter[] { new CartoonFilter(false) },sourceBitmap);
                    break;
                case "Sepia":
                    filterAppliedBitmap = await ApplyFilterAsync(new IFilter[] { new SepiaFilter() },sourceBitmap);
                    break;
                case "Grayscale":
                    filterAppliedBitmap = await ApplyFilterAsync(new IFilter[] { new GrayscaleFilter() },sourceBitmap);
                    break;
                case "Lomo":
                    filterAppliedBitmap = await ApplyFilterAsync(new IFilter[] { new LomoFilter(0.5, 0.5, LomoVignetting.High, LomoStyle.Yellow) }, sourceBitmap);
                    break;
                case "Antique":
                    filterAppliedBitmap = await ApplyFilterAsync(new IFilter[] { new AntiqueFilter() }, sourceBitmap);
                    break;
                case "Auto Levels":
                    filterAppliedBitmap = await ApplyFilterAsync(new IFilter[] { new AutoLevelsFilter() },sourceBitmap);
                    break;
                case "Color Boost":
                    filterAppliedBitmap = await ApplyFilterAsync(new IFilter[] { new ColorBoostFilter(5) },sourceBitmap);
                    break;
                case "Magic Pen":
                    filterAppliedBitmap = await ApplyFilterAsync(new IFilter[] { new MagicPenFilter() },sourceBitmap);
                    break;
                case "Negative":
                    filterAppliedBitmap = await ApplyFilterAsync(new IFilter[] { new NegativeFilter() },sourceBitmap);
                    break;
                case "Watercolor":
                    filterAppliedBitmap = await ApplyFilterAsync(new IFilter[] { new WatercolorFilter() },sourceBitmap);
                    break;
                case "Temp and Tint":
                    filterAppliedBitmap = await ApplyFilterAsync(new IFilter[] { new TemperatureAndTintFilter(0.6, 0) }, sourceBitmap);
                    break;
                case "Hue Saturation":
                    filterAppliedBitmap = await ApplyFilterAsync(new IFilter[] { new HueSaturationFilter(0.9, 0.8) }, sourceBitmap);
                    break;
                case "Emboss":
                    filterAppliedBitmap = await ApplyFilterAsync(new IFilter[] { new EmbossFilter(1.0) },sourceBitmap);
                    break;
                case "Fog":
                    filterAppliedBitmap = await ApplyFilterAsync(new IFilter[] { new FogFilter() },sourceBitmap);
                    break;
                case "Mirror":
                    filterAppliedBitmap = await ApplyFilterAsync(new IFilter[] { new MirrorFilter() },sourceBitmap);
                    break;
                case "Moonlight":
                    filterAppliedBitmap = await ApplyFilterAsync(new IFilter[] { new MoonlightFilter() },sourceBitmap);
                    break;
                case "Posterize":
                    filterAppliedBitmap = await ApplyFilterAsync(new IFilter[] { new PosterizeFilter() },sourceBitmap);
                    break;
                case "Sketch":
                    filterAppliedBitmap = await ApplyFilterAsync(new IFilter[] { new SketchFilter() },sourceBitmap);
                    break;
                case "Solarize":
                    filterAppliedBitmap = await ApplyFilterAsync(new IFilter[] { new SolarizeFilter() },sourceBitmap);
                    break;
                case "Blur":
                    var blurHeight = App.ViewModel.MeTileSize.Height / 2 + 5;
                    var yOffset = App.ViewModel.MeTileSize.Height - blurHeight;
                    var blurRect = new Rect(0, yOffset, 210, blurHeight);
                    filterAppliedBitmap = await ApplyFilterAsync(new IFilter[] { new BlurFilter(5, blurRect, BlurRegionShape.Rectangular) },sourceBitmap);
                    break;
                case "Despeckle":
                    filterAppliedBitmap = await ApplyFilterAsync(new IFilter[] { new DespeckleFilter() },sourceBitmap);
                    break;
                case "Gaussian Noise":
                    filterAppliedBitmap = await ApplyFilterAsync(new IFilter[] { new GaussianNoiseFilter() },sourceBitmap);
                    break;
                case "Local Boost":
                    filterAppliedBitmap = await ApplyFilterAsync(new IFilter[] { new LocalBoostAutomaticFilter() },sourceBitmap);
                    break;
                case "Sharpness":
                    filterAppliedBitmap = await ApplyFilterAsync(new IFilter[] { new SharpnessFilter() },sourceBitmap);
                    break;
                case "Vibrance":
                    filterAppliedBitmap = await ApplyFilterAsync(new IFilter[] { new VibranceFilter() },sourceBitmap);
                    break;
                case "White Balance":
                    filterAppliedBitmap = await ApplyFilterAsync(new IFilter[] { new WhiteBalanceFilter() },sourceBitmap);
                    break;
                default:
                    filterAppliedBitmap = await ApplyFilterAsync(new IFilter[] { new LomoFilter() },sourceBitmap);
                    break;
            }

            return filterAppliedBitmap;
        }

        public static async Task<WriteableBitmap> DownloadAndCropAsync(Photo photo)
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
                                            "Tip: you can add the photo to your Favorites and try at another time.")
                        .ShowAsync();
                    return null;
                }

                var handler = new HttpClientHandler();
                if (handler.SupportsAutomaticDecompression)
                    handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

                using (var client = new HttpClient(handler))
                using (var stream = await client.GetStreamAsync(imageInfo.Url))
                using (var memStream = new MemoryStream())
                {
                    await stream.CopyToAsync(memStream);
                    memStream.Position = 0;

                    App.ViewModel.IsBusyMessage = "cropping photo...";

                    var outputWriteableBitmap = new WriteableBitmap((int) App.ViewModel.MeTileSize.Width, (int) App.ViewModel.MeTileSize.Height);

                    using (var source = new RandomAccessStreamImageSource(memStream.AsRandomAccessStream(), ImageFormat.Jpeg))
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
            finally
            {
                App.ViewModel.IsBusyMessage = "";
                App.ViewModel.IsBusy = false;
            }
        }

    }
}
