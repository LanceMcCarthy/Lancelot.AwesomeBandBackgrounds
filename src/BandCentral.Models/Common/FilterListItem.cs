using System.Runtime.Serialization;

namespace BandCentral.Models.Common
{
    [DataContract]
    public class FilterListItem
    {
        [DataMember]
        public string Title { get; set; }
        [DataMember]
        public string ThumbnailPath { get; set; }
    }
}
