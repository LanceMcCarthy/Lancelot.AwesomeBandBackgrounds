namespace BandCentral.Models.Secrets
{
    public class GeneralConstants
    {
        //IAP Keys
        public const string BackgroundTasksIapKey = "BackgroundTasksUnlock";
        public const string SmallDonationIapKey = "SmallDonation";
        public const string MediumDonationIapKey = "MediumDonation";
        public const string LargeDonationIapKey = "LargeDonation";
        
        //Local Settings Keys
        public const string UpdateMessageShownOnVersion = "LastUpdateMessageVersionSum";
        public const string PreferredBandNameKey = "PreferredBandName";
        public const string BandModelKey = "BandModel";

        //BG task local settings keys
        public const string BackgroundRotatorTaskName = "BackgroundRotatorTask";
        public const string BackgroundRotatorLastAttemptedKey = "BRLAttempted";
        public const string BackgroundRotatorLastCompletedKey = "BRLCompleted";
        public const string BackgroundRotatorStatusKey = "BRStatus";
        public const string BackgroundRotatorUpdateFrequencyKey = "BRUFrequency";
        public const string BackgroundRotatorUpdateQuietHoursEnabledKey = "BRQHEnabled";
        public const string BackgroundRotatorUpdateQuietHoursStartKey = "BRQHStart";
        public const string BackgroundRotatorUpdateQuietHoursEndKey = "BRQHEnd";
        public const string BackgroundRotatorFavoriteIndexKey = "BRFIndex";
        public const string BackgroundRotatorNotificationMuteKey = "BRNMute";
        public const string BackgroundRotatorEnabledKey = "BREnabled";
        public const string BackgroundRotatorThemeEnabledKey = "BRThemeEnabled";

        // HockeyApp/AppInsights
        public const string HockeyAppClientId = "";
    }
}
