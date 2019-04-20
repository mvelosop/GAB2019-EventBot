# GAB2019-EventBot

This repo contains the implementation of an simple bot with Bot Builder v4 C# SDK using an ASP.NET Core Web API app.

This repo goes along with my blog post: **TBD**

The bot can respond to arbitrary events sent to the endpoint POST /simple-bot/events/{event-name}/{user-name} where the Body contains an object { "Value": "anything, simple value or complex object" }

The bot can be used locally with Bot Emulator, published through **ngrok** or published to Azure.

If using Bot Service the bot `AppId` and `AppPassword` must be configured in `appsettings.json` or, better yet, as user secrets like this:

```json
{
  "BotWebApiApp:AppId": "your-bot-app-id",
  "BotWebApiApp:AppPassword": "<your-bot-app-password>"
}
```

To send an event you can use Postman or whatever, but it's important that you first send something to the bot, a simple "hi" is enough, so the conversation reference is saved (in memory), in `Conversations` class, keyed by username.

Keep in mind that username is "You" when using Azure's test web chat and "User" when using the emulator.

Hope this helps you.

Happy coding!
