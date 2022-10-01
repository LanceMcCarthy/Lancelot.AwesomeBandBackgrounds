using System;
using System.Runtime.Serialization;
using BandCentral.Models.Common;

namespace BandCentral.Models.Favorites
{
    [KnownType(typeof(FavPalette))]
    [KnownType(typeof(BandColorTheme))]
    [DataContract]
    public class FlickrFav : FavBase
    {
        private bool isExpanded;
        private FavPalette palette;
        private BandColorTheme theme;
        private string downloadProgress;
        
        [DataMember]
        public FlickrNet.Photo Photo { get; set; }

        [DataMember]
        public DateTime DateFavorited { get; set; }
        
        [DataMember]
        public bool IsExpanded
        {
            get => isExpanded;
            set { isExpanded = value; OnPropertyChanged(); }
        }

        [DataMember]
        public FavPalette Palette
        {
            get => palette ?? ( palette = new FavPalette());
            set { palette = value; OnPropertyChanged(); }
        }

        [DataMember]
        public BandColorTheme Theme
        {
            get => theme ?? (theme = new BandColorTheme());
            set { theme = value; OnPropertyChanged(); }
        }

        public string DownloadProgress
        {
            get => downloadProgress;
            set { downloadProgress = value; OnPropertyChanged(); }
        }
    }
}
