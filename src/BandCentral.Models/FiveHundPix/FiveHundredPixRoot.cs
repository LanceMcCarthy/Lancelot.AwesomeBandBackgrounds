using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BandCentral.Models.FiveHundPix
{
    [DataContract]
    public class FiveHundredPixSearchResultRoot
    {
        [DataMember]
        public int current_page { get; set; }

        [DataMember]
        public int total_pages { get; set; }

        [DataMember]
        public int total_items { get; set; }

        [DataMember]
        public List<PixPhoto> photos { get; set; }
    }

    [DataContract]
    public class GetPixPhotoRoot
    {
        [DataMember]
        public PixPhoto photo { get; set; }

        [DataMember]
        public List<PixComment> comments { get; set; }
    }

    [DataContract]
    public class GetPixCommentsRoot
    {
        [DataMember]
        public int status { get; set; }

        [DataMember]
        public string message { get; set; }

        [DataMember]
        public string error { get; set; }

        [DataMember]
        public PixComment comment { get; set; }
    }

    [DataContract]
    public class PixPhoto
    {
        [DataMember]
        public int id { get; set; }

        [DataMember]
        public int user_id { get; set; }

        [DataMember]
        public string name { get; set; }

        [DataMember]
        public string description { get; set; }

        [DataMember]
        public string camera { get; set; }

        [DataMember]
        public string lens { get; set; }

        [DataMember]
        public string focal_length { get; set; }

        [DataMember]
        public string iso { get; set; }

        [DataMember]
        public string shutter_speed { get; set; }

        [DataMember]
        public string aperture { get; set; }

        [DataMember]
        public int times_viewed { get; set; }

        [DataMember]
        public float rating { get; set; }

        [DataMember]
        public int status { get; set; }

        [DataMember]
        public DateTime created_at { get; set; }

        [DataMember]
        public int category { get; set; }

        [DataMember]
        public object location { get; set; }

        [DataMember]
        public float? latitude { get; set; }

        [DataMember]
        public float? longitude { get; set; }

        [DataMember]
        public DateTime? taken_at { get; set; }

        [DataMember]
        public int hi_res_uploaded { get; set; }

        [DataMember]
        public bool for_sale { get; set; }

        [DataMember]
        public int width { get; set; }

        [DataMember]
        public int height { get; set; }

        [DataMember]
        public int votes_count { get; set; }

        [DataMember]
        public int favorites_count { get; set; }

        [DataMember]
        public int comments_count { get; set; }

        [DataMember]
        public bool nsfw { get; set; }

        [DataMember]
        public int sales_count { get; set; }

        [DataMember]
        public DateTime? for_sale_date { get; set; }

        [DataMember]
        public float highest_rating { get; set; }

        [DataMember]
        public DateTime highest_rating_date { get; set; }

        [DataMember]
        public int license_type { get; set; }

        [DataMember]
        public bool converted { get; set; }

        [DataMember]
        public int collections_count { get; set; }

        [DataMember]
        public int crop_version { get; set; }

        [DataMember]
        public bool privacy { get; set; }

        [DataMember]
        public bool profile { get; set; }

        [DataMember]
        public string image_url { get; set; }

        [DataMember]
        public List<PixImage> images { get; set; }
        
        [DataMember]
        public string url { get; set; }

        [DataMember]
        public int positive_votes_count { get; set; }

        [DataMember]
        public int converted_bits { get; set; }

        [DataMember]
        public Share_Counts share_counts { get; set; }

        [DataMember]
        public bool watermark { get; set; }

        [DataMember]
        public string image_format { get; set; }

        [DataMember]
        public bool licensing_requested { get; set; }

        [DataMember]
        public PixUser user { get; set; }
    }

    [DataContract]
    public class Share_Counts
    {
        [DataMember]
        public int twitter { get; set; }

        [DataMember]
        public int pinterest { get; set; }

        [DataMember]
        public int facebook { get; set; }
    }

    [DataContract]
    public class PixUser
    {
        [DataMember]
        public int id { get; set; }

        [DataMember]
        public string username { get; set; }

        [DataMember]
        public string firstname { get; set; }

        [DataMember]
        public string lastname { get; set; }

        [DataMember]
        public string city { get; set; }

        [DataMember]
        public string country { get; set; }

        [DataMember]
        public int usertype { get; set; }

        [DataMember]
        public string fullname { get; set; }

        [DataMember]
        public string userpic_url { get; set; }

        [DataMember]
        public string userpic_https_url { get; set; }

        [DataMember]
        public string cover_url { get; set; }

        [DataMember]
        public int upgrade_status { get; set; }

        [DataMember]
        public bool store_on { get; set; }

        [DataMember]
        public int affection { get; set; }

        [DataMember]
        public Avatars avatars { get; set; }

        [DataMember]
        public int followers_count { get; set; }

        [DataMember]
        public int admin { get; set; }
    }

    [DataContract]
    public class Avatars
    {
        [DataMember]
        public Default _default { get; set; }

        [DataMember]
        public Large large { get; set; }

        [DataMember]
        public Small small { get; set; }

        [DataMember]
        public Tiny tiny { get; set; }
    }

    [DataContract]
    public class Default
    {
        [DataMember]
        public string https { get; set; }
    }

    [DataContract]
    public class Large
    {
        [DataMember]
        public string https { get; set; }
    }

    [DataContract]
    public class Small
    {
        [DataMember]
        public string https { get; set; }
    }

    [DataContract]
    public class Tiny
    {
        [DataMember]
        public string https { get; set; }
    }

    [DataContract]
    public class PixImage
    {
        [DataMember]
        public int size { get; set; }

        [DataMember]
        public string url { get; set; }

        [DataMember]
        public string https_url { get; set; }

        [DataMember]
        public string format { get; set; }
    }

    [DataContract]
    public class EditoredBy
    {
    }

    [DataContract]
    public class PixComment
    {
        [DataMember]
        public int id { get; set; }

        [DataMember]
        public int user_id { get; set; }

        [DataMember]
        public int to_whom_user_id { get; set; }

        [DataMember]
        public string body { get; set; }

        [DataMember]
        public string created_at { get; set; }

        [DataMember]
        public object parent_id { get; set; }

        [DataMember]
        public bool flagged { get; set; }

        [DataMember]
        public int rating { get; set; }

        [DataMember]
        public bool voted { get; set; }

        [DataMember]
        public PixUser user { get; set; }
    }
}
