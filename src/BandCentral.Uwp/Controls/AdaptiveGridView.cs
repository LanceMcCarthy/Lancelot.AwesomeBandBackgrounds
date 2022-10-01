// Lance McCarthy 2013-2023 MIT
// Free to use, maintain attribution to original
// https://github.com/LanceMcCarthy/Lancelot.AwesomeBandBackgrounds

using System;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace BandCentral.Uwp.Controls
{
    // Original version, better version here https://github.com/LanceMcCarthy/UwpProjects
    public class AdaptiveGridView : GridView
    {
        public AdaptiveGridView()
        {
            // Have the content of each item stretch the full item width
            if (this.ItemContainerStyle == null)
            {
                this.ItemContainerStyle = new Style(typeof(GridViewItem));
            }

            this.ItemContainerStyle.Setters.Add(new Setter(GridViewItem.HorizontalContentAlignmentProperty, HorizontalAlignment.Stretch));

            this.Loaded += (s, a) =>
            {
                if (this.ItemsPanelRoot != null)
                {
                    // trigger an initial pass to calculate item sizes
                    this.InvalidateMeasure();
                }
            };
        }

        protected override void OnItemsChanged(object e)
        {
            base.OnItemsChanged(e);

            this.HasItems = Items?.Count > 0;
        }

        public static readonly DependencyProperty HasItemsProperty = DependencyProperty.Register(
            nameof(HasItems), typeof(bool), typeof(AdaptiveGridView), new PropertyMetadata(default(bool)));

        public bool HasItems
        {
            get => (bool)GetValue(HasItemsProperty);
            set => SetValue(HasItemsProperty, value);
        }

        public double MinItemHeight
        {
            get => (double) GetValue(MinItemHeightProperty);
            set => SetValue(MinItemHeightProperty, value);
        }

        public static readonly DependencyProperty MinItemHeightProperty =
            DependencyProperty.Register(
                nameof(MinItemHeight),
                typeof(double),
                typeof(AdaptiveGridView),
                new PropertyMetadata(1.0, (s, a) =>
                {
                    if (!Double.IsNaN((double) a.NewValue))
                    {
                        ((AdaptiveGridView) s).InvalidateMeasure();
                    }
                }));


        public double MinItemWidth
        {
            get => (double) GetValue(MinimumItemWidthProperty);
            set => SetValue(MinimumItemWidthProperty, value);
        }

        public static readonly DependencyProperty MinimumItemWidthProperty =
            DependencyProperty.Register(
                nameof(MinItemWidth),
                typeof(double),
                typeof(AdaptiveGridView),
                new PropertyMetadata(1.0, (s, a) =>
                {
                    if (!Double.IsNaN((double) a.NewValue))
                    {
                        ((AdaptiveGridView) s).InvalidateMeasure();
                    }
                }));

        protected override Size MeasureOverride(Size availableSize)
        {
            if (this.ItemsPanelRoot is ItemsWrapGrid panel)
            {
                var numColumns = Math.Floor(availableSize.Width / MinItemWidth);
                numColumns = numColumns == 0 ? 1 : numColumns;
                var numRows = Math.Ceiling(this.Items.Count / numColumns);

                var itemWidth = availableSize.Width / numColumns;
                var aspectRatio = MinItemHeight / MinItemWidth;
                var itemHeight = itemWidth * aspectRatio;

                panel.ItemWidth = itemWidth;
                panel.ItemHeight = itemHeight;
            }

            return base.MeasureOverride(availableSize);
        }
    }
}
