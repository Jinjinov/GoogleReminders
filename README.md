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