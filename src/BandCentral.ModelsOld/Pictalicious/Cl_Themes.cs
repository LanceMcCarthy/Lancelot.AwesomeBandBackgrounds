using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BandCentral.Models.Pictalicious
{
    public class Cl_Themes
    {
        [DataMember(Name = "id")]
        public int Id { get; set; }

        [DataMember(Name = "title")]
        public string Title { get; set; }

        [DataMember(Name = "userName")]
        public string UserName { get; set; }

        [DataMember(Name = "numViews")]
        public double NumViews { get; set; }

        [DataMember(Name = "numVotes")]
        public double NumVotes { get; set; }

        [DataMember(Name = "numComments")]
        public double NumComments { get; set; }

        [DataMember(Name = "numHearts")]
        public double NumHearts { get; set; }

        [DataMember(Name = "rank")]
        public double Rank { get; set; }

        [DataMember(Name = "dateCreated")]
        public string DateCreated { get; set; }

        [DataMember(Name = "colors")]
        public List<string> Colors { get; set; }

        [DataMember(Name = "description")]
        public string Description { get; set; }

        [DataMember(Name = "url")]
        public string Url { get; set; }

        [DataMember(Name = "imageUrl")]
        public string ImageUrl { get; set; }

        [DataMember(Name = "badgeUrl")]
        public string BadgeUrl { get; set; }

        [DataMember(Name = "apiUrl")]
        public string ApiUrl { get; set; }

        [DataMember(Name = "rating")]
        public double Rating { get; set; }

        [DataMember(Name = "author")]
        public string Author { get; set; }

        [DataMember(Name = "badge")]
        public string Badge { get; set; }

        [DataMember(Name = "thumb")]
        public string Thumb { get; set; }
    }
}