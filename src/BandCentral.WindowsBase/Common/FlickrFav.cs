using System;
using System.Runtime.Serialization;
using BandCentralBase.Common;

namespace BandCentral.WindowsBase.Common
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
            get { return isExpanded; }
            set { isExpanded = value; OnPropertyChanged(); }
        }

        [DataMember]
        public FavPalette Palette
        {
            get { return palette ?? ( palette = new FavPalette()); }
            set { palette = value; OnPropertyChanged(); }
        }

        [DataMember]
        public BandColorTheme Theme
        {
            get { return theme ?? (theme = new BandColorTheme()); }
            set { theme = value; OnPropertyChanged(); }
        }

        public string DownloadProgress
        {
            get { return downloadProgress; }
            set { downloadProgress = value; OnPropertyChanged(); }
        }
    }
}
