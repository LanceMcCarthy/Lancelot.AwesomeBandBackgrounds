// Lance McCarthy 2013-2023 MIT
// Free to use, maintain attribution to original
// https://github.com/LanceMcCarthy/Lancelot.AwesomeBandBackgrounds

using BandCentral.Models.Helpers;
using BandCentral.Models.Secrets;
using BandCentral.Uwp.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

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
            if (await GeneralConstants.BackgroundTasksIapKey.PurchaseProductAsync())
            {
                ((MainViewModel) DataContext).IapBackgroundTasks = true;
                ShowThanks("background task unlock");
            }
        }

        private async void DonateSmallButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (await GeneralConstants.SmallDonationIapKey.PurchaseProductAsync())
                ShowThanks("small donation");
        }

        private async void DonateMediumButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (await GeneralConstants.MediumDonationIapKey.PurchaseProductAsync())
                ShowThanks("medium donation");
        }

        private async void DonateLargeButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (await GeneralConstants.LargeDonationIapKey.PurchaseProductAsync())
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
