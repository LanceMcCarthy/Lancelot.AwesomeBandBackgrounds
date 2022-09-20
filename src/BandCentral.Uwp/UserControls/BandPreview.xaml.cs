using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using BandCentral.Uwp.Annotations;
using BandCentral.Uwp.Common;

namespace BandCentral.Uwp.UserControls
{
    public sealed partial class BandPreview : UserControl, INotifyPropertyChanged
    {
        public static readonly DependencyProperty MeTileImageProperty = DependencyProperty.Register(
            "MeTileImage", typeof (WriteableBitmap), typeof (BandPreview), new PropertyMetadata(default(WriteableBitmap), PropertyChangedCallback));

        private static void PropertyChangedCallback(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            Debug.WriteLine($"----BandPreview control MeTileImage DP Callback - Old PixelHeight {((WriteableBitmap) args.OldValue)?.PixelHeight}");
            Debug.WriteLine($"----BandPreview control MeTileImage DP Callback - New PixelHeight {((WriteableBitmap)args.NewValue)?.PixelHeight}");
        }

        public WriteableBitmap MeTileImage
        {
            get { return (WriteableBitmap) GetValue(MeTileImageProperty); }
            set { SetValue(MeTileImageProperty, value); }
        }

        public static readonly DependencyProperty ButtonCommandProperty = DependencyProperty.Register(
            "ButtonCommand", typeof (DelegateCommand), typeof (BandPreview), new PropertyMetadata(default(DelegateCommand)));

        public DelegateCommand ButtonCommand
        {
            get { return (DelegateCommand) GetValue(ButtonCommandProperty); }
            set { SetValue(ButtonCommandProperty, value); }
        }

        public static readonly DependencyProperty ButtonCommandParameterProperty = DependencyProperty.Register(
            "ButtonCommandParameter", typeof (object), typeof (BandPreview), new PropertyMetadata(default(object)));
        

        public object ButtonCommandParameter
        {
            get { return (object) GetValue(ButtonCommandParameterProperty); }
            set { SetValue(ButtonCommandParameterProperty, value); }
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
