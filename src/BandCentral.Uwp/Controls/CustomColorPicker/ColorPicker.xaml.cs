// Lance McCarthy 2013-2023 MIT
// Free to use, maintain attribution to original
// https://github.com/LanceMcCarthy/Lancelot.AwesomeBandBackgrounds

using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace BandCentral.Uwp.Controls.CustomColorPicker
{
    public sealed partial class ColorPicker : UserControl
    {
        private readonly ColorPickerViewModel viewModel;

        public ColorPicker()
        {
            this.InitializeComponent();
            viewModel = this.DataContext as ColorPickerViewModel;
        }

        public event EventHandler ColorSelectionChanged;

        public static readonly DependencyProperty SelectedColorProperty = DependencyProperty.Register(
            nameof(SelectedColor), typeof(SolidColorBrush), typeof(ColorPicker), new PropertyMetadata(default(SolidColorBrush)));

        public SolidColorBrush SelectedColor
        {
            get => (SolidColorBrush)GetValue(SelectedColorProperty);
            private set => SetValue(SelectedColorProperty, value);
        }

        private async void SwatchesGridView_OnItemClick(object sender, ItemClickEventArgs e)
        {
            viewModel.SelectedSwatch = e.ClickedItem as Swatch;
            await viewModel.ToggleViewStateAsync();
        }

        private async void SwatchChooserButton_OnClick(object sender, RoutedEventArgs e)
        {
            await viewModel.ToggleViewStateAsync();
        }

        private void ColorRectangle_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var rectangle = sender as Rectangle;

            if(rectangle == null) return;

            this.SelectedColor = rectangle.Fill as SolidColorBrush;

            if(ColorSelectionChanged != null)
                ColorSelectionChanged(sender, new EventArgs());
        }
    }
}