using Windows.UI.Xaml;

namespace BandCentral.Extensions.XAML
{
    public class Extension : DependencyObject
    {
        public static readonly DependencyProperty IsVisibleProperty = DependencyProperty.RegisterAttached("IsVisible", typeof(bool),
            typeof(Extension), new PropertyMetadata(true, IsVisibleCallback));

        private static void IsVisibleCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((UIElement)d).Visibility = (bool)e.NewValue ? Visibility.Visible : Visibility.Collapsed;
        }

        public static void SetIsVisible(UIElement element, bool value)
        {
            element.SetValue(IsVisibleProperty, value);
        }

        public static bool GetIsVisible(UIElement element)
        {
            return (bool)element.GetValue(IsVisibleProperty);
        }
    }
}
