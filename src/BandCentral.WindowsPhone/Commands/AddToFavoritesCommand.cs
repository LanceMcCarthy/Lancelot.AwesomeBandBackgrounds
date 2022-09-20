using System;
using System.Linq;
using System.Windows.Input;
using Windows.UI.Popups;
using BandCentral.WindowsBase.Common;

namespace BandCentral.Commands
{
    public class AddToFavoritesCommand : ICommand
    {
        public bool CanExecute(object parameter)
        {
            var photo = (FlickrNet.Photo) parameter;
            if (photo == null) return false;
            return App.ViewModel.FlickrFavs.FirstOrDefault(x => x.Photo.PhotoId == photo.PhotoId) == null; //if item exists, disable button
        }

        public async void Execute(object parameter)
        {
            var photo = (FlickrNet.Photo) parameter;
            if (photo == null) return;
            if(App.ViewModel.FlickrFavs == null) return;

            var fav = new FlickrFav
            {
                DateFavorited = DateTime.UtcNow,
                Photo = photo
            };

            if(App.ViewModel.FlickrFavs.Contains(fav))
            {
                await new MessageDialog("This item is already in your favorties").ShowAsync();
                return;
            }

            App.ViewModel.FlickrFavs.Add(fav);

            await App.ViewModel.SaveFavoritesJsonAsync();// because CollectionChanged is not working
        }

        public event EventHandler CanExecuteChanged;
    }
}
