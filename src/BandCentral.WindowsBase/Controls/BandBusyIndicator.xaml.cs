using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;

namespace BandCentral.WindowsBase.Controls
{
    public sealed partial class BandBusyIndicator : UserControl
    {
        public static readonly DependencyProperty DisplayMessageProperty = DependencyProperty.Register(
            "DisplayMessage", typeof (string), typeof (BandBusyIndicator), new PropertyMetadata(""));

        public string DisplayMessage
        {
            get { return (string) GetValue(DisplayMessageProperty); }
            set { SetValue(DisplayMessageProperty, value); }
        }

        public bool IsActive
        {
            get { return (bool) GetValue(IsActiveProperty); }
            set { SetValue(IsActiveProperty, value); }
        }

        public static readonly DependencyProperty IsActiveProperty = DependencyProperty.Register(
            "IsActive", typeof(bool), typeof(BandBusyIndicator), new PropertyMetadata(false, (o, args) =>
            {
                ((BandBusyIndicator) o).IsActivePropertyChanged(args);
            }));

        private void IsActivePropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (!isLoaded)
                return;

            if ((bool) e.NewValue)
            {
                TopDownBusyStory.RepeatBehavior = RepeatBehavior.Forever;

                this.Visibility = Visibility.Visible;

                if (IsReversed)
                    TopDownBusyStory.AutoReverse = true;

                TopDownBusyStory.Begin();
            }
            else
            {
                this.Visibility = Visibility.Collapsed;
                TopDownBusyStory.Stop();
            }
        }

        public static readonly DependencyProperty IsReversedProperty = DependencyProperty.Register(
            "IsReversed", typeof(bool), typeof(BandBusyIndicator), new PropertyMetadata(false));

        public bool IsReversed
        {
            get { return (bool) GetValue(IsReversedProperty); }
            set { SetValue(IsReversedProperty, value); }
        }

        private bool isLoaded;

        public BandBusyIndicator()
        {
            this.InitializeComponent();
            Loaded += BandBusyIndicator_Loaded;
        }

        private void BandBusyIndicator_Loaded(object sender, RoutedEventArgs e)
        {
            this.isLoaded = true;
            IsActive = IsActive;
            
        }
    }
}
