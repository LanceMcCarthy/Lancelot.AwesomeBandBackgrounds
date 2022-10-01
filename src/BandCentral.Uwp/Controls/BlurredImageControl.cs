// Lance McCarthy 2013-2023 MIT
// Free to use, maintain attribution to original
// https://github.com/LanceMcCarthy/Lancelot.AwesomeBandBackgrounds

using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Numerics;
using Windows.Graphics.Display;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace BandCentral.Uwp.Controls
{
    public sealed class BlurredImageControl : Control
    {
        private CanvasControl control;
        private bool imageLoaded;
        private CanvasBitmap image;
        private ContentPresenter contentPresenter;
        private ScaleEffect scaleEffect;
        private GaussianBlurEffect blurEffect;

        public BlurredImageControl()
        {
            this.DefaultStyleKey = typeof(BlurredImageControl);
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            contentPresenter = GetTemplateChild("imagePresenter") as ContentPresenter;

            RenderImage();
        }

        public static readonly DependencyProperty ImageUrlProperty = DependencyProperty.Register(
            nameof(ImageUrl), typeof(string), typeof(string), new PropertyMetadata(string.Empty, SourceSet));
        
        public static readonly DependencyProperty BlurProperty = DependencyProperty.Register(
            nameof(Blur), typeof(float), typeof(BlurredImageControl), new PropertyMetadata(15.0f));
        
        public float Blur
        {
            get => (float) GetValue(BlurProperty);
            set => SetValue(BlurProperty, value);
        }

        public string ImageUrl
        {
            get => (string) GetValue(ImageUrlProperty);
            set => SetValue(ImageUrlProperty, value);
        }

        private static void SourceSet(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.NewValue.ToString()))
            {
                var control = d as BlurredImageControl;
                control?.RenderImage();
            }
        }

        private void RenderImage()
        {
            if (contentPresenter != null)
            {
                control = new CanvasControl();
                control.Draw += OnDraw;
                control.CreateResources += OnCreateResources;

                contentPresenter.Content = control;
            }
        }

        private async void OnCreateResources(CanvasControl sender, object args)
        {
            if (!string.IsNullOrWhiteSpace(ImageUrl))
            {
                scaleEffect = new ScaleEffect();
                blurEffect = new GaussianBlurEffect();

                image = await CanvasBitmap.LoadAsync(sender.Device,
                  new Uri(ImageUrl));
                
                imageLoaded = true;

                sender.Invalidate();
            }
            
        }

        private void OnDraw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            if (imageLoaded)
            {
                using (var session = args.DrawingSession)
                {
                    session.Units = CanvasUnits.Pixels;

                    double displayScaling = DisplayInformation.GetForCurrentView().LogicalDpi / 96.0;

                    double pixelWidth = sender.ActualWidth * displayScaling;

                    var scalefactor = pixelWidth / image.Size.Width;

                    scaleEffect.Source = this.image;
                    scaleEffect.Scale = new Vector2()
                    {
                        X = (float) scalefactor,
                        Y = (float) scalefactor
                    };

                    blurEffect.Source = scaleEffect;
                    blurEffect.BlurAmount = Blur;

                    session.DrawImage(blurEffect, 0.0f, 0.0f);
                }
            }
        }
    }
}
