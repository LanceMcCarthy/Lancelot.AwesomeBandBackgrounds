using Windows.UI.Xaml;

namespace BandCentral.Models.Extensions
{
    public class XamlExtensions : DependencyObject
    {
        //IsVisible
        public static readonly DependencyProperty IsVisibleProperty = DependencyProperty.RegisterAttached("IsVisible", 
            typeof(bool),
            typeof(XamlExtensions), 
            new PropertyMetadata(true, (o, e) =>
            {
                ((UIElement) o).Visibility = (bool) e.NewValue ? Visibility.Visible : Visibility.Collapsed;
            }));
        
        public static void SetIsVisible(UIElement element, bool value)
        {
            element.SetValue(IsVisibleProperty, value);
        }

        public static bool GetIsVisible(UIElement element) => (bool)element.GetValue(IsVisibleProperty);

        //IsHidden
        public static readonly DependencyProperty IsHiddenProperty = DependencyProperty.RegisterAttached(
            "IsHidden",
            typeof(bool),
            typeof(XamlExtensions),
            new PropertyMetadata(true, (o, e) =>
            {
                ((UIElement) o).Visibility = (bool) e.NewValue ? Visibility.Collapsed : Visibility.Visible;
            }));

        public static void SetIsHidden(DependencyObject element, bool value)
        {
            element.SetValue(IsHiddenProperty, value);
        }

        public static bool GetIsHidden(DependencyObject element) => (bool) element.GetValue(IsHiddenProperty);
    }
}
