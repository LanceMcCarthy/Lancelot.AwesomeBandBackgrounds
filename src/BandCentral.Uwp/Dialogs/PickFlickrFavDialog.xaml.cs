using System;
using System.Linq;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
using BandCentral.WindowsBase.Common;

namespace BandCentral.Uwp.Dialogs
{
    public sealed partial class PickFlickrFavDialog : ContentDialog
    {
        public PickFlickrFavDialog()
        {
            this.InitializeComponent();
            DataContext = App.ViewModel;
        }

        private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if (App.ViewModel.SelectedFav == null)
            {
                await new MessageDialog("Please choose an item to continue or click cancel to quit.", "You have not selected an item").ShowAsync();
                return;
            }
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            App.ViewModel.SelectedFav = null;
        }

        private void FavoritesList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Any())
            {
                this.IsPrimaryButtonEnabled = true;
                App.ViewModel.SelectedFav = e.AddedItems[0] as FlickrFav;
            }
            else
            {
                this.IsPrimaryButtonEnabled = false;
                App.ViewModel.SelectedFav = null;
            }
        }
        
    }
}
