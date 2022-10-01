using BandCentral.Models.Favorites;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Photo = FlickrNet.Photo;

namespace BandCentral.Uwp.Selectors
{
    public class ImageTemplateSelector : DataTemplateSelector
    {
        public DataTemplate NormalTemplate { get; set; }

        public DataTemplate ExpandedTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            switch (item)
            {
                case null:
                    return NormalTemplate;
                case Photo _:
                    return NormalTemplate;
                case FlickrFav fav:
                    return fav.IsExpanded ? ExpandedTemplate : NormalTemplate;
                default:
                    return base.SelectTemplateCore(item, container);
            }
        }

    }
}
