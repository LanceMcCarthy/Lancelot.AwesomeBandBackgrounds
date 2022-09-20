using System.Xml;

namespace BandCentral.UwpBackgroundTasks.Helpers
{
    public interface IFlickrParsable
    {
        void Load(XmlReader reader);
    }
}