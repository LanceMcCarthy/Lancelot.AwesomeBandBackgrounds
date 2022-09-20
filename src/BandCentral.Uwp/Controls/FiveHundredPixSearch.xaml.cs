using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using BandCentral.WindowsBase.Common;
using BandCentralBase.Common;

namespace BandCentral.WindowsBase.Controls
{
    public sealed partial class FiveHundredPixSearch : UserControl
    {
        #region dependency properties

        public static readonly DependencyProperty PhotosProperty = DependencyProperty.Register(
            "Photos", 
            typeof (IncrementalLoadingCollection<PixPhoto>), 
            typeof (FiveHundredPixSearch), 
            new PropertyMetadata(default(IncrementalLoadingCollection<PixPhoto>)));

        public IncrementalLoadingCollection<PixPhoto> Photos
        {
            get { return (IncrementalLoadingCollection<PixPhoto>) GetValue(PhotosProperty); }
            set { SetValue(PhotosProperty, value); }
        }

        public static readonly DependencyProperty SelectedPhotoProperty = DependencyProperty.Register(
            "SelectedPhoto", 
            typeof (PixPhoto), 
            typeof (FiveHundredPixSearch),
            new PropertyMetadata(default(PixPhoto)));

        public PixPhoto SelectedPhoto
        {
            get { return (PixPhoto) GetValue(SelectedPhotoProperty); }
            set { SetValue(SelectedPhotoProperty, value); }
        }

        public static readonly DependencyProperty PhotosPerPageProperty = DependencyProperty.Register(
            "PhotosPerPage", 
            typeof (int), 
            typeof (FiveHundredPixSearch), 
            new PropertyMetadata(50));

        public int PhotosPerPage
        {
            get { return (int) GetValue(PhotosPerPageProperty); }
            set { SetValue(PhotosPerPageProperty, value); }
        }

        public static readonly DependencyProperty TotalPhotosProperty = DependencyProperty.Register(
            "TotalPhotos", 
            typeof (int), 
            typeof (FiveHundredPixSearch), 
            new PropertyMetadata(default(int)));

        public int TotalPhotos
        {
            get { return (int) GetValue(TotalPhotosProperty); }
            set { SetValue(TotalPhotosProperty, value); }
        }

        public static readonly DependencyProperty TotalPagesProperty = DependencyProperty.Register(
            "TotalPages", 
            typeof (int), 
            typeof (FiveHundredPixSearch), 
            new PropertyMetadata(default(int)));

        public int TotalPages
        {
            get { return (int) GetValue(TotalPagesProperty); }
            set { SetValue(TotalPagesProperty, value); }
        }

        public static readonly DependencyProperty CurrentPageProperty = DependencyProperty.Register(
            "CurrentPage", 
            typeof (int), 
            typeof (FiveHundredPixSearch), 
            new PropertyMetadata(1));

        public int CurrentPage
        {
            get { return (int) GetValue(CurrentPageProperty); }
            set { SetValue(CurrentPageProperty, value); }
        }

        public static readonly DependencyProperty SearchTermProperty = DependencyProperty.Register(
            "SearchTerm", 
            typeof (string), 
            typeof (FiveHundredPixSearch), 
            new PropertyMetadata("beaches"));

        public string SearchTerm
        {
            get { return (string) GetValue(SearchTermProperty); }
            set { SetValue(SearchTermProperty, value); }
        }

        #endregion

        public delegate void FiveHundredPixSelectionChanged(object sender, SelectionChangedEventArgs args);
        public event FiveHundredPixSelectionChanged SelectionChanged;
        
        private FiveHundredPix pixHelper;
        
        private void OnFiveHundredPixSelectionChanged(SelectionChangedEventArgs args)
        {
            SelectionChanged?.Invoke(this, args);
        }

        public FiveHundredPixSearch()
        {
            this.InitializeComponent();
        }

        private void ExecuteSearchButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                SearchTerm = TermTextBox.Text;

                pixHelper = new FiveHundredPix(Constants.FiveHundredPixConsumerKey, "");

                //ResultRoot = await pixHelper.SearchPhotosAsync(term); //test succeeded, now trying with incremental colleciton

                Photos = new IncrementalLoadingCollection<PixPhoto>((token, count) => Task.Run(GetMorePhotos, token));
            }
            catch(Exception ex)
            {
                Debug.WriteLine($"ExecuteSearchButton_OnClick Exception. Message: {ex.Message}");
            }
        }

        private async Task<ObservableCollection<PixPhoto>> GetMorePhotos()
        {
            try
            {
                int page = 0;
                int ppp = 20;
                string term = "";

                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    page = this.CurrentPage;
                    ppp = this.PhotosPerPage;
                    term = this.SearchTerm;
                });

                var result = await pixHelper.SearchPhotosAsync(page, ppp, term);

                if (result == null)
                {
                    throw new ArgumentOutOfRangeException(nameof(result.photos),"The API call did not return any photos. Please check your internet connection and try again with a different search term.");
                }
                
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    this.CurrentPage = result.current_page + 1;
                    this.TotalPhotos = result.total_items;
                    this.TotalPages = result.total_pages;
                });
                
                return new ObservableCollection<PixPhoto>(result.photos);
            }
            catch (Exception ex)
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    await new MessageDialog($"There was a problem searching 500px.\r\n\nError: {ex.Message}").ShowAsync();
                });
                
                return null;
            }
        }

        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            OnFiveHundredPixSelectionChanged(e);
        }
        
    }
}
