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
        public static readonly DependencyProperty MaximumScreenSizeForMobileProperty = DependencyProperty.Register(
            "MaximumScreenSizeForMobile", typeof(double), typeof(ContinuumModeTrigger), new PropertyMetadata(6d, (o, e) =>
            {
                ((ContinuumModeTrigger)o)?.UpdateTrigger();
            }));

        public double MaximumScreenSizeForMobile
        {
            get { return (double) GetValue(MaximumScreenSizeForMobileProperty); }
            set { SetValue(MaximumScreenSizeForMobileProperty, value); }
        }
        public ContinuumModeTrigger()
        {
            this.UpdateTrigger();
        }

        private Page target;
        public Page Target
        {
            get
            {
                return target;
            }
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
