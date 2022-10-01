using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace BandCentral.Models.Helpers
{
    public static class ImageHelpers
    {
        /// <summary>
        /// Saves a WriteableBitmap to a file
        /// </summary>
        /// <param name="writeableBitmap">Bitmap to save</param>
        /// <param name="fileName">Full file name WITH extension</param>
        /// <param name="folder">Folder to save the file to</param>
        /// <returns>StorageFile object of the saved image</returns>
        public static async Task<StorageFile> SaveToJpgFileAsync(this WriteableBitmap writeableBitmap, string fileName, StorageFolder folder)
        {
            Guid bitmapEncoderGuid = BitmapEncoder.JpegEncoderId;

            var file = await folder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);

            using (IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.ReadWrite))
            {
                BitmapEncoder encoder = await BitmapEncoder.CreateAsync(bitmapEncoderGuid, stream);
                Stream pixelStream = writeableBitmap.PixelBuffer.AsStream();
                byte[] pixels = new byte[pixelStream.Length];
                await pixelStream.ReadAsync(pixels, 0, pixels.Length);
                encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore, (uint) writeableBitmap.PixelWidth, (uint) writeableBitmap.PixelHeight, 96.0, 96.0, pixels);

                await encoder.FlushAsync();
            }

            Debug.WriteLine($"---ImageHelpers.SaveToJpgFileAsync {fileName} saved to {file.Path}");

            return file;
        }
    }
}
