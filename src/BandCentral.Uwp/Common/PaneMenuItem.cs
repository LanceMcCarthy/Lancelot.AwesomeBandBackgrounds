using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using BandCentral.Uwp.Annotations;

namespace BandCentral.Uwp.Common
{
    public class PaneMenuItem : INotifyPropertyChanged
    {
        private string _glyphIcon;
        private string _label;
        private Type _destinationType;
        private Uri _destinationUri;
        private object _arguments = null;

        public string GlyphIcon
        {
            get { return _glyphIcon; }
            set { _glyphIcon = value; OnPropertyChanged(); }
        }

        public string Label
        {
            get { return _label; }
            set { _label = value; OnPropertyChanged(); }
        }

        public Type DestinationType
        {
            get { return _destinationType; }
            set { _destinationType = value; OnPropertyChanged(); }
        }

        public Uri DestinationUri
        {
            get { return _destinationUri; }
            set { _destinationUri = value; OnPropertyChanged(); }
        }
        
        public object Arguments
        {
            get { return _arguments; }
            set { _arguments = value; OnPropertyChanged(); }
        }

        #region INPC

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion


    }
}
