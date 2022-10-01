using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BandCentral.Models.Bing
{
    [DataContract]
    public class BingImagesResult
    {
        [DataMember]
        public List<BingImage> images { get; set; }
        [DataMember]
        public Tooltips tooltips { get; set; }
    }
}
