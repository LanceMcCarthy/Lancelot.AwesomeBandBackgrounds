using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BandCentral.UwpBackgroundTasks.Models
{
    [DataContract]
    public sealed class BingImagesResult
    {
        [DataMember]
        public IList<BingImage> images { get; set; }
        [DataMember]
        public Tooltips tooltips { get; set; }
    }
}
