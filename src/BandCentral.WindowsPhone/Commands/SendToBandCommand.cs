using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Popups;
using Windows.UI.Xaml.Media.Imaging;
using BandCentral.Common;
using BandCentral.WindowsBase.Common;
using FlickrNet;

namespace BandCentral.Commands
{
    public class SendToBandCommand : ICommand
    {
        public bool CanExecute(object parameter)
        {
            return true;
        }

        public async void Execute(object parameter)
        {
            if (parameter is Photo)
            {
                await SetBandImageAsync(await FilterEffectsHelper.DownloadAndCropAsync((Photo) parameter));
            }

            if (parameter is FlickrFav)
            {
                await SetBandImageAsync(await FilterEffectsHelper.DownloadAndCropAsync(((FlickrFav)parameter).Photo));
            }
        }

        public event EventHandler CanExecuteChanged;
        
        private static async Task SetBandImageAsync(WriteableBitmap outputWb)
        {
            if (outputWb == null) return;

            try
            {
                await App.ViewModel.SetMeTileAsync(outputWb);
            }
            catch (Exception ex)
            {
                await new MessageDialog("Sorry, there was a problem sending image to the band: " + ex).ShowAsync();
            }
            finally
            {
                App.ViewModel.IsBusyMessage = "";
                App.ViewModel.IsBusy = false;
            }
        }
        
    }
}
