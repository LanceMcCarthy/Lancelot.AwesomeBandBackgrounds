using System;
using System.Runtime.Serialization;
using FlickrNet.Drawing;

namespace BandCentral.Models.Common
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
            get => url;
            set => url = value;
        }

        [DataMember]
        public Size OriginalSize
        {
            get => originalSize;
            set => originalSize = value;
        }

        [DataMember]
        public double OriginalRatio
        {
            get => originalRatio;
            set => originalRatio = value;
        }
    }
}
