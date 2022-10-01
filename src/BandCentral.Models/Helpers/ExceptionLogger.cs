namespace BandCentral.Models.Helpers
{
    //public class ExceptionLogger
    //{
    //    private static readonly int daysToKeepLog = 14;

    //    private static ExceptionLogger current;

    //    public static ExceptionLogger Current => current ?? (current = new ExceptionLogger());

    //    private ExceptionLogger() { }

    //    public void LogException(Exception ex)
    //    {
    //        Microsoft.HockeyApp.HockeyClient.Current.TrackException(ex);

    //        string exceptionMessage = CreateErrorMessage(ex);
    //        LogFileWrite(exceptionMessage);
    //        PurgeLogFiles();
    //    }

    //    /// <summary>
    //    /// This method is for prepare the error message to log using Exception object
    //    /// </summary>
    //    /// <param name="currentException">
    //    /// <returns></returns>
    //    private static string CreateErrorMessage(Exception currentException)
    //    {
    //        StringBuilder messageBuilder = new StringBuilder();
    //        try
    //        {
    //            messageBuilder.AppendLine("-----------------------------------------------------------------");
    //            messageBuilder.AppendLine("Source: " + currentException.Source.ToString().Trim());
    //            messageBuilder.AppendLine("Date Time: " + DateTime.Now);
    //            messageBuilder.AppendLine("-----------------------------------------------------------------");
    //            messageBuilder.AppendLine("Method: " + currentException.Message.ToString().Trim());
    //            messageBuilder.AppendLine("Exception :: " + currentException.ToString());
    //            if (currentException.InnerException != null)
    //            {
    //                messageBuilder.AppendLine("InnerException :: " + currentException.InnerException.ToString());
    //            }
    //            messageBuilder.AppendLine("");
    //            return messageBuilder.ToString();
    //        }
    //        catch
    //        {
    //            messageBuilder.AppendLine("Exception:: Unknown Exception.");
    //            return messageBuilder.ToString();
    //        }
    //    }

    //    /// <summary>
    //    /// This method is for writing the Log file with the current exception message
    //    /// </summary>
    //    /// <param name="exceptionMessage">
    //    private static async void LogFileWrite(string exceptionMessage)
    //    {
    //        try
    //        {
    //            string fileName = "ABB_UWP_Exceptions" + "-" + DateTime.Today.ToString("yyyyMMdd") + "." + "log";
    //            var localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
    //            var logFolder = await localFolder.CreateFolderAsync("Logs", CreationCollisionOption.OpenIfExists);
    //            var logFile = await logFolder.CreateFileAsync(fileName, CreationCollisionOption.OpenIfExists);

    //            if (!String.IsNullOrEmpty(exceptionMessage))
    //            {
    //                await FileIO.AppendTextAsync(logFile, exceptionMessage);
    //            }
    //        }
    //        catch (Exception)
    //        {
    //        }
    //    }

    //    /// <summary> 
    //    /// This method purge old log files in the log folder, which are older than daysToKeepLog. 
    //    /// </summary> 
    //    public static async void PurgeLogFiles()
    //    {
    //        DateTime todaysDate;
    //        var logFolder = ApplicationData.Current.LocalFolder;

    //        try
    //        {
    //            todaysDate = DateTime.Now.Date;

    //            logFolder = await logFolder.GetFolderAsync("Logs");
    //            var files = await logFolder.GetFilesAsync();

    //            if (files.Count < 1) return;

    //            foreach (var file in files)
    //            {
    //                BasicProperties basicProperties = await file.GetBasicPropertiesAsync();
    //                if (file.FileType == ".log")
    //                {
    //                    if (DateTime.Compare(todaysDate, basicProperties.DateModified.AddDays(daysToKeepLog).DateTime.Date) >= 0)
    //                    {
    //                        await file.DeleteAsync(StorageDeleteOption.PermanentDelete);
    //                    }
    //                }
    //            }
    //        }
    //        catch (Exception)
    //        {
    //        }
    //    }

    //}
}
