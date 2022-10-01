using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BandCentral.Models.Pictalicious
{
    [DataContract]
    public class Info
    {
        [DataMember(Name = "colors")]
        public List<string> Colors { get; set; }
        [DataMember(Name = "url")]
        public string Url { get; set; }
    }
}