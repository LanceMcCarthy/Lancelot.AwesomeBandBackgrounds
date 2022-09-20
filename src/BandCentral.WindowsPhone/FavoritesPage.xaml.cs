using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using BandCentral.Common;
using BandCentral.WindowsBase.Common;
using BandCentralBase.Common;
using Telerik.UI.Xaml.Controls.Data;

namespace BandCentral
{
    public sealed partial class FavoritesPage : Page
    {
        public static readonly DependencyProperty ListItemSizeProperty = DependencyProperty.Register(
            "ListItemSize", typeof(Size), typeof(ImageSearchPage), new PropertyMetadata(App.ViewModel.ListItemSize));

        public Size ListItemSize
        {
            get { return (Size)GetValue(ListItemSizeProperty); }
            private set { SetValue(ListItemSizeProperty, value); }
        }

        private NavigationHelper navigationHelper;
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        public FavoritesPage()
        {
            this.InitializeComponent();
            DataContext = App.ViewModel;

            DisplayInformation.AutoRotationPreferences = DisplayOrientations.Portrait;
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
        }

        private void ResultsList_OnItemTap(object sender, ListBoxItemTapEventArgs e)
        {
            var dblbSelectedItem = e.Item as RadDataBoundListBoxItem;
            var fav = dblbSelectedItem.DataContext as FlickrFav;

            if(fav == null) return;

            fav.IsExpanded = !fav.IsExpanded;
        }
        
        #region NavigationHelper registration

        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
        }

        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion
    }
}
