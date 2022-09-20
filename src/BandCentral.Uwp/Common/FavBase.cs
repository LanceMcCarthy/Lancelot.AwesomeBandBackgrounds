using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using BandCentralBase.Annotations;

namespace BandCentralBase.Common
{
    [DataContract]
    public class FavBase : INotifyPropertyChanged
    {
        private string localImageFilePath;
        private string localImageFileName;
        private bool hasCustomTheme;
        private bool useCustomTheme;
        private bool isBackgroundFav;

        [DataMember]
        public string LocalImageFilePath
        {
            get { return localImageFilePath; }
            set { localImageFilePath = value; OnPropertyChanged(); }
        }

        [DataMember]
        public string LocalImageFileName
        {
            get { return localImageFileName; }
            set { localImageFileName = value; OnPropertyChanged(); }
        }

        [DataMember]
        public bool HasCustomTheme
        {
            get { return hasCustomTheme; }
            set { hasCustomTheme = value; OnPropertyChanged(); }
        }

        [DataMember]
        public bool UseCustomTheme
        {
            get { return useCustomTheme; }
            set { useCustomTheme = value; OnPropertyChanged(); }
        }

        [DataMember]
        public bool IsBackgroundFav
        {
            get { return isBackgroundFav; }
            set { isBackgroundFav = value; OnPropertyChanged();}
        }

        #region INPC

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
