using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using BandCentral.Uwp.Common;
using Microsoft.Xaml.Interactivity;
using Windows.Foundation.Metadata;
using BandCentral.Models.Extensions;

namespace BandCentral.Uwp.Behaviors
{
    public class ParallaxBehavior : Behavior<FrameworkElement>
    {
        #region DependencyProperties

        /// <summary>
        /// Gets or sets the element that will parallax while scrolling.
        /// </summary>
        public UIElement ParallaxContent
        {
            get { return (UIElement)GetValue(ParallaxContentProperty); }
            set { SetValue(ParallaxContentProperty, value); }
        }

        public static readonly DependencyProperty ParallaxContentProperty = DependencyProperty.Register(
            "ParallaxContent",
            typeof(UIElement),
            typeof(ParallaxBehavior),
            new PropertyMetadata(null, OnParallaxContentChanged));

        /// <summary>
        /// Gets or sets the rate at which the ParallaxContent parallaxes.
        /// </summary>
        public double ParallaxMultiplier
        {
            get { return (double)GetValue(ParallaxMultiplierProperty); }
            set { SetValue(ParallaxMultiplierProperty, value); }
        }

        public static readonly DependencyProperty ParallaxMultiplierProperty = DependencyProperty.Register(
            "ParallaxMultiplier",
            typeof(double),
            typeof(ParallaxBehavior),
            new PropertyMetadata(0.3d));

        public static readonly DependencyProperty ClampMaxProperty = DependencyProperty.Register(
            "ClampMax", typeof (double), typeof (ParallaxBehavior), new PropertyMetadata(default(double)));

        /// <summary>
        /// Maximum distance you want the element to be offset by the scrolling
        /// </summary>
        public double ClampMax
        {
            get { return (double) GetValue(ClampMaxProperty); }
            set { SetValue(ClampMaxProperty, value); }
        }

        public static readonly DependencyProperty UseOpacityProperty = DependencyProperty.Register(
            "UseOpacity", typeof(bool), typeof(ParallaxBehavior), new PropertyMetadata(default(bool)));

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
            
            //if there is a clamp value set
            if (ClampMax > 0)
            {
                expression = compositor.CreateExpressionAnimation("-(Clamp((ScrollManipululation.Translation.Y * ParallaxMultiplier), 0, ClampMax))");
                
            }
            else
            {
                expression = compositor.CreateExpressionAnimation("-(ScrollManipululation.Translation.Y * ParallaxMultiplier)");
            }

            expression.SetScalarParameter("ParallaxMultiplier", (float)ParallaxMultiplier);
            expression.SetScalarParameter("ClampMax", (float)ClampMax);
            expression.SetReferenceParameter("ScrollManipululation", scrollerViewerManipulation);

            //---- expression animation for opacity -----//

            ExpressionAnimation opacityExpression = null;
            if (UseOpacity)
            {
                opacityExpression = compositor.CreateExpressionAnimation("Clamp(1 - (ScrollManipululation.Translation.Y * -0.01), 0, 1)");

                opacityExpression.SetReferenceParameter("ScrollManipululation", scrollerViewerManipulation);
                opacityExpression.SetScalarParameter("ClampMax", (float)ClampMax);
            }

            //---------begin animating----------//
            
            //get the visual
            Visual targetElement = ElementCompositionPreview.GetElementVisual(ParallaxContent);

            //start Y offset animation
            targetElement.StartAnimation("Offset.Y", expression);
            
            //If there's an opacity expression, fire it off as well
            if(opacityExpression != null)
                targetElement.StartAnimation("Opacity", opacityExpression);
        }

        private static void OnParallaxContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var b = d as ParallaxBehavior;
            b?.AssignParallax();
        }
        
    }
}
