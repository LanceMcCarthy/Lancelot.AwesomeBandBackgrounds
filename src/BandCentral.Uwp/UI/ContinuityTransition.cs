using BandCentral.Uwp.Controls;
using System;
using System.Numerics;
using Windows.Foundation;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Media;

namespace BandCentral.Uwp.UI
{
    public class ContinuityTransition
    {
        private UIElement host;
        private UIElement parent;
        private object payload;
        private SpriteVisual sprite;
        private bool imageLoaded;
        private bool animationCompleted;
        private CompositionScopedBatch scopeBatch;
        private CompositionImage targetImage;

        public object Payload => payload;
        public object Host => host;

        public ContinuityTransition()
        {
        }

        public void Initialize(UIElement hostElement, CompositionImage sourceElement)
        {
            this.host = hostElement;
            parent = hostElement;

            // Make a copy of the sourceElement's sprite so we can hand it off to the next page
            SpriteVisual sourceSprite = sourceElement.SpriteVisual;
            Compositor compositor = sourceSprite.Compositor;
            sprite = compositor.CreateSpriteVisual();
            sprite.Size = sourceSprite.Size;
            sprite.Brush = sourceElement.SurfaceBrush;

            // We're going to use the backing surface, make sure it doesn't get released
            sourceElement.SharedSurface = true;

            // Determine the offset from the hostElement to the source element used in the transition
            GeneralTransform coordinate = sourceElement.TransformToVisual(parent);
            Point position = coordinate.TransformPoint(new Point(0, 0));

            // Set the sprite to that offset relative to the hostElement
            sprite.Offset = new Vector3((float) position.X, (float) position.Y, 0);

            // Set the sprite as the content under the hostElement
            ElementCompositionPreview.SetElementChildVisual(parent, sprite);
        }

        public void Initialize(UIElement hostElement, CompositionImage sourceElement, object payload)
        {
            this.host = hostElement;
            parent = hostElement;
            this.payload = payload;

            // Make a copy of the sourceElement's sprite so we can hand it off to the next page
            SpriteVisual sourceSprite = sourceElement.SpriteVisual;
            Compositor compositor = sourceSprite.Compositor;
            sprite = compositor.CreateSpriteVisual();
            sprite.Size = sourceSprite.Size;
            sprite.Brush = sourceElement.SurfaceBrush;

            // We're going to use the backing surface, make sure it doesn't get released
            sourceElement.SharedSurface = true;

            // Determine the offset from the hostElement to the source element used in the transition
            GeneralTransform coordinate = sourceElement.TransformToVisual(parent);
            Point position = coordinate.TransformPoint(new Point(0, 0));

            // Set the sprite to that offset relative to the hostElement
            sprite.Offset = new Vector3((float) position.X, (float) position.Y, 0);

            // Set the sprite as the content under the hostElement
            ElementCompositionPreview.SetElementChildVisual(parent, sprite);
        }

        public void Start(UIElement newParent, CompositionImage targetImage, ScrollViewer scrollViewer, UIElement animationTarget)
        {
            Visual transitionVisual = ElementCompositionPreview.GetElementChildVisual(parent);
            ElementCompositionPreview.SetElementChildVisual(parent, null);


            //
            // We need to reparent the transition visual under the new parent.  This is important to ensure
            // it's propertly clipped, etc.
            //

            GeneralTransform coordinate = newParent.TransformToVisual(parent);
            Point position = coordinate.TransformPoint(new Point(0, 0));

            Vector3 currentOffset = transitionVisual.Offset;
            currentOffset.X -= (float) position.X;
            currentOffset.Y -= (float) position.Y;
            transitionVisual.Offset = currentOffset;

            parent = newParent;
            this.targetImage = targetImage;

            // Move the transition visual to it's new parent
            ElementCompositionPreview.SetElementChildVisual(parent, transitionVisual);

            // Hide the target Image now since the handoff visual is still transitioning
            targetImage.Opacity = 0f;

            // Load image if necessary
            imageLoaded = targetImage.IsContentLoaded;
            if (!imageLoaded)
            {
                targetImage.ImageOpened += CompositionImage_ImageOpened;
            }

            //
            // Create a scoped batch around the animations.  When the batch completes, we know the animations
            // have finished and we can cleanup the transition related objects.
            //

            Compositor compositor = transitionVisual.Compositor;
            scopeBatch = compositor.CreateScopedBatch(CompositionBatchTypes.Animation);

            //
            // Determine the offset between the parent and the target UIElement.  This will be used to calculate the
            // target position we are animating to.
            //

            coordinate = targetImage.TransformToVisual(parent);
            position = coordinate.TransformPoint(new Point(0, 0));

            TimeSpan totalDuration = TimeSpan.FromMilliseconds(1000);
            Vector3KeyFrameAnimation offsetAnimation = compositor.CreateVector3KeyFrameAnimation();

            if (scrollViewer != null)
            {
                CompositionPropertySet scrollProperties = ElementCompositionPreview.GetScrollViewerManipulationPropertySet(scrollViewer);

                // Include the scroller offset as that is a factor
                position.X += scrollViewer.HorizontalOffset;
                position.Y += scrollViewer.VerticalOffset;


                //
                // Since the target position is relative to the target UIElement which can move, we need to construct
                // an expression to bind the target's position to the end position of our animation.
                //

                string expression = "Vector3(scrollingProperties.Translation.X, scrollingProperties.Translation.Y, 0) + itemOffset";
                offsetAnimation.InsertExpressionKeyFrame(1f, expression);
                offsetAnimation.SetReferenceParameter("scrollingProperties", scrollProperties);
                offsetAnimation.SetVector3Parameter("itemOffset", new Vector3((float) position.X, (float) position.Y, 0));
                offsetAnimation.Duration = totalDuration;
            }
            else
            {
                offsetAnimation.InsertKeyFrame(1, new Vector3((float) position.X, (float) position.Y, 0));
                offsetAnimation.Duration = totalDuration;
            }

            // Create size animation to change size of the visual
            Vector2KeyFrameAnimation sizeAnimation = compositor.CreateVector2KeyFrameAnimation();
            sizeAnimation.InsertKeyFrame(1f, new Vector2((float) targetImage.ActualWidth, (float) targetImage.ActualHeight));
            sizeAnimation.Duration = totalDuration;

            // Create the fade in animation for the other page content
            if (animationTarget != null)
            {
                Visual fadeVisual = ElementCompositionPreview.GetElementVisual(animationTarget);
                ScalarKeyFrameAnimation fadeIn = compositor.CreateScalarKeyFrameAnimation();
                fadeIn.InsertKeyFrame(0f, 0.0f);
                fadeIn.InsertKeyFrame(1f, 1.0f);
                fadeIn.Duration = totalDuration;
                fadeVisual.StartAnimation("Opacity", fadeIn);
            }

            //Start Animations 
            sprite.StartAnimation("Size", sizeAnimation);
            sprite.StartAnimation("Offset", offsetAnimation);

            //Scoped batch completed event
            scopeBatch.Completed += ScopeBatch_Completed;
            scopeBatch.End();

            // Clear the flag
            animationCompleted = false;
        }

        public void Cancel()
        {
            if (!Completed)
            {
                Complete(true);
            }
        }

        public bool Completed
        {
            get
            {
                // Either we aren't actively transitioning or the image and animation have completed
                return (sprite == null) || (imageLoaded && animationCompleted);
            }
        }

        private void Complete(bool forceComplete)
        {
            // If we're forcing completion, make sure the scope batch is cleaned up
            if (forceComplete && (scopeBatch != null))
            {
                CleanupScopeBatch();
            }

            // If we've completed the transition or we're forcing completion, cleanup
            if (forceComplete || (imageLoaded && animationCompleted))
            {
                sprite = null;

                // Clear the sprite from the UIElement
                ElementCompositionPreview.SetElementChildVisual(parent, null);

                // Clean up the image and show it
                if (targetImage != null)
                {
                    targetImage.ImageOpened -= CompositionImage_ImageOpened;

                    targetImage.Opacity = 1f;

                    targetImage = null;
                }
            }
        }

        private void CompositionImage_ImageOpened(object sender, RoutedEventArgs e)
        {
            imageLoaded = true;
            Complete(false);
        }

        private void ScopeBatch_Completed(object sender, CompositionBatchCompletedEventArgs args)
        {
            animationCompleted = true;
            Complete(false);

            CleanupScopeBatch();
        }

        private void CleanupScopeBatch()
        {
            if (scopeBatch != null)
            {
                scopeBatch.Completed -= ScopeBatch_Completed;
                scopeBatch.Dispose();
                scopeBatch = null;
            }
        }
    }
}
