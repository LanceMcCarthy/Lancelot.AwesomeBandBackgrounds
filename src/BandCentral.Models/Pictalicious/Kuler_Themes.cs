using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BandCentral.Models.Pictalicious
{
    [DataContract]
    public class Kuler_Themes
    {
        [DataMember(Name = "title")]
        public string Title { get; set; }

        [DataMember(Name = "colors")]
        public List<string> Colors { get; set; }

        [DataMember(Name = "id")]
        public string Id { get; set; }

        [DataMember(Name = "author")]
        public string Author { get; set; }

        [DataMember(Name = "url")]
        public string Url { get; set; }

        [DataMember(Name = "rating")]
        public double Rating { get; set; }

        [DataMember(Name = "thumb")]
        public string Thumb { get; set; }
    }
}