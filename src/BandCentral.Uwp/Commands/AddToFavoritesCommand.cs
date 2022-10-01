using BandCentral.Models.Common;
using BandCentral.Models.Extensions;
using BandCentral.Models.Favorites;
using BandCentral.Models.Helpers;
using Lumia.Imaging;
using Lumia.Imaging.Transforms;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Input;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml.Media.Imaging;

namespace BandCentral.Uwp.Commands
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
            try
            {
                var photo = (FlickrNet.Photo) parameter;
                if (photo == null) return;
                if (App.ViewModel.FlickrFavs == null) return;

                var fav = new FlickrFav
                {
                    DateFavorited = DateTime.UtcNow,
                    Photo = photo
                };

                if (App.ViewModel.FlickrFavs.Contains(fav))
                {
                    await new MessageDialog("This item is already in your favorties").ShowAsync();
                    return;
                }

                App.ViewModel.IsBusy = true;

                var reporter = new Progress<DownloadProgressArgs>();
                reporter.ProgressChanged += Reporter_ProgressChanged;

                using (var stream = await HttpClientExtensions.DownloadStreamWithProgressAsync(fav.Photo.MediumUrl, reporter))
                {
                    var memStream = new MemoryStream();
                    await stream.CopyToAsync(memStream);
                    memStream.Position = 0;

                    App.ViewModel.IsBusyMessage = "cropping photo...";

                    //NOTE - getting size form VM now to support Band 2
                    var tileSize = App.ViewModel.MeTileSize;

                    var outputWriteableBitmap = new WriteableBitmap((int) tileSize.Width, (int) tileSize.Height);

                    using (var source = new RandomAccessStreamImageSource(memStream.AsRandomAccessStream()))
                    using (var effect = new CropEffect(source, new Rect(0, 0, (int) tileSize.Width, (int) tileSize.Height)))
                    using (var renderer = new WriteableBitmapRenderer(effect, outputWriteableBitmap))
                    {
                        await renderer.RenderAsync();
                    }

                    var file = await outputWriteableBitmap.SaveToJpgFileAsync($"{fav.Photo.PhotoId}.jpg", ApplicationData.Current.LocalFolder);

                    fav.LocalImageFilePath = file?.Path;
                    fav.LocalImageFileName = file?.Name;
                }

                reporter.ProgressChanged -= Reporter_ProgressChanged;

                App.ViewModel.FlickrFavs.Add(fav);

                await App.ViewModel.SaveFavoritesJsonAsync();// because CollectionChanged is not working
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"AddToFavoritesCommand.Execute Exception\r\n{ex}");
            }
            finally
            {
                App.ViewModel.IsBusy = false;
                App.ViewModel.IsBusyMessage = "";
            }
            
        }

        private void Reporter_ProgressChanged(object sender, DownloadProgressArgs e)
        {
            App.ViewModel.IsBusyMessage = $"downloading {e.PercentComplete}%";
        }
        
        public event EventHandler CanExecuteChanged;
    }
}
