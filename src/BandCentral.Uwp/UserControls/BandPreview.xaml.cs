// Lance McCarthy 2013-2023 MIT
// Free to use, maintain attribution to original
// https://github.com/LanceMcCarthy/Lancelot.AwesomeBandBackgrounds

using BandCentral.Uwp.Annotations;
using CommonHelpers.Mvvm;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace BandCentral.Uwp.UserControls
{
    public sealed partial class BandPreview : UserControl, INotifyPropertyChanged
    {
        public static readonly DependencyProperty MeTileImageProperty = DependencyProperty.Register(
            nameof(MeTileImage), typeof (WriteableBitmap), typeof (BandPreview), new PropertyMetadata(default(WriteableBitmap), PropertyChangedCallback));

        private static void PropertyChangedCallback(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            Debug.WriteLine($"----BandPreview control MeTileImage DP Callback - Old PixelHeight {((WriteableBitmap) args.OldValue)?.PixelHeight}");
            Debug.WriteLine($"----BandPreview control MeTileImage DP Callback - New PixelHeight {((WriteableBitmap)args.NewValue)?.PixelHeight}");
        }

        public WriteableBitmap MeTileImage
        {
            get => (WriteableBitmap) GetValue(MeTileImageProperty);
            set => SetValue(MeTileImageProperty, value);
        }

        public static readonly DependencyProperty ButtonCommandProperty = DependencyProperty.Register(
            nameof(ButtonCommand), typeof (DelegateCommand), typeof (BandPreview), new PropertyMetadata(default(DelegateCommand)));

        public DelegateCommand ButtonCommand
        {
            get => (DelegateCommand) GetValue(ButtonCommandProperty);
            set => SetValue(ButtonCommandProperty, value);
        }

        public static readonly DependencyProperty ButtonCommandParameterProperty = DependencyProperty.Register(
            nameof(ButtonCommandParameter), typeof (object), typeof (BandPreview), new PropertyMetadata(default(object)));
        

        public object ButtonCommandParameter
        {
            get => (object) GetValue(ButtonCommandParameterProperty);
            set => SetValue(ButtonCommandParameterProperty, value);
        }

        public BandPreview()
        {
            this.InitializeComponent();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
