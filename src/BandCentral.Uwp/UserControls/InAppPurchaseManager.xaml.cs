using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using BandCentral.Uwp.ViewModels;
using BandCentral.WindowsBase.Common;
using BandCentralBase.Common;

namespace BandCentral.Uwp.UserControls
{
    public sealed partial class InAppPurchaseManager : UserControl
    {
        public InAppPurchaseManager()
        {
            this.InitializeComponent();
            DataContext = App.ViewModel;
        }

        private async void UnlockTasksButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (await Constants.BackgroundTasksIapKey.PurchaseProductAsync())
            {
                ((MainViewModel) DataContext).IapBackgroundTasks = true;
                ShowThanks("background task unlock");
            }
        }

        private async void DonateSmallButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (await Constants.SmallDonationIapKey.PurchaseProductAsync())
                ShowThanks("small donation");
        }

        private async void DonateMediumButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (await Constants.MediumDonationIapKey.PurchaseProductAsync())
                ShowThanks("medium donation");
        }

        private async void DonateLargeButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (await Constants.LargeDonationIapKey.PurchaseProductAsync())
                ShowThanks("large donation");
        }

        private void ShowThanks(string productPurchased)
        {
            ResultTextBlock.Text =
                $"Your {productPurchased} purchase was successful, thank you for your support!\r\n\nIf you have any questions or concerns, please contact us at awesome.apps@outlook.com";
        }

        private void DoneButton_OnClick(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
        }
    }
}
