using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using BandCentral.WindowsBase.Annotations;
using BandCentralBase.Common;

namespace BandCentral.WindowsBase.Common
{
    //[KnownType(typeof(BandColorTheme))]
    //[DataContract]
    //public class BackgroundFav : FavBase
    //{
    //    private BandColorTheme theme;
    //    private bool isUserPhoto;
    //    private string photoId;

    //    //Moved to FavBase
    //    //private string localImageFilePath;
    //    //private string localImageFileName;

    //    [DataMember]
    //    public string PhotoId
    //    {
    //        get { return photoId; }
    //        set { photoId = value; OnPropertyChanged(); }
    //    }

    //    //moved to base
    //    //[DataMember]
    //    //public string LocalImageFilePath
    //    //{
    //    //    get { return localImageFilePath; }
    //    //    set { localImageFilePath = value; OnPropertyChanged(); }
    //    //}

    //    //[DataMember]
    //    //public string LocalImageFileName
    //    //{
    //    //    get { return localImageFileName; }
    //    //    set { localImageFileName = value; OnPropertyChanged(); }
    //    //}

    //    [DataMember]
    //    public bool IsUserPhoto
    //    {
    //        get { return isUserPhoto; }
    //        set { isUserPhoto = value; OnPropertyChanged(); }
    //    }

    //    [DataMember]
    //    public BandColorTheme Theme
    //    {
    //        get { return theme ?? (theme = new BandColorTheme()); }
    //        set { theme = value; OnPropertyChanged(); }
    //    }

    //}
}
