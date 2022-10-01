using BandCentral.Models.Common;
using BandCentral.Models.Helpers;
using BandCentral.Uwp.ViewModels;
using BandCentral.Uwp.Views;
using Microsoft.HockeyApp;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Email;
using Windows.Foundation;
using Windows.Foundation.Metadata;
using Windows.Media.SpeechRecognition;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using BandCentral.Models.Secrets;
using UnhandledExceptionEventArgs = Windows.UI.Xaml.UnhandledExceptionEventArgs;

namespace BandCentral.Uwp
{
    sealed partial class App : Application
    {
        public static bool IsSuspended;
        public static bool SupressMainBackpressHandler;

        private static MainViewModel viewModel;
        public static MainViewModel ViewModel
        {
            get
            {
                if (viewModel == null)
                {
                    if (SuspensionManager.SessionState.ContainsKey("MainViewModel"))
                    {
                        Debug.WriteLine("---NOTE---- viewModel was null, but SessionState contains MainViewModel");
                        viewModel = Deserialize<MainViewModel>(SuspensionManager.SessionState["MainViewModel"] as string);
                    }
                    else
                    {
                        Debug.WriteLine("---NOTE---- viewModel was null, returning new MainViewModel");
                        viewModel = new MainViewModel();
                    }

                    return viewModel;
                }

                return viewModel;
            }
        }

        public Frame ShellFrame { get; private set; }

        public App()
        {

            HockeyClient.Current.Configure(
                    GeneralConstants.HockeyAppClientId,
                new TelemetryConfiguration
                {
                    Collectors = WindowsCollectors.Metadata | WindowsCollectors.Session | WindowsCollectors.UnhandledException,
                    EnableDiagnostics = true
                })
                .SetExceptionDescriptionLoader(exception => "Exception HResult: " + exception.HResult.ToString());

            this.InitializeComponent();
            SuspensionManager.KnownTypes.Add(typeof(MainViewModel));
            this.Suspending += this.OnSuspending;
            this.UnhandledException += App_UnhandledException;

            //ulong v = ulong.Parse(AnalyticsInfo.VersionInfo.DeviceFamilyVersion);
            //ulong v1 = (v & 0xFFFF000000000000L) >> 48;
            //ulong v2 = (v & 0x0000FFFF00000000L) >> 32;
            //ulong v3 = (v & 0x00000000FFFF0000L) >> 16;
            //ulong v4 = (v & 0x000000000000FFFFL);
            //var version = $"OS Version {v1}.{v2}.{v3}.{v4}";

            
        }

        protected override async void OnLaunched(LaunchActivatedEventArgs e)
        {
            if (e.PrelaunchActivated)
                return;

            try
            {
                var shell = Window.Current.Content as Shell;

                if (shell == null)
                {
                    // Create a Shell which navigates to the first page
                    shell = new Shell();

                    // hook-up shell root frame navigation events
                    shell.RootFrame.NavigationFailed += OnNavigationFailed;
                    shell.RootFrame.Navigated += OnNavigated;
                    this.ShellFrame = shell.RootFrame;

                    await RestoreStatusAsync(e.PreviousExecutionState);

                    // set the Shell as content
                    Window.Current.Content = shell;

                    //software back button
                    SystemNavigationManager.GetForCurrentView().BackRequested += OnBackRequested;
                    //hardware back button
                    //if (ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons"))
                    //{
                    //    HardwareButtons.BackPressed += OnBackPressed;
                    //}

                    UpdateBackButtonVisibility();
                }

                if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
                {
                    var statusBar = StatusBar.GetForCurrentView();
                    await statusBar.HideAsync();
                }

                ApplicationView.PreferredLaunchViewSize = new Size {Width = 900, Height = 600};
                ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.Auto;

                var applicationView = ApplicationView.GetForCurrentView();
                applicationView.SetDesiredBoundsMode(ApplicationViewBoundsMode.UseVisible);
                applicationView.SetPreferredMinSize(new Size {Width = 360, Height = 533.33});

                //---------VOICE COMMANDS----------//
                try
                {
                    StorageFile vcdStorageFile = await Package.Current.InstalledLocation.GetFileAsync(@"VoiceCommandDefinitions.xml");

                    await Windows.ApplicationModel.VoiceCommands.VoiceCommandDefinitionManager.InstallCommandDefinitionsFromStorageFileAsync(vcdStorageFile);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"VCD INSTALLATION FAILED: {ex}");
                }
                
                //TODO set up DevCenter push notifications
                //var managerParams = new StoreServicesNotificationChannelParameters();
                //managerParams.
                //var servicesManager = new StoreServicesEngagementManager(managerParams, );
                //await servicesManager.RegisterNotificationChannelAsync();

                Window.Current.Activate();
            }
            catch (Exception ex)
            {
                await new MessageDialog($"There was a problem in OnLaunched. Error: {ex.Message}").ShowAsync();
            }
        }

        private async Task RestoreStatusAsync(ApplicationExecutionState previousExecutionState)
        {
            try
            {
                Debug.WriteLine("RESTORE STATUS from " + previousExecutionState);

                IsSuspended = false;

                switch (previousExecutionState)
                {
                    case ApplicationExecutionState.Terminated:
                        // Restore the saved session state only when appropriate
                        try
                        {
                            await SuspensionManager.RestoreAsync();

                            //TODO Uncomment to start testing Bluetooth
                            await ViewModel.RefreshBandInfoAsync();
                        }
                        catch (SuspensionManagerException ex)
                        {
                            Debug.WriteLine("Suspension Manager Error: " + ex.Message);
                        }
                        break;
                    case ApplicationExecutionState.Running:
                        //Check to make sure Band is connected
                        break;
                    case ApplicationExecutionState.NotRunning:
                        //let new instance run
                        break;
                    case ApplicationExecutionState.ClosedByUser:
                        //let new instance run
                        break;
                    case ApplicationExecutionState.Suspended:
                        try
                        {
                            await SuspensionManager.RestoreAsync();

                            //TODO Uncomment to start testing Bluetooth
                            await ViewModel.RefreshBandInfoAsync();
                        }
                        catch (SuspensionManagerException ex)
                        {
                            Debug.WriteLine("Suspension Manager Error: " + ex.Message);
                        }
                        break;
                }
            }
            catch(Exception ex)
            {
                await new MessageDialog($"There was a problem in RestoreStatusAsync. Error: {ex.Message}").ShowAsync();
            }
        }

        protected override async void OnActivated(IActivatedEventArgs e)
        {
            try
            {
                base.OnActivated(e);
                
                //-------------------ORINGAL WAY----------------------//
                //var shell = CreateRootShell();
                //await RestoreStatusAsync(e.PreviousExecutionState);

                //if (shell.AppFrame.Content == null)
                //    shell.AppFrame.Navigate(typeof(MainPage));

                //-----------------INTENSE WAY-----------------------//
                var shell = Window.Current.Content as Shell;

                if (shell == null)
                {
                    // Create a Shell which navigates to the first page
                    shell = new Shell();

                    // hook-up shell root frame navigation events
                    shell.RootFrame.NavigationFailed += OnNavigationFailed;
                    shell.RootFrame.Navigated += OnNavigated;
                    this.ShellFrame = shell.RootFrame;

                    await RestoreStatusAsync(e.PreviousExecutionState);

                    // set the Shell as content
                    Window.Current.Content = shell;

                    // listen for back button clicks (both soft- and hardware)
                    SystemNavigationManager.GetForCurrentView().BackRequested += OnBackRequested;

                    //if (ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons"))
                    //{
                    //    HardwareButtons.BackPressed += OnBackPressed;
                    //}

                    UpdateBackButtonVisibility();
                }

                #region FOP code

                //ORIGINAL WAY
                //var localPhotoPage = shell.AppFrame.Content as LocalPhotoPage;
                //var args = e as FileOpenPickerContinuationEventArgs;
                //if (args != null)
                //{
                //    Debug.WriteLine("FOP LAUNCH");
                //    localPhotoPage?.ContinueFileOpenPicker(args);
                //}

                //INTENSE WAY

                //if local photo page
                var page = shell.RootFrame.Content as LocalPhotoPage;
                if (page != null)
                {
                    var args = e as FileOpenPickerContinuationEventArgs;
                    if (args != null)
                    {
                        Debug.WriteLine("FOP LAUNCH");
                        page?.ContinueFileOpenPicker(args);
                    }
                }

                #endregion

                #region Toast activation

                if (e.Kind == ActivationKind.ToastNotification)
                {
                    
                    var toastArgs = e as ToastNotificationActivatedEventArgs;
                    var argument = toastArgs?.Argument;
                    Debug.WriteLine($"OnActivated ToastNotification argument: {argument}");
                    if (argument == "refreshBingTask")
                    {
                        shell.RootFrame.Navigate(typeof(BingImagePage), "refreshBingTask");
                    }
                }

                #endregion

                if (e.Kind == ActivationKind.VoiceCommand)
                {
                    var commandArgs = e as VoiceCommandActivatedEventArgs;

                    SpeechRecognitionResult speechRecognitionResult = commandArgs.Result;

                    // Get the name of the voice command and the text spoken. See AdventureWorksCommands.xml for
                    // the <Command> tags this can be filled with.
                    string voiceCommandName = speechRecognitionResult.RulePath[0];
                    string textSpoken = speechRecognitionResult.Text;

                    // The commandMode is either "voice" or "text", and it indicates how the voice command was entered by the user.
                    // Apps should respect "text" mode by providing feedback in silent form.
                    string commandMode = this.SemanticInterpretation("commandMode", speechRecognitionResult);

                    switch (voiceCommandName)
                    {
                        case "showFavorites":
                            string photoType = this.SemanticInterpretation("photoType", speechRecognitionResult).ToLowerInvariant();

                            if (photoType == "bing" || photoType == "bing image" || photoType == "bing images")
                            {
                                this.ShellFrame.Navigate(typeof(BingImagePage));
                            }
                            
                            break;
                        default:
                            this.ShellFrame.Navigate(typeof(MainPage));
                            break;
                    }

                    this.ShellFrame.Navigate(typeof(MainPage));
                }

                Window.Current.Activate();
            }
            catch (Exception ex)
            {
                await new MessageDialog($"There was a problem in OnActivated. Error: {ex.Message}").ShowAsync();
            }
        }

        private async void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            Debug.WriteLine("-------------OnSuspending-------------------");

            IsSuspended = true;

            if (ViewModel != null)
            {
                ViewModel.IsBandConnected = false;
                
                SuspensionManager.SessionState["MainViewModel"] = Serialize(ViewModel);
            }

            await SuspensionManager.SaveAsync();

            deferral.Complete();
        }

        #region Intense methods

        // handle hardware back button press
        //void OnBackPressed(object sender, BackPressedEventArgs e)
        //{
        //    if (SupressMainBackpressHandler)
        //        return;

        //    var shell = (Shell)Window.Current.Content;
        //    if (shell.RootFrame.CanGoBack)
        //    {
        //        e.Handled = true;
        //        shell.RootFrame.GoBack();
        //    }
        //}

        // handle software back button press
        void OnBackRequested(object sender, BackRequestedEventArgs e)
        {
            if (SupressMainBackpressHandler)
                return;

            var shell = (Shell)Window.Current.Content;
            if (shell.RootFrame.CanGoBack)
            {
                e.Handled = true;
                shell.RootFrame.GoBack();
            }
        }

        void OnNavigated(object sender, NavigationEventArgs e)
        {
            UpdateBackButtonVisibility();
        }

        private void UpdateBackButtonVisibility()
        {
            var shell = (Shell)Window.Current.Content;

            var visibility = AppViewBackButtonVisibility.Collapsed;
            if (shell.RootFrame.CanGoBack)
            {
                visibility = AppViewBackButtonVisibility.Visible;
            }

            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = visibility;
        }

        #endregion

        private void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        private async void App_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                e.Handled = true;
                HockeyClient.Current.TrackException(e.Exception);

                var dialog = new MessageDialog("Sorry, there has been an unexpected error. If you'd like to send a techincal summary to the app development team, click Yes.", "Unexpected Error");

                var yes = new UICommand("Yes");
                dialog.Commands.Add(yes);

                var no = new UICommand("No");
                dialog.Commands.Add(no);

                dialog.CancelCommandIndex = 1;

                IUICommand selected = await dialog.ShowAsync();
                if (selected == yes)
                {
                    EmailMessage accessRequest = new EmailMessage();
                    accessRequest.To.Add(new EmailRecipient("awesome.apps@outlook.com", "Lancelot Software"));
                    accessRequest.Subject = "Awesome Band Background UWP";

                    var dumpDeviceInfo = false;
#if DEBUG
                    dumpDeviceInfo = true;
#endif
                    int appRuns = 0;
                    if (ViewModel?.ApplicationRuns != null)
                        appRuns = ViewModel.ApplicationRuns;

                    accessRequest.Body = await DiagnosticsHelper.Dump(e.Exception, appRuns, dumpDeviceInfo);

                    await EmailManager.ShowComposeNewEmailAsync(accessRequest);
                }
            }
            catch(Exception ex)
            {
                HockeyClient.Current.TrackException(e.Exception);
            }
        }

        public static string Serialize(object o)
        {
            Debug.WriteLine("---SERIALIZING---");
            var jsonSerializer = new DataContractJsonSerializer(o.GetType());
            var stream = new MemoryStream();
            jsonSerializer.WriteObject(stream, o);
            byte[] buffer = stream.ToArray();
            string json = Encoding.UTF8.GetString(buffer, 0, buffer.Length);
            return json;
        }

        public static T Deserialize<T>(string json)
        {
            Debug.WriteLine("---DESERIALIZING---");
            var jsonDeserializer = new DataContractJsonSerializer(typeof(T));
            T instance = (T)jsonDeserializer.ReadObject(new MemoryStream(Encoding.UTF8.GetBytes(json)));
            return instance;
        }

        private string SemanticInterpretation(string interpretationKey, SpeechRecognitionResult speechRecognitionResult)
        {
            return speechRecognitionResult.SemanticInterpretation.Properties[interpretationKey].FirstOrDefault();
        }
    }
}
