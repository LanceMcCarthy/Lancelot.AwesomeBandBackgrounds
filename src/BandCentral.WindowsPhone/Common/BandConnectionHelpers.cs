using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Microsoft.Band;
using Windows.ApplicationModel;

namespace BandCentral.Common
{
    public static class BandConnectionHelper
    {
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
                    return null;
                }
            }
            catch (BandException ex)
            {
                Debug.WriteLine("Band Connection Exception: {0}", ex.Message);
                await new MessageDialog($"Error finding bands. This can also happen if you're in an activity like Sleep Tracking or Workout. \r\n\nError Details: {ex.Message}")
                        .ShowAsync();
                return null;
            }
            catch (Exception ex)
            {
                await new MessageDialog($"Error finding bands, you may not have any paired. \r\n\nError Details: {ex.Message}").ShowAsync();
                return null;
            }
            finally
            {

            }
        }

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
                await new MessageDialog($"RefreshCurrentBandInfo() Error connecting to Band, make sure it is:" +
                                        $"\r\n-Powered ON" +
                                        $"\r\n-Is within range" +
                                        $"\r\n-You are not currently in an activity like Sleep tracking or Workout " +
                                        $"\r\n-Is paired " +
                                        $"\r\n\nNOTE: This can happen if you no longer have the paired Band. Go to your Bluetooth settings and delete any old Bands." +
                                        $"\r\n\nError: {ex.Message}").ShowAsync();
                return null;
            }
            catch (Exception ex)
            {
                await new MessageDialog($"BandConnectionHelper -> GetCurrentBandInfo() BandException \r\n\nMessage: {ex.Message}").ShowAsync();
                return null;
            }
        }
    }
}
