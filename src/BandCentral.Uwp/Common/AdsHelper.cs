using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.UI.Popups;
using CommonHelpers.Common;
using Microsoft.Advertising.WinRT.UI;

namespace BandCentral.Uwp.Common
{
    public class AdsHelper : BindableBase
    {
        private InterstitialAd myVideoAd;
        readonly string myAppId;
        readonly string myAdUnitId;
        private bool isLoading;
        private int errorCount;

        public bool IsLoading
        {
            get { return isLoading; }
            set { isLoading = value; OnPropertyChanged(); }
        }

        public AdsHelper()
        {
            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                myAppId = "d64751a9-6d30-4d83-9048-fff78f328f9c";
                myAdUnitId = "279613";
            }
            else
            {
                myAppId = "a5d6c1b6-2f94-451d-ade1-ca2369b1c2a2";
                myAdUnitId = "279612";
            }

            // pre-fetch an ad 30-60 seconds before you need it
            myVideoAd.RequestAd(AdType.Video, myAppId, myAdUnitId);

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
            if (++errorCount < 4)
                myVideoAd.RequestAd(AdType.Video, myAppId, myAdUnitId);
        }

        private void MyVideoAd_Completed(object sender, object e)
        {
            IsLoading = true;
            myVideoAd.RequestAd(AdType.Video, myAppId, myAdUnitId);
        }

        private void MyVideoAd_Cancelled(object sender, object e)
        {
            IsLoading = true;
            myVideoAd.RequestAd(AdType.Video, myAppId, myAdUnitId);
        }

        #endregion
    }
}
