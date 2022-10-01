// Lance McCarthy 2013-2023 MIT
// Free to use, maintain attribution to original
// https://github.com/LanceMcCarthy/Lancelot.AwesomeBandBackgrounds

using BandCentral.Uwp.Common;
using System;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace BandCentral.Uwp.Dialogs
{
    public sealed partial class ConnectToBandDialog : ContentDialog
    {
        public ConnectToBandDialog()
        {
            this.InitializeComponent();
            DataContext = App.ViewModel;
            Loaded += ConnectToBandDialog_Loaded;
        }

        private async void ConnectToBandDialog_Loaded(object sender, RoutedEventArgs e)
        {
            await InitializeBandConnectorAsync();
        }

        private void BandsComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ConnectToBandButton.IsEnabled = e.AddedItems.Count > 0;
        }

        private async void RefreshPairedBandButton_OnClick(object sender, RoutedEventArgs e)
        {
            DialogBusyIndicator.IsActive = true;
            App.ViewModel.IsBusyMessage = "refreshing...";

            App.ViewModel.Bands = await BandConnectionHelper.FindPairedBandsAsync();

            App.ViewModel.IsBusyMessage = "";
            DialogBusyIndicator.IsActive = false;
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
                        this.Hide();
                    }
                    
                }
            }
            catch (Exception ex)
            {
                await new MessageDialog($"There was a problem connecting to your selected Band. Please try again.\r\n\nError: {ex.Message}", "Error Connecting").ShowAsync();
                DisconnectedModeButton.Visibility = Visibility.Visible;
            }
            finally
            {
                DialogBusyIndicator.IsActive = false;
            }
        }

        private async Task InitializeBandConnectorAsync()
        {
            //if connected, back out, we dont need to do anything
            if (App.ViewModel.IsBandConnected)
            {
                this.Hide();
                return;
            }

            try
            {
                DialogBusyIndicator.IsActive = true;
                App.ViewModel.IsBusyMessage = "initializing...";

                //if automatic connection is enabled and a preferred is already in memory, 
                if (App.ViewModel.ConnectOnLaunch == true && !string.IsNullOrEmpty(App.ViewModel.PreferredBandName))
                {
                    DialogBusyIndicator.IsActive = true;

                    if (await App.ViewModel.InitializeBandInformationAsync())
                    {
                        DisconnectedModeButton.Visibility = Visibility.Collapsed;
                        this.Hide();
                        App.ViewModel.IsBandConnected = true;
                        return;
                    }
                    else
                    {
                        DisconnectedModeButton.Visibility = Visibility.Visible;
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
                DisconnectedModeButton.Visibility = Visibility.Visible;
            }
            finally
            {
                DialogBusyIndicator.IsActive = false;
                App.ViewModel.IsBusyMessage = "";
            }
        }

        private void DiconnectedModeButton_OnClick(object sender, RoutedEventArgs e)
        {
            App.ViewModel.DisconnectedMode = true;
            this.Hide();
        }
    }
}
