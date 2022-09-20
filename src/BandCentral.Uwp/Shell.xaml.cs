using System.Linq;
using Windows.UI.Xaml.Controls;
using BandCentral.Uwp.ViewModels;
using BandCentral.Uwp.Views;
using Intense.Presentation;
using Windows.UI.ViewManagement;
using System.Diagnostics;
using Windows.Graphics.Display;

namespace BandCentral.Uwp
{
    public sealed partial class Shell : UserControl
    {
        private ShellViewModel viewModel;
        private const string HomeGlyph = "";
        private const string SearchGlyph = "";
        private const string ThemeColorsGlyph = "";
        private const string FavortiesListGlyph = "";
        private const string PhotoProvdersGlyph = "";
        private const string FileFolderGlyph = "";
        private const string SettingsGlyph = "";
        private const string AboutGlyph = "";
        //static string favoriteSolidGlyph = "";
        //static string unFavoriteSolidGlyph = "";
        //static string photosGlyph = "";
        //static string storeGlyph = "";
        //static string questionGlyph = "";
        //static string cropGlyph = "";
        //static string favoriteGlyph = "";
        //static string unlockedGlyph = "";
        //static string lockedGlyph = "";
        //static string bluetoothGlyph = "";

        public Shell()
        {
            this.InitializeComponent();

            var appView = ApplicationView.GetForCurrentView();
            appView.VisibleBoundsChanged += VisibleBoundsChanged;
            DisplayInformation.AutoRotationPreferences = DisplayOrientations.Portrait;

            // select home on launch
            this.ViewModel.SelectedItem = this.ViewModel.TopItems.First();
        }

        public ShellViewModel ViewModel => viewModel ?? (viewModel = InitViewModel());

        public Frame RootFrame => this.Frame;

        private static ShellViewModel InitViewModel()
        {
            var vm = new ShellViewModel();

            vm.TopItems.Add(new NavigationItem { Icon = HomeGlyph, DisplayName = "Home", PageType = typeof(MainPage) });
            vm.TopItems.Add(new NavigationItem { Icon = SearchGlyph, DisplayName = "Search", PageType = typeof(PhotoSearchPage) });
            //vm.TopItems.Add(new NavigationItem { Icon = SearchGlyph, DisplayName = "Search", PageType = typeof(TempSearchPage) });
            vm.TopItems.Add(new NavigationItem { Icon = FileFolderGlyph, DisplayName = "Local Photo", PageType = typeof(LocalPhotoPage) });
            vm.TopItems.Add(new NavigationItem { Icon = ThemeColorsGlyph, DisplayName = "Theme", PageType = typeof(ThemesPage) });
            //vm.TopItems.Add(new NavigationItem { Icon = CropGlyph, DisplayName = "Crop (ALPHA)", PageType = typeof(CropImageView) });
            vm.TopItems.Add(new NavigationItem { Icon = FavortiesListGlyph, DisplayName = "Favs Background Task", PageType = typeof(BackgroundRotatorPage) });
            vm.TopItems.Add(new NavigationItem { Icon = PhotoProvdersGlyph, DisplayName = "Bing Background Task", PageType = typeof(BingImagePage) });

            vm.BottomItems.Add(new NavigationItem { Icon = SettingsGlyph, DisplayName = "Settings", PageType = typeof(SettingsPage) });
            vm.BottomItems.Add(new NavigationItem { Icon = AboutGlyph, DisplayName = "About", PageType = typeof(AboutPage) });

            return vm;
        }

        private void VisibleBoundsChanged(ApplicationView sender, object args)
        {
            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                var statusBar = StatusBar.GetForCurrentView();
                var appView = ApplicationView.GetForCurrentView();
                var inputPane = InputPane.GetForCurrentView();

                double newHeight;
                double newWidth;

                var orientation = DisplayInformation.GetForCurrentView().CurrentOrientation;

                Debug.WriteLine($"CurrentOrientation: {orientation}");

                switch (orientation)
                {
                    case DisplayOrientations.None:
                        break;
                    case DisplayOrientations.Landscape:
                        newWidth = appView.VisibleBounds.Width +
                            (statusBar?.OccludedRect.Width) ?? 0 +
                            (inputPane?.OccludedRect.Width) ?? 0;

                        if ((inputPane?.OccludedRect.Width ?? 0) <= 0 || newWidth < this.Width)
                            this.Width = newWidth;

                        this.HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Left;
                        this.VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Top;
                        break;
                    case DisplayOrientations.Portrait:
                        newHeight = appView.VisibleBounds.Height +
                            (statusBar?.OccludedRect.Height) ?? 0 +
                            (inputPane?.OccludedRect.Height) ?? 0;

                        if ((inputPane?.OccludedRect.Height ?? 0) <= 0 || newHeight < this.Height)
                            this.Height = newHeight;

                        this.VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Top;
                        this.HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Stretch;
                        break;
                    case DisplayOrientations.LandscapeFlipped:
                        newWidth = appView.VisibleBounds.Width +
                            (statusBar?.OccludedRect.Width) ?? 0 +
                            (inputPane?.OccludedRect.Width) ?? 0;

                        if ((inputPane?.OccludedRect.Width ?? 0) <= 0 || newWidth < this.Width)
                            this.Width = newWidth;

                        this.HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Right;
                        this.VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Top;
                        break;
                    case DisplayOrientations.PortraitFlipped:
                        newHeight = appView.VisibleBounds.Height +
                            (statusBar?.OccludedRect.Height) ?? 0 +
                            (inputPane?.OccludedRect.Height) ?? 0;

                        if ((inputPane?.OccludedRect.Height ?? 0) <= 0 || newHeight < this.Height)
                            this.Height = newHeight;

                        this.VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Bottom;
                        this.HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Stretch;
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
