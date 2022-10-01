// Lance McCarthy 2013-2023 MIT
// Free to use, maintain attribution to original
// https://github.com/LanceMcCarthy/Lancelot.AwesomeBandBackgrounds

using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace BandCentral.Uwp.Controls.ImageCropper.Helpers
{
    public class CropBitmap
    {
        /// <summary>
        /// Get a cropped bitmap from a image file.
        /// </summary>
        /// <param name="originalImageFile">
        /// The original image file.
        /// </param>
        /// <param name="startPoint">
        /// The start point of the region to be cropped.
        /// </param>
        /// <param name="corpSize">
        /// The size of the region to be cropped.
        /// </param>
        /// <returns>
        /// The cropped image.
        /// </returns>
        public static async Task<WriteableBitmap> GetCroppedBitmapAsync(StorageFile originalImageFile,
            Point startPoint, Size corpSize, double scale)
        {
            if (double.IsNaN(scale) || double.IsInfinity(scale))
            {
                scale = 1;
            }

            // Convert start point and size to integer.
            uint startPointX = (uint)Math.Floor(startPoint.X * scale);
            uint startPointY = (uint)Math.Floor(startPoint.Y * scale);
            uint height = (uint)Math.Floor(corpSize.Height * scale);
            uint width = (uint)Math.Floor(corpSize.Width * scale);

            using (IRandomAccessStream stream = await originalImageFile.OpenReadAsync())
            {

                // Create a decoder from the stream. With the decoder, we can get 
                // the properties of the image.
                BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);

                // The scaledSize of original image.
                uint scaledWidth = (uint)Math.Floor(decoder.PixelWidth * scale);
                uint scaledHeight = (uint)Math.Floor(decoder.PixelHeight * scale);


                // Refine the start point and the size. 
                if (startPointX + width > scaledWidth)
                {
                    startPointX = scaledWidth - width;
                }

                if (startPointY + height > scaledHeight)
                {
                    startPointY = scaledHeight - height;
                }

                // Get the cropped pixels.
                var pixels = await GetPixelData(decoder, startPointX, startPointY, width, height,
                    scaledWidth, scaledHeight);

                // Stream the bytes into a WriteableBitmap
                var cropBmp = new WriteableBitmap((int)width, (int)height);
                var pixStream = cropBmp.PixelBuffer.AsStream();

                await pixStream.WriteAsync(pixels, 0, (int)(width * height * 4));

                return cropBmp;
            }
        }

        /// <summary>
        /// Use BitmapTransform to define the region to crop, and then get the pixel data in the region
        /// </summary>
        private static async Task<byte[]> GetPixelData(BitmapDecoder decoder, uint startPointX, uint startPointY,
            uint width, uint height)
        {
            return await GetPixelData(decoder, startPointX, startPointY, width, height,
                decoder.PixelWidth, decoder.PixelHeight);
        }

        /// <summary>
        /// Use BitmapTransform to define the region to crop, and then get the pixel data in the region.
        /// If you want to get the pixel data of a scaled image, set the scaledWidth and scaledHeight
        /// of the scaled image.
        /// </summary>
        private static async Task<byte[]> GetPixelData(BitmapDecoder decoder, uint startPointX, uint startPointY,
            uint width, uint height, uint scaledWidth, uint scaledHeight)
        {
            var transform = new BitmapTransform();
            var bounds = new BitmapBounds();
            bounds.X = startPointX;
            bounds.Y = startPointY;
            bounds.Height = height;
            bounds.Width = width;
            transform.Bounds = bounds;

            transform.ScaledWidth = scaledWidth;
            transform.ScaledHeight = scaledHeight;

            // Get the cropped pixels within the bounds of transform.
            var pix = await decoder.GetPixelDataAsync(
                BitmapPixelFormat.Bgra8,
                BitmapAlphaMode.Straight,
                transform,
                ExifOrientationMode.IgnoreExifOrientation,
                ColorManagementMode.ColorManageToSRgb);
            var pixels = pix.DetachPixelData();
            return pixels;
        }
    }
}
