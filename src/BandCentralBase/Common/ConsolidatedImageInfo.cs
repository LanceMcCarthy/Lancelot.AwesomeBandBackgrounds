using FlickrNet.Drawing;
using System;
using System.Drawing;
using System.Runtime.Serialization;

namespace BandCentralBase.Common
{
    [DataContract]
    public class ConsolidatedImageInfo 
    {
        private string url;
        private Size originalSize;
        private double originalRatio;

        public ConsolidatedImageInfo(string imageUrl, Size imageSize, double currentScreenWidth)
        {
            this.url = imageUrl;
            this.originalSize = imageSize;
            var imageHeightBasedOnScreenWidth = imageSize.Height * currentScreenWidth / imageSize.Width;
            this.originalRatio = Math.Round((double)(Convert.ToInt32(originalSize.Height / OriginalSize.Width)), 2);
        }

        [DataMember]
        public string Url
        {
            get { return url; }
            set { url = value; }
        }

        [DataMember]
        public Size OriginalSize
        {
            get { return originalSize; }
            set { originalSize = value; }
        }

        [DataMember]
        public double OriginalRatio
        {
            get { return originalRatio; }
            set { originalRatio = value; }
        }
    }
}
