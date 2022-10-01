using System;
using Windows.Foundation.Metadata;
using Windows.Graphics.Display;
using Windows.System.Profile;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace BandCentral.Uwp.Triggers
{
    public class ContinuumModeTrigger : StateTriggerBase
    {
        private Page target;

        public ContinuumModeTrigger()
        {
            this.UpdateTrigger();
        }

        public static readonly DependencyProperty MaximumScreenSizeForMobileProperty = DependencyProperty.Register(
            nameof(MaximumScreenSizeForMobile), typeof(double), typeof(ContinuumModeTrigger), new PropertyMetadata(6d, MaxScreenSizeChanged));
        
        public double MaximumScreenSizeForMobile
        {
            get => (double) GetValue(MaximumScreenSizeForMobileProperty);
            set => SetValue(MaximumScreenSizeForMobileProperty, value);
        }

        public Page Target
        {
            get => target;
            set
            {
                if (target != value)
                {
                    if (target != null)
                        target.SizeChanged -= Page_SizeChanged;
                    target = value;

                    if (target != null)
                        target.SizeChanged += Page_SizeChanged;
                    this.UpdateTrigger();
                }
            }
        }

        private static void MaxScreenSizeChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            ((ContinuumModeTrigger)o)?.UpdateTrigger();
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.UpdateTrigger();
        }

        private void UpdateTrigger()
        {
            if (ApiInformation.IsPropertyPresent(nameof(DisplayInformation), "DiagonalSizeInInches"))
            {
                var size = DisplayInformation.GetForCurrentView().DiagonalSizeInInches;
                if (size.HasValue && size.Value > MaximumScreenSizeForMobile
                    && UIViewSettings.GetForCurrentView().UserInteractionMode == UserInteractionMode.Mouse
                    && AnalyticsInfo.VersionInfo.DeviceFamily.Equals("Windows.Mobile", StringComparison.CurrentCultureIgnoreCase))
                {
                    this.SetActive(true);
                    return;
                }
            }
            
            //if all conditions are not met above, then we're still on a phone
            this.SetActive(false);
        }
    }
}
