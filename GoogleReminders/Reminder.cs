﻿using System;

namespace GoogleReminders
{
    public class Reminder
    {
        public int Id;
        public string Title;
        public DateTime Dt;
        public long? CreationTimestampMsec;
        public bool Done;

        public Reminder(int id, string title, DateTime dt, long? creationTimestampMsec = null, bool done = false)
        {
            Id = id;
            Title = title;
            Dt = dt;
            CreationTimestampMsec = creationTimestampMsec;
            Done = done;
        }
    }
}
