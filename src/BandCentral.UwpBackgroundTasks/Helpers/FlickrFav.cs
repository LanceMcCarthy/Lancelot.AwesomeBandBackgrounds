using System;
using System.Runtime.Serialization;
using BandCentral.UwpBackgroundTasks.Models;
using CommonHelpers.Common;

namespace BandCentral.UwpBackgroundTasks.Helpers
{
    [KnownType(typeof(FavPalette))]
    [KnownType(typeof(BandColorTheme))]
    [DataContract]
    public sealed class FlickrFav : BindableBase
    {
        private bool isExpanded;
        private FavPalette palette;
        private BandColorTheme theme;
        private string downloadProgress;
        private string localImageFilePath;
        private string localImageFileName;
        private bool hasCustomTheme;
        private bool useCustomTheme;
        private bool isBackgroundFav;

        [DataMember]
        public Photo Photo { get; set; }

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
            get => palette ?? (palette = new FavPalette());
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
