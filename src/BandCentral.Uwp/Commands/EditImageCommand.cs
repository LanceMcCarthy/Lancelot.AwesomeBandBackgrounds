using System;
using System.Windows.Input;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using BandCentral.Uwp.Views;
using BandCentral.WindowsBase.Common;

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
                if(parameter is FlickrNet.Photo)
                {
                    //when triggered from search page, only a PixPhoto object can be passed
                    App.ViewModel.SelectedFlickrPhoto = (FlickrNet.Photo)parameter;
                    (Window.Current.Content as Shell)?.RootFrame?.Navigate(typeof(PhotoDetailsPage));
                }
                else if (parameter is FlickrFav)
                {
                    //when a FlickrFav is available, get the PixPhoto object from the fav
                    App.ViewModel.SelectedFlickrPhoto = ((FlickrFav) parameter).Photo;
                    (Window.Current.Content as Shell)?.RootFrame?.Navigate(typeof(PhotoDetailsPage));
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
