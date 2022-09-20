using BandCentral.Common;
using System;
using System.Diagnostics;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Phone.UI.Input;
using Windows.Storage;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using BandCentral.WindowsBase.Common;
using BandCentralBase.Common;
using Telerik.Core;
using Telerik.UI.Xaml.Controls.Data;

namespace BandCentral
{
    public sealed partial class HubPage : Page
    {
        private readonly NavigationHelper navigationHelper;
        public NavigationHelper NavigationHelper => this.navigationHelper;
        private static ApplicationDataContainer roamingSettings = ApplicationData.Current.RoamingSettings;
        private string selectedBandImagePath;
        private string bandColorToChange;
        
        public static readonly DependencyProperty AreColorsLinkedProperty = DependencyProperty.Register(
            "AreColorsLinked", typeof(bool), typeof(HubPage), new PropertyMetadata(default(bool), PropertyChangedCallback));

        private static void PropertyChangedCallback(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            roamingSettings.Values["AreColorsLinked"] = args.NewValue;
        }

        public bool AreColorsLinked
        {
            get { return (bool) GetValue(AreColorsLinkedProperty); }
            set { SetValue(AreColorsLinkedProperty, value); }
        }

        public HubPage()
        {
            this.InitializeComponent();
            DataContext = App.ViewModel;
            
            if(DesignMode.DesignModeEnabled) return;

            var statusBar = StatusBar.GetForCurrentView();
            statusBar.BackgroundOpacity = 1;
            statusBar.BackgroundColor = ((SolidColorBrush)Application.Current.Resources["BlueAgainGrayBrush"]).Color;
            statusBar.ForegroundColor = new SolidColorBrush(Colors.White).Color;

            var animation = new RadMoveAndFadeAnimation();
            animation.FadeAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(1000));
            animation.FadeAnimation.StartOpacity = 0;
            animation.FadeAnimation.EndOpacity = 1;
            animation.MoveAnimation.StartPoint = new Point(0, 40);
            animation.MoveAnimation.EndPoint = new Point(0, 0);
            animation.MoveAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(1000));
            this.FavoritesList.ItemAddedAnimation = animation;
            
            this.NavigationCacheMode = NavigationCacheMode.Required;
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;

            MessageGrid.Visibility = CheckUpdateMessageStatus();
        }

        #region click events
        
        private void ThemeHxListBox_OnItemTap(object sender, ListBoxItemTapEventArgs e)
        {
            var hxTheme = ThemeHxListBox.SelectedItem as BandColorTheme;

            if(hxTheme != null)
                App.ViewModel.CurrentBandTheme = hxTheme;
        }

        private void ThemeColorGrid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Picker.Visibility = Visibility.Visible;

            var tag = ((Grid)sender).Tag as string;
            bandColorToChange = tag;
        }

        private async void SetBandThemeAppBarButton_OnClick(object sender, RoutedEventArgs e)
        {
            await App.ViewModel.SetBandThemeAsync(App.ViewModel.CurrentBandTheme);
        }

        private async void ResetThemeAppBarButton_OnClick(object sender, RoutedEventArgs e)
        {
            await App.ViewModel.GetBandThemeAsync();
        }

        private void SearchAppbatButton_OnClick(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(TestingPage));
        }

        private void FavoritesAppbarButton_OnClick(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(FavoritesPage));
        }

        private void SettingsAppBarButton_OnClick(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(SettingsPage));
        }

        private void AboutAppBarButton_OnClickAppBarButton_OnClick(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(SettingsPage), (int)1);
        }

        private void BetaAppBarButton_OnClickAppBarButton_OnClick(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(SettingsPage), (int) 2);
        }

        private void ResultsList_OnItemTap(object sender, ListBoxItemTapEventArgs e)
        {
            var dblbSelectedItem = e.Item as RadDataBoundListBoxItem;
            App.ViewModel.SelectedFav = dblbSelectedItem.DataContext as FlickrFav;

            if(App.ViewModel.SelectedFav == null) return;

            App.ViewModel.SelectedFav.IsExpanded = !App.ViewModel.SelectedFav.IsExpanded;
        }

        private async void ClearThemeHistoryButton_OnClick(object sender, RoutedEventArgs e)
        {
            await App.ViewModel.ClearThemeHistoryAsync();
        }

        private void LinkUnlinkAppBarButton_OnClick(object sender, RoutedEventArgs e)
        {
            if(AreColorsLinked)
            {
                AreColorsLinked = false;
                LinkUnlinkAppBarButton.Icon = new SymbolIcon(Symbol.Link);
                LinkUnlinkAppBarButton.Label = "link similar";
            }
            else
            {
                AreColorsLinked = true;
                LinkUnlinkAppBarButton.Icon = new SymbolIcon(Symbol.Cut);
                LinkUnlinkAppBarButton.Label = "unlink";
            }
        }

        private void LocalPhotoAppbarButton_OnClick(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(LocalPhotoPage));
        }

        #endregion

        #region selection changed events

        private void ColorPicker_OnColorSelectionChanged(object sender, EventArgs e)
        {
            switch(bandColorToChange)
            {
                case "Base":
                    App.ViewModel.CurrentBandTheme.BaseBrush = Picker.SelectedColor;
                    if(AreColorsLinked) App.ViewModel.CurrentBandTheme.LowLightBrush = Picker.SelectedColor;
                    break;
                case "Highlight":
                    App.ViewModel.CurrentBandTheme.HighlightBrush = Picker.SelectedColor;
                    if(AreColorsLinked) App.ViewModel.CurrentBandTheme.HighContrastBrush = Picker.SelectedColor;
                    break;
                case "LowLight":
                    App.ViewModel.CurrentBandTheme.LowLightBrush = Picker.SelectedColor;
                    if(AreColorsLinked) App.ViewModel.CurrentBandTheme.BaseBrush = Picker.SelectedColor;
                    break;
                case "Secondary":
                    App.ViewModel.CurrentBandTheme.SecondaryBrush = Picker.SelectedColor;
                    break;
                case "HighContrast":
                    App.ViewModel.CurrentBandTheme.HighContrastBrush = Picker.SelectedColor;
                    if(AreColorsLinked) App.ViewModel.CurrentBandTheme.HighlightBrush = Picker.SelectedColor;
                    break;
                case "Muted":
                    App.ViewModel.CurrentBandTheme.MutedBrush = Picker.SelectedColor;
                    break;
                default:
                    break;
            }

            Picker.Visibility = Visibility.Collapsed;
        }

        private void MainPivot_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(MainPivot.SelectedIndex == 0)
            {
                LinkUnlinkAppBarButton.Visibility = Visibility.Collapsed;
                ClearThemeHistoryButton.Visibility = Visibility.Collapsed;
                ResetThemeAppBarButton.Visibility = Visibility.Collapsed;
                SetBandThemeAppBarButton.Visibility = Visibility.Collapsed;
                LocalPhotoAppbarButton.Visibility = Visibility.Visible;
                //FavoritesAppbarButton.Visibility = Visibility.Visible;
                SearchAppbarButton.Visibility = Visibility.Visible;
                BetaAppBarButton.Visibility = Visibility.Visible;
            }

            if(MainPivot.SelectedIndex == 1)
            {
                LinkUnlinkAppBarButton.Visibility = Visibility.Visible;
                ClearThemeHistoryButton.Visibility = Visibility.Visible;
                ResetThemeAppBarButton.Visibility = Visibility.Visible;
                SetBandThemeAppBarButton.Visibility = Visibility.Visible;
                LocalPhotoAppbarButton.Visibility = Visibility.Collapsed;
                //FavoritesAppbarButton.Visibility = Visibility.Collapsed;
                SearchAppbarButton.Visibility = Visibility.Collapsed;
                BetaAppBarButton.Visibility = Visibility.Collapsed;
            }
        }

        #endregion

        #region tasks

        //private async Task<bool> ConnectAndGetImageAsync()
        //{
        //    Exception ex = null;

        //    try
        //    {
        //        if (await App.ViewModel.FindAndConnectToBandAsync())
        //        {
        //            App.ViewModel.CurrentBandImage = await App.ViewModel.GetCurrentMeTileBackground();
        //            return true;
        //        }
        //    }
        //    catch (Exception exception)
        //    {
        //        ex = exception;
        //    }

        //    if (ex != null)
        //    {
        //        await new MessageDialog("There was a problem connecting to the Band. Make sure your Bluetooth is turned on and you are paired to the Band.").ShowAsync();
        //    }

        //    return false;
        //}

        #endregion

        #region NavigationHelper registration

        private async void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            if (!App.ViewModel.IsBandConnected && App.ViewModel.ConnectOnLaunch == false)
            {
                BandConnectionDialog.Visibility = Visibility.Visible;
            }
        }

        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            //List<string> emps = new List<string>();
            //IQueryable<string> employees = new EnumerableQuery<string>(emps);
            //IQueryable<string> filteredList = from emp 
            //                                  in employees
            //                                  select emp;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
            HardwareButtons.BackPressed += HardwareButtons_BackPressed;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
            HardwareButtons.BackPressed -= HardwareButtons_BackPressed;
        }

        void HardwareButtons_BackPressed(object sender, BackPressedEventArgs e)
        {
            if(Picker.Visibility == Visibility.Visible)
            {
                Picker.Visibility = Visibility.Collapsed;
                e.Handled = true;
                return;
            }

            if (BandConnectionDialog.Visibility == Visibility.Visible)
            {
                BandConnectionDialog.Visibility = Visibility.Collapsed;
                e.Handled = true;
                return;
            }
        }

        #endregion

        #region Update Message

        private static Visibility CheckUpdateMessageStatus()
        {
            try
            {
                var localSettings = ApplicationData.Current.LocalSettings;
                if (localSettings == null)
                    return Visibility.Collapsed;

                object lastupdateVersion;
                if (localSettings.Values.TryGetValue(Constants.UpdateMessageShownOnVersion, out lastupdateVersion))
                {
                    int previousVersionSum;
                    if (int.TryParse(lastupdateVersion.ToString(), out previousVersionSum))
                    {
                        var packageId = Package.Current.Id;
                        var currentVersionSum = packageId.Version.Major + packageId.Version.Minor + packageId.Version.Build;
                        return currentVersionSum > previousVersionSum ? Visibility.Visible : Visibility.Collapsed;
                    }
                }

                //if the UpdateMessageShownOnVersion setting doesnt exist, we'll save it now
                var id = Package.Current.Id;
                var sum = id.Version.Major + id.Version.Minor + id.Version.Build;
                localSettings.Values[Constants.UpdateMessageShownOnVersion] = sum;
                Debug.WriteLine($"{Constants.UpdateMessageShownOnVersion} = {sum}");
                return Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                return Visibility.Collapsed;
            }
        }

        private void CloseMessageButton_OnClick(object sender, RoutedEventArgs e)
        {
            var localSettings = ApplicationData.Current.LocalSettings;
            if (localSettings != null)
            {
                var packageId = Package.Current.Id;
                var currentVersionSum = packageId.Version.Major + packageId.Version.Minor + packageId.Version.Build;
                localSettings.Values[Constants.UpdateMessageShownOnVersion] = currentVersionSum;
                Debug.WriteLine($"LastUpdateMessageVersionSum = {currentVersionSum}");
            }

            MessageGrid.Visibility = Visibility.Collapsed;
        }

        #endregion

        private async void ReloadMeTileImageButton_OnClick(object sender, RoutedEventArgs e)
        {
            await App.ViewModel.GetMeTileAsync();
        }
    }
}