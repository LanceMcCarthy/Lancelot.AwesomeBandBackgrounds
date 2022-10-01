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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.Storage;
using Windows.Storage.Pickers;
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

namespace BandCentral.Uwp.Views
{
    public sealed partial class LocalPhotoPage : Page
    {
        private readonly NavigationHelper navigationHelper;
        public NavigationHelper NavigationHelper => this.navigationHelper;

        public static readonly DependencyProperty CropRectProperty =
            DependencyProperty.Register(nameof(CropRect), typeof(Rect), typeof(LocalPhotoPage), new PropertyMetadata(Rect.Empty));

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
                object obj;
                if (localSettings != null && localSettings.Values.TryGetValue("LibraryPhotoDragTutorialShown", out obj))
                {
                    dragTutorialShown = (bool)obj;
                }
                return dragTutorialShown;
            }
            set
            {
                dragTutorialShown = value;
                if (localSettings != null) localSettings.Values["LibraryPhotoDragTutorialShown"] = value;
            }
        }

        private readonly ApplicationDataContainer localSettings;
        private ConsolidatedImageInfo imageInfo = null;
        private WriteableBitmap baseWriteableBitmap;
        private WriteableBitmap filterAppliedBitmap;
        private WriteableBitmap previewWriteableBitmap;
        private readonly GestureRecognizer gestureRecognizer;
        private IImageProvider2 effectsSource = null;

        private Size loadedSize;

        public LocalPhotoPage()
        {
            this.InitializeComponent();
            DataContext = App.ViewModel;

            HockeyClient.Current.TrackPageView("LocalPhotoPage");

            if (DesignMode.DesignModeEnabled) return;
            localSettings = ApplicationData.Current.LocalSettings;

            DisplayInformation.AutoRotationPreferences = DisplayOrientations.Portrait;

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;

            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
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

            Loaded += LocalPhotoPage_Loaded;
        }


        private async void PickAFileButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var openPicker = new FileOpenPicker();
                openPicker.ViewMode = PickerViewMode.Thumbnail;
                openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
                openPicker.FileTypeFilter.Add(".jpg");
                openPicker.FileTypeFilter.Add(".jpeg");
                openPicker.FileTypeFilter.Add(".png");

                App.ViewModel.IsBusy = true;
                App.ViewModel.IsBusyMessage = "preparing selected photo";

                var chosenPhotoFile = await openPicker.PickSingleFileAsync();
                if (chosenPhotoFile == null)
                {
                    App.ViewModel.IsBusy = false;
                    App.ViewModel.IsBusyMessage = "";
                    return;
                }

                string fileType = chosenPhotoFile.FileType;

                if (fileType == ".jpg" || fileType == ".jpeg" || fileType == ".png")
                {
                    App.ViewModel.IsBusyMessage = "preparing selected photo";
                    Debug.WriteLine("Picked photo: " + chosenPhotoFile.Name);

                    using (var stream = await chosenPhotoFile.OpenStreamForReadAsync())
                    {
                        var bm = new BitmapImage();
                        await bm.SetSourceAsync(stream.AsRandomAccessStream());

                        var tempWbm = new WriteableBitmap(bm.PixelWidth, bm.PixelHeight);

                        stream.Seek(0, SeekOrigin.Begin);
                        await tempWbm.SetSourceAsync(stream.AsRandomAccessStream());

                        //first UWP way
                        //baseWriteableBitmap = await AspectRatioResizeAsync(
                        //    tempWbm,
                        //    (int)CropRect.Width,
                        //    (int)App.ViewModel.WindowBounds.Height);

                        //updated for SplitView
                        baseWriteableBitmap = await AspectRatioResizeAsync(
                            tempWbm,
                            (int)CropRect.Width,
                            (int)loadedSize.Height);

                        HockeyClient.Current.TrackEvent("LocalPhotoFilePicked");
                    }

                    if (baseWriteableBitmap == null)
                    {
                        await new MessageDialog("There was a problem loading the image, try again").ShowAsync();
                        return;
                    }

                    OriginalImage.Source = baseWriteableBitmap; //set loaded image to UI
                    LibraryPhotoSelectionPanel.Visibility = Visibility.Collapsed; //hide button
                    CropToSelectionButton.Visibility = Visibility.Visible; //allow cropping
                    CropToSelectionButton.IsEnabled = true;
                    CroppingPath.Stroke = new SolidColorBrush(Colors.LimeGreen);

                    CheckTutorial();
                }
                else
                {
                    await new MessageDialog("Sorry, this file is not a supported video file (MP4, AVI or WMV).").ShowAsync();
                }
            }
            catch (Exception ex)
            {
                await new MessageDialog("Photos FOP Exception" + ex.Message).ShowAsync();
            }
            finally
            {
                App.ViewModel.IsBusy = false;
                App.ViewModel.IsBusyMessage = "";
            }
        }

        #region navigation and page state

        public async void ContinueFileOpenPicker(FileOpenPickerContinuationEventArgs args)
        {
            try
            {
                App.ViewModel.IsBusy = true;
                App.ViewModel.IsBusyMessage = "preparing selected photo";

                if (args.Files.Count > 0)
                {
                    var chosenPhotoFile = args.Files.FirstOrDefault();
                    if (chosenPhotoFile == null)
                        return;

                    Debug.WriteLine("Picked photo: " + chosenPhotoFile.Name);

                    using (var stream = await chosenPhotoFile.OpenStreamForReadAsync())
                    {
                        var bm = new BitmapImage();
                        await bm.SetSourceAsync(stream.AsRandomAccessStream());

                        var tempWbm = new WriteableBitmap(bm.PixelWidth, bm.PixelHeight);

                        stream.Seek(0, SeekOrigin.Begin);
                        await tempWbm.SetSourceAsync(stream.AsRandomAccessStream());

                        //first UWP way
                        //baseWriteableBitmap = await AspectRatioResizeAsync(
                        //    tempWbm,
                        //    (int)CropRect.Width,
                        //    (int)App.ViewModel.WindowBounds.Height);

                        //updated for SplitView
                        baseWriteableBitmap = await AspectRatioResizeAsync(
                            tempWbm,
                            (int)CropRect.Width,
                            (int)loadedSize.Height);
                    }

                    if (baseWriteableBitmap == null)
                    {
                        await new MessageDialog("There was a problem loading the image, try again").ShowAsync();
                        return;
                    }

                    OriginalImage.Source = baseWriteableBitmap; //set loaded image to UI
                    LibraryPhotoSelectionPanel.Visibility = Visibility.Collapsed; //hide button
                    CropToSelectionButton.Visibility = Visibility.Visible; //allow cropping
                    CropToSelectionButton.IsEnabled = true;
                    CroppingPath.Stroke = new SolidColorBrush(Colors.LimeGreen);

                    CheckTutorial();
                }
                else
                {
                    LibraryPhotoSelectionPanel.Visibility = Visibility.Visible;
                    CropToSelectionButton.Visibility = Visibility.Collapsed;
                    CropToSelectionButton.IsEnabled = false;
                    CroppingPath.Stroke = new SolidColorBrush(Colors.Transparent);
                    Debug.WriteLine("Operation cancelled.");
                }
            }
            catch (Exception ex)
            {
                await new MessageDialog("There was a problem loading the image: " + ex.Message).ShowAsync();
                LibraryPhotoSelectionPanel.Visibility = Visibility.Visible;
                CropToSelectionButton.Visibility = Visibility.Collapsed;
                CroppingPath.Stroke = new SolidColorBrush(Colors.Transparent);
            }
            finally
            {
                App.ViewModel.IsBusy = false;
                App.ViewModel.IsBusyMessage = "";
            }
        }

        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
        }

        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }


        private void LocalPhotoPage_Loaded(object sender, RoutedEventArgs e)
        {
            //CropRect = new Rect(0, 0, App.ViewModel.WindowBounds.Width, Math.Floor(App.ViewModel.WindowBounds.Width * 0.329) - 2);
            //CropRect = new Rect(0, 0, App.ViewModel.WindowBounds.Width, Math.Floor(App.ViewModel.WindowBounds.Width * App.ViewModel.TileAspectRatio) - 2);

            //updated for UWP use in a SplitView TODO may need to update with dynamic sizing option
            loadedSize = new Size(ContentGrid.ActualWidth, ContentGrid.ActualHeight);
            this.CropRect = new Rect(0, 0, loadedSize.Width,
                Math.Floor(loadedSize.Width * App.ViewModel.TileAspectRatio) - 2);

            Debug.WriteLine("CropRect Set: {0}", CropRect);//subtract 2 pixels to show the sizes of the path

            BaseCanvas.Width = CropRect.Width;
            OriginalImage.Width = CropRect.Width;

            //the fill will be changed to Green after a photo is selected
            CroppingPath.Stroke = new SolidColorBrush(Colors.Transparent);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        private void CheckTutorial()
        {
            if (DragTutorialShown) return;

            FingerTutorialStoryboard.Begin();

            DragTutorialShown = true;
        }

        #endregion

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
        }

        private async void CropImageButton_Click(object sender, RoutedEventArgs e)
        {
            if (baseWriteableBitmap == null) return;

            var tempBitmap = await ApplyCropAsync(this.CropRect);
            if (tempBitmap == null)
                return;

            previewWriteableBitmap = tempBitmap;
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

        private async Task<WriteableBitmap> AspectRatioResizeAsync(WriteableBitmap originalBitmap, int currentScreenWidth, int maxHeight)
        {
            //if image is not wider than screen, just use original
            if (originalBitmap.PixelWidth <= currentScreenWidth) return originalBitmap;

            var newHeight = await GetHeightForScreenWidth(originalBitmap.PixelHeight,
                originalBitmap.PixelWidth, currentScreenWidth);

            return originalBitmap.Resize(currentScreenWidth, newHeight, WriteableBitmapExtensions.Interpolation.Bilinear);
        }

        private async Task<int> GetHeightForScreenWidth(int pixelHeight, int pixelWidth, int screenWidth)
        {
            return await Task.Run(() => pixelHeight * screenWidth / pixelWidth);
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
                await new MessageDialog("Sorry, there was a problem sending image to the Microsoft Band: " + ex.Message).ShowAsync();
            }
            finally
            {
                App.ViewModel.IsBusy = false;
                App.ViewModel.IsBusyMessage = "";
            }

            return false;
        }
        

        #endregion

        #region manipulation events

        void BaseCanvas_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (e == null) return;

            BaseCanvas.CapturePointer(e.Pointer);

            //gestureRecognizer.ProcessDownEvent(startingPoint);
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
    }
}
