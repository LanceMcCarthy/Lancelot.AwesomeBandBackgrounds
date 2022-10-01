using Windows.Foundation.Metadata;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using BandCentral.Models.Extensions;
using BandCentral.Uwp.Common;
using Microsoft.Xaml.Interactivity;

namespace BandCentral.Uwp.Behaviors
{
    public class PivotParallaxBehavior : Behavior<FrameworkElement>
    {
        #region DependencyProperties

        public static readonly DependencyProperty ParallaxContentProperty = DependencyProperty.Register(
            "ParallaxContent", typeof(UIElement), typeof(PivotParallaxBehavior), new PropertyMetadata(null, OnParallaxContentChanged));

        /// <summary>
        /// Gets or sets the element that will parallax while scrolling.
        /// </summary>
        public UIElement ParallaxContent
        {
            get { return (UIElement) GetValue(ParallaxContentProperty); }
            set { SetValue(ParallaxContentProperty, value); }
        }

        public static readonly DependencyProperty ParallaxMultiplierProperty = DependencyProperty.Register(
            "ParallaxMultiplier", typeof(double), typeof(PivotParallaxBehavior), new PropertyMetadata(0.3d));

        /// <summary>
        /// Gets or sets the rate at which the ParallaxContent parallaxes.
        /// </summary>
        public double ParallaxMultiplier
        {
            get { return (double) GetValue(ParallaxMultiplierProperty); }
            set { SetValue(ParallaxMultiplierProperty, value); }
        }

        public static readonly DependencyProperty ClampMaxProperty = DependencyProperty.Register(
            "ClampMax", typeof(double), typeof(PivotParallaxBehavior), new PropertyMetadata(default(double)));

        /// <summary>
        /// Maximum distance you want the element to be offset by the scrolling
        /// </summary>
        public double ClampMax
        {
            get { return (double) GetValue(ClampMaxProperty); }
            set { SetValue(ClampMaxProperty, value); }
        }

        public static readonly DependencyProperty UseOpacityProperty = DependencyProperty.Register(
            "UseOpacity", typeof(bool), typeof(PivotParallaxBehavior), new PropertyMetadata(default(bool)));

        /// <summary>
        /// Enabling this will hide the target UI element after user scrolls 100 pixels down
        /// </summary>
        public bool UseOpacity
        {
            get { return (bool) GetValue(UseOpacityProperty); }
            set { SetValue(UseOpacityProperty, value); }
        }

        #endregion

        protected override void OnAttached()
        {
            base.OnAttached();
            AssignParallax();
        }

        private void AssignParallax()
        {
            //if we're running on earlier than 10586
            if (!ApiInformation.IsMethodPresent("Windows.UI.Xaml.Hosting.ElementCompositionPreview", "GetScrollViewerManipulationPropertySet"))
                return;

            if (ParallaxContent == null) return;
            if (AssociatedObject == null) return;

            var scroller = AssociatedObject as ScrollViewer ?? AssociatedObject.GetChildOfType<ScrollViewer>();
            if (scroller == null) return;

            CompositionPropertySet scrollerViewerManipulation = ElementCompositionPreview.GetScrollViewerManipulationPropertySet(scroller);
            Compositor compositor = scrollerViewerManipulation.Compositor;

            //---- expression animation for Y offset -----//
            ExpressionAnimation expression;

            //if there is a clamp value set, use clamp expression
            expression = compositor.CreateExpressionAnimation(ClampMax > 0 
                ? "-(Clamp((ScrollManipululation.Translation.Y * ParallaxMultiplier), 0, ClampMax))" 
                : "-(ScrollManipululation.Translation.Y * ParallaxMultiplier)");

            expression.SetScalarParameter("ParallaxMultiplier", (float) ParallaxMultiplier);
            expression.SetScalarParameter("ClampMax", (float) ClampMax);
            expression.SetReferenceParameter("ScrollManipululation", scrollerViewerManipulation);
            
            //---------begin animating----------//

            //get the visual
            Visual targetElement = ElementCompositionPreview.GetElementVisual(ParallaxContent);

            //start Y offset animation
            targetElement.StartAnimation("Offset.Y", expression);

            //animate size.y for the same amount being translated (this prevents the pivot from scrolling up)
            targetElement.StartAnimation("Size.Y", expression);
        }

        private static void OnParallaxContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var b = d as PivotParallaxBehavior;
            b?.AssignParallax();
        }
    }
}