using System;
using System.Globalization;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Store;
using Windows.Devices.Enumeration;
using Windows.Devices.Input;
using Windows.Graphics.Display;
using Windows.Networking.Connectivity;
using Windows.Security.ExchangeActiveSyncProvisioning;
using Windows.Storage;
using Windows.UI.Xaml;

namespace BandCentral.Models.Helpers
{
    public class DiagnosticsHelper
    {
        public static async Task<string> Dump(Exception e, int applicationRuns, bool shouldDumpCompleteDeviceInfos = false)
        {
            var builder = new StringBuilder();
            var packageId = Package.Current.Id;
            var clientDeviceInformation = new EasClientDeviceInformation();

            // Get hardware Id
            //var token = HardwareIdentification.GetPackageSpecificToken(null);
            //var stream = token.Id.AsStream();
            //string hardwareId;
            //using(var reader = new BinaryReader(stream))
            //{
            //    var bytes = reader.ReadBytes((int)stream.Length);
            //    hardwareId = BitConverter.ToString(bytes);
            //}

            builder.AppendLine("***** Diagnositic Information *****");
            builder.AppendLine(); 
#if DEBUG
            builder.AppendLine("DEBUG");
            builder.AppendLine();
#endif
            builder.AppendLine("Awesome Band Background Exception");
            builder.AppendLine(); 
            builder.AppendFormat(e.Message);
            builder.AppendLine();
            builder.AppendLine();

            builder.AppendFormat("Time: {0}", DateTime.Now.ToUniversalTime().ToString("r"));
            builder.AppendLine(); 
            builder.AppendFormat("App Name: {0}", packageId.Name);
            builder.AppendLine();
            builder.AppendFormat($"App Runs: {0}", applicationRuns);
            builder.AppendLine(); 
            builder.AppendFormat("App Version: {0}.{1}.{2}.{3}", packageId.Version.Major, packageId.Version.Minor, packageId.Version.Build, packageId.Version.Revision);
            builder.AppendLine(); 
            builder.AppendFormat("App Publisher: {0}", packageId.Publisher);
            builder.AppendLine(); 
            builder.AppendFormat("Supported Package Architecture: {0}", packageId.Architecture);
            builder.AppendLine(); 
            builder.AppendFormat("Store App Id: {0}", CurrentApp.AppId);
            builder.AppendLine();
            builder.AppendLine();

#if DEBUG

#else
            if(CurrentApp.LicenseInformation.IsActive)
            {
                var listingInformation = await CurrentApp.LoadListingInformationAsync();
                builder.AppendFormat("Store Current Market: {0}", listingInformation.CurrentMarket);
                builder.AppendLine();
            }
#endif

            builder.AppendFormat("Culture: {0}", CultureInfo.CurrentCulture);
            builder.AppendLine(); builder.AppendLine();
            builder.AppendFormat("OS: {0}", clientDeviceInformation.OperatingSystem);
            builder.AppendLine(); builder.AppendLine();
            builder.AppendFormat("System Manufacturer: {0}", clientDeviceInformation.SystemManufacturer);
            builder.AppendLine(); builder.AppendLine();
            builder.AppendFormat("System Product Name: {0}", clientDeviceInformation.SystemProductName);
            builder.AppendLine(); builder.AppendLine();
            //builder.AppendFormat("System Sku: {0}", clientDeviceInformation.SystemSku);
            //builder.AppendLine();
            builder.AppendFormat("Friendly System Name: {0}", clientDeviceInformation.FriendlyName);
            builder.AppendLine(); builder.AppendLine();
            builder.AppendFormat("Friendly System ID: {0}", clientDeviceInformation.Id);
            builder.AppendLine(); builder.AppendLine();

            //builder.AppendFormat("User Display Name: {0}", await UserInformation.GetDisplayNameAsync());
            //builder.AppendLine();

            builder.AppendFormat("Current Memory Usage: {0:f3} MB", GC.GetTotalMemory(false) / 1024f / 1024f);
            builder.AppendLine(); builder.AppendLine();
            builder.AppendFormat("Window Bounds w x h: {0} x {1}", Window.Current.Bounds.Width, Window.Current.Bounds.Height);
            builder.AppendLine(); builder.AppendLine();
            builder.AppendFormat("Logical DPI: {0}", DisplayProperties.LogicalDpi);
            builder.AppendLine(); builder.AppendLine();
            builder.AppendFormat("Resolution Scale: {0}", DisplayProperties.ResolutionScale);
            builder.AppendLine(); builder.AppendLine();
            builder.AppendFormat("Current Orientation: {0}", DisplayProperties.CurrentOrientation);
            builder.AppendLine(); builder.AppendLine();
            builder.AppendFormat("Native Orientation: {0}", DisplayProperties.NativeOrientation);
            builder.AppendLine(); builder.AppendLine();
            builder.AppendFormat("Is Stereo Enabled: {0}", DisplayProperties.StereoEnabled);
            builder.AppendLine(); builder.AppendLine();
            builder.AppendFormat("Supports Keyboard: {0}", IsKeyboardPresent());
            builder.AppendLine(); builder.AppendLine();
            builder.AppendFormat("Supports Mouse: {0}", IsMousePresent());
            builder.AppendLine(); builder.AppendLine();
            builder.AppendFormat("Supports Touch (contacts): {0} ({1})", IsTouchPresent(), new TouchCapabilities().Contacts);
            builder.AppendLine(); builder.AppendLine();
            builder.AppendFormat("Is Network Available: {0}", NetworkInterface.GetIsNetworkAvailable());
            builder.AppendLine(); builder.AppendLine();
            builder.AppendFormat("Is Internet Connection Available: {0}", NetworkInformation.GetInternetConnectionProfile() != null);
            builder.AppendLine(); builder.AppendLine();

#if DEBUG
            //builder.AppendLine();
            //builder.AppendFormat("Hardware ID: {0}", hardwareId);
            builder.AppendLine();
            builder.AppendFormat("Installed Location: {0}", Package.Current.InstalledLocation.Path);
            builder.AppendLine();
            builder.AppendFormat("App Temp  Folder: {0}", ApplicationData.Current.TemporaryFolder.Path);
            builder.AppendLine();
            builder.AppendFormat("App Local Folder: {0}", ApplicationData.Current.LocalFolder.Path);
            builder.AppendLine();
            builder.AppendFormat("App Roam  Folder: {0}", ApplicationData.Current.RoamingFolder.Path);
            builder.AppendLine();
            builder.AppendLine();

            builder.AppendFormat("Network Host Names: ");
            foreach(var hostName in NetworkInformation.GetHostNames())
            {
                builder.AppendFormat("{0} ({1}), ", hostName.DisplayName, hostName.Type);
            }

            if(shouldDumpCompleteDeviceInfos)
            {
                var devInfos = await DeviceInformation.FindAllAsync();
                builder.AppendLine();
                builder.AppendLine("Complete Device Infos:");
                foreach(var devInfo in devInfos)
                {
                    builder.AppendFormat("Name: {0} Id: {1} - Properties: ", devInfo.Name, devInfo.Id);
                    foreach(var pair in devInfo.Properties)
                    {
                        builder.AppendFormat("{0} = {1}, ", pair.Key, pair.Value);
                    }
                    builder.AppendLine();
                }
            }
#endif


            return builder.ToString();
        }

        public static bool IsTouchPresent()
        {
            return new TouchCapabilities().TouchPresent == 1;
        }

        public static bool IsMousePresent()
        {
            return new MouseCapabilities().MousePresent == 1;
        }

        public static bool IsKeyboardPresent()
        {
            return new KeyboardCapabilities().KeyboardPresent == 1;
        }

    }
}
