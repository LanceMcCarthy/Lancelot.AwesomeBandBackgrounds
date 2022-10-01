using System.Numerics;
using Windows.Foundation.Metadata;
using Windows.Graphics.Effects;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using BandCentral.Models.Extensions;
using BandCentral.Uwp.Common;
using Microsoft.Graphics.Canvas.Effects;
using Microsoft.Xaml.Interactivity;

namespace BandCentral.Uwp.Behaviors
{
    public class HeaderDesaturationBehavior : Behavior<FrameworkElement>
    {
        public static readonly DependencyProperty HeaderImageProperty = DependencyProperty.Register(
            "HeaderImage",
            typeof(UIElement),
            typeof(HeaderDesaturationBehavior),
            new PropertyMetadata(default(UIElement), (o, a) =>
            {
                var b = o as HeaderDesaturationBehavior;
                b?.ApplyMovement();
            }));

        public UIElement HeaderImage
        {
            get { return (UIElement) GetValue(HeaderImageProperty); }
            set { SetValue(HeaderImageProperty, value); }
        }

        public static readonly DependencyProperty ScaleVectorMultiplierProperty = DependencyProperty.Register(
            "ScaleVectorMultiplier", typeof(double), typeof(HeaderDesaturationBehavior), new PropertyMetadata(1.5));

        public double ScaleVectorMultiplier
        {
            get { return (double) GetValue(ScaleVectorMultiplierProperty); }
            set { SetValue(ScaleVectorMultiplierProperty, value); }
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            ApplyMovement();
        }

        private void ApplyMovement()
        {
            if (!ApiInformation.IsMethodPresent("Windows.UI.Xaml.Hosting.ElementCompositionPreview", "GetScrollViewerManipulationPropertySet"))
                return;

            //make sure our properties are populated
            if (HeaderImage == null) return;
            //if (HeaderContainer == null) return;
            if (AssociatedObject == null) return;

            //get the ScrollViewer from the AssociatedObject. If it IS a Scrollviewer, then good. If not, then find the ScrollViewer (i.e. ListView)
            var scroller = AssociatedObject as ScrollViewer ?? AssociatedObject.GetChildOfType<ScrollViewer>();
            if (scroller == null) return;

            var scrollerViewerManipulation = ElementCompositionPreview.GetScrollViewerManipulationPropertySet(scroller);

            //get the compositor
            Compositor compositor = scrollerViewerManipulation.Compositor;

            ExpressionAnimation scaleExpression = compositor.CreateExpressionAnimation("Scale (ScaleVector, ScaleVectorMultiplier * ScrollManipululation)");
            scaleExpression.SetVector2Parameter("ScaleVector", new Vector2(1, 1));
            scaleExpression.SetScalarParameter("ScaleVectorMultiplier", (float) ScaleVectorMultiplier);
            scaleExpression.SetReferenceParameter("ScrollManipululation", scrollerViewerManipulation);

            Visual imageElement = ElementCompositionPreview.GetElementVisual(HeaderImage);

            imageElement.StartAnimation("Visual.Size", scaleExpression);

            IGraphicsEffect graphicsEffect = new Microsoft.Graphics.Canvas.Effects.ArithmeticCompositeEffect
            {
                Source1 = new CompositionEffectSourceParameter("source1"),
                Source2 = new SaturationEffect
                {
                    Saturation = 0,
                    Source = new CompositionEffectSourceParameter("source2")
                },
                MultiplyAmount = 0,
                Source1Amount = 0.5f,
                Source2Amount = 0.5f,
                Offset = 0
            };
        }
    }
}
