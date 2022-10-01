using BandCentral.Models.Favorites;
using BandCentral.Uwp.Views;
using System;
using System.Windows.Input;
using Windows.UI.Popups;
using Windows.UI.Xaml;

namespace BandCentral.Uwp.Commands
{
    public class EditPaletteCommand : ICommand
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
                if (parameter is FlickrFav fav)
                {
                    //if we're going to the custom theme page using a FlickrFav context
                    App.ViewModel.SelectedFav = fav;

                    if (App.ViewModel.SelectedFav == null)
                        return;
                }
                //else if(parameter is BackgroundFav)
                //{
                //    //if we're going to the custom theme page using a BackgrondFav context

                //    var bgFav = (BackgroundFav) parameter;
                //    var fav = App.ViewModel.FlickrFavs.FirstOrDefault(f => f.Photo.PhotoId == bgFav.PhotoId);
                //    if (fav == null)
                //        return;

                //    App.ViewModel.SelectedFav = fav;
                //}

                //(Window.Current.Content as Shell)?.RootFrame?.Navigate(typeof (ImageSwatchTest));
                (Window.Current.Content as Shell)?.RootFrame?.Navigate(typeof(PairedThemePage));

            }
            catch (Exception ex)
            {
                await new MessageDialog($"There was a problem in EditPaletteCommand. Error: {ex.Message}").ShowAsync();
            }
        }

        public event EventHandler CanExecuteChanged;
    }
}
