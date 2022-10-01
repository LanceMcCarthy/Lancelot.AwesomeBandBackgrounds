using System.Runtime.Serialization;

namespace BandCentral.Models.Bing
{
    [DataContract]
    public class ImageHints
    {
        [DataMember]
        public string desc { get; set; }
        [DataMember]
        public string link { get; set; }
        [DataMember]
        public string query { get; set; }
        [DataMember]
        public int locx { get; set; }
        [DataMember]
        public int locy { get; set; }
    }
}