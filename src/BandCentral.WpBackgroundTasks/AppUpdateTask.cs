using Windows.ApplicationModel.Background;
using Windows.UI.Notifications;

namespace BandCentral.WpBackgroundTasks
{
    public sealed class AppUpdateTask : IBackgroundTask
    {
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            var toastnotifier = ToastNotificationManager.CreateToastNotifier();

            var toastDescriptor = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText02);

            var txtNodes = toastDescriptor.GetElementsByTagName("text");

            txtNodes[0].AppendChild(toastDescriptor.CreateTextNode("Updated!"));
            txtNodes[1].AppendChild(toastDescriptor.CreateTextNode($"Awesome Band Backgrounds has been updated. New features and bug fixes available now."));

            var toast = new ToastNotification(toastDescriptor);

            toastnotifier.Show(toast);
        }
    }
}
