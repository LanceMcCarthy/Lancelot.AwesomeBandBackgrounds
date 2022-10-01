using System.Runtime.Serialization;
using BandCentral.Models.Common;

namespace BandCentral.Models.Favorites
{
    [KnownType(typeof(BandColorTheme))]
    [DataContract]
    public class BackgroundFav : FavBase
    {
        private BandColorTheme theme;
        private bool isUserPhoto;
        private string photoId;
        
        [DataMember]
        public string PhotoId
        {
            get => photoId;
            set => SetProperty(ref photoId, value);
        }
        
        [DataMember]
        public bool IsUserPhoto
        {
            get => isUserPhoto;
            set => SetProperty(ref isUserPhoto, value);
        }

        [DataMember]
        public BandColorTheme Theme
        {
            get => theme ?? (theme = new BandColorTheme());
            set => SetProperty(ref theme, value);
        }

    }
}
