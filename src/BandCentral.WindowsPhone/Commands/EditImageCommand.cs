using System;
using System.Windows.Input;
using Windows.UI.Popups;
using BandCentral.WindowsBase.Common;

namespace BandCentral.Commands
{
    public class EditImageCommand : ICommand
    {
        public bool CanExecute(object parameter)
        {
            return true;
        }

        public async void Execute(object parameter)
        {
            Exception exception = null;
            if (parameter == null) return;

            try
            {
                if(parameter is FlickrNet.Photo)
                {
                    App.ViewModel.SelectedFlickrPhoto = (FlickrNet.Photo)parameter;
                    App.ActiveFrameService.Navigate(typeof(PhotoDetailsPage));
                }
                else if (parameter is FlickrFav)
                {
                    App.ViewModel.SelectedFlickrPhoto = ((FlickrFav) parameter).Photo;
                    App.ActiveFrameService.Navigate(typeof(PhotoDetailsPage));
                }
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            if(exception != null)
            {
                await new MessageDialog("There was a problem loading the image: " + exception.Message).ShowAsync();
            }
        }

        public event EventHandler CanExecuteChanged;
    }
}
