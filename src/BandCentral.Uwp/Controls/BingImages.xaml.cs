using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using BandCentral.Models.Bing;

namespace BandCentral.Uwp.Controls
{
    public sealed partial class BingImages : UserControl
    {
        public static readonly DependencyProperty IsBusyProperty = DependencyProperty.Register(
            "IsBusy", typeof (bool), typeof (BingImages), new PropertyMetadata(default(bool)));

        public bool IsBusy
        {
            get { return (bool) GetValue(IsBusyProperty); }
            set { SetValue(IsBusyProperty, value); }
        }

        public static readonly DependencyProperty ImagesProperty = DependencyProperty.Register(
            "Images", typeof (List<BingImage>), typeof (BingImages), new PropertyMetadata(default(List<BingImage>)));

        public List<BingImage> Images
        {
            get { return (List<BingImage>) GetValue(ImagesProperty); }
            set { SetValue(ImagesProperty, value); }
        }

        public delegate void BingImageSelectionChanged(object sender, SelectionChangedEventArgs args);
        public event BingImageSelectionChanged SelectionChanged;

        private void OnSelectionChanged(SelectionChangedEventArgs args)
        {
            SelectionChanged?.Invoke(this, args);
        }

        public BingImages()
        {
            this.InitializeComponent();
            Loaded += BingImages_Loaded;
        }

        private async void BingImages_Loaded(object sender, RoutedEventArgs e)
        {
            this.IsBusy = true;
            var bingImages = new BingImageHelper();
            var result = await bingImages.GetBingImagesAsync();

            if (result != null)
                Images = result;
            this.IsBusy = false;
        }
        
        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //bubble up the event args
            this.OnSelectionChanged(e);
        }
    }
}
