using System;
using System.Linq;
using System.Windows.Input;
using BandCentral.WindowsBase.Common;
using FlickrNet;

namespace BandCentral.Commands
{
    public class RemoveFavoriteCommand : ICommand
    {
        public bool CanExecute(object parameter)
        {
            if (parameter is FlickrFav)
            {
                var fav = (FlickrFav)parameter;
                return App.ViewModel.FlickrFavs.FirstOrDefault(x => x.Photo.PhotoId == fav.Photo.PhotoId) != null; //if item does not exist, disable button
            }

            if (parameter is Photo)
            {
                var photo = (FlickrNet.Photo)parameter;
                return App.ViewModel.FlickrFavs.FirstOrDefault(x => x.Photo.PhotoId == photo.PhotoId) != null; //if item exists, disable button
            }

            return false;
        }

        public async void Execute(object parameter)
        {
            if(parameter is FlickrFav)
            {
                var fav = (FlickrFav)parameter;
                if(App.ViewModel.FlickrFavs == null) return;

                if(App.ViewModel.FlickrFavs.Contains(fav))
                {
                    App.ViewModel.FlickrFavs.Remove(fav);
                }
            }

            if(parameter is Photo)
            {
                var photo = (Photo)parameter;
                var fav = App.ViewModel.FlickrFavs.FirstOrDefault(x => x.Photo.PhotoId == photo.PhotoId); //if item exists, disable button

                if(fav != null)
                {
                    App.ViewModel.FlickrFavs.Remove(fav);
                }
            }

            await App.ViewModel.SaveFavoritesJsonAsync();

        }

        public event EventHandler CanExecuteChanged;
    }
}
