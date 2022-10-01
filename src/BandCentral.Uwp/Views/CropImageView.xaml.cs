// Lance McCarthy 2013-2023 MIT
// Free to use, maintain attribution to original
// https://github.com/LanceMcCarthy/Lancelot.AwesomeBandBackgrounds

using BandCentral.Models.Common;
using BandCentral.Uwp.Common;
using BandCentral.Uwp.Controls.ImageCropper.Helpers;
using Microsoft.HockeyApp;
using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace BandCentral.Uwp.Views
{
    public sealed partial class CropImageView : Page
    {
        private WriteableBitmap originalBitmap;
        private WriteableBitmap filterAppliedBitmap;
        private WriteableBitmap workingBitmap;

        public CropImageView()
        {
            this.InitializeComponent();
            DataContext = App.ViewModel;

            HockeyClient.Current.TrackPageView("CropImageView");
        }

        private async void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            var openPicker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.Thumbnail,
                SuggestedStartLocation = PickerLocationId.PicturesLibrary
            };
            openPicker.FileTypeFilter.Add(".jpg");
            openPicker.FileTypeFilter.Add(".bmp");
            openPicker.FileTypeFilter.Add(".gif");
            openPicker.FileTypeFilter.Add(".png");
            StorageFile imgFile = await openPicker.PickSingleFileAsync();
            if (imgFile != null)
            {
                originalBitmap = new WriteableBitmap(1, 1);
                await originalBitmap.LoadAsync(imgFile);
                this.ImageCropper.SourceImage = originalBitmap;
            }
        }

        #region Button and ListItem click events

        private void UndoButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (filterAppliedBitmap != null && workingBitmap != null)
            {
                filterAppliedBitmap = null;
                PreviewImage.Source = workingBitmap;
                return;
            }

            if (workingBitmap != null && filterAppliedBitmap == null)
            {
                workingBitmap = null;
                BandPreviewGrid.Visibility = Visibility.Collapsed;
                return;
            }

            filterAppliedBitmap = null;
            workingBitmap = null;
            BandPreviewGrid.Visibility = Visibility.Collapsed;
        }

        private async void EffectsListView_OnItemClick(object sender, ItemClickEventArgs e)
        {
            AddEffectsButton.Flyout?.Hide();

            try
            {
                var item = e.ClickedItem as FilterListItem;
                if (item == null) return;
                if (string.IsNullOrEmpty(item.Title))
                {
                    await new MessageDialog("The selected filter could not be determined, please try again").ShowAsync();
                    return;
                }

                workingBitmap = this.ImageCropper.CroppedImage;

                filterAppliedBitmap = await FilterEffectsHelper.ApplySelectedFilterAsync(this.ImageCropper.CroppedImage, item.Title);
                PreviewImage.Source = filterAppliedBitmap;
            }
            catch (Exception ex)
            {
                PreviewImage.Source = workingBitmap;
            }
        }


        private async void SendToBandButton_OnClick(object sender, RoutedEventArgs e)
        {
            //if a filter was applied
            if (filterAppliedBitmap != null)
            {
                await SetBandImageAsync(filterAppliedBitmap);
                return;
            }
            //otherwise use previewBitmap
            if (workingBitmap != null)
            {
                await SetBandImageAsync(workingBitmap);
            }
        }

        #endregion

        private static async Task<bool> SetBandImageAsync(WriteableBitmap outputWb)
        {
            if (outputWb == null) return false;

            if (Math.Abs((double) outputWb.PixelHeight/outputWb.PixelWidth - App.ViewModel.TileAspectRatio) > 1)
            {
                await new MessageDialog("The cropped area is too small, resize your image to fit the Band area and try again").ShowAsync();
                return false;
            }

            //if (outputWb.PixelWidth < App.ViewModel.MeTileSize.Width ||
            //    outputWb.PixelHeight < App.ViewModel.MeTileSize.Height)
            //{
            //    await new MessageDialog("The cropped area is too small, resize your image to fit the Band area and try again").ShowAsync();
            //    return false;
            //}

            //TODO Uncomment after test for image too small
            //App.ViewModel.IsBusy = true;

            //try
            //{
            //    if (App.ViewModel.SelectedBand == null)
            //    {
            //        App.ViewModel.IsBusyMessage = "You were not connected, reconnecting now...";
            //        await App.ViewModel.FindAndConnectToBandAsync();
            //    }

            //    App.ViewModel.IsBusyMessage = "Connected! Sending image to Band...";

            //    await App.ViewModel.SetBandMeTileBackground(outputWb);

            //    return true;

            //}
            //catch (Exception ex)
            //{
            //    await new MessageDialog("Sorry, there was a problem sending image to the Microsoft Band: " + ex.Message).ShowAsync();
            //}
            //finally
            //{
            //    App.ViewModel.IsBusy = false;
            //    App.ViewModel.IsBusyMessage = "";
            //}

            return false;
        }
    }
}
