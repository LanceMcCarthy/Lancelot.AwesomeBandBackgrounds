using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.UI.Popups;

namespace BandCentral.Uwp.Common
{
    public static class BackgroundTaskEngine
    {
        /// <summary>
        /// Sets a new TimeTrigger background task with the supplied parameters
        /// </summary>
        /// <param name="taskFriendlyName">friendly name</param>
        /// <param name="taskEntryPoint">namespace + class name(is also used in app manifest declarations)</param>
        /// <param name="taskRunFrequency">Frequency of background task run</param>
        /// <returns></returns>
        internal static async Task<bool> RegisterTaskRequiringInternetAsync(string taskFriendlyName, string taskEntryPoint, uint taskRunFrequency)
        {
            try
            {
                App.ViewModel.IsBusy = true;
                App.ViewModel.IsBusyMessage = $"setting {taskFriendlyName} background task...";

                //if task already exists, unregister it before adding it
                foreach (var task in BackgroundTaskRegistration.AllTasks.Where(cur => cur.Value.Name == taskFriendlyName))
                {
                    task.Value.Unregister(true);
                }

                var builder = new BackgroundTaskBuilder
                {
                    Name = taskFriendlyName,
                    TaskEntryPoint = taskEntryPoint
                };
                builder.SetTrigger(new TimeTrigger(taskRunFrequency, false));
                builder.AddCondition(new SystemCondition(SystemConditionType.InternetAvailable));
                builder.Register();

                return true;
            }
            catch (Exception ex)
            {
                await new MessageDialog($"BackgroundTaskEngine RegisterTaskAsync Exception\r\n\nError: {ex.Message}").ShowAsync();
                return false;
            }
            finally
            {
                App.ViewModel.IsBusy = false;
                App.ViewModel.IsBusyMessage = "";
            }
        }
        
        /// <summary>
        /// Checks if background task already exists
        /// </summary>
        /// <param name="taskFriendlyName"></param>
        /// <returns>True if task exists</returns>
        internal static async Task<bool> CheckBackgroundTasksAsync(string taskFriendlyName)
        {
            try
            {
                App.ViewModel.IsBusy = true;
                App.ViewModel.IsBusyMessage = "checking background task...";

                await BackgroundExecutionManager.RequestAccessAsync();

                return BackgroundTaskRegistration.AllTasks.Any(task => task.Value.Name == taskFriendlyName);
            }
            catch (Exception ex)
            {
                await new MessageDialog($"Something went wrong checking for background tasks. Error: {ex.Message}").ShowAsync();
                return false;
            }
            finally
            {
                App.ViewModel.IsBusy = false;
                App.ViewModel.IsBusyMessage = "";
            }
        }

        /// <summary>
        /// Removes the background task
        /// </summary>
        /// <param name="taskFriendlyName"></param>
        /// <returns>True if the removal was successful</returns>
        internal static async Task<bool> UnregisterTaskAsync(string taskFriendlyName)
        {
            try
            {
                App.ViewModel.IsBusy = true;
                App.ViewModel.IsBusyMessage = $"removing {taskFriendlyName} background task...";

                await BackgroundExecutionManager.RequestAccessAsync();
                
                foreach (var task in BackgroundTaskRegistration.AllTasks.Where(cur => cur.Value.Name == taskFriendlyName))
                {
                    task.Value.Unregister(true);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                await new MessageDialog($"BackgroundTaskEngine UnregisterTaskAsync Exception\r\n\nError: {ex.Message}").ShowAsync();
                return false;
            }
            finally
            {
                App.ViewModel.IsBusy = false;
                App.ViewModel.IsBusyMessage = "";
            }
        }
    }
}
