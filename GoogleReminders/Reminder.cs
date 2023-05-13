using System;

namespace GoogleReminders
{
    public class Reminder
    {
        public int id;
        public string title;
        public DateTime dt;
        public long? creation_timestamp_msec;
        public bool done;

        public Reminder(int id, string title, DateTime dt, long? creation_timestamp_msec = null, bool done = false)
        {
            this.id = id;
            this.title = title;
            this.dt = dt;
            this.creation_timestamp_msec = creation_timestamp_msec;
            this.done = done;
        }

        public override string ToString()
        {
            if (done)
            {
                return $"{FormatDate(dt)} {title} [Done]";
            }
            else
            {
                return $"{FormatDate(dt)} {title}";
            }
        }

        public string FormatDate(DateTime date)
        {
            int day = date.Day;
            day = day < 10 ? ' ' + day : day;
            int month = date.Month;
            month = month < 10 ? '0' + month : month;
            int hours = date.Hour;
            hours = hours < 10 ? ' ' + hours : hours;
            int minutes = date.Minute;
            minutes = minutes < 10 ? '0' + minutes : minutes;
            return day + "." + month + "." + date.Year + " " + hours + ':' + minutes;
        }
    }
}
