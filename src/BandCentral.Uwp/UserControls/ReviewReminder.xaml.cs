// Lance McCarthy 2013-2023 MIT
// Free to use, maintain attribution to original
// https://github.com/LanceMcCarthy/Lancelot.AwesomeBandBackgrounds

using System;
using System.Collections.Generic;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Store;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace BandCentral.Uwp.UserControls
{
    public sealed partial class ReviewReminder : UserControl
    {
        private readonly List<int> reminderRunPoints;
        
        public ReviewReminder()
        {
            this.InitializeComponent();
            reminderRunPoints = new List<int>{ 5, 10, 15, 20 };
            Loaded += ReviewReminder_Loaded;
        }

        void ReviewReminder_Loaded(object sender, RoutedEventArgs e)
        {
            if (DesignMode.DesignModeEnabled) return;

            if (App.ViewModel.HasBeenRated || App.ViewModel.HideReviewReminderForSession)
            {
                this.Visibility = Visibility.Collapsed;
                return;
            }

            if(reminderRunPoints.Contains(App.ViewModel.ApplicationRuns))
            {
                this.Visibility = Visibility.Visible;
            }
            else
            {
                this.Visibility = Visibility.Collapsed;
            }
        }

        private async void OkButton_OnClick(object sender, RoutedEventArgs e)
        {
            if(DesignMode.DesignModeEnabled) return;
            await Launcher.LaunchUriAsync(new Uri("ms-windows-store:reviewapp?appid=" + CurrentApp.AppId));
            this.Visibility = Visibility.Collapsed;
            App.ViewModel.HasBeenRated = true;
        }

        private void CancelButton_OnClick(object sender, RoutedEventArgs e)
        {
            if(DesignMode.DesignModeEnabled) return;
            this.Visibility = Visibility.Collapsed;
            App.ViewModel.HideReviewReminderForSession = true;
        }

        private void SkipRemindersCheckBox_OnChecked(object sender, RoutedEventArgs e)
        {
            if(DesignMode.DesignModeEnabled) return;
            if(!SkipRemindersCheckBox.IsChecked.HasValue) return;

            App.ViewModel.HasBeenRated = SkipRemindersCheckBox.IsChecked.Value;
        }

        private async void RatingControl_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            if(DesignMode.DesignModeEnabled) return;
            await Launcher.LaunchUriAsync(new Uri("ms-windows-store:reviewapp?appid=" + CurrentApp.AppId));
            this.Visibility = Visibility.Collapsed;
            App.ViewModel.HasBeenRated = true;
        }
    }
}
