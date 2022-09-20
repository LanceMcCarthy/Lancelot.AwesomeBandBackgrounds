using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using Windows.UI;
using Windows.UI.Xaml.Media;

namespace BandCentral.UwpBackgroundTasks
{
    [DataContract]
    public sealed class BandColorTheme :  INotifyPropertyChanged
    {
        private SolidColorBrush baseBrush;
        private SolidColorBrush highlightBrush;
        private SolidColorBrush lowLightBrush;
        private SolidColorBrush secondaryBrush;
        private SolidColorBrush highContrastBrush;
        private SolidColorBrush mutedBrush;
        private Color baseColor;
        private Color highlightColor;
        private Color lowLightColor;
        private Color secondaryColor;
        private Color highContrastColor;
        private Color mutedColor;

        public BandColorTheme()
        {
            this.BaseBrush = new SolidColorBrush(Colors.DarkGray);
            this.HighlightBrush = new SolidColorBrush(Colors.DarkGray);
            this.LowLightBrush = new SolidColorBrush(Colors.DarkGray);
            this.SecondaryBrush = new SolidColorBrush(Colors.DarkGray);
            this.HighContrastBrush = new SolidColorBrush(Colors.DarkGray);
            this.MutedBrush = new SolidColorBrush(Colors.DarkGray);
        }

        public BandColorTheme(Color baseColor, Color highlightColor, Color lowLightColor, Color secondaryColor, Color highContrastColor, Color mutedColor)
        {
            this.BaseColor = baseColor;
            this.HighlightColor = highlightColor;
            this.LowLightColor = lowLightColor;
            this.SecondaryColor = secondaryColor;
            this.HighContrastColor = highContrastColor;
            this.MutedColor = mutedColor;
        }

        [DataMember]
        public Color BaseColor
        {
            get { return baseColor; }
            set
            {
                baseColor = value;
                baseBrush = new SolidColorBrush(value);
                OnPropertyChanged();
            }
        }

        [IgnoreDataMember]
        public SolidColorBrush BaseBrush
        {
            get { return baseBrush; }
            set
            {
                baseBrush = value;
                baseColor = value.Color;
                OnPropertyChanged();
            }
        }

        [DataMember]
        public Color HighlightColor
        {
            get { return highlightColor; }
            set
            {
                highlightColor = value;
                highlightBrush = new SolidColorBrush(value);
                OnPropertyChanged();
            }
        }

        [IgnoreDataMember]
        public SolidColorBrush HighlightBrush
        {
            get { return highlightBrush; }
            set
            {
                highlightBrush = value;
                highlightColor = value.Color;
                OnPropertyChanged();
            }
        }

        [DataMember]
        public Color LowLightColor
        {
            get { return lowLightColor; }
            set
            {
                lowLightColor = value;
                lowLightBrush = new SolidColorBrush(value);
                OnPropertyChanged();
            }
        }

        [IgnoreDataMember]
        public SolidColorBrush LowLightBrush
        {
            get { return lowLightBrush; }
            set
            {
                lowLightBrush = value; 
                lowLightColor = value.Color;
                OnPropertyChanged();
            }
        }

        [DataMember]
        public Color SecondaryColor
        {
            get { return secondaryColor; }
            set
            {
                secondaryColor = value;
                secondaryBrush = new SolidColorBrush(value);
                OnPropertyChanged();
            }
        }

        [IgnoreDataMember]
        public SolidColorBrush SecondaryBrush
        {
            get { return secondaryBrush; }
            set
            {
                secondaryBrush = value;
                secondaryColor = value.Color;
                OnPropertyChanged();
            }
        }

        [DataMember]
        public Color HighContrastColor
        {
            get { return highContrastColor; }
            set
            {
                highContrastColor = value;
                highContrastBrush = new SolidColorBrush(value);
                OnPropertyChanged();
            }
        }

        [IgnoreDataMember]
        public SolidColorBrush HighContrastBrush
        {
            get { return highContrastBrush; }
            set
            {
                highContrastBrush = value;
                highContrastColor = value.Color;
                OnPropertyChanged();
            }
        }

        [DataMember]
        public Color MutedColor
        {
            get { return mutedColor; }
            set
            {
                mutedColor = value;
                mutedBrush = new SolidColorBrush(value);
                OnPropertyChanged();
            }
        }

        [IgnoreDataMember]
        public SolidColorBrush MutedBrush
        {
            get { return mutedBrush; }
            set
            {
                mutedBrush = value;
                mutedColor = value.Color;
                OnPropertyChanged();
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
