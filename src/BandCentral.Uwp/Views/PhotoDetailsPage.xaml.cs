// Lance McCarthy 2013-2023 MIT
// Free to use, maintain attribution to original
// https://github.com/LanceMcCarthy/Lancelot.AwesomeBandBackgrounds

using BandCentral.Models.Common;
using BandCentral.Models.Helpers;
using BandCentral.Uwp.Common;
using Lumia.Imaging;
using Lumia.Imaging.Transforms;
using Microsoft.HockeyApp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Foundation.Metadata;
using Windows.Graphics.Display;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Input;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Photo = FlickrNet.Photo;
using Size = Windows.Foundation.Size;

namespace BandCentral.Uwp.Views
{
    public sealed partial class PhotoDetailsPage : Page
    {
        public static readonly DependencyProperty CropRectProperty = DependencyProperty.Register(
            nameof(CropRect), typeof(Rect), typeof(PhotoDetailsPage), new PropertyMetadata(Rect.Empty));

        public Rect CropRect
        {
            get => (Rect)this.GetValue(CropRectProperty);
            set => this.SetValue(CropRectProperty, value);
        }

        private bool dragTutorialShown = false;
        public bool DragTutorialShown
        {
            get
            {
                if (localSettings.Values.TryGetValue("DragTutorialShown", out var obj))
                {
                    dragTutorialShown = (bool)obj;
                }

                return dragTutorialShown;
            }
            set
            {
                dragTutorialShown = value;
                localSettings.Values["DragTutorialShown"] = value;
            }
        }

        private ApplicationDataContainer localSettings;
        private ConsolidatedImageInfo imageInfo = null;
        private WriteableBitmap baseWriteableBitmap;
        private WriteableBitmap filterAppliedBitmap;
        private WriteableBitmap previewWriteableBitmap = null;
        private GestureRecognizer gestureRecognizer = null;
        //private FilterEffect effectsSource = null;
        //private double maxPositionY;

        private bool isLoaded;
        private Size loadedSize;

        public PhotoDetailsPage()
        {
            this.InitializeComponent();
            DataContext = App.ViewModel;

            if (DesignMode.DesignModeEnabled)
                return;

            HockeyClient.Current.TrackPageView("PhotoDetailsPage");

            localSettings = ApplicationData.Current.LocalSettings;

            DisplayInformation.AutoRotationPreferences = DisplayOrientations.Portrait;
            
            if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                var statusBar = StatusBar.GetForCurrentView();
                statusBar.BackgroundOpacity = 1;
                statusBar.BackgroundColor = ((SolidColorBrush)Application.Current.Resources["BlueAgainGrayBrush"]).Color;
                statusBar.ForegroundColor = new SolidColorBrush(Colors.White).Color;
            }

            gestureRecognizer = new GestureRecognizer();
            gestureRecognizer.GestureSettings = GestureSettings.ManipulationTranslateRailsY;
            gestureRecognizer.ManipulationStarted += gestureRecognizer_ManipulationStarted;
            gestureRecognizer.ManipulationUpdated += gestureRecognizer_ManipulationUpdated;
            gestureRecognizer.ManipulationCompleted += gestureRecognizer_ManipulationCompleted;

            BaseCanvas.PointerPressed += BaseCanvas_PointerPressed;
            BaseCanvas.PointerReleased += BaseCanvas_PointerReleased;
            BaseCanvas.PointerMoved += BaseCanvas_PointerMoved;

            this.Loaded += PhotoDetailsPage_Loaded;
        }
        
        #region Button and ListItem click events

        private void UndoButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (filterAppliedBitmap != null && previewWriteableBitmap != null)
            {
                filterAppliedBitmap = null;
                PreviewImage.Source = previewWriteableBitmap;
                return;
            }

            if (previewWriteableBitmap != null && filterAppliedBitmap == null)
            {
                previewWriteableBitmap = null;
                BandPreviewGrid.Visibility = Visibility.Collapsed;
                return;
            }

            filterAppliedBitmap = null;
            previewWriteableBitmap = null;
            BandPreviewGrid.Visibility = Visibility.Collapsed;
        }

        private async void EffectsListView_OnItemClick(object sender, ItemClickEventArgs e)
        {
            AddEffectsButton.Flyout?.Hide();

            var item = e.ClickedItem as FilterListItem;
            if (item == null) return;
            if (string.IsNullOrEmpty(item.Title))
            {
                await new MessageDialog("the selected filter could not be determined, please try again").ShowAsync();
                return;
            }

            //await ApplySelectedFilterAsync(item.Title); //moved method to helper
            filterAppliedBitmap = await FilterEffectsHelper.ApplySelectedFilterAsync(previewWriteableBitmap, item.Title);
            PreviewImage.Source = filterAppliedBitmap;

            var metrics = new Dictionary<string, string>();
            metrics.Add("FilterName", item.Title);
            metrics.Add("PageUsedOn", "PhotoDetailsPage");

            HockeyClient.Current.TrackEvent("FilterApplied", metrics);
        }

        private async void CropImageButton_Click(object sender, RoutedEventArgs e)
        {
            if (baseWriteableBitmap == null) return;
            previewWriteableBitmap = await ApplyCropAsync(this.CropRect);
            PreviewImage.Source = previewWriteableBitmap;
            BandPreviewGrid.Visibility = Visibility.Visible;
        }

        private async void SendToBandButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (filterAppliedBitmap != null)
            {
                await SetBandImageAsync(filterAppliedBitmap);
                return;
            }
            if (previewWriteableBitmap != null)
            {
                await SetBandImageAsync(previewWriteableBitmap);
            }
        }

        #endregion

        #region image processing

        private async Task<WriteableBitmap> DownloadImageAsync(Photo photo)
        {
            App.ViewModel.IsBusy = true;
            App.ViewModel.IsBusyMessage = "loading high-res version of your selection...";

            try
            {
                //original 8.1 way
                //imageInfo = await Helpers.GetPhotoInfoAsync(photo, (int)App.ViewModel.WindowBounds.Width);

                //updated for UWP in a splitView TODO may need to update with dynamic sizing option
                imageInfo = FlickrHelpers.GetPhotoInfo(photo, (int)loadedSize.Width);

                if (imageInfo == null)
                {
                    await new MessageDialog("Something went wrong getting the photo. Try again.").ShowAsync();
                    Frame.GoBack();
                    return null;
                }
                else if (string.IsNullOrEmpty(imageInfo.Url))
                {
                    await new MessageDialog("The photo's links are invalid, try a different photo.\r\n\n" +
                                            "Tip: you can add the photo to your Favorites and try at another time.")
                        .ShowAsync();
                    Frame.GoBack();
                }

                var handler = new HttpClientHandler();
                if (handler.SupportsAutomaticDecompression)
                    handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

                var client = new HttpClient(handler);

                using(var stream = await client.GetStreamAsync(imageInfo?.Url))
                using(var memStream = new MemoryStream())
                {
                    //seekable stream
                    await stream.CopyToAsync(memStream);
                    memStream.Seek(0, SeekOrigin.Begin);

                    var downloadedImageWbm = new WriteableBitmap(
                        (int)imageInfo.OriginalSize.Width,
                        (int)imageInfo.OriginalSize.Height); //use explicitly set size from flickr

                    var tempWbm = await downloadedImageWbm.FromStream(memStream); //YAY Working!

                    //old 8.1 way
                    //return AspectRatioResizeAsync(tempWbm, (int)CropRect.Width, (int)App.ViewModel.WindowBounds.Height);

                    //updated for UWP in a splitView TODO may need to update with dynamic sizing option
                    return AspectRatioResizeAsync(tempWbm, (int)CropRect.Width, (int)loadedSize.Height);
                }
            }
            catch (Exception ex)
            {
                await new MessageDialog("Downloading Error: " + ex.Message).ShowAsync();
                return null;
            }
            finally
            {
                App.ViewModel.IsBusy = false;
                App.ViewModel.IsBusyMessage = "";
            }
        }

        private static WriteableBitmap AspectRatioResizeAsync(WriteableBitmap originalBitmap, int currentScreenWidth, int maxHeight)
        {
            //if image is not wider than screen, just use original
            if (originalBitmap.PixelWidth <= currentScreenWidth) return originalBitmap;

            var newHeight = GetHeightForScreenWidth(originalBitmap.PixelHeight,
                originalBitmap.PixelWidth, currentScreenWidth);

            //if(newHeight > maxHeight)
            //{
            //    //TODO determine if using ScrollViewer messes up Crop coordinates,if I cant use scrollviewer, might need to resize for height
            //    currentScreenWidth = originalBitmap.PixelWidth * maxHeight / originalBitmap.PixelHeight;
            //    newHeight = maxHeight;
            //}

            return originalBitmap.Resize(currentScreenWidth, newHeight, WriteableBitmapExtensions.Interpolation.Bilinear);
        }

        private static int GetHeightForScreenWidth(int pixelHeight, int pixelWidth, int screenWidth)
        {
            return pixelHeight * screenWidth / pixelWidth;
        }

        private async Task<WriteableBitmap> ApplyCropAsync(Rect cropRectangleGeometryFromUI)
        {
            try
            {
                App.ViewModel.IsBusy = true;
                App.ViewModel.IsBusyMessage = "cropping photo...";

                //----------- Size Calculations ---------//

                //calculate the proper height for the cropping bounds based on the bitmap, not the screen
                //var cropSize = new Size(baseWriteableBitmap.PixelWidth, Math.Floor(baseWriteableBitmap.PixelWidth*0.329));
                var cropSize = new Size(baseWriteableBitmap.PixelWidth, Math.Floor(baseWriteableBitmap.PixelWidth * App.ViewModel.TileAspectRatio));

                var pixelFactor = baseWriteableBitmap.PixelHeight / OriginalImage.ActualHeight;
                //get the ratio of real pixels to logical pixels
                var finalY = Math.Floor(pixelFactor * cropRectangleGeometryFromUI.Y);
                //get the true Y value in relation to the bitmap

                //calculation to determine if crop extends the image boundaries
                if ((cropRectangleGeometryFromUI.Y + cropRectangleGeometryFromUI.Height) * pixelFactor >=
                    pixelFactor * OriginalImage.ActualHeight)
                {
                    //trim a pixel off the Y value to bring back in the bitmap's bounds
                    finalY = finalY - 1;
                }

                //create the final cropping rect based on the bitmap's dimensions and the UI rectangle
                var croppingBounds = new Rect
                {
                    X = cropRectangleGeometryFromUI.X,
                    //UI rectangle's X (should alway be zero until I implement zooming)
                    Y = finalY, //calculated Y (accounting for difference between displayed pixels and actual pixels)
                    Width = cropSize.Width, //width of the source image
                    Height = cropSize.Height //caluclated height based on the width (has a fixed ratio [width x 0.329])
                };

                //instantiate the target bitmap to write the filter output to
                //var outputWB = new WriteableBitmap((int)Math.Ceiling(cropSize.Width), (int)Math.Ceiling(cropSize.Height)); //attempting with actual rect's dimensions (if fail go back to 310x102)
                var outputWB = new WriteableBitmap((int)App.ViewModel.MeTileSize.Width, (int)App.ViewModel.MeTileSize.Height);

                Debug.WriteLine($"UI's RectangleGeometry dimensions - cropRectangleGeometryFromUI: {cropRectangleGeometryFromUI}");
                Debug.WriteLine($"CropRect based on WriteableBitmap dimensions- cropSize: {cropSize}");
                Debug.WriteLine($"Image Control: ActualWidth={OriginalImage.ActualWidth} ActualHeight={OriginalImage.ActualHeight}");

                //----------- image processing ---------//
                //converter to the proper type for Imaging SDK
                var bitmapSource = await FilterEffectsHelper.BitmapImageSourceConvertAsync(baseWriteableBitmap);

                using (var source = bitmapSource)
                using (var filters = new CropEffect(source, croppingBounds))
                using (var renderer = new WriteableBitmapRenderer(filters, outputWB))
                {
                    App.ViewModel.IsBusyMessage = "rendering...";
                    await renderer.RenderAsync();
                }

                return outputWB;
            }
            catch (Exception ex)
            {
                await new MessageDialog($"Cropping Error: {ex.Message}\r\n\nThis can happen if the crop is at the bottom of the photo.\r\n\nTry again with the cropping rectangle slightly higher.").ShowAsync();
                return null;
            }
            finally
            {
                App.ViewModel.IsBusy = false;
                App.ViewModel.IsBusyMessage = "";
            }
        }

        private static async Task<bool> SetBandImageAsync(WriteableBitmap outputWb)
        {
            if (outputWb == null) return false;

            App.ViewModel.IsBusy = true;

            try
            {
                await App.ViewModel.SetMeTileAsync(outputWb);
                return true;
            }
            catch (Exception ex)
            {
                await new MessageDialog($"Sorry, there was a problem sending image to the Microsoft Band: {ex.Message}").ShowAsync();
                return false;
            }
            finally
            {
                App.ViewModel.IsBusy = false;
                App.ViewModel.IsBusyMessage = "";
            }
        }

        #endregion

        #region NavigationHelper registration

        private async void PhotoDetailsPage_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (isLoaded)
                    return;

                //original 8.1 way
                //this.CropRect = new Rect(0, 0, App.ViewModel.WindowBounds.Width, Math.Floor(App.ViewModel.WindowBounds.Width * 0.329) - 2);

                //updated for UWP
                //this.CropRect = new Rect(0, 0, App.ViewModel.WindowBounds.Width,
                //    Math.Floor(App.ViewModel.WindowBounds.Width * App.ViewModel.TileAspectRatio) - 2);

                //updated for UWP use in a SplitView TODO may need to update with dynamic sizing option
                loadedSize = new Size(ContentGrid.ActualWidth, ContentGrid.ActualHeight);
                this.CropRect = new Rect(0, 0, loadedSize.Width,
                    Math.Floor(loadedSize.Width * App.ViewModel.TileAspectRatio) - 2);

                Debug.WriteLine("CropRect Set: {0}", CropRect); //subtract 2 pixels to show the sides of the path

                BaseCanvas.Width = CropRect.Width;
                OriginalImage.Width = CropRect.Width;

                baseWriteableBitmap = await DownloadImageAsync(App.ViewModel.SelectedFlickrPhoto);

                if (baseWriteableBitmap == null)
                {
                    isLoaded = false;
                    if(Frame.CanGoBack)
                        Frame.GoBack();
                }

                OriginalImage.Source = baseWriteableBitmap; //set downloaded image to the UI

                CropToSelectionButton.IsEnabled = true; //allow cropping

                isLoaded = true;
            }
            catch (Exception ex)
            {
                await new MessageDialog("There was a problem loading the image: " + ex.Message).ShowAsync();
                isLoaded = false;
                Frame.GoBack();
            }
            finally
            {
                CheckTutorial();
            }
        }
        
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            
            gestureRecognizer = null;
        }

        private void CheckTutorial()
        {
            if (DragTutorialShown)
                return;

            FingerTutorialStoryboard.Begin();

            DragTutorialShown = true;
        }

        #endregion

        #region manipulation events

        void BaseCanvas_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (e == null) return;

            BaseCanvas.CapturePointer(e.Pointer);
            gestureRecognizer.ProcessDownEvent(e.GetCurrentPoint(BaseCanvas));
            e.Handled = true;
        }

        void BaseCanvas_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (!gestureRecognizer.IsActive) return;
            gestureRecognizer.ProcessMoveEvents(e.GetIntermediatePoints(BaseCanvas));
            e.Handled = true;
        }

        void BaseCanvas_PointerReleased(object sender, PointerRoutedEventArgs e) //kevin
        {
            if (e == null) return;
            gestureRecognizer.ProcessUpEvent(e.GetCurrentPoint(BaseCanvas));
            e.Handled = true;

            BaseCanvas.ReleasePointerCapture(e.Pointer);
        }

        //gesture events
        void gestureRecognizer_ManipulationStarted(GestureRecognizer sender, ManipulationStartedEventArgs args)
        {
            //Debug.WriteLine("------------------Started-- Cumulative Y: {0} Current Position Y: {1}", args.Cumulative.Translation.Y, args.Position.Y);
        }

        void gestureRecognizer_ManipulationUpdated(GestureRecognizer sender, ManipulationUpdatedEventArgs args)
        {
            if (args == null) return;
            UpdateRectangleGeometryAsync(args.Cumulative.Translation.Y, args.Position.Y);
        }

        void gestureRecognizer_ManipulationCompleted(GestureRecognizer sender, ManipulationCompletedEventArgs args)
        {
            //Debug.WriteLine("------------------Complete: Cumulative Y: {0} Current Position Y: {1}", args.Cumulative.Translation.Y, args.Position.Y);
        }

        private void UpdateRectangleGeometryAsync(double totalPixelsMovedInY, double actualPositionY)
        {
            //NOTE Kevin's work
            var fingerPosition = Math.Floor(actualPositionY);
            var adjustedPositon = fingerPosition - CropRect.Height / 2;

            if (adjustedPositon < 0)
            {
                adjustedPositon = 0;
            }
            else if (adjustedPositon > OriginalImage.ActualHeight - CropRect.Height)
            {
                adjustedPositon = OriginalImage.ActualHeight - CropRect.Height;
            }

            this.CropRect = new Rect(CropRect.Left, adjustedPositon, CropRect.Width, CropRect.Height);
        }

        #endregion

        private async void SaveButton_OnClick(object sender, RoutedEventArgs e)
        {
            WriteableBitmap bitmapToSave = null;

            if (filterAppliedBitmap != null)
            {
                bitmapToSave = filterAppliedBitmap;
            }
            else if (previewWriteableBitmap != null)
            {
                bitmapToSave = previewWriteableBitmap;
            }

            if(bitmapToSave != null)
                await bitmapToSave.SaveToJpgFileAsync($"{App.ViewModel.SelectedFlickrPhoto.PhotoId}.jpg", ApplicationData.Current.LocalFolder);
        }
    }
}
