using System;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using BandCentral.Common;

namespace BandCentral.UserControls
{
    public sealed partial class BandConnection : UserControl
    {
        public BandConnection()
        {
            this.InitializeComponent();
            Loaded += BandConnection_Loaded;
            DataContext = App.ViewModel;
        }

        private async void BandConnection_Loaded(object sender, RoutedEventArgs e)
        {
            await InitializeBandConnectorAsync();
        }
        
        private void BandsComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ConnectToBandButton.IsEnabled = e.AddedItems.Count > 0;
        }

        private async void ConnectToBandButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                DialogBusyIndicator.IsActive = true;

                if (App.ViewModel.CurrentBand == null)
                {
                    await new MessageDialog("You need to select a Band first.", "No Selection Made").ShowAsync();
                    App.ViewModel.IsBandConnected = false;
                }
                else
                {
                    //if there is a CurrentBand selected, call the complete connect and refresh
                    if (await App.ViewModel.InitializeBandInformationAsync())
                    {
                        App.ViewModel.IsBandConnected = true;
                        this.Visibility = Visibility.Collapsed;
                    }
                }
            }
            catch (Exception ex)
            {
                await new MessageDialog($"There was a problem connecting to your selected Band. Please try again.\r\n\nError: {ex.Message}", "Error Connecting").ShowAsync();
            }
            finally
            {
                DialogBusyIndicator.IsActive = false;
            }
        }

        private async void RefreshPairedBandButton_OnClick(object sender, RoutedEventArgs e)
        {
            DialogBusyIndicator.IsActive = true;
            App.ViewModel.IsBusyMessage = "refreshing...";

            App.ViewModel.Bands = await BandConnectionHelper.FindPairedBandsAsync();

            App.ViewModel.IsBusyMessage = "";
            DialogBusyIndicator.IsActive = false;
        }

        private async Task InitializeBandConnectorAsync()
        {
            //if connected, back out, we dont need to do anything
            if (App.ViewModel.IsBandConnected)
            {
                this.Visibility = Visibility.Collapsed;
                return;
            }

            try
            {
                DialogBusyIndicator.IsActive = true;
                App.ViewModel.IsBusyMessage = "initializing...";

                //if automatic connection is enabled and a preferred is already in memory, 
                if (App.ViewModel.ConnectOnLaunch == true && !string.IsNullOrEmpty(App.ViewModel.PreferredBandName))
                {
                    if (await App.ViewModel.InitializeBandInformationAsync())
                    {
                        this.Visibility = Visibility.Collapsed;
                        App.ViewModel.IsBandConnected = true;
                        return;
                    }
                }

                //----- if no band name is saved or if auto connection is disabled -----//

                //if they need to choose a band, then show the dialog
                //BandsListView.Header = "Finding paired Bands...";
                BandsComboBox.Header = "Finding paired Bands...";

                //find paired bands
                App.ViewModel.Bands = await BandConnectionHelper.FindPairedBandsAsync();

                //if more than one paired Band is found, let the user choose
                //BandsListView.Header = "Select a Band to continue:";
                BandsComboBox.Header = "Select a Band to continue:";

            }
            catch (Exception ex)
            {
                await new MessageDialog($"There was a problem finding paired Bands. If this happens again, contact us at awesome.apps@outlook.com and share this error message:" +
                                        $"\r\n\nConnectToBandDialog Exception: {ex.Message}").ShowAsync();
            }
            finally
            {
                //show the refresh button if there were no results
                RefreshPairedBandButton.Visibility = App.ViewModel.Bands.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

                DialogBusyIndicator.IsActive = false;
                App.ViewModel.IsBusyMessage = "";
            }
        }
        
    }
}
