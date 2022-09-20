using System;
using System.Diagnostics;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using BandCentralBase.Common;

namespace BandCentral.Uwp.Common
{
    public class AutoWidthItemsPanel : StackPanel
    {
        public static readonly DependencyProperty MinItemWidthProperty = DependencyProperty.Register(
            "MinItemWidth",
            typeof(double),
            typeof(AutoWidthItemsPanel),
            new PropertyMetadata(0));

        public static readonly DependencyProperty MinItemHeightProperty = DependencyProperty.Register(
            "MinItemHeight",
            typeof(double),
            typeof(AutoWidthItemsPanel),
            new PropertyMetadata(0));

        public double MinItemWidth
        {
            get { return (double)this.GetValue(MinItemWidthProperty); }
            set { this.SetValue(MinItemWidthProperty, value); }
        }

        public double MinItemHeight
        {
            get { return (double)this.GetValue(MinItemHeightProperty); }
            set { this.SetValue(MinItemHeightProperty, value); }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            var numColumns = Math.Floor(availableSize.Width / MinItemWidth);
            numColumns = numColumns == 0 ? 1 : numColumns;
            var numRows = Math.Ceiling(this.Children.Count / numColumns);
            
            var itemWidth = availableSize.Width / numColumns;
            var aspectRatio = MinItemHeight / MinItemWidth;
            var itemHeight = itemWidth * aspectRatio;

            foreach (var child in this.Children)
            {
                var item = child as GridViewItem;
                child.Measure(new Size(itemWidth, itemHeight));
            }

            return new Size(itemWidth * numColumns, itemHeight * numRows);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            var numColumns = Math.Floor(finalSize.Width / MinItemWidth);
            numColumns = numColumns == 0 ? 1 : numColumns;
            var numRows = Math.Ceiling(this.Children.Count / numColumns);
            
            var itemWidth = finalSize.Width / numColumns;
            var aspectRatio = MinItemHeight / MinItemWidth;
            var itemHeight = itemWidth * aspectRatio;

            int row = 0;
            int column = 0;

            foreach (var child in this.Children)
            {
                var item = child as GridViewItem;
                child.Arrange(new Rect(column * itemWidth, row * itemHeight, itemWidth, itemHeight));
                column++;

                if (column >= numColumns)
                {
                    column = 0;
                    row++;
                }
            }

            return finalSize;
        }
    }
}
