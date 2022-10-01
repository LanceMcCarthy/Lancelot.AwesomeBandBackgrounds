using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BandCentral.Models.Pictalicious
{
    [DataContract]
    public class PictaliciousRoot
    {
        [DataMember(Name = "info")]
        public Info Info { get; set; }

        [DataMember(Name = "kuler_themes")]
        public List<Kuler_Themes> Kuler_themes { get; set; }

        [DataMember(Name = "cl_themes")]
        public List<Cl_Themes> Cl_themes { get; set; }
    }
}
