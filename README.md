# GoogleReminders

Google Reminders .Net API - port of https://github.com/jonahar/google-reminders-cli python API to .Net

To run this code locally, you need to set values for the YOUR_CLIENT_ID and REDIRECT_URI variables that correspond to your authorization credentials. The REDIRECT_URI should be the same URL where the page is being served. The value must exactly match one of the authorized redirect URIs for the OAuth 2.0 client, which you configured in the API Console. If this value doesn't match an authorized URI, you will get a 'redirect_uri_mismatch' error. Your project in the Google API Console must also have enabled the appropriate API for this request.

```csharp
string YOUR_CLIENT_ID = 'REPLACE_THIS_VALUE';
string YOUR_REDIRECT_URI = 'REPLACE_THIS_VALUE';
```

For instructions on how to get these values, see:

https://developers.google.com/identity/protocols/OAuth2UserAgent

## How to use:

1. Include NuGet package from https://www.nuget.org/packages/GoogleReminders

        <ItemGroup>
            <PackageReference Include="GoogleReminders" Version="0.0.0.1" />
        </ItemGroup>

2. Add `builder.Services.AddScoped<IReminders,Reminders>();`

3. Use it:

        @page "/"

        @using System.Text.RegularExpressions;

        @inject NavigationManager navigationManager
        @inject IReminders googleReminders

        <h1>Reminders:</h1>

        <div>@((MarkupString)innerHTML)</div>

        @code {
            private const string YOUR_CLIENT_ID = "REPLACE_THIS_VALUE";
            private const string YOUR_REDIRECT_URI = "REPLACE_THIS_VALUE";

            Dictionary<string, string> oauth2Params = new Dictionary<string, string>();
            string innerHTML = "";

            protected override async Task OnInitializedAsync()
            {
                string fragment = new Uri(navigationManager.Uri).Fragment;

                if (fragment.Length > 0)
                {
                    string fragmentString = fragment.Substring(1);

                    // Parse query string to see if page request is coming from OAuth 2.0 server.
                    Regex regex = new Regex("([^&=]+)=([^&]*)");

                    foreach (Match match in regex.Matches(fragmentString))
                    {
                        oauth2Params[Uri.UnescapeDataString(match.Groups[1].Value)] = Uri.UnescapeDataString(match.Groups[2].Value);
                    }
                }

                await SendRequest();
            }

            async Task SendRequest()
            {
                if (oauth2Params != null && oauth2Params.TryGetValue("access_token", out string? accessToken))
                {
                    List<Reminder>? reminders = await googleReminders.ListReminders(accessToken, 10);

                    if (reminders != null)
                    {
                        foreach (Reminder reminder in reminders)
                        {
                            innerHTML += "<p>" + reminder + "</p>";
                        }
                    }
                    else
                    {
                        // Token invalid, so prompt for user permission.
                        OAuth2SignIn();
                    }
                }
                else
                {
                    OAuth2SignIn();
                }
            }

            void OAuth2SignIn()
            {
                // Google's OAuth 2.0 endpoint for requesting an access token
                string oauth2Endpoint = "https://accounts.google.com/o/oauth2/v2/auth";

                // Parameters to pass to OAuth 2.0 endpoint.
                Dictionary<string, string> queryParams = new Dictionary<string, string>
                {
                    { "client_id", YOUR_CLIENT_ID },
                    { "redirect_uri", YOUR_REDIRECT_URI },
                    { "scope", "https://www.googleapis.com/auth/reminders" },
                    { "include_granted_scopes", "true" },
                    { "response_type", "token" }
                };

                // Build the query string from the parameters
                string queryString = string.Join("&", queryParams.Select(p => $"{p.Key}={Uri.EscapeDataString(p.Value)}"));

                // Navigate to the OAuth 2.0 endpoint in a new window.
                navigationManager.NavigateTo($"{oauth2Endpoint}?{queryString}", true);
            }
        }

## Version history:

- 0.0.0.1
    - `CreateReminder`
    - `DeleteReminder`
    - `GetReminder`
    - `ListReminders`

---

Related projects:
- Python original: https://github.com/jonahar/google-reminders-cli
- JavaScript port: https://github.com/Jinjinov/google-reminders-js
- PHP port: https://github.com/Jinjinov/google-reminders-php