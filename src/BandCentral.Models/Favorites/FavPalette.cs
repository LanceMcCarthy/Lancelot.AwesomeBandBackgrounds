using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BandCentral.Models.Favorites
{
    [KnownType(typeof(List<string>))]
    [DataContract]
    public class FavPalette
    {
        private List<string> colors;

        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public string Title { get; set; }
        [DataMember]
        public string Author { get; set; }
        [DataMember]
        public double Rating { get; set; }
        [DataMember]
        public string Thumb { get; set; }
        [DataMember]
        public string Url { get; set; }

        [DataMember]
        public List<string> Colors
        {
            get => colors ?? (colors = new List<string>());
            set => colors = value;
        }
    }
}
