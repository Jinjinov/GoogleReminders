using System;

namespace GoogleReminders
{
    public class Reminder
    {
        public string Id;
        public string Title;
        public DateTime Dt;
        public long? CreationTimestampMsec;
        public bool Done;

        public Reminder(string id, string title, DateTime dt, long? creationTimestampMsec = null, bool done = false)
        {
            Id = id;
            Title = title;
            Dt = dt;
            CreationTimestampMsec = creationTimestampMsec;
            Done = done;
        }

        public override string ToString()
        {
            if (Done)
            {
                return $"{Dt:yyyy.MM.dd} {Title} [Done]";
            }
            else
            {
                return $"{Dt:yyyy.MM.dd} {Title}";
            }
        }
    }
}
