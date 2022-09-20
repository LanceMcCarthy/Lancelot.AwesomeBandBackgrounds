using System;

namespace BandCentral.Models.Common
{
    public class DownloadProgressArgs : EventArgs
    {
        public DownloadProgressArgs(int bytesReceived, int totalBytes)
        {
            BytesReceived = bytesReceived;
            TotalBytes = totalBytes;
        }

        public int TotalBytes { get; }

        public int BytesReceived { get; }

        public float PercentComplete => 100 * ((float) BytesReceived / TotalBytes);
    }
}
