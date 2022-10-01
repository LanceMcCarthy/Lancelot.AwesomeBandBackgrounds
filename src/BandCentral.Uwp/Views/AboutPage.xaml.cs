// Lance McCarthy 2013-2023 MIT
// Free to use, maintain attribution to original
// https://github.com/LanceMcCarthy/Lancelot.AwesomeBandBackgrounds

using Microsoft.HockeyApp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Windows.ApplicationModel.Email;
using Windows.ApplicationModel.Store;
using Windows.Graphics.Display;
using Windows.System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace BandCentral.Uwp.Views
{
    public sealed partial class AboutPage : Page
    {
        public AboutPage()
        {
            this.InitializeComponent();
            DataContext = App.ViewModel;

            HockeyClient.Current.TrackPageView("AboutPage");

            DisplayInformation.AutoRotationPreferences = DisplayOrientations.Portrait;
        }

        private async void RatingTapped(object sender, TappedRoutedEventArgs e)
        {
            var selectedRating = Rating.Value;
            
            var metrics = new Dictionary<string, string> { { "RatingValue", selectedRating.ToString(CultureInfo.InvariantCulture) } };

            HockeyClient.Current.TrackEvent("RatingTapped", metrics);

            if (selectedRating >= 3)
            {
                await ReviewAppAsync();
            }
            else
            {
                var md = new MessageDialog("Leaving a negative review won't help us fix the problem. If you send an email, we will be able to fix it and push out an update.\r\n\nLeave a negative review or send us an email?", "Help us fix it");
                md.Commands.Add(new UICommand("send feedback", async (a) =>
                {
                    await EmailSupportAsync("Feedback");
                }));
                md.Commands.Add(new UICommand("leave review", async (a) =>
                {
                    await ReviewAppAsync();
                }));
                await md.ShowAsync();
            }
        }

        private async void SendEmailButton_OnClick(object sender, RoutedEventArgs e)
        {
            await EmailSupportAsync("Feedback");
        }

        private async Task EmailSupportAsync(string context)
        {
            var subjectName = "Band Background " + context;
            var emailAddress = "awesome.apps@outlook.com";

            var emailName = "Awesome Band Background";
            var email = new EmailMessage()
            {
                Subject = subjectName
            };
            email.To.Add(new EmailRecipient(emailAddress, emailName));
            await EmailManager.ShowComposeNewEmailAsync(email);

        }

        private async Task ReviewAppAsync()
        {
            await Launcher.LaunchUriAsync(new Uri("ms-windows-store:reviewapp?appid=" + CurrentApp.AppId));
        }

        #region Navigation
        
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //if (Microsoft.Services.Store.Engagement.Feedback.IsSupported) //breaking change
            if (Microsoft.Services.Store.Engagement.StoreServicesFeedbackLauncher.IsSupported())
                FeedbackHubButton.Visibility = Visibility.Visible;

            base.OnNavigatedTo(e);
        }

        private async void FeedbackHubButton_Click(object sender, RoutedEventArgs e)
        {
            //await Microsoft.Services.Store.Engagement.Feedback.LaunchFeedbackAsync(); //breaking change
            var launcher = Microsoft.Services.Store.Engagement.StoreServicesFeedbackLauncher.GetDefault();
            await launcher.LaunchAsync();
        }

        #endregion
    }
}
