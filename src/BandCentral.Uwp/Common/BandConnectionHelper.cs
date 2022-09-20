using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.UI.Popups;
using Microsoft.Band;
using Microsoft.HockeyApp;

namespace BandCentral.Uwp.Common
{
    public static class BandConnectionHelper
    {
        //DOES NOT NEED DISPOSAL
        public static async Task<ObservableCollection<IBandInfo>> FindPairedBandsAsync()
        {
            if (DesignMode.DesignModeEnabled) return null;
            
            try
            {
                var results = await BandClientManager.Instance.GetBandsAsync();

                if (results.Any())
                {
                    var tmpBands = new ObservableCollection<IBandInfo>();
                    foreach (var band in results) tmpBands.Add(band);
                    return tmpBands;
                }
                else
                {
                    await new MessageDialog("Your device is not paired with any Microsoft Bands. Try again after you've paired to a band.")
                            .ShowAsync();
                    return new ObservableCollection<IBandInfo>();
                }
            }
            catch (BandException ex)
            {
                HockeyClient.Current.TrackException(ex);
                Debug.WriteLine($"Band Connection Exception: {ex.Message}");
                
                await BandExceptionMessageDialog(ex, "FindPairedBandsAsync").ShowAsync();

                return null;
            }
            catch (Exception ex)
            {
                HockeyClient.Current.TrackException(ex);
                await new MessageDialog($"BandConnectionHelper.FindPairedBandsAsync() Exception \r\n\nMessage: {ex.Message}").ShowAsync();
                return null;
            }
        }

        //DOES NOT NEED DISPOSAL
        public static async Task<IBandInfo> RefreshCurrentBandInfo(string preferredBandName, IBandInfo[] bands = null)
        {
            try
            {
                if (bands == null || !bands.Any())
                    bands = await BandClientManager.Instance.GetBandsAsync();
                
                if (bands == null || !bands.Any())
                {
                    await new MessageDialog("You do not have any Microsoft Bands paired with this device. Please pair a Band and try again").ShowAsync();
                    return null;
                }

                //if the preferrendBandName is empty or there is only one result
                if (string.IsNullOrEmpty(preferredBandName) || bands.Length == 0)
                {
                    return bands[0];
                }

                var match = bands.FirstOrDefault(b => b.Name == preferredBandName);

                //if I have a match, return it
                if (match != null)
                    return match;

                //otherwise pick first band in list
                return bands[0];
            }
            catch (BandException ex)
            {
                HockeyClient.Current.TrackException(ex);
                await BandExceptionMessageDialog(ex, "RefreshCurrentBandInfo").ShowAsync();
                //var message = $"This error can occur for several different reasons, check to make sure your Band:" +
                //              "\r\n\n-Is powered on" +
                //              "\r\n\n-Is within range" +
                //              "\r\n-Is paired " +
                //              "\r\n-You are not currently in an activity like Sleep tracking or Workout " +
                //              "\r\n\nNOTE: This can also happen if you have an old Band still paired to your device. Go to your Bluetooth settings and remove any old Bands.";

                //await new MessageDialog(message, $"RefreshCurrentBandInfo Error: {ex.Message}").ShowAsync();
                return null;
            }
            catch (Exception ex)
            {
                HockeyClient.Current.TrackException(ex);
                await new MessageDialog($"BandConnectionHelper.RefreshCurrentBandInfo() Exception \r\n\nMessage: {ex.Message}").ShowAsync();
                return null;
            }
        }

        public static MessageDialog BandExceptionMessageDialog(BandException bandEx, string callerName)
        {
            var message = $"This error can occur for several different reasons, check to make sure your Band:" +
                              "\r\n\n-Is powered on" +
                              "\r\n-Is within range" +
                              "\r\n-Is paired " +
                              "\r\n-Is not currently in an activity like Sleep tracking or Workout " +
                              "\r\n\nThis can also happen if you have an old Band still paired to your device, go to your Bluetooth settings and remove any old Bands." +
                              $"\r\n\n*For contacting app support: Exception thrown by {callerName}()";

            return new MessageDialog(message, $"Error: {bandEx.Message}");
        }
    }
}
