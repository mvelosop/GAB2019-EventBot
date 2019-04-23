using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SimpleWebApiBot.Timer;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleWebApiBot.Bots
{
    public class ProactiveBot : IBot
    {
        private readonly ILogger<ProactiveBot> _logger;
        private readonly Timers _timers;
        private readonly Conversations _conversations;

        public ProactiveBot(
            ILogger<ProactiveBot> logger,
            Timers timers,
            Conversations conversations)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _timers = timers ?? throw new ArgumentNullException(nameof(timers));
            _conversations = conversations ?? throw new ArgumentNullException(nameof(conversations));
        }

        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            var activityType = turnContext.Activity.Type;
            var conversationReference = (ConversationReference)null;

            if (activityType == ActivityTypes.Message || activityType == ActivityTypes.ConversationUpdate)
            {
                conversationReference = turnContext.Activity.GetConversationReference();

                _logger.LogTrace("----- ProactiveBot - Get conversation reference - Activity type: {ActivityType} User: \"{User}\" - ConversationReference: {@ConversationReference}", activityType,  conversationReference.User.Name, conversationReference);

                if (conversationReference.User.Name != null)
                {
                    _conversations.Save(conversationReference);
                }
            }

            if (activityType == ActivityTypes.Message)
            {
                var text = turnContext.Activity.Text.Trim();

                _logger.LogInformation("----- Receiving message activity - Text: {Text}", text);

                var username = conversationReference.User.Name;

                if (text.StartsWith("timer ", StringComparison.InvariantCultureIgnoreCase))
                {
                    var seconds = Convert.ToInt32(text.Substring(text.IndexOf(" ")));

                    await turnContext.SendActivityAsync($"Starting a {seconds}s timer");

                    _logger.LogInformation("----- Adding timer - ConversationReference: {@ConversationReference}", conversationReference);

                    _timers.AddTimer(conversationReference, seconds);
                }
                else if (text.StartsWith("timers", StringComparison.InvariantCultureIgnoreCase))
                {
                    var alarms = string.Join("\n", _timers.List.Select(a => $"- **#{a.Number}** [{a.Seconds}s] - {a.Status} ({a.Elapsed / 1000:n3}s)"));

                    await turnContext.SendActivityAsync($"**TIMERS**\n{alarms}");
                }
                else if (text.StartsWith("conversations", StringComparison.InvariantCultureIgnoreCase))
                {
                    var conversations = string.Join("\n", _conversations.Select(c => $"- **{c.Key}**: {c.Value.ChannelId} ({c.Value.User.Id})"));

                    await turnContext.SendActivityAsync($"**CONVERSATIONS**\n{conversations}");
                }
                else
                {
                    // Echo back to the user whatever they typed.
                    await turnContext.SendActivityAsync($"You (\"**{username}**\") typed \"{text}\"");
                }
            }
            else if (activityType == ActivityTypes.Event)
            {
                var value = JsonConvert.SerializeObject(turnContext.Activity.Value);

                _logger.LogInformation("----- Receiving event activity - Name: {Name} ({Value})", turnContext.Activity.Name, value);

                await turnContext.SendActivityAsync($"**{turnContext.Activity.Name}** event detected - {value}");
            }
            else
            {
                await turnContext.SendActivityAsync($"{turnContext.Activity.Type} event detected");
            }
        }
    }
}