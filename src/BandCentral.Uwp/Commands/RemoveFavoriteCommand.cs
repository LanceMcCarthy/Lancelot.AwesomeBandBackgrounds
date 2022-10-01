using BandCentral.Models.Favorites;
using System;
using System.Linq;
using System.Windows.Input;
using Windows.UI.Popups;
using Photo = FlickrNet.Photo;

namespace BandCentral.Uwp.Commands
{
    public class RemoveFavoriteCommand : ICommand
    {
        public bool CanExecute(object parameter)
        {
            switch (parameter)
            {
                case FlickrFav fav:
                    return App.ViewModel.FlickrFavs.FirstOrDefault(x => x.Photo.PhotoId == fav.Photo.PhotoId) != null; //if item does not exist, disable button
                case Photo photo:
                    return App.ViewModel.FlickrFavs.FirstOrDefault(x => x.Photo.PhotoId == photo.PhotoId) != null; //if item exists, disable button
                default:
                    return false;
            }
        }

        public async void Execute(object parameter)
        {
            var deleteConfirmed = false;
            var md = new MessageDialog("Are you sure you want to remove this item?", "Delete confirmation");

            md.Commands.Add(new UICommand("delete", command =>
            {
                deleteConfirmed = true;
            }));
            md.Commands.Add(new UICommand("cancel"));

            await md.ShowAsync();

            if (!deleteConfirmed)
                return;

            await App.ViewModel.DeleteFavAsync(parameter);
        }

        public event EventHandler CanExecuteChanged;
    }
}
