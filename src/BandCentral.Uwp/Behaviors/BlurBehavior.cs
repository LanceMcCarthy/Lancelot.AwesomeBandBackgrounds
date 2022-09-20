using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;
using Microsoft.Xaml.Interactivity;

namespace BandCentral.Uwp.Behaviors
{
    public class BlurBehavior : Behavior<FrameworkElement>
    {
        public static readonly DependencyProperty BlurContentProperty = DependencyProperty.Register(
            "BlurContent", typeof (UIElement), typeof (BlurBehavior), new PropertyMetadata(default(UIElement)));

        public UIElement BlurContent
        {
            get { return (UIElement) GetValue(BlurContentProperty); }
            set { SetValue(BlurContentProperty, value); }
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            ApplyBlur();
        }

        private async void ApplyBlur()
        {
            if (BlurContent == null)
                return;
            
            using (var stream = await RenderToImageStream(BlurContent))
            {
                var device = new CanvasDevice();
                var bitmap = await CanvasBitmap.LoadAsync(device, stream);

                var renderer = new CanvasRenderTarget(device,
                                                      bitmap.SizeInPixels.Width,
                                                      bitmap.SizeInPixels.Height, bitmap.Dpi);

                using (var ds = renderer.CreateDrawingSession())
                {
                    var blur = new GaussianBlurEffect();
                    blur.BlurAmount = 5.0f;
                    blur.Source = bitmap;
                    ds.DrawImage(blur);
                }

                stream.Seek(0);
                await renderer.SaveAsync(stream, CanvasBitmapFileFormat.Png);

                BitmapImage image = new BitmapImage();
                image.SetSource(stream);

                //BlurContent.Background = image;

            }


        }

        public static async Task<IRandomAccessStream> RenderToImageStream(UIElement element)
        {
            var rtb = new RenderTargetBitmap();
            await rtb.RenderAsync(element);

            var pixelBuffer = await rtb.GetPixelsAsync();
            var pixels = pixelBuffer.ToArray();
            
            var displayInformation = DisplayInformation.GetForCurrentView();

            var stream = new InMemoryRandomAccessStream();
            var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
            encoder.SetPixelData(BitmapPixelFormat.Bgra8,
                                 BitmapAlphaMode.Premultiplied,
                                 (uint) rtb.PixelWidth,
                                 (uint) rtb.PixelHeight,
                                 displayInformation.RawDpiX,
                                 displayInformation.RawDpiY,
                                 pixels);

            await encoder.FlushAsync();
            stream.Seek(0);

            return stream;
        }
    }
}
