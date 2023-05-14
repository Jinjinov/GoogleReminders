using System.Collections.Generic;
using System.Threading.Tasks;

namespace GoogleReminders
{
    public interface IReminders
    {
        /// <summary>
        /// Send a 'create reminder' request.
        /// </summary>
        /// <returns>True upon a successful creation of a reminder</returns>
        Task<bool> CreateReminder(string accessToken, Reminder reminder);

        /// <summary>
        /// Delete the reminder with the given id.
        /// </summary>
        /// <returns>True upon a successful deletion</returns>
        Task<bool> DeleteReminder(string accessToken, string reminderId);

        /// <summary>
        /// Retrieve information about the reminder with the given id.
        /// </summary>
        /// <returns>Null if an error occurred</returns>
        Task<Reminder?> GetReminder(string accessToken, string reminderId);

        /// <summary>
        /// Returns a list of the last numReminders created reminders
        /// </summary>
        /// <returns>null if an error occurred</returns>
        Task<List<Reminder>?> ListReminders(string accessToken, int numReminders);
    }
}