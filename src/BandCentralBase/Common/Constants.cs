namespace BandCentralBase.Common
{
    public class Constants
    {
        //IAP Keys
        public const string BackgroundTasksIapKey = "BackgroundTasksUnlock";
        public const string SmallDonationIapKey = "SmallDonation";
        public const string MediumDonationIapKey = "MediumDonation";
        public const string LargeDonationIapKey = "LargeDonation";

        //flickr
        //public const string ApiKey = "bbb547e79114f986ed0e512c3e712307";
        public const string WindowsDevFlickrApiKey = "877abff9c018ab6be6289cace50b5ca4";
        public const string WindowsDevFlickrSharedSecret = "5fa460125f12c8b8";
        public const string BaseUrl = "https://api.flickr.com/services/rest/";
        public const string Method = "flickr.photos.getRecent";
        public const string Format = "json";
        public const string Media = "photos";

        //500px
        public const string FiveHundredPixConsumerKey = "sCV6qWYFCqWTgFWa9iA5MFPNk2dI8yNzNPVPtIjX";
        public const string FiveHundredPixConsumerSecret = "PT4uRQh79Ojo4rZj1gHljc8qinAlj9mP1LZGcdnq";
        public const string FiveHundredPixJavacriptSdkKey = "19d7f2837bdc4940e72d410b6ad9ade7b7549b26";
        public const string FiveHundredPixBaseUrl = "https://api.500px.com/v1/";

        //filenames
        //public const string BackgroundFavoritesFileName = "BackgroundFavoritesJson.txt";
        public const string FlickrFavoritesFileName = "FavoritesJson.txt";
        public const string FlickrFavoritesBackupFileName = "FavoritesBackupJson.txt";
        public const string ThemeHistoryFileName = "ThemeHistoryJson.txt";

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

        //Bing Image BG task keys
        public const string BingTaskName = "BingImageBackgroundTask";
        public const string BingTaskLastAttemptedKey = "BTLAttempted";
        public const string BingTaskLastCompletedKey = "BTLCompleted";
        public const string BingTaskStatusKey = "BTStatus";
        public const string BingTaskLastDayUpdatedKey = "BTLDayUpdated";
        public const string BingTaskLastSetKey = "BTLSet";

        //Roaming Settings Keys



    }
}
