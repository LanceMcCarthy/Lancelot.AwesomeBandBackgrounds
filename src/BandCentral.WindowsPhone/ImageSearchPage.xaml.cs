using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using Windows.ApplicationModel;
using Windows.Graphics.Display;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using BandCentral.Common;
using BandCentral.ViewModels;
using BandCentralBase.Common;
using FlickrNet;
using Telerik.Core.Data;
using Telerik.UI.Xaml.Controls.Data;
using Size = Windows.Foundation.Size;
using Photo = FlickrNet.Photo;

namespace BandCentral
{
    public sealed partial class ImageSearchPage : Page
    {
        public static readonly DependencyProperty ListItemSizeProperty = DependencyProperty.Register(
            "ListItemSize", typeof(Size), typeof(ImageSearchPage), new PropertyMetadata(App.ViewModel.ListItemSize));

        public Size ListItemSize
        {
            get { return (Size)GetValue(ListItemSizeProperty); }
            private set { SetValue(ListItemSizeProperty, value); }
        }

        //public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
        //    "ViewModel", typeof(SearchViewModel), typeof(ImageSearchPage), new PropertyMetadata(default(SearchViewModel)));

        //public SearchViewModel ViewModel
        //{
        //    get { return (SearchViewModel)GetValue(ViewModelProperty); }
        //    private set { SetValue(ViewModelProperty, value); }
        //}

        private NavigationHelper navigationHelper;
        public NavigationHelper NavigationHelper { get { return this.navigationHelper; } }

        public ImageSearchPage()
        {
            this.InitializeComponent();
            //ViewModel = new SearchViewModel();

            if(DesignMode.DesignModeEnabled) return;

            var statusBar = StatusBar.GetForCurrentView();
            statusBar.BackgroundOpacity = 1;
            statusBar.BackgroundColor = ((SolidColorBrush)Application.Current.Resources["BlueAgainGrayBrush"]).Color;
            statusBar.ForegroundColor = new SolidColorBrush(Colors.White).Color;
            
            DisplayInformation.AutoRotationPreferences = DisplayOrientations.Portrait;

            this.NavigationCacheMode = NavigationCacheMode.Required;
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;

        }

        private async void SearchButton_OnClick(object sender, RoutedEventArgs e)
        {
            //ResultsList.ItemsSource = null;
            //await ViewModel.ClearSearchAsync();
            //ResultsList.ItemsSource = ViewModel.PhotoList;
        }

        private void ResultsList_OnItemTap(object sender, ListBoxItemTapEventArgs e)
        {
            var dblbSelectedItem = e.Item as RadDataBoundListBoxItem;
            var photo = dblbSelectedItem.DataContext as Photo;

            if(photo == null) return;

            //if(photo.IsExpanded)
            //{
            //    photo.IsExpanded = false;
            //}
            //else
            //{
            //    photo.IsExpanded = true;
            //}
        }

        #region NavigationHelper registration

        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            TagAutoCompleteBox.ItemsSource = new List<string>()
            {
                "Eyes","Lips","Nails","Hair","Wallpaper","Backgrounds","Technology",
                "Art","Animals","Cars","Women","Men","Landscape",
                "Panorama","Trees","Water","Space","Planets","Forest",
                "Sunrise","Sunset","Geeky","Sci-Fi","TV"
            };
        }

        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //this.ListItemSize = new Size { Width = Window.Current.Bounds.Width, Height = Math.Floor(Window.Current.Bounds.Width * 0.329) };

            this.navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

    }
}
