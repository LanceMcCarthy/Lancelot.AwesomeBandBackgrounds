// Lance McCarthy 2013-2023 MIT
// Free to use, maintain attribution to original
// https://github.com/LanceMcCarthy/Lancelot.AwesomeBandBackgrounds

using System;
using System.ComponentModel;
using System.Diagnostics;
using Windows.Foundation;

namespace BandCentral.Uwp.Controls.ImageCropper.Helpers
{
    public class SelectedRegion : INotifyPropertyChanged
    {
        private const string TopLeftCornerCanvasLeftPropertyName = "TopLeftCornerCanvasLeft";
        private const string TopLeftCornerCanvasTopPropertyName = "TopLeftCornerCanvasTop";
        private const string BottomRightCornerCanvasLeftPropertyName = "BottomRightCornerCanvasLeft";
        private const string BottomRightCornerCanvasTopPropertyName = "BottomRightCornerCanvasTop";
        private const string OutterRectPropertyName = "OuterRect";
        private const string SelectedRectPropertyName = "SelectedRect";

        public const string TopLeftCornerName = "TopLeftCorner";
        public const string TopRightCornerName = "TopRightCorner";
        public const string BottomLeftCornerName = "BottomLeftCorner";
        public const string BottomRightCornerName = "BottomRightCorner";

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// The minimum size of the selected region
        /// </summary>
        public double MinSelectRegionSize { get; set; }

        private double topLeftCornerCanvasLeft;

        /// <summary>
        /// The Canvas.Left property of the TopLeft corner.
        /// </summary>
        public double TopLeftCornerCanvasLeft
        {
            get => topLeftCornerCanvasLeft;
            protected set
            {
                if (Math.Abs(topLeftCornerCanvasLeft - value) > 0.001)
                {
                    topLeftCornerCanvasLeft = value;
                    this.OnPropertyChanged(TopLeftCornerCanvasLeftPropertyName);
                }
            }
        }

        private double topLeftCornerCanvasTop;

        /// <summary>
        /// The Canvas.Top property of the TopLeft corner.
        /// </summary>
        public double TopLeftCornerCanvasTop
        {
            get => topLeftCornerCanvasTop;
            protected set
            {
                if (Math.Abs(topLeftCornerCanvasTop - value) > 0.001)
                {
                    topLeftCornerCanvasTop = value;
                    this.OnPropertyChanged(TopLeftCornerCanvasTopPropertyName);
                }
            }
        }

        private double bottomRightCornerCanvasLeft;

        /// <summary>
        /// The Canvas.Left property of the BottomRight corner.
        /// </summary>
        public double BottomRightCornerCanvasLeft
        {
            get => bottomRightCornerCanvasLeft;
            protected set
            {
                if (Math.Abs(bottomRightCornerCanvasLeft - value) > 0.001)
                {
                    bottomRightCornerCanvasLeft = value;
                    this.OnPropertyChanged(BottomRightCornerCanvasLeftPropertyName);
                }
            }
        }

        private double bottomRightCornerCanvasTop;

        /// <summary>
        /// The Canvas.Top property of the BottomRight corner.
        /// </summary>
        public double BottomRightCornerCanvasTop
        {
            get => bottomRightCornerCanvasTop;
            protected set
            {
                if (Math.Abs(bottomRightCornerCanvasTop - value) > 0.001)
                {
                    bottomRightCornerCanvasTop = value;
                    this.OnPropertyChanged(BottomRightCornerCanvasTopPropertyName);
                }
            }
        }

        private Rect outerRect;

        /// <summary>
        /// The outer rect. The non-selected region can be represented by the 
        /// OuterRect and the SelectedRect.
        /// </summary>
        public Rect OuterRect
        {
            get => outerRect;
            set
            {
                if (outerRect == value) 
                    return;

                outerRect = value;
                this.OnPropertyChanged(OutterRectPropertyName);
            }
        }

        private Rect selectedRect;

        /// <summary>
        /// The selected region, which is represented by the four corners.
        /// </summary>
        public Rect SelectedRect
        {
            get => selectedRect;
            protected set
            {
                if (selectedRect == value)
                    return;

                selectedRect = value;
                this.OnPropertyChanged(SelectedRectPropertyName);
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            // When the corner is moved, update the SelectedRect.
            if (propertyName == TopLeftCornerCanvasLeftPropertyName ||
                propertyName == TopLeftCornerCanvasTopPropertyName ||
                propertyName == BottomRightCornerCanvasLeftPropertyName ||
                propertyName == BottomRightCornerCanvasTopPropertyName)
            {
                SelectedRect = new Rect(
                    TopLeftCornerCanvasLeft,
                    TopLeftCornerCanvasTop,
                    BottomRightCornerCanvasLeft - TopLeftCornerCanvasLeft,
                    BottomRightCornerCanvasTop - topLeftCornerCanvasTop);
            }
        }


        public void ResetCorner(double tll, double tlt, double brl, double brt)
        {
            this.TopLeftCornerCanvasLeft = tll;
            this.TopLeftCornerCanvasTop = tlt;
            this.BottomRightCornerCanvasLeft = brl;
            this.BottomRightCornerCanvasTop = brt;
        }

        /// <summary>
        /// Update the Canvas.Top and Canvas.Left of the corner.
        /// </summary>
        public void UpdateCorner(string cornerName, double leftUpdate, double topUpdate)
        {
            Debug.WriteLine($"UpdateSelectedRect() Was Performed {SelectedRect.Width}w X {SelectedRect.Height}h - Ratio: {SelectedRect.Height / SelectedRect.Width}");

            UpdateCorner(cornerName, leftUpdate, topUpdate, this.MinSelectRegionSize, this.MinSelectRegionSize);
        }

        /// <summary>
        /// Update the Canvas.Top and Canvas.Left of the corner.
        /// </summary>
        public void UpdateCorner(string cornerName, double leftUpdate, double topUpdate, double minWidthSize, double minHeightSize)
        {
            switch (cornerName)
            {
                case SelectedRegion.TopLeftCornerName:
                    TopLeftCornerCanvasLeft = ValidateValue(topLeftCornerCanvasLeft + leftUpdate, 0, bottomRightCornerCanvasLeft - minWidthSize);
                    TopLeftCornerCanvasTop = ValidateValue(topLeftCornerCanvasTop + topUpdate, 0, bottomRightCornerCanvasTop - minHeightSize);
                    break;
                case SelectedRegion.TopRightCornerName:
                    BottomRightCornerCanvasLeft = ValidateValue(bottomRightCornerCanvasLeft + leftUpdate, topLeftCornerCanvasLeft + minWidthSize, outerRect.Width);
                    TopLeftCornerCanvasTop = ValidateValue(topLeftCornerCanvasTop + topUpdate, 0, bottomRightCornerCanvasTop - minHeightSize);
                    break;
                case SelectedRegion.BottomLeftCornerName:
                    TopLeftCornerCanvasLeft = ValidateValue(topLeftCornerCanvasLeft + leftUpdate, 0, bottomRightCornerCanvasLeft - minWidthSize);
                    BottomRightCornerCanvasTop = ValidateValue(bottomRightCornerCanvasTop + topUpdate, topLeftCornerCanvasTop + minHeightSize, outerRect.Height);
                    break;
                case SelectedRegion.BottomRightCornerName:
                    BottomRightCornerCanvasLeft = ValidateValue(bottomRightCornerCanvasLeft + leftUpdate, topLeftCornerCanvasLeft + minWidthSize, outerRect.Width);
                    BottomRightCornerCanvasTop = ValidateValue(bottomRightCornerCanvasTop + topUpdate, topLeftCornerCanvasTop + minHeightSize, outerRect.Height);
                    break;
                default:
                    throw new ArgumentException("cornerName: " + cornerName + "  is not recognized.");
            }
        }

        private double ValidateValue(double tempValue, double from, double to)
        {
            if (tempValue < from)
            {
                tempValue = from;
            }

            if (tempValue > to)
            {
                tempValue = to;
            }

            return tempValue;
        }

        /// <summary>
        /// Update the SelectedRect when it is moved or scaled.
        /// </summary>
        public void UpdateSelectedRect(double scale, double leftUpdate, double topUpdate) //I added an aspect ratio param
        {
            double width = bottomRightCornerCanvasLeft - topLeftCornerCanvasLeft;
            double height = bottomRightCornerCanvasTop - topLeftCornerCanvasTop;

            if (Math.Abs(scale - 1) > 0.01)
            {
                double scaledLeftUpdate = width * (scale - 1) / 2;
                double scaledTopUpdate = height * (scale - 1) / 2;

                if (scale > 1)
                {
                    this.UpdateCorner(SelectedRegion.BottomRightCornerName, scaledLeftUpdate, scaledTopUpdate);
                    this.UpdateCorner(SelectedRegion.TopLeftCornerName, -scaledLeftUpdate, -scaledTopUpdate);
                }
                else
                {
                    this.UpdateCorner(SelectedRegion.TopLeftCornerName, -scaledLeftUpdate, -scaledTopUpdate);
                    this.UpdateCorner(SelectedRegion.BottomRightCornerName, scaledLeftUpdate, scaledTopUpdate);
                }

                return;
            }

            double minWidth = Math.Max(this.MinSelectRegionSize, width * scale);
            double minHeight = Math.Max(this.MinSelectRegionSize, height * scale);

            switch (leftUpdate >= 0)
            {
                // Move towards BottomRight: Move BottomRightCorner first, and then move TopLeftCorner.
                case true when topUpdate >= 0:
                    this.UpdateCorner(SelectedRegion.BottomRightCornerName, leftUpdate, topUpdate, minWidth, minHeight);
                    this.UpdateCorner(SelectedRegion.TopLeftCornerName, leftUpdate, topUpdate, minWidth, minHeight);
                    break;
                // Move towards TopRight: Move TopRightCorner first, and then move BottomLeftCorner.
                case true when topUpdate < 0:
                    this.UpdateCorner(SelectedRegion.TopRightCornerName, leftUpdate, topUpdate, minWidth, minHeight);
                    this.UpdateCorner(SelectedRegion.BottomLeftCornerName, leftUpdate, topUpdate, minWidth, minHeight);
                    break;
                // Move towards BottomLeft: Move BottomLeftCorner first, and then move TopRightCorner.
                default:
                {
                    if (leftUpdate < 0 && topUpdate >= 0)
                    {
                        this.UpdateCorner(SelectedRegion.BottomLeftCornerName, leftUpdate, topUpdate, minWidth, minHeight);
                        this.UpdateCorner(SelectedRegion.TopRightCornerName, leftUpdate, topUpdate, minWidth, minHeight);
                    }

                    // Move towards TopLeft: Move TopLeftCorner first, and then move BottomRightCorner.
                    else if (leftUpdate < 0 && topUpdate < 0)
                    {
                        this.UpdateCorner(SelectedRegion.TopLeftCornerName, leftUpdate, topUpdate, minWidth, minHeight);
                        this.UpdateCorner(SelectedRegion.BottomRightCornerName, leftUpdate, topUpdate, minWidth, minHeight);
                    }

                    break;
                }
            }
        }
    }
}
