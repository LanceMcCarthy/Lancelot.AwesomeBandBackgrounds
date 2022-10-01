using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Popups;
using Windows.UI.Xaml.Media.Imaging;
using BandCentral.Models.Common;
using Lumia.Imaging;
using Lumia.Imaging.Adjustments;
using Lumia.Imaging.Artistic;
using Lumia.Imaging.Transforms;
using Lumia.InteropServices.WindowsRuntime;
using ImageFormat = Lumia.Imaging.ImageFormat;

namespace BandCentral.Uwp.Common
{
    public static class FilterEffectsHelper
    {
        public static async Task<ObservableCollection<FilterListItem>> GetAvailableFiltersAsync()
        {
            return new ObservableCollection<FilterListItem>
                    {
                        new FilterListItem { Title = "Antique", ThumbnailPath = "Images/ImagingSDK/AntiqueFilter.jpg" },
                        new FilterListItem { Title = "Auto Levels", ThumbnailPath = "Images/ImagingSDK/AutoLevelsFilter.jpg" },
                        //new FilterListItem { Title = "Blur", ThumbnailPath = "Images/ImagingSDK/Blur.jpg" },
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
                    return ImageFormat.Jpeg;
            }
        }

        public static async Task<BitmapImageSource> BitmapImageSourceConvertAsync(WriteableBitmap wb)
        {
            return new BitmapImageSource(wb.AsBitmap());
        }

        private static async Task<WriteableBitmap> ApplyCropAsync(WriteableBitmap wb, Rect cropRect)
        {
            try
            {
                App.ViewModel.IsBusy = true;
                App.ViewModel.IsBusyMessage = "cropping photo...";

                //NOTE - getting size form VM now to support Band 2
                var size = App.ViewModel.MeTileSize;
                var outputWB = new WriteableBitmap((int)size.Width, (int)size.Height);

                using (var source = new BitmapImageSource(wb.AsBitmap()))
                using (var filters = new CropEffect(source, cropRect))
                using (var renderer = new WriteableBitmapRenderer(filters, outputWB))
                {
                    App.ViewModel.IsBusyMessage = "rendering...";
                    await renderer.RenderAsync();
                }

                App.ViewModel.IsBusy = false;
                App.ViewModel.IsBusyMessage = "";

                return outputWB;
            }
            catch (Exception ex)
            {
                await new MessageDialog("Cropping Error: " + ex.Message).ShowAsync();
                return null;
            }
            finally
            {
                App.ViewModel.IsBusy = false;
                App.ViewModel.IsBusyMessage = "";
            }
        }

        private static async Task<WriteableBitmap> ApplyCropAsync(ConsolidatedImageInfo info, Rect cropRect)
        {
            try
            {
                App.ViewModel.IsBusy = true;
                App.ViewModel.IsBusyMessage = "cropping photo...";

                MemoryStream memStream = null;

                var handler = new HttpClientHandler();
                if (handler.SupportsAutomaticDecompression)
                    handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

                var client = new HttpClient(handler);
                var stream = await client.GetStreamAsync(info.Url);

                //seekable stream
                memStream = new MemoryStream();
                await stream.CopyToAsync(memStream);
                memStream.Seek(0, SeekOrigin.Begin);

                //NOTE - getting size form VM now to support Band 2
                var size = App.ViewModel.MeTileSize;
                var outputWB = new WriteableBitmap((int)size.Width, (int)size.Height);

                using (var source = new RandomAccessStreamImageSource(memStream.AsRandomAccessStream()))
                using (var filters = new CropEffect(source, cropRect))
                using (var renderer = new WriteableBitmapRenderer(filters, outputWB))
                {
                    await renderer.RenderAsync();
                }

                App.ViewModel.IsBusy = false;
                App.ViewModel.IsBusyMessage = "";

                return outputWB;
            }
            catch (Exception ex)
            {
                await new MessageDialog($"Cropping Error: {ex.Message}").ShowAsync();
                return null;
            }
            finally
            {
                App.ViewModel.IsBusy = false;
                App.ViewModel.IsBusyMessage = "";
            }
        }

        public static async Task<WriteableBitmap> ApplyFilterAsync(EffectList filterArray, WriteableBitmap wb)
        {
            try
            {
                App.ViewModel.IsBusy = true;
                App.ViewModel.IsBusyMessage = "scanning bitmap...";

                using (var source = new BitmapImageSource(wb.AsBitmap()))
                {
                    filterArray.Source = source;

                    var size = App.ViewModel.MeTileSize;
                    var target = new WriteableBitmap((int)size.Width, (int)size.Height);

                    using (var renderer = new WriteableBitmapRenderer(filterArray, target))
                    {
                        App.ViewModel.IsBusyMessage = "rendering...";

                        await renderer.RenderAsync();

                        App.ViewModel.IsBusyMessage = "";
                        App.ViewModel.IsBusy = false;
                        return target;
                    }
                }
            }
            catch (Exception ex)
            {
                await new MessageDialog($"Apply filter task error: {ex.Message}").ShowAsync();
                return null;
            }
            finally
            {
                App.ViewModel.IsBusy = false;
                App.ViewModel.IsBusyMessage = "";
            }
        }

        public static async Task<WriteableBitmap> ApplySelectedFilterAsync(WriteableBitmap sourceBitmap, string selection)
        {
            if (string.IsNullOrEmpty(selection) || sourceBitmap == null) return null;

            WriteableBitmap filterAppliedBitmap = null;

            switch (selection)
            {
                case "Cartoon":
                    filterAppliedBitmap = await ApplyFilterAsync(new EffectList { new CartoonEffect(false) }, sourceBitmap);
                    break;
                case "Sepia":
                    filterAppliedBitmap = await ApplyFilterAsync(new EffectList { new SepiaEffect() }, sourceBitmap);
                    break;
                case "Grayscale":
                    filterAppliedBitmap = await ApplyFilterAsync(new EffectList { new GrayscaleEffect() }, sourceBitmap);
                    break;
                case "Lomo":
                    filterAppliedBitmap = await ApplyFilterAsync(new EffectList { new LomoEffect(0.5, 0.5, LomoVignetting.High, LomoStyle.Yellow) }, sourceBitmap);
                    break;
                case "Antique":
                    filterAppliedBitmap = await ApplyFilterAsync(new EffectList { new AntiqueEffect() }, sourceBitmap);
                    break;
                case "Auto Levels":
                    filterAppliedBitmap = await ApplyFilterAsync(new EffectList { new AutoLevelsEffect() }, sourceBitmap);
                    break;
                case "Color Boost":
                    filterAppliedBitmap = await ApplyFilterAsync(new EffectList { new ColorBoostEffect(5) }, sourceBitmap);
                    break;
                case "Magic Pen":
                    filterAppliedBitmap = await ApplyFilterAsync(new EffectList { new MagicPenEffect() }, sourceBitmap);
                    break;
                case "Negative":
                    filterAppliedBitmap = await ApplyFilterAsync(new EffectList { new NegativeEffect() }, sourceBitmap);
                    break;
                case "Watercolor":
                    filterAppliedBitmap = await ApplyFilterAsync(new EffectList { new WatercolorEffect() }, sourceBitmap);
                    break;
                case "Temp and Tint":
                    filterAppliedBitmap = await ApplyFilterAsync(new EffectList { new TemperatureAndTintEffect(0.6, 0) }, sourceBitmap);
                    break;
                case "Hue Saturation":
                    filterAppliedBitmap = await ApplyFilterAsync(new EffectList { new HueSaturationEffect(0.9, 0.8) }, sourceBitmap);
                    break;
                case "Emboss":
                    filterAppliedBitmap = await ApplyFilterAsync(new EffectList { new EmbossEffect(1.0) }, sourceBitmap);
                    break;
                case "Fog":
                    filterAppliedBitmap = await ApplyFilterAsync(new EffectList { new FogEffect() }, sourceBitmap);
                    break;
                case "Mirror":
                    filterAppliedBitmap = await ApplyFilterAsync(new EffectList { new MirrorEffect() }, sourceBitmap);
                    break;
                case "Moonlight":
                    filterAppliedBitmap = await ApplyFilterAsync(new EffectList { new MoonlightEffect() }, sourceBitmap);
                    break;
                case "Posterize":
                    filterAppliedBitmap = await ApplyFilterAsync(new EffectList { new PosterizeEffect() }, sourceBitmap);
                    break;
                case "Sketch":
                    filterAppliedBitmap = await ApplyFilterAsync(new EffectList { new SketchEffect() }, sourceBitmap);
                    break;
                case "Solarize":
                    filterAppliedBitmap = await ApplyFilterAsync(new EffectList { new SolarizeEffect() }, sourceBitmap);
                    break;
                //case "Blur":
                //    filterAppliedBitmap = await ApplyFilterAsync(new EffectList { new BlurEffect(10, new Rect(0, 0, 210, 102), BlurRegionShape.Rectangular) }, sourceBitmap);
                //    break;
                case "Despeckle":
                    filterAppliedBitmap = await ApplyFilterAsync(new EffectList { new DespeckleEffect() }, sourceBitmap);
                    break;
                case "Gaussian Noise":
                    filterAppliedBitmap = await ApplyFilterAsync(new EffectList { new GaussianNoiseEffect() }, sourceBitmap);
                    break;
                case "Local Boost":
                    filterAppliedBitmap = await ApplyFilterAsync(new EffectList { new LocalBoostAutomaticEffect() }, sourceBitmap);
                    break;
                case "Sharpness":
                    filterAppliedBitmap = await ApplyFilterAsync(new EffectList { new SharpnessEffect() }, sourceBitmap);
                    break;
                case "Vibrance":
                    filterAppliedBitmap = await ApplyFilterAsync(new EffectList { new VibranceEffect() }, sourceBitmap);
                    break;
                case "White Balance":
                    filterAppliedBitmap = await ApplyFilterAsync(new EffectList { new WhiteBalanceEffect() }, sourceBitmap);
                    break;
                default:
                    filterAppliedBitmap = await ApplyFilterAsync(new EffectList { new LomoEffect() }, sourceBitmap);
                    break;
            }

            return filterAppliedBitmap;
        }

    }
}
