using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Media;
using BandCentral.Uwp.UI;

namespace BandCentral.Uwp.Controls
{
    public sealed class CompositionImage : Control
    {
        private bool unloaded;
        private readonly Compositor compositor;
        private SpriteVisual sprite;
        private Uri uri;
        private CompositionDrawingSurface surface;
        private readonly CompositionSurfaceBrush surfaceBrush;
        private CompositionBrush brush;
        private CompositionStretch stretchMode;
        private DispatcherTimer timer;
        public event RoutedEventHandler ImageOpened;
        public event RoutedEventHandler ImageFailed;
        private CompositionBrush placeholderBrush;

        private static CompositionBrush _defaultPlaceholderBrush;
        private static ScalarKeyFrameAnimation _fadeOutAnimation;
        private static bool _staticsInitialized;

        public CompositionImage()
        {
            this.DefaultStyleKey = typeof(CompositionImage);
            this.Background = new SolidColorBrush(Colors.Transparent);
            this.stretchMode = CompositionStretch.Uniform;
            this.Loading += CompImage_Loading;
            this.Unloaded += CompImage_Unloaded;
            this.SizeChanged += CompImage_SizeChanged;

            compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;

            // Intialize the statics as needed
            if (!_staticsInitialized)
            {
                _defaultPlaceholderBrush = compositor.CreateColorBrush(Colors.DarkGray);

                TimeSpan duration = TimeSpan.FromMilliseconds(1000);
                _fadeOutAnimation = compositor.CreateScalarKeyFrameAnimation();
                _fadeOutAnimation.InsertKeyFrame(0, 1);
                _fadeOutAnimation.InsertKeyFrame(1, 0);
                _fadeOutAnimation.Duration = duration;

                var scaleAnimation = compositor.CreateVector2KeyFrameAnimation();
                scaleAnimation.InsertKeyFrame(0, new Vector2(1.25f, 1.25f));
                scaleAnimation.InsertKeyFrame(1, new Vector2(1, 1));
                scaleAnimation.Duration = duration;

                _staticsInitialized = true;
            }

            // Initialize the surface loader if needed
            if (!SurfaceLoader.IsInitialized)
            {
                SurfaceLoader.Initialize(ElementCompositionPreview.GetElementVisual(this).Compositor);
            }

            PlaceholderDelay = TimeSpan.FromMilliseconds(50);
            surfaceBrush = compositor.CreateSurfaceBrush(null);
        }
        
        public Stretch Stretch
        {
            get
            {
                Stretch stretch;

                switch (stretchMode)
                {
                    case CompositionStretch.Fill:
                        stretch = Stretch.Fill;
                        break;
                    case CompositionStretch.Uniform:
                        stretch = Stretch.Uniform;
                        break;
                    case CompositionStretch.UniformToFill:
                        stretch = Stretch.UniformToFill;
                        break;
                    case CompositionStretch.None:
                    default:
                        stretch = Stretch.None;
                        break;
                }

                return stretch;
            }

            set
            {
                CompositionStretch stretch;

                switch (value)
                {
                    case Stretch.Fill:
                        stretch = CompositionStretch.Fill;
                        break;
                    case Stretch.Uniform:
                        stretch = CompositionStretch.Uniform;
                        break;
                    case Stretch.UniformToFill:
                        stretch = CompositionStretch.UniformToFill;
                        break;
                    case Stretch.None:
                    default:
                        stretch = CompositionStretch.None;
                        break;
                }

                if (stretch != stretchMode)
                {
                    stretchMode = stretch;

                    if (surfaceBrush != null)
                    {
                        surfaceBrush.Stretch = stretch;
                    }
                }
            }
        }

        public Uri Source
        {
            get => uri;
            set
            {
                if (uri != value)
                {
                    uri = value;
                    LoadSurface();
                }
            }
        }

        public bool IsContentLoaded => surface != null;

        public bool SharedSurface { get; set; }

        public LoadTimeEffectHandler LoadTimeEffectHandler { get; set; }

        public CompositionBrush Brush
        {
            get => brush;
            set
            {
                brush = value;
                UpdateBrush();
            }
        }

        public CompositionBrush PlaceholderBrush
        {
            get => placeholderBrush;
            set
            {
                placeholderBrush = value;

                if (sprite != null)
                {
                    // Update the loading sprite if set
                    SpriteVisual loadingSprite = (SpriteVisual) sprite.Children.FirstOrDefault();
                    if (loadingSprite != null)
                    {
                        loadingSprite.Brush = placeholderBrush;
                    }
                }
            }
        }

        public CompositionSurfaceBrush SurfaceBrush => surfaceBrush;

        public SpriteVisual SpriteVisual => sprite;

        public TimeSpan PlaceholderDelay { get; set; }
        
        private void UpdateBrush()
        {
            surfaceBrush.Surface = surface;
            surfaceBrush.Stretch = stretchMode;

            if (sprite != null)
            {
                // If the active brush is not set, use the surface brush
                if (brush != null)
                {
                    if (brush is CompositionEffectBrush)
                    {
                        //
                        // If there is an EffectBrush set, it must supply ImageSource reference parameter for setitng
                        // the Image content.
                        //

                        ((CompositionEffectBrush) brush).SetSourceParameter("ImageSource", surfaceBrush);
                    }

                    // Update the sprite to use the brush
                    sprite.Brush = brush;
                }
                else
                {
                    sprite.Brush = surfaceBrush;
                }
            }
        }

        private async void LoadSurface()
        {
            // If we're clearing out the content, return
            if (uri == null)
            {
                ReleaseSurface();
                return;
            }

            try
            {
                // Start a timer to enable the placeholder image if requested
                if (this.surface == null && PlaceholderDelay >= TimeSpan.Zero)
                {
                    timer = new DispatcherTimer
                    {
                        Interval = PlaceholderDelay
                    };
                    timer.Tick += Timer_Tick;
                    timer.Start();
                }

                // Load the image asynchronously
                CompositionDrawingSurface drawingSurface = await SurfaceLoader.LoadFromUri(uri, Size.Empty, LoadTimeEffectHandler);

                if (this.surface != null)
                {
                    ReleaseSurface();
                }

                this.surface = drawingSurface;

                // The surface has changed, so we need to re-measure with the new surface dimensions
                InvalidateMeasure();

                // Async operations may take a while.  If we've unloaded, return now.
                if (unloaded)
                {
                    ReleaseSurface();
                    return;
                }

                // Update the brush
                UpdateBrush();

                // Success, fire the Opened event
                if (ImageOpened != null)
                {
                    ImageOpened(this, null);
                }

                //
                // If we created the loading placeholder, now that the image has loaded 
                // cross-fade it out.
                //

                if (sprite != null && sprite.Children.Count > 0)
                {
                    Debug.Assert(timer == null);
                    StartCrossFade();
                }
                else if (timer != null)
                {
                    // We didn't end up loading the placeholder, so just stop the timer
                    timer.Stop();
                    timer = null;
                }
            }
            catch (FileNotFoundException)
            {
                ImageFailed?.Invoke(this, null);
            }
        }

        private void Timer_Tick(object sender, object e)
        {
            if (timer != null)
            {
                Debug.Assert(sprite.Children.Count == 0, "Should not be any children");

                // Create a second sprite to show while the image is still loading
                SpriteVisual loadingSprite = compositor.CreateSpriteVisual();
                loadingSprite = compositor.CreateSpriteVisual();
                loadingSprite.Size = new Vector2((float) ActualWidth, (float) ActualHeight);
                loadingSprite.Brush = placeholderBrush ?? _defaultPlaceholderBrush;
                sprite.Children.InsertAtTop(loadingSprite);

                // Stop and null out the time, no more need for it.
                timer.Stop();
                timer = null;
            }
        }

        private void StartCrossFade()
        {
            Debug.Assert(sprite.Children.Count > 0, "Unexpected number of children");

            // Start a batch so we can cleanup the loading sprite
            CompositionScopedBatch batch = compositor.CreateScopedBatch(CompositionBatchTypes.Animation);
            batch.Completed += EndCrossFade;

            // Animate the opacity of the loading sprite to fade it out and the texture scale just for effect
            Visual loadingVisual = sprite.Children.LastOrDefault();
            loadingVisual.StartAnimation("Opacity", _fadeOutAnimation);

#if SDKVERSION_INSIDER
            _surfaceBrush.StartAnimation("Scale", _scaleAnimation);
            _surfaceBrush.CenterPoint = new Vector2((float)_surface.Size.Width *.5f, (float)_surface.Size.Height * .5f);
#endif
            // End the batch after those animations complete
            batch.End();
        }

        private void EndCrossFade(object sender, CompositionBatchCompletedEventArgs args)
        {
            // If the sprite is still valid, remove the loading sprite from the children collection
            if (sprite != null && sprite.Children.Count > 0)
            {
                sprite.Children.RemoveAll();
            }
        }

        private void ReleaseSurface()
        {
            if (surface != null)
            {
                // If no one has asked to share, dispose it to free the memory
                if (!SharedSurface)
                {
                    surface.Dispose();
                    surfaceBrush.Surface = null;
                }
                surface = null;
            }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            Size desiredSize = new Size(0, 0);

            // We override measure to implement similar semantics to the normal XAML Image UIElement
            if (surface != null)
            {
                Size scaling = new Size(1, 1);
                Size imageSize = surface.Size;

                // If we're not stretching or have infinite space, request the full surface size
                if (!(double.IsInfinity(availableSize.Width) && double.IsInfinity(availableSize.Height)) &&
                    stretchMode != CompositionStretch.None)
                {
                    // Calculate the amount of horizontal and vertical scaling to fit into available space
                    scaling = new Size(availableSize.Width / imageSize.Width, availableSize.Height / imageSize.Height);


                    //
                    // If we've got infinite space in either dimension, scale by the same amount as the constrained
                    // dimension.
                    //

                    if (double.IsInfinity(availableSize.Width))
                    {
                        scaling.Width = scaling.Height;
                    }
                    else if (double.IsInfinity(availableSize.Height))
                    {
                        scaling.Height = scaling.Width;
                    }
                    else
                    {
                        //
                        // We're fitting into a space confined by both width and height, do appropriate scaling
                        // based on the stretch mode.
                        //

                        switch (stretchMode)
                        {
                            case CompositionStretch.Uniform:
                                scaling.Width = scaling.Height = Math.Min(scaling.Width, scaling.Height);
                                break;
                            case CompositionStretch.UniformToFill:
                                scaling.Width = scaling.Height = Math.Max(scaling.Width, scaling.Height);
                                break;
                            case CompositionStretch.Fill:
                            default:
                                break;
                        }
                    }
                }

                // Apply the scale to get the final desired size
                desiredSize.Width = imageSize.Width * scaling.Width;
                desiredSize.Height = imageSize.Height * scaling.Height;
            }
            else
            {
                // We don't have any content, so default to zero unless a specific size was requested
                if (!double.IsNaN(Width))
                {
                    desiredSize.Width = Width;
                }
                if (!double.IsNaN(Height))
                {
                    desiredSize.Height = Height;
                }
            }

            return new Size(Math.Min(availableSize.Width, desiredSize.Width), Math.Min(availableSize.Height, desiredSize.Height));
        }

        private void CompImage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (sprite != null)
            {
                // Calculate the new size
                Vector2 size = new Vector2((float) ActualWidth, (float) ActualHeight);

                // Update the sprite
                sprite.Size = size;

                // Update the loading sprite if set
                Visual loadingSprite = sprite.Children.FirstOrDefault();
                if (loadingSprite != null)
                {
                    loadingSprite.Size = size;
                }
            }
        }

        private void CompImage_Loading(FrameworkElement sender, object args)
        {
            sprite = compositor.CreateSpriteVisual();
            sprite.Size = new Vector2((float) ActualWidth, (float) ActualHeight);

            // Reset the loading flag
            unloaded = false;

            // If the surface is not yet loaded, do so now
            if (!IsContentLoaded)
            {
                LoadSurface();
            }
            else
            {
                // Already had content, just update the brush
                UpdateBrush();
            }

            ElementCompositionPreview.SetElementChildVisual(this, sprite);
        }

        private void CompImage_Unloaded(object sender, RoutedEventArgs e)
        {
            unloaded = true;

            ReleaseSurface();

            if (sprite != null)
            {
                sprite.Dispose();
                sprite = null;
            }
        }
    }
}
