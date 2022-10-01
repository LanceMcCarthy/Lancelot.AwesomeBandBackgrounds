using System;
using Windows.Networking.Connectivity;

namespace BandCentral.Models.Helpers
{
    public class InternetConnectionChangedEventArgs : EventArgs
    {
        public InternetConnectionChangedEventArgs(bool connected)
        {
            this.IsConnected = connected;
        }

        public bool IsConnected { get; }
    }

    public static class Network
    {
        public static event EventHandler<InternetConnectionChangedEventArgs>
            InternetConnectionChanged;

        static Network()
        {
            NetworkInformation.NetworkStatusChanged += (s) =>
            {
                if (InternetConnectionChanged == null) return;
                var arg = new InternetConnectionChangedEventArgs(IsConnected);
                InternetConnectionChanged(null, arg);
            };
        }

        public static bool IsConnected
        {
            get
            {
                var profile = NetworkInformation.GetInternetConnectionProfile();
                var isConnected = profile?.GetNetworkConnectivityLevel() ==
                    NetworkConnectivityLevel.InternetAccess;
                return isConnected;
            }
        }
    }
}
