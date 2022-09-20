using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using BandCentral.WindowsBase.Common;

namespace BandCentral.UserControls
{
    public sealed partial class FavoriteBandPreview : UserControl
    {
        private bool areButtonsVisible;

        public static readonly DependencyProperty FavoriteProperty = DependencyProperty.Register(
            "Favorite", typeof(FlickrFav), typeof(FavoriteBandPreview), new PropertyMetadata(default(FlickrFav), FavoritePropertyChangedCallback));

        private static void FavoritePropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            //var debugPoint = dependencyPropertyChangedEventArgs.NewValue;
        }

        public FlickrFav Favorite
        {
            get { return (FlickrFav)GetValue(FavoriteProperty); }
            set { SetValue(FavoriteProperty, value); }
        }

        public static readonly DependencyProperty PhotoHeightProperty = DependencyProperty.Register(
            "PhotoHeight", typeof (double), typeof (FavoriteBandPreview), new PropertyMetadata(138, PhotoHeightPropertyChangedCallback));

        private static void PhotoHeightPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            //Debug.WriteLine($"PhotoHeight Callback: {args.NewValue}");
        }

        public double PhotoHeight
        {
            get { return (double) GetValue(PhotoHeightProperty); }
            set { SetValue(PhotoHeightProperty, value); }
        }

        public FavoriteBandPreview()
        {
            this.InitializeComponent();
            Loaded += FavoriteBandPreview_Loaded;
        }

        private void FavoriteBandPreview_Loaded(object sender, RoutedEventArgs e)
        {
            //not needed anymore
            //this.PhotoHeight = App.ViewModel.ListItemSize.Height;
            //Debug.WriteLine($"FavoriteBandPreview_Loaded: {App.ViewModel.ListItemSize.Height}");
        }

        private void BaseGrid_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (areButtonsVisible)
            {
                Debug.WriteLine("BaseGrid_PointerPressed - HideButtons");
                //HideButtonsStory.Begin();
                //this.GoToElementStateCore("HiddenButtons", true);
                areButtonsVisible = false;
            }
            else
            {
                Debug.WriteLine("BaseGrid_PointerPressed - ShowButtons");
                //ShowButtonsStory.Begin();
                //this.GoToElementStateCore("VisibleButtons", true);
                areButtonsVisible = true;
            }
        }
    }
}
