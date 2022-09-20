using CommonHelpers.Common;
using System.Runtime.Serialization;

namespace BandCentral.Models.Favorites
{
    [DataContract]
    public class FavBase : BindableBase
    {
        private string localImageFilePath;
        private string localImageFileName;
        private bool hasCustomTheme;
        private bool useCustomTheme;
        private bool isBackgroundFav;

        [DataMember]
        public string LocalImageFilePath
        {
            get => localImageFilePath;
            set => SetProperty(ref localImageFilePath, value);
        }

        [DataMember]
        public string LocalImageFileName
        {
            get => localImageFileName;
            set => SetProperty(ref localImageFileName, value);
        }

        [DataMember]
        public bool HasCustomTheme
        {
            get => hasCustomTheme;
            set => SetProperty(ref hasCustomTheme, value);
        }

        [DataMember]
        public bool UseCustomTheme
        {
            get => useCustomTheme;
            set => SetProperty(ref useCustomTheme, value);
        }

        [DataMember]
        public bool IsBackgroundFav
        {
            get => isBackgroundFav;
            set => SetProperty(ref isBackgroundFav, value);
        }
    }
}
