using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using BandCentral.WindowsBase.Common;
using Photo = FlickrNet.Photo;

namespace BandCentral.Selectors
{
    public class ImageTemplateSelector : DataTemplateSelector
    {
        public DataTemplate NormalTemplate { get; set; }
        public DataTemplate ExpandedTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            if(item == null)return NormalTemplate;

            if (item is Photo)
            {
                return NormalTemplate;
            }

            if (item is FlickrFav)
            {
                var fav = item as FlickrFav;

                return fav.IsExpanded ? ExpandedTemplate : NormalTemplate;
            }

            

            return base.SelectTemplateCore(item, container);
        }

    }
}
