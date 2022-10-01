// Lance McCarthy 2013-2023 MIT
// Free to use, maintain attribution to original
// https://github.com/LanceMcCarthy/Lancelot.AwesomeBandBackgrounds

using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using BandCentral.Models.Bing;

namespace BandCentral.Uwp.Controls
{
    public sealed partial class BingImages : UserControl
    {
        public BingImages()
        {
            this.InitializeComponent();
            Loaded += BingImages_Loaded;
        }

        public delegate void BingImageSelectionChanged(object sender, SelectionChangedEventArgs args);

        public event BingImageSelectionChanged SelectionChanged;

        public static readonly DependencyProperty IsBusyProperty = DependencyProperty.Register(
            nameof(IsBusy), typeof (bool), typeof (BingImages), new PropertyMetadata(default(bool)));

        public static readonly DependencyProperty ImagesProperty = DependencyProperty.Register(
            nameof(Images), typeof (List<BingImage>), typeof (BingImages), new PropertyMetadata(default(List<BingImage>)));

        public bool IsBusy
        {
            get => (bool) GetValue(IsBusyProperty);
            set => SetValue(IsBusyProperty, value);
        }

        public List<BingImage> Images
        {
            get => (List<BingImage>) GetValue(ImagesProperty);
            set => SetValue(ImagesProperty, value);
        }

        private void OnSelectionChanged(SelectionChangedEventArgs args)
        {
            SelectionChanged?.Invoke(this, args);
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
