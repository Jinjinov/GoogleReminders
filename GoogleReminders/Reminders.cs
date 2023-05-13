using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace GoogleReminders
{
    public class Reminders
    {
        public string CreateReminderRequestBody(Reminder reminder)
        {
            Dictionary<string, Dictionary<string, object>> body = new Dictionary<string, Dictionary<string, object>>
            {
                {
                    "2", new Dictionary<string, object>
                    {
                        { "1", 7 }
                    }
                },
                {
                    "3", new Dictionary<string, object>
                    {
                        { "2", reminder.id }
                    }
                },
                {
                    "4", new Dictionary<string, object>
                    {
                        {
                            "1", new Dictionary<string, object>
                            {
                                { "2", reminder.id }
                            }
                        },
                        { "3", reminder.title },
                        {
                            "5", new Dictionary<string, object>
                            {
                                { "1", reminder.dt.Year },
                                { "2", reminder.dt.Month },
                                { "3", reminder.dt.Day },
                                { "4", new Dictionary<string, object>
                                    {
                                        { "1", reminder.dt.Hour },
                                        { "2", reminder.dt.Minute },
                                        { "3", reminder.dt.Second }
                                    }
                                }
                            }
                        },
                        { "8", 0 }
                    }
                }
            };

            return JsonSerializer.Serialize(body);
        }

        public string GetReminderRequestBody(int reminderId)
        {
            Dictionary<string, List<Dictionary<string, int>>> body = new Dictionary<string, List<Dictionary<string, int>>>
            {
                {
                    "2", new List<Dictionary<string, int>>
                    {
                        new Dictionary<string, int>
                        {
                            { "2", reminderId }
                        }
                    }
                }
            };

            return JsonSerializer.Serialize(body);
        }

        public string DeleteReminderRequestBody(int reminderId)
        {
            Dictionary<string, List<Dictionary<string, int>>> body = new Dictionary<string, List<Dictionary<string, int>>>
            {
                {
                    "2", new List<Dictionary<string, int>>
                    {
                        new Dictionary<string, int>
                        {
                            { "2", reminderId }
                        }
                    }
                }
            };

            return JsonSerializer.Serialize(body);
        }

        public string ListReminderRequestBody(int numReminders, long maxTimestampMsec = 0)
        {
            /*
            The body corresponds to a request that retrieves a maximum of numReminders reminders, 
            whose creation timestamp is less than maxTimestampMsec.
            maxTimestampMsec is a unix timestamp in milliseconds. 
            if its value is 0, treat it as current time.
            */
            Dictionary<string, object> body = new Dictionary<string, object>
            {
                { "5", 1 }, // boolean field: 0 or 1. 0 doesn't work ¯\_(ツ)_/¯
                { "6", numReminders }, // number of reminders to retrieve
            };

            if (maxTimestampMsec != 0)
            {
                maxTimestampMsec += 15 * 3600 * 1000;
                body["16"] = maxTimestampMsec;
                /*
                Empirically, when requesting with a certain timestamp, reminders with the given timestamp 
                or even a bit smaller timestamp are not returned. 
                Therefore we increase the timestamp by 15 hours, which seems to solve this...  ~~voodoo~~
                (I wish Google had a normal API for reminders)
                */
            }

            return JsonSerializer.Serialize(body);
        }

        public Reminder? BuildReminder(Dictionary<string, dynamic> reminderDict)
        {
            try
            {
                int id = reminderDict["1"]["2"];
                string title = reminderDict["3"];

                int year = reminderDict["5"]["1"];
                int month = reminderDict["5"]["2"];
                int day = reminderDict["5"]["3"];

                DateTime dateTime = new DateTime(year, month, day);

                if (reminderDict["5"].ContainsKey("4"))
                {
                    int hour = reminderDict["5"]["4"]["1"];
                    int minute = reminderDict["5"]["4"]["2"];
                    int second = reminderDict["5"]["4"]["3"];

                    dateTime = dateTime.AddHours(hour).AddMinutes(minute).AddSeconds(second);
                }

                int? creationTimestampMsec = reminderDict.ContainsKey("18") ? reminderDict["18"] : null;
                bool done = reminderDict.ContainsKey("8") && reminderDict["8"] == 1;

                return new Reminder(id, title, dateTime, creationTimestampMsec, done);
            }
            catch (KeyNotFoundException)
            {
                Console.WriteLine("BuildReminder failed: unrecognized reminder dictionary format");
                return null;
            }
        }

        public async Task<bool> CreateReminder(string accessToken, Reminder reminder)
        {
            /*
            send a 'create reminder' request.
            returns True upon a successful creation of a reminder
            */

            using HttpClient httpClient = new HttpClient();

            /*
            using HttpRequestMessage request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri("https://reminders-pa.clients6.google.com/v1internalOP/reminders/create" + "?access_token=" + accessToken),
                Content = new StringContent(CreateReminderRequestBody(reminder), Encoding.UTF8, "application/json+protobuf")
            };

            using HttpResponseMessage response = await httpClient.SendAsync(request);
            /**/

            Uri requestUri = new Uri($"https://reminders-pa.clients6.google.com/v1internalOP/reminders/create?access_token={accessToken}");
            using StringContent httpContent = new StringContent(CreateReminderRequestBody(reminder), Encoding.UTF8, "application/json+protobuf");

            using HttpResponseMessage response = await httpClient.PostAsync(requestUri, httpContent);

            if (response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync();
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<Reminder?> GetReminder(string accessToken, int reminderId)
        {
            /*
            retrieve information about the reminder with the given id. 
            Null if an error occurred
            */

            using HttpClient httpClient = new HttpClient();

            Uri requestUri = new Uri($"https://reminders-pa.clients6.google.com/v1internalOP/reminders/get?access_token={accessToken}");
            using StringContent httpContent = new StringContent(GetReminderRequestBody(reminderId), Encoding.UTF8, "application/json+protobuf");

            using HttpResponseMessage response = await httpClient.PostAsync(requestUri, httpContent);

            if (response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync();

                Dictionary<string, object>? contentDict = JsonSerializer.Deserialize<Dictionary<string, object>>(content);

                if (contentDict == null || !contentDict.TryGetValue("1", out object remindersObj) || !(remindersObj is object[] reminders) || reminders.Length == 0)
                {
                    Console.WriteLine($"Couldn't find reminder with id={reminderId}");
                    return null;
                }

                Dictionary<string, dynamic> reminderDict = (Dictionary<string, dynamic>)reminders[0];

                return BuildReminder(reminderDict);
            }
            else
            {
                return null;
            }
        }

        public async Task<bool> DeleteReminderAsync(string accessToken, int reminderId)
        {
            /*
            delete the reminder with the given id.
            Returns True upon a successful deletion
            */

            using HttpClient httpClient = new HttpClient();

            Uri requestUri = new Uri($"https://reminders-pa.clients6.google.com/v1internalOP/reminders/delete?access_token={accessToken}");
            using StringContent requestContent = new StringContent(DeleteReminderRequestBody(reminderId), Encoding.UTF8, "application/json+protobuf");

            using HttpResponseMessage response = await httpClient.PostAsync(requestUri, requestContent);

            if (response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync();
                return true;
            }
            else
            {
                return false;
            }
        }

        async Task<List<Reminder>?> ListReminders(string accessToken, int numReminders)
        {
            /*
            returns a list of the last numReminders created reminders, or
            null if an error occurred
            */

            using HttpClient httpClient = new HttpClient();

            Uri requestUrl = new Uri($"https://reminders-pa.clients6.google.com/v1internalOP/reminders/list?access_token={accessToken}");
            using StringContent requestContent = new StringContent(ListReminderRequestBody(numReminders), Encoding.UTF8, "application/json+protobuf");

            using HttpResponseMessage response = await httpClient.PostAsync(requestUrl, requestContent);

            if (response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync();

                Dictionary<string, object>? contentDict = JsonSerializer.Deserialize<Dictionary<string, object>>(content);

                if (contentDict == null || !contentDict.TryGetValue("1", out object remindersObj) || !(remindersObj is object[] remindersArray) || remindersArray.Length == 0)
                {
                    return new List<Reminder>();
                }

                List<Reminder> reminders = new List<Reminder>();

                foreach (object reminderObject in remindersArray)
                {
                    if (!(reminderObject is Dictionary<string, dynamic> reminderDict))
                    {
                        continue;
                    }

                    Reminder? reminder = BuildReminder(reminderDict);

                    if (reminder != null)
                        reminders.Add(reminder);
                }

                return reminders;
            }
            else
            {
                return null;
            }
        }
    }
}
