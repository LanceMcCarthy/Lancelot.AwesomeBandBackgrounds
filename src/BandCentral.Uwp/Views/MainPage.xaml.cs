using System;
using System.Linq;
using Windows.ApplicationModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using BandCentral.Uwp.Behaviors;
using BandCentral.Uwp.Dialogs;
using Microsoft.HockeyApp;
using Microsoft.Xaml.Interactivity;

namespace BandCentral.Uwp.Views
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            DataContext = App.ViewModel;

            HockeyClient.Current.TrackPageView("MainPage");

            if (DesignMode.DesignModeEnabled) return;

            this.NavigationCacheMode = NavigationCacheMode.Required;

            if (FavoritesList.Items != null)
                FavoritesList.Items.VectorChanged += Items_VectorChanged;
        }

        #region click and selection eventm handlers
        
        private void GoToBrowseButton_OnClick(object sender, RoutedEventArgs e)
        {
            var shell = (Window.Current.Content as Shell);
            if (shell == null) return;

            shell.ViewModel.SelectedItem = shell.ViewModel.TopItems.FirstOrDefault(m => m.PageType == typeof(PhotoSearchPage));
        }

        private void SettingsHyperLinkClick(object sender, RoutedEventArgs e)
        {
            var shell = (Window.Current.Content as Shell);
            if (shell == null) return;

            shell.ViewModel.SelectedItem = shell.ViewModel.BottomItems.FirstOrDefault(m => m.PageType == typeof(SettingsPage));
            
            //Interaction.GetBehaviors(FavoritesList).Add(new FadeHeaderBehavior { HeaderElement = HeaderPanel });
        }

        #endregion

        #region Navigation

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            //if (!App.ViewModel.IsBandConnected && App.ViewModel.ConnectOnLaunch == false)
            //{
            //    await new ConnectToBandDialog().ShowAsync();
            //}

            //dont keep showing the dialog if in Disconected Mode
            if (App.ViewModel.DisconnectedMode)
                return;

            if (!App.ViewModel.IsBandConnected)
                await new ConnectToBandDialog().ShowAsync();
            
        }

        private void Items_VectorChanged(Windows.Foundation.Collections.IObservableVector<object> sender, Windows.Foundation.Collections.IVectorChangedEventArgs @event)
        {
            NoFavsGrid.Visibility = App.ViewModel.FlickrFavs.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
        }

        #endregion
        
        //TODO transition test
        //private void PaletteButton_OnClick(object sender, RoutedEventArgs e)
        //{
        //    var button = sender as Button;

        //    var gridViewItem = button.GetFirstAncestorOfType<GridViewItem>();
        //    var compositionImage = gridViewItem.GetFirstDescendantOfType<CompositionImage>();

        //    (Window.Current.Content as Shell)?.RootFrame.Transitions.Clear();

        //    var transition = new ContinuityTransition();
        //    transition.Initialize((Window.Current.Content as Shell), compositionImage);
        //    (Window.Current.Content as Shell)?.RootFrame.Navigate(typeof(ImageSwatchTest), transition);
        //}

    }
}
