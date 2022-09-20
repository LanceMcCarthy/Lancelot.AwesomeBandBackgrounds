using System;
using System.Linq;
using System.Windows.Input;
using Windows.Storage;
using Windows.UI.Popups;
using BandCentral.WindowsBase.Common;
using BandCentralBase.Common;
using FlickrNet;
using Photo = FlickrNet.Photo;

namespace BandCentral.Uwp.Commands
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
                var photo = (Photo)parameter;
                return App.ViewModel.FlickrFavs.FirstOrDefault(x => x.Photo.PhotoId == photo.PhotoId) != null; //if item exists, disable button
            }

            return false;
        }

        public async void Execute(object parameter)
        {
            bool deleteConfirmed = false;
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
