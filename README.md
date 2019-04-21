# GAB2019-EventBot

This repo contains the implementation of a simple bot with Bot Builder v4 C# SDK using an ASP.NET Core Web API app.

This repo goes along with my blog post: **TBD**

The bot can respond to arbitrary events sent to the endpoint POST /simple-bot/events/{event-name}/{user-name} where the Body contains an object { "Value": "anything, simple value or complex object" }

## Set up

### Run locally

These are the minimal steps to test the app locally:

1. Run the app locally with VS 2019
2. Run Bot Emulator and open the **SimpleWebApiBot.bot** file.

## Test

1. When you send "**TIMER 5**" message, you should receive a timer setup acknowledge reply.

2. When you send a "**TIMERS**" message, you should get the list of current timers (since last startup) with their statuses.

3. You should receive a "**Timer #X finished!**" when a timer's up.

   ![](images/proactive-bot-timer-interaction.png)

4. When you POST to the events endpoint you should get the posted value in the chat.

   ```console
   curl -d "{'Value':'Success'}" -H "Content-Type: application/json" -X POST http://localhost:5000/simple-bot/events/process-completed/User
   ```

   ![](images/proactive-bot-event-interaction.png)

5. When you refresh the home page (http://localhost:5000) you should see something like this

   ![](images/proactive-bot-home-page.png)

**NOTE:** It's necessary that there has been at least one interaction with the bot, to send events to the user.

Keep in mind that username is "You" when using Azure's test web chat and "User" when using the emulator. You can check this in the home page.

## ngrok

You can use the script `ngrok\start-ngrok` to start **ngrok**.

## Bot Service

To use Bot Service the bot's `AppId` and `AppPassword` must be configured in `appsettings.json` or, better yet, as user secrets like this:

```json
{
  "BotWebApiApp:AppId": "<your-bot-app-id>",
  "BotWebApiApp:AppPassword": "<your-bot-app-password>"
}
```

Hope this helps you.

Happy coding!
