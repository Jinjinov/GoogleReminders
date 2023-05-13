using System.Collections.Generic;
using System.Threading.Tasks;

namespace GoogleReminders
{
    public interface IReminders
    {
        Task<bool> CreateReminder(string accessToken, Reminder reminder);
        Task<bool> DeleteReminderAsync(string accessToken, int reminderId);
        Task<Reminder?> GetReminder(string accessToken, int reminderId);
        Task<List<Reminder>?> ListReminders(string accessToken, int numReminders);
    }
}