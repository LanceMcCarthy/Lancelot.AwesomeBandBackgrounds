using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.UI.Popups;
using BandCentral.WindowsBase.Annotations;
using Microsoft.Advertising.WinRT.UI;

namespace BandCentral.Common
{
    public class AdsHelper : INotifyPropertyChanged
    {
        private InterstitialAd myVideoAd;
        string MyAppId = "d64751a9-6d30-4d83-9048-fff78f328f9c";
        string MyAdUnitId = "279592";
        private bool isLoading;
        private int errorCount;

        public bool IsLoading
        {
            get { return isLoading; }
            set { isLoading = value; OnPropertyChanged(); }
        }

        public AdsHelper()
        {
            myVideoAd = new InterstitialAd();
            // pre-fetch an ad 30-60 seconds before you need it
            myVideoAd.RequestAd(AdType.Video, MyAppId, MyAdUnitId);
            
            myVideoAd.AdReady += MyVideoAd_AdReady;
            myVideoAd.ErrorOccurred += MyVideoAd_ErrorOccurred;
            myVideoAd.Completed += MyVideoAd_Completed;
            myVideoAd.Cancelled += MyVideoAd_Cancelled;
        }

        public async Task ShowVideoAd()
        {
            switch (myVideoAd.State)
            {
                case InterstitialAdState.NotReady:
                    await new MessageDialog("The video is not ready yet, try again in a few seconds.").ShowAsync();
                    break;
                case InterstitialAdState.Ready:
                    myVideoAd.Show();
                    break;
                case InterstitialAdState.Showing:
                    break;
                case InterstitialAdState.Closed:
                    await new MessageDialog("You've already seen a video this time. Come back to this page in a little bit and try again.").ShowAsync();
                    break;
                default:
                    break;
            }
        }

        #region ad events

        private void MyVideoAd_AdReady(object sender, object e)
        {
            IsLoading = false;
        }

        private void MyVideoAd_ErrorOccurred(object sender, AdErrorEventArgs e)
        {
            IsLoading = false;

            //to prevent a continuous loop of bad attempts
            if(++errorCount < 4)
                myVideoAd.RequestAd(AdType.Video, MyAppId, MyAdUnitId);
        }

        private void MyVideoAd_Completed(object sender, object e)
        {
            IsLoading = true;
            myVideoAd.RequestAd(AdType.Video, MyAppId, MyAdUnitId);
        }

        private void MyVideoAd_Cancelled(object sender, object e)
        {
            IsLoading = true;
            myVideoAd.RequestAd(AdType.Video, MyAppId, MyAdUnitId);
        }

        #endregion

        #region INPC

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
