// Lance McCarthy 2013-2023 MIT
// Free to use, maintain attribution to original
// https://github.com/LanceMcCarthy/Lancelot.AwesomeBandBackgrounds

using BandCentral.Models.Common;
using Microsoft.HockeyApp;
using System;
using System.Collections.Generic;
using Telerik.UI.Xaml.Controls.Data;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace BandCentral.Uwp.Views
{
    public sealed partial class ThemesPage : Page
    {
        #region fields

        private static ApplicationDataContainer roamingSettings = ApplicationData.Current.RoamingSettings;
        private string bandColorToChange;
        //private bool areColorsLinked = false;

        public static readonly DependencyProperty ColorLinkVisibilityProperty = DependencyProperty.Register(
            "ColorLinkVisibility", typeof(Visibility), typeof(MainPage), new PropertyMetadata(Windows.UI.Xaml.Visibility.Collapsed));

        public Visibility ColorLinkVisibility
        {
            get { return (Visibility)GetValue(ColorLinkVisibilityProperty); }
            set { SetValue(ColorLinkVisibilityProperty, value); }
        }

        public static readonly DependencyProperty AreColorsLinkedProperty = DependencyProperty.Register(
            "AreColorsLinked", typeof (bool), typeof (ThemesPage), new PropertyMetadata(default(bool), PropertyChangedCallback));

        private static void PropertyChangedCallback(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            roamingSettings.Values["AreColorsLinked"] = args.NewValue;
        }

        public bool AreColorsLinked
        {
            get { return (bool) GetValue(AreColorsLinkedProperty); }
            set { SetValue(AreColorsLinkedProperty, value); }
        }

        #endregion

        public ThemesPage()
        {
            this.InitializeComponent();
            DataContext = App.ViewModel;

            HockeyClient.Current.TrackPageView("ThemesPage");

            if (roamingSettings != null && roamingSettings.Values.TryGetValue("AreColorsLinked", out var obj))
            {
                AreColorsLinked = (bool)obj;
            }

            UpdateAppBar();
        }

        #region event handlers

        private void ThemeHxListBox_OnItemTap(object sender, ListBoxItemTapEventArgs e)
        {
            var hxTheme = ThemeHxListBox.SelectedItem as BandColorTheme;

            if (hxTheme != null)
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
            //if band theme was set, enable undo button
            ResetThemeAppBarButton.IsEnabled = await App.ViewModel.SetBandThemeAsync(App.ViewModel.CurrentBandTheme);

            var properties = new Dictionary<string, string>();
            properties.Add("PageUsedOn", "ThemesPage");
            HockeyClient.Current.TrackEvent("SetTheme", properties);
        }

        private async void ResetThemeAppBarButton_OnClick(object sender, RoutedEventArgs e)
        {
            //if we've reset the theme successfully, disable the undo button
            ResetThemeAppBarButton.IsEnabled = !await App.ViewModel.GetBandThemeAsync();
        }

        private async void ClearThemeHistoryButton_OnClick(object sender, RoutedEventArgs e)
        {
            await App.ViewModel.ClearThemeHistoryAsync();
            HockeyClient.Current.TrackEvent("ClearThemeHistory");
        }

        private void LinkUnlinkAppBarButton_OnClick(object sender, RoutedEventArgs e)
        {
            UpdateAppBar();
        }

        private void ColorPicker_OnColorSelectionChanged(object sender, EventArgs e)
        {
            switch (bandColorToChange)
            {
                case "Base":
                    App.ViewModel.CurrentBandTheme.BaseBrush = Picker.SelectedColor;
                    if (AreColorsLinked) App.ViewModel.CurrentBandTheme.LowLightBrush = Picker.SelectedColor;
                    break;
                case "Highlight":
                    App.ViewModel.CurrentBandTheme.HighlightBrush = Picker.SelectedColor;
                    if (AreColorsLinked) App.ViewModel.CurrentBandTheme.HighContrastBrush = Picker.SelectedColor;
                    break;
                case "LowLight":
                    App.ViewModel.CurrentBandTheme.LowLightBrush = Picker.SelectedColor;
                    if (AreColorsLinked) App.ViewModel.CurrentBandTheme.BaseBrush = Picker.SelectedColor;
                    break;
                case "Secondary":
                    App.ViewModel.CurrentBandTheme.SecondaryBrush = Picker.SelectedColor;
                    break;
                case "HighContrast":
                    App.ViewModel.CurrentBandTheme.HighContrastBrush = Picker.SelectedColor;
                    if (AreColorsLinked) App.ViewModel.CurrentBandTheme.HighlightBrush = Picker.SelectedColor;
                    break;
                case "Muted":
                    App.ViewModel.CurrentBandTheme.MutedBrush = Picker.SelectedColor;
                    break;
                default:
                    break;
            }

            HockeyClient.Current.TrackEvent("ColorPickerUsed");

            Picker.Visibility = Visibility.Collapsed;
        }

        #endregion

        #region methods

        private void UpdateAppBar()
        {
            if (AreColorsLinked)
            {
                AreColorsLinked = false;
                LinkUnlinkAppBarButton.Icon = new SymbolIcon(Symbol.Link);
                LinkUnlinkAppBarButton.Label = "link similar";
                ColorLinkVisibility = Visibility.Collapsed;
            }
            else
            {
                AreColorsLinked = true;
                LinkUnlinkAppBarButton.Icon = new SymbolIcon(Symbol.Cut);
                LinkUnlinkAppBarButton.Label = "unlink";
                ColorLinkVisibility = Visibility.Visible;
            }
        }

        #endregion

        #region navigation and lifecycle

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            SystemNavigationManager.GetForCurrentView().BackRequested += SoftwareBackPressed;

            //if (ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons"))
            //{
            //    HardwareButtons.BackPressed += HardwareBackPressed;
            //}
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            SystemNavigationManager.GetForCurrentView().BackRequested -= SoftwareBackPressed;

            //if (ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons"))
            //{
            //    HardwareButtons.BackPressed -= HardwareBackPressed;
            //}
        }

        //hardware back button press
        //void HardwareBackPressed(object sender, BackPressedEventArgs e)
        //{
        //    App.SupressMainBackpressHandler = true;

        //    if (Picker.Visibility == Visibility.Visible)
        //    {
        //        Picker.Visibility = Visibility.Collapsed;
        //        if (ApiInformation.IsTypePresent("Windows.Phone.UI.Input.BackPressedEventArgs"))
        //        {
        //            e.Handled = true;
        //        }
        //    }

        //    App.SupressMainBackpressHandler = false;
        //}

        //Hide theme pick if it is open
        void SoftwareBackPressed(object sender, BackRequestedEventArgs e)
        {
            App.SupressMainBackpressHandler = true;

            if (Picker.Visibility == Visibility.Visible)
            {
                Picker.Visibility = Visibility.Collapsed;
                e.Handled = true;
            }

            App.SupressMainBackpressHandler = false;
        }

        #endregion
    }
}
