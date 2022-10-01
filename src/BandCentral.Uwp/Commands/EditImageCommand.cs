using BandCentral.Models.Favorites;
using BandCentral.Uwp.Views;
using System;
using System.Windows.Input;
using Windows.UI.Popups;
using Windows.UI.Xaml;

namespace BandCentral.Uwp.Commands
{
    public class EditImageCommand : ICommand
    {
        public bool CanExecute(object parameter)
        {
            return true;
        }

        public async void Execute(object parameter)
        {
            if (parameter == null) return;

            try
            {
                switch (parameter)
                {
                    case FlickrNet.Photo photo:
                        //when triggered from search page, only a PixPhoto object can be passed
                        App.ViewModel.SelectedFlickrPhoto = photo;
                        (Window.Current.Content as Shell)?.RootFrame?.Navigate(typeof(PhotoDetailsPage));
                        break;
                    case FlickrFav fav:
                        //when a FlickrFav is available, get the PixPhoto object from the fav
                        App.ViewModel.SelectedFlickrPhoto = fav.Photo;
                        (Window.Current.Content as Shell)?.RootFrame?.Navigate(typeof(PhotoDetailsPage));
                        break;
                }
            }
            catch (Exception ex)
            {
                await new MessageDialog($"There was a problem in EditImageCommand. Error: {ex.Message}").ShowAsync();
            }
        }

        public event EventHandler CanExecuteChanged;
    }
}
