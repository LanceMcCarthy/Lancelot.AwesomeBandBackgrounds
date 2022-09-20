using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Xml;

namespace BandCentral.UwpBackgroundTasks.Helpers
{
    public sealed class Photo : IFlickrParsable
    {
        private string urlSquare;
        private string urlLargeSquare;
        private string urlThumbnail;
        private string urlMedium;
        private string urlMedium640;
        private string urlMedium800;
        private string urlSmall;
        private string urlSmall320;
        private string urlLarge;
        private string urlOriginal;

        public Photo() => this.Tags = new Collection<string>();

        public Collection<string> Tags { get; set; }

        public string PhotoId { get; set; }

        public string UserId { get; set; }

        public string Secret { get; set; }

        public string Server { get; set; }

        public string Farm { get; set; }

        public string Title { get; set; }

        public bool IsPublic { get; set; }

        public bool IsFriend { get; set; }

        public bool IsFamily { get; set; }

        public LicenseType License { get; set; }

        public int OriginalWidth { get; set; }

        public int OriginalHeight { get; set; }

        public DateTime DateUploaded { get; set; }

        public DateTime LastUpdated { get; set; }

        public DateTime DateTaken { get; set; }

        public DateTime? DateAddedToGroup { get; set; }

        public DateTime? DateFavorited { get; set; }

        public string OwnerName { get; set; }

        public string IconServer { get; set; }

        public string IconFarm { get; set; }

        public string OriginalFormat { get; set; }

        public string OriginalSecret { get; set; }

        public string MachineTags { get; set; }

        public string WebUrl => string.Format((IFormatProvider)CultureInfo.InvariantCulture, "https://www.flickr.com/photos/{0}/{1}/", new object[2]
        {
            string.IsNullOrEmpty(this.PathAlias) ? (object) this.UserId : (object) this.PathAlias,
            (object) this.PhotoId
        });

        public string SquareThumbnailUrl => this.urlSquare ?? UtilityMethods.UrlFormat(this, "_s", "jpg");

        public int? SquareThumbnailWidth { get; set; }

        public int? SquareThumbnailHeight { get; set; }

        public string LargeSquareThumbnailUrl => this.urlLargeSquare ?? UtilityMethods.UrlFormat(this, "_q", "jpg");

        public int? LargeSquareThumbnailWidth { get; set; }

        public int? LargeSquareThumbnailHeight { get; set; }

        public string ThumbnailUrl => this.urlThumbnail ?? UtilityMethods.UrlFormat(this, "_t", "jpg");

        public int? ThumbnailWidth { get; set; }

        public int? ThumbnailHeight { get; set; }

        public string SmallUrl => this.urlSmall ?? UtilityMethods.UrlFormat(this, "_m", "jpg");

        public int? SmallWidth { get; set; }

        public int? SmallHeight { get; set; }

        public string Small320Url => this.urlSmall320 ?? UtilityMethods.UrlFormat(this, "_n", "jpg");

        public int? Small320Width { get; set; }

        public int? Small320Height { get; set; }

        public string Medium640Url => this.urlMedium640 ?? UtilityMethods.UrlFormat(this, "_z", "jpg");

        public int? Medium640Width { get; set; }

        public int? Medium640Height { get; set; }

        public string Medium800Url => this.urlMedium800 ?? UtilityMethods.UrlFormat(this, "_z", "jpg");

        public int? Medium800Width { get; set; }

        public int? Medium800Height { get; set; }

        public string Large1600Url { get; set; }

        public int? Large1600Width { get; set; }

        public int? Large1600Height { get; set; }

        public string Large2048Url { get; set; }

        public int? Large2048Width { get; set; }

        public int? Large2048Height { get; set; }

        public string MediumUrl => this.urlMedium ?? UtilityMethods.UrlFormat(this, string.Empty, "jpg");

        public int? MediumWidth { get; set; }

        public int? MediumHeight { get; set; }

        public string LargeUrl => this.urlLarge ?? UtilityMethods.UrlFormat(this, "_b", "jpg");

        public int? LargeWidth { get; set; }

        public int? LargeHeight { get; set; }

        public string OriginalUrl
        {
            get
            {
                if (this.urlOriginal != null)
                    return this.urlOriginal;
                return this.OriginalFormat == null || this.OriginalFormat.Length == 0 ? (string)null : UtilityMethods.UrlFormat(this, "_o", this.OriginalFormat);
            }
        }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public string PlaceId { get; set; }

        public string WoeId { get; set; }

        public GeoAccuracy Accuracy { get; set; }

        public GeoContext? GeoContext { get; set; }

        public bool? CanComment { get; set; }

        public bool? CanPrint { get; set; }

        public bool? CanDownload { get; set; }

        public bool? CanAddMeta { get; set; }

        public bool? CanBlog { get; set; }

        public bool? CanShare { get; set; }

        public int? Views { get; set; }

        public string Media { get; set; }

        public string PathAlias { get; set; }

        public string MediaStatus { get; set; }

        public string Description { get; set; }

        public GeoPermissions GeoPermissions { get; set; }

        public int? Rotation { get; set; }

        public int? CountFaves { get; set; }

        public int? CountComments { get; set; }

        public bool DoesLargeExist => this.urlLarge != null || this.OriginalHeight > 1280 || this.OriginalWidth > 1280;

        public bool DoesMediumExist => this.urlMedium != null || this.OriginalHeight > 500 || this.OriginalWidth > 500;

        void IFlickrParsable.Load(XmlReader reader)
        {
            this.Load(reader, false);
            if (!(reader.LocalName == "photo") || reader.NodeType != XmlNodeType.EndElement)
                return;
            reader.Read();
        }

        protected void Load(XmlReader reader, bool allowExtraAtrributes)
        {
            if (!(reader.LocalName != "photo") || !(reader.LocalName != "primary_photo_extras"))
                ;
            while (reader.MoveToNextAttribute())
            {
                switch (reader.LocalName)
                {
                    case "id":
                        this.PhotoId = reader.Value;
                        if (string.IsNullOrEmpty(reader.Value))
                        {
                            reader.Skip();
                            return;
                        }
                        continue;
                    case "owner":
                        this.UserId = reader.Value;
                        continue;
                    case "secret":
                        this.Secret = reader.Value;
                        continue;
                    case "server":
                        this.Server = reader.Value;
                        continue;
                    case "farm":
                        this.Farm = reader.Value;
                        continue;
                    case "title":
                        this.Title = reader.Value;
                        continue;
                    case "ispublic":
                        this.IsPublic = reader.Value == "1";
                        continue;
                    case "isfamily":
                        this.IsFamily = reader.Value == "1";
                        continue;
                    case "isfriend":
                        this.IsFriend = reader.Value == "1";
                        continue;
                    case "tags":
                        string str1 = reader.Value;
                        char[] chArray = new char[1] { ' ' };
                        foreach (string str2 in str1.Split(chArray))
                            this.Tags.Add(str2);
                        continue;
                    case "datetaken":
                        this.DateTaken = UtilityMethods.ParseDateWithGranularity(reader.Value).DateTime;
                        continue;
                    case "datetakengranularity":
                    case "isprimary":
                    case "is_primary":
                    case "has_comment":
                        continue;
                    case "dateupload":
                        this.DateUploaded = UtilityMethods.UnixTimestampToDate(reader.Value).DateTime;
                        continue;
                    case "license":
                        this.License = (LicenseType)int.Parse(reader.Value, (IFormatProvider)CultureInfo.InvariantCulture);
                        continue;
                    case "ownername":
                        this.OwnerName = reader.Value;
                        continue;
                    case "lastupdate":
                        this.LastUpdated = UtilityMethods.UnixTimestampToDate(reader.Value).DateTime;
                        continue;
                    case "originalformat":
                        this.OriginalFormat = reader.Value;
                        continue;
                    case "originalsecret":
                        this.OriginalSecret = reader.Value;
                        continue;
                    case "place_id":
                        this.PlaceId = reader.Value;
                        continue;
                    case "woeid":
                        this.WoeId = reader.Value;
                        continue;
                    case "accuracy":
                        this.Accuracy = (GeoAccuracy)reader.ReadContentAsInt();
                        continue;
                    case "latitude":
                        this.Latitude = reader.ReadContentAsDouble();
                        continue;
                    case "longitude":
                        this.Longitude = reader.ReadContentAsDouble();
                        continue;
                    case "machine_tags":
                        this.MachineTags = reader.Value;
                        continue;
                    case "o_width":
                        this.OriginalWidth = int.Parse(reader.Value, (IFormatProvider)CultureInfo.InvariantCulture);
                        continue;
                    case "o_height":
                        this.OriginalHeight = int.Parse(reader.Value, (IFormatProvider)CultureInfo.InvariantCulture);
                        continue;
                    case "views":
                        this.Views = new int?(int.Parse(reader.Value, (IFormatProvider)CultureInfo.InvariantCulture));
                        continue;
                    case "media":
                        this.Media = reader.Value;
                        continue;
                    case "media_status":
                        this.MediaStatus = reader.Value;
                        continue;
                    case "iconserver":
                        this.IconServer = reader.Value;
                        continue;
                    case "iconfarm":
                        this.IconFarm = reader.Value;
                        continue;
                    case "username":
                        this.OwnerName = reader.Value;
                        continue;
                    case "pathalias":
                    case "path_alias":
                        this.PathAlias = reader.Value;
                        continue;
                    case "url_sq":
                        this.urlSquare = reader.Value;
                        continue;
                    case "width_sq":
                        this.SquareThumbnailWidth = new int?(reader.ReadContentAsInt());
                        continue;
                    case "height_sq":
                        this.SquareThumbnailHeight = new int?(reader.ReadContentAsInt());
                        continue;
                    case "url_t":
                        this.urlThumbnail = reader.Value;
                        continue;
                    case "width_t":
                        this.ThumbnailWidth = new int?(reader.ReadContentAsInt());
                        continue;
                    case "height_t":
                        this.ThumbnailHeight = new int?(reader.ReadContentAsInt());
                        continue;
                    case "url_q":
                        this.urlLargeSquare = reader.Value;
                        continue;
                    case "width_q":
                        this.LargeSquareThumbnailWidth = new int?(reader.ReadContentAsInt());
                        continue;
                    case "height_q":
                        this.LargeSquareThumbnailHeight = new int?(reader.ReadContentAsInt());
                        continue;
                    case "url_n":
                        this.urlSmall320 = reader.Value;
                        continue;
                    case "width_n":
                        this.Small320Width = new int?(reader.ReadContentAsInt());
                        continue;
                    case "height_n":
                        this.Small320Height = new int?(reader.ReadContentAsInt());
                        continue;
                    case "url_s":
                        this.urlSmall = reader.Value;
                        continue;
                    case "width_s":
                        this.SmallWidth = new int?(reader.ReadContentAsInt());
                        continue;
                    case "height_s":
                        this.SmallHeight = new int?(reader.ReadContentAsInt());
                        continue;
                    case "url_m":
                        this.urlMedium = reader.Value;
                        continue;
                    case "width_m":
                        this.MediumWidth = new int?(reader.ReadContentAsInt());
                        continue;
                    case "height_m":
                        this.MediumHeight = new int?(reader.ReadContentAsInt());
                        continue;
                    case "url_c":
                        this.urlMedium800 = reader.Value;
                        continue;
                    case "width_c":
                        this.Medium800Width = new int?(reader.ReadContentAsInt());
                        continue;
                    case "height_c":
                        this.Medium800Height = new int?(reader.ReadContentAsInt());
                        continue;
                    case "url_l":
                        this.urlLarge = reader.Value;
                        continue;
                    case "width_l":
                        this.LargeWidth = new int?(reader.ReadContentAsInt());
                        continue;
                    case "height_l":
                        this.LargeHeight = new int?(reader.ReadContentAsInt());
                        continue;
                    case "url_z":
                        this.urlMedium640 = reader.Value;
                        continue;
                    case "width_z":
                        this.Medium640Width = new int?(reader.ReadContentAsInt());
                        continue;
                    case "height_z":
                        this.Medium640Height = new int?(reader.ReadContentAsInt());
                        continue;
                    case "url_o":
                        this.urlOriginal = reader.Value;
                        continue;
                    case "width_o":
                        this.OriginalWidth = reader.ReadContentAsInt();
                        continue;
                    case "height_o":
                        this.OriginalHeight = reader.ReadContentAsInt();
                        continue;
                    case "url_h":
                        this.Large1600Url = reader.Value;
                        continue;
                    case "width_h":
                        this.Large1600Width = new int?(reader.ReadContentAsInt());
                        continue;
                    case "height_h":
                        this.Large1600Height = new int?(reader.ReadContentAsInt());
                        continue;
                    case "url_k":
                        this.Large2048Url = reader.Value;
                        continue;
                    case "width_k":
                        this.Large2048Width = new int?(reader.ReadContentAsInt());
                        continue;
                    case "height_k":
                        this.Large2048Height = new int?(reader.ReadContentAsInt());
                        continue;
                    case "dateadded":
                        this.DateAddedToGroup = new DateTime?(UtilityMethods.UnixTimestampToDate(reader.Value).DateTime);
                        continue;
                    case "date_faved":
                        this.DateFavorited = new DateTime?(UtilityMethods.UnixTimestampToDate(reader.Value).DateTime);
                        continue;
                    case "can_comment":
                        this.CanComment = new bool?(reader.Value == "1");
                        continue;
                    case "can_addmeta":
                        this.CanAddMeta = new bool?(reader.Value == "1");
                        continue;
                    case "can_blog":
                        this.CanBlog = new bool?(reader.Value == "1");
                        continue;
                    case "can_print":
                        this.CanPrint = new bool?(reader.Value == "1");
                        continue;
                    case "can_download":
                        this.CanDownload = new bool?(reader.Value == "1");
                        continue;
                    case "can_share":
                        this.CanShare = new bool?(reader.Value == "1");
                        continue;
                    case "geo_is_family":
                        if (this.GeoPermissions == null)
                        {
                            this.GeoPermissions = new GeoPermissions();
                            this.GeoPermissions.PhotoId = this.PhotoId;
                        }
                        this.GeoPermissions.IsFamily = reader.Value == "1";
                        continue;
                    case "geo_is_friend":
                        if (this.GeoPermissions == null)
                        {
                            this.GeoPermissions = new GeoPermissions();
                            this.GeoPermissions.PhotoId = this.PhotoId;
                        }
                        this.GeoPermissions.IsFriend = reader.Value == "1";
                        continue;
                    case "geo_is_public":
                        if (this.GeoPermissions == null)
                        {
                            this.GeoPermissions = new GeoPermissions();
                            this.GeoPermissions.PhotoId = this.PhotoId;
                        }
                        this.GeoPermissions.IsPublic = reader.Value == "1";
                        continue;
                    case "geo_is_contact":
                        if (this.GeoPermissions == null)
                        {
                            this.GeoPermissions = new GeoPermissions();
                            this.GeoPermissions.PhotoId = this.PhotoId;
                        }
                        this.GeoPermissions.IsContact = reader.Value == "1";
                        continue;
                    case "context":
                        this.GeoContext = new GeoContext?((GeoContext)reader.ReadContentAsInt());
                        continue;
                    case "rotation":
                        this.Rotation = new int?(reader.ReadContentAsInt());
                        continue;
                    case "count_faves":
                        this.CountFaves = new int?(reader.ReadContentAsInt());
                        continue;
                    case "count_comments":
                        this.CountComments = new int?(reader.ReadContentAsInt());
                        continue;
                    default:
                        int num = allowExtraAtrributes ? 1 : 0;
                        continue;
                }
            }
            reader.Read();
            if (!(reader.LocalName == "description"))
                return;
            this.Description = reader.ReadElementContentAsString();


        }
    }
}