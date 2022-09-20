using System.Diagnostics;
using System;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using BandCentral.Common;
using BandCentral.ViewModels;
using Windows.ApplicationModel.Email;
using Windows.Graphics.Display;
using Windows.UI.ViewManagement;
using BandCentral.WindowsBase.Common;

namespace BandCentral
{
    public sealed partial class App : Application
    {
		public static FrameService ActiveFrameService { get; set; }
        public static bool IsSuspended;
        
        private TransitionCollection transitions;
        
        private static MainViewModel viewModel;
		public static MainViewModel ViewModel
	    {
            get
            {
                if(viewModel == null)
                {
                    if(SuspensionManager.SessionState.ContainsKey("MainViewModel"))
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

	    public App()
        {
            this.InitializeComponent();
            SuspensionManager.KnownTypes.Add(typeof(MainViewModel));
            this.Suspending += this.OnSuspending;
            this.UnhandledException += App_UnhandledException;
        }

        protected override async void OnLaunched(LaunchActivatedEventArgs e)
        {
            Frame rootFrame = CreateRootFrame();
            await RestoreStatusAsync(e.PreviousExecutionState);
            
            // Removes the turnstile navigation for startup.
            if(rootFrame.ContentTransitions != null)
            {
                this.transitions = new TransitionCollection();
                foreach(var c in rootFrame.ContentTransitions)
                {
                    this.transitions.Add(c);
                }
            }

            rootFrame.ContentTransitions = null;
            rootFrame.Navigated += this.RootFrame_FirstNavigated;

            if(!rootFrame.Navigate(typeof(HubPage), e.Arguments))
            {
                throw new Exception("Failed to create initial page");
            }
            
            ApplicationView.GetForCurrentView().SetDesiredBoundsMode(ApplicationViewBoundsMode.UseVisible);
            DisplayInformation.AutoRotationPreferences = DisplayOrientations.Portrait;

            App.ActiveFrameService = new FrameService(rootFrame);
            

            Window.Current.Activate();
        } 
        
        private Frame CreateRootFrame()
        {
            Frame rootFrame = Window.Current.Content as Frame;

            if(rootFrame == null)
            {
                rootFrame = new Frame();
                rootFrame.NavigationFailed += OnNavigationFailed;
                Window.Current.Content = rootFrame;
            }

            return rootFrame;
        }

        private async Task RestoreStatusAsync(ApplicationExecutionState previousExecutionState)
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
                        //ViewModel.IsBandConnected = false;
                        //await ViewModel.ResumeConnectionsAsync();
                    }
                    catch(SuspensionManagerException ex)
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
                        //ViewModel.IsBandConnected = false;
                        //await ViewModel.ResumeConnectionsAsync();
                    }
                    catch(SuspensionManagerException ex)
                    {
                        Debug.WriteLine("Suspension Manager Error: " + ex.Message);
                    }
                    break;
            }
        }

        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        } 
        
        protected override async void OnActivated(IActivatedEventArgs e)
        {
            base.OnActivated(e);
            
            Frame rootFrame = CreateRootFrame();
            await RestoreStatusAsync(e.PreviousExecutionState);

            if(rootFrame.Content == null)
            {
                rootFrame.Navigate(typeof(HubPage));
            }

            #region FOP code

            var localPhotoPage = rootFrame.Content as LocalPhotoPage;

            var args = e as FileOpenPickerContinuationEventArgs;
            if(args != null)
            {
                Debug.WriteLine("FOP LAUNCH");
                localPhotoPage?.ContinueFileOpenPicker(args);
            }

            #endregion

            Window.Current.Activate();
        } 


        private void RootFrame_FirstNavigated(object sender, NavigationEventArgs e)
        {
            var rootFrame = sender as Frame;
            rootFrame.ContentTransitions = this.transitions ?? new TransitionCollection() { new NavigationThemeTransition() };
            rootFrame.Navigated -= this.RootFrame_FirstNavigated;
        }
        

        private async void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            Debug.WriteLine("-------------OnSuspending-------------------");

            IsSuspended = true;

            if(ViewModel != null)
            {
                SuspensionManager.SessionState["MainViewModel"] = Serialize(ViewModel);
            }

            await SuspensionManager.SaveAsync();

            deferral.Complete();
        }

        async void App_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            e.Handled = true;

#if WINDOWS_PHONE_APP
            var dialog = new MessageDialog("Sorry, there has been an unexpected error. If you'd like to send a techincal summary to the app development team, click Yes.", "Unexpected Error");
            var yes = new UICommand("Yes");
            dialog.Commands.Add(yes);
            var no = new UICommand("No");
            dialog.Commands.Add(no);
            dialog.CancelCommandIndex = 1;

            var selected = await dialog.ShowAsync();

            if(selected == yes)
            {
                EmailMessage accessRequest = new EmailMessage();
                accessRequest.To.Add(new EmailRecipient("awesome.apps@outlook.com", "Lancelot Software"));
                accessRequest.Subject = $"ABB WP 8.1 v{App.ViewModel.AppVersion}";
                accessRequest.Body = await DiagnosticsHelper.Dump(e.Exception, (int)App.ViewModel?.ApplicationRuns, true);

                await EmailManager.ShowComposeNewEmailAsync(accessRequest);
            }
#else
            var message = "Sorry, there has been an unexpected error. If you'd like to send a techincal summary to the app development team, click Yes.";
            var md = new MessageDialog(message, "Unexpected Error");
            md.Commands.Add(new UICommand("yes", async delegate
            {
                var text = await DiagnosticsHelper.Dump(e.Exception, false);
                await ReportErrorMessage(text);
                //DataTransferManager.ShowShareUI();
            }));
            await md.ShowAsync();
#endif
        }

        public static string Serialize(object o)
        {
            Debug.WriteLine("SERIALIZING");
            var jsonSerializer = new DataContractJsonSerializer(o.GetType());
            var stream = new MemoryStream();
            jsonSerializer.WriteObject(stream, o);
            byte[] buffer = stream.ToArray();
            string json = Encoding.UTF8.GetString(buffer, 0, buffer.Length);

            return json;
        }

        public static T Deserialize<T>(string json)
        {
            Debug.WriteLine("DESERIALIZING");
            var jsonDeserializer = new DataContractJsonSerializer(typeof(T));
            T instance = (T)jsonDeserializer.ReadObject(new MemoryStream(Encoding.UTF8.GetBytes(json)));
            return instance;
        }
    }
}