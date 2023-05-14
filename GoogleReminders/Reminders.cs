using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace GoogleReminders
{
    public class Reminders : IReminders
    {
        readonly HttpClient httpClient;

        public Reminders(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        private string CreateReminderRequestBody(Reminder reminder)
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
                        { "2", reminder.Id }
                    }
                },
                {
                    "4", new Dictionary<string, object>
                    {
                        {
                            "1", new Dictionary<string, object>
                            {
                                { "2", reminder.Id }
                            }
                        },
                        { "3", reminder.Title },
                        {
                            "5", new Dictionary<string, object>
                            {
                                { "1", reminder.Dt.Year },
                                { "2", reminder.Dt.Month },
                                { "3", reminder.Dt.Day },
                                { "4", new Dictionary<string, object>
                                    {
                                        { "1", reminder.Dt.Hour },
                                        { "2", reminder.Dt.Minute },
                                        { "3", reminder.Dt.Second }
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

        private string DeleteReminderRequestBody(string reminderId)
        {
            Dictionary<string, List<Dictionary<string, string>>> body = new Dictionary<string, List<Dictionary<string, string>>>
            {
                {
                    "2", new List<Dictionary<string, string>>
                    {
                        new Dictionary<string, string>
                        {
                            { "2", reminderId }
                        }
                    }
                }
            };

            return JsonSerializer.Serialize(body);
        }

        private string GetReminderRequestBody(string reminderId)
        {
            Dictionary<string, List<Dictionary<string, string>>> body = new Dictionary<string, List<Dictionary<string, string>>>
            {
                {
                    "2", new List<Dictionary<string, string>>
                    {
                        new Dictionary<string, string>
                        {
                            { "2", reminderId }
                        }
                    }
                }
            };

            return JsonSerializer.Serialize(body);
        }

        private string ListReminderRequestBody(int numReminders, long maxTimestampMsec = 0)
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

        private Reminder? BuildReminder(JsonElement reminderElement)
        {
            try
            {
                string id = reminderElement.GetProperty("1").GetProperty("2").GetString() ?? throw new ArgumentNullException();
                string title = reminderElement.GetProperty("3").GetString() ?? id;

                JsonElement dateElement = reminderElement.GetProperty("5");

                int year = dateElement.GetProperty("1").GetInt32();
                int month = dateElement.GetProperty("2").GetInt32();
                int day = dateElement.GetProperty("3").GetInt32();

                DateTime dateTime = new DateTime(year, month, day);

                if (dateElement.TryGetProperty("4", out JsonElement timeElement))
                {
                    int hour = timeElement.GetProperty("1").GetInt32();
                    int minute = timeElement.GetProperty("2").GetInt32();
                    int second = timeElement.GetProperty("3").GetInt32();

                    dateTime = dateTime.AddHours(hour).AddMinutes(minute).AddSeconds(second);
                }

                long? creationTimestampMsec = null;

                if (reminderElement.TryGetProperty("18", out JsonElement creationTimestampElement) && long.TryParse(creationTimestampElement.GetString(), out long timestamp))
                    creationTimestampMsec = timestamp;

                bool done = reminderElement.TryGetProperty("8", out JsonElement doneElement) && doneElement.GetInt32() == 1;

                return new Reminder(id, title, dateTime, creationTimestampMsec, done);
            }
            catch (KeyNotFoundException)
            {
                return null;
            }
        }

        /// <summary>
        /// Send a 'create reminder' request.
        /// </summary>
        /// <returns>True upon a successful creation of a reminder</returns>
        public async Task<bool> CreateReminder(string accessToken, Reminder reminder)
        {
            /*
            using HttpRequestMessage request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"https://reminders-pa.clients6.google.com/v1internalOP/reminders/create?access_token={accessToken}"),
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

        /// <summary>
        /// Delete the reminder with the given id.
        /// </summary>
        /// <returns>True upon a successful deletion</returns>
        public async Task<bool> DeleteReminder(string accessToken, string reminderId)
        {
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

        /// <summary>
        /// Retrieve information about the reminder with the given id.
        /// </summary>
        /// <returns>Null if an error occurred</returns>
        public async Task<Reminder?> GetReminder(string accessToken, string reminderId)
        {
            Uri requestUri = new Uri($"https://reminders-pa.clients6.google.com/v1internalOP/reminders/get?access_token={accessToken}");
            using StringContent httpContent = new StringContent(GetReminderRequestBody(reminderId), Encoding.UTF8, "application/json+protobuf");

            using HttpResponseMessage response = await httpClient.PostAsync(requestUri, httpContent);

            if (response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync();

                JsonDocument contentDoc = JsonDocument.Parse(content);
                JsonElement contentRoot = contentDoc.RootElement;

                if (!contentRoot.TryGetProperty("1", out JsonElement remindersElement) || remindersElement.GetArrayLength() == 0)
                {
                    return null;
                }

                JsonElement reminderElement = remindersElement[0];

                return BuildReminder(reminderElement);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Returns a list of the last numReminders created reminders
        /// </summary>
        /// <returns>null if an error occurred</returns>
        public async Task<List<Reminder>?> ListReminders(string accessToken, int numReminders)
        {
            Uri requestUrl = new Uri($"https://reminders-pa.clients6.google.com/v1internalOP/reminders/list?access_token={accessToken}");
            using StringContent requestContent = new StringContent(ListReminderRequestBody(numReminders), Encoding.UTF8, "application/json+protobuf");

            using HttpResponseMessage response = await httpClient.PostAsync(requestUrl, requestContent);

            if (response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync();

                JsonDocument contentDoc = JsonDocument.Parse(content);
                JsonElement contentRoot = contentDoc.RootElement;

                if (!contentRoot.TryGetProperty("1", out JsonElement remindersElement) || remindersElement.GetArrayLength() == 0)
                {
                    return new List<Reminder>();
                }

                List<Reminder> reminders = new List<Reminder>();

                foreach (JsonElement reminderElement in remindersElement.EnumerateArray())
                {
                    Reminder? reminder = BuildReminder(reminderElement);

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
