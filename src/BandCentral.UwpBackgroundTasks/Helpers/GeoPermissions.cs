using System.Xml;

namespace BandCentral.UwpBackgroundTasks.Helpers
{
    public sealed class GeoPermissions : IFlickrParsable
    {
        public string PhotoId { get; set; }

        public bool IsPublic { get; set; }

        public bool IsContact { get; set; }

        public bool IsFriend { get; set; }

        public bool IsFamily { get; set; }

        void IFlickrParsable.Load(XmlReader reader)
        {
            while (reader.MoveToNextAttribute())
            {
                switch (reader.LocalName)
                {
                    case "id":
                        this.PhotoId = reader.Value;
                        continue;
                    case "ispublic":
                        this.IsPublic = reader.Value == "1";
                        continue;
                    case "iscontact":
                        this.IsContact = reader.Value == "1";
                        continue;
                    case "isfamily":
                        this.IsFamily = reader.Value == "1";
                        continue;
                    case "isfriend":
                        this.IsFriend = reader.Value == "1";
                        continue;
                    default:
                        continue;
                }
            }
            reader.Read();
        }
    }
}