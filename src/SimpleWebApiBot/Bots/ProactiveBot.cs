﻿using Microsoft.Bot.Builder;
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
            if (turnContext.Activity.Type == ActivityTypes.Message)
            {
                var text = turnContext.Activity.Text.Trim();

                _logger.LogInformation("----- Receiving message activity - Text: {Text}", text);

                var conversationReference = turnContext.Activity.GetConversationReference();

                _conversations.Save(conversationReference);

                if (text.StartsWith("timer", StringComparison.InvariantCultureIgnoreCase))
                {
                    var seconds = Convert.ToInt32(text.Substring(text.IndexOf(" ")));

                    await turnContext.SendActivityAsync($"Starting a timer to go off in {seconds}s");

                    _logger.LogInformation("----- Adding timer - ConversationReference: {@ConversationReference}", conversationReference);

                    _timers.AddTimer(conversationReference, seconds);
                }
                else if (text.StartsWith("list", StringComparison.InvariantCultureIgnoreCase))
                {
                    var alarms = string.Join("\n", _timers.List.Select(a => $"- #{a.Number} [{a.Seconds}s] - {a.Status} ({a.Elapsed / 1000:n3}s)"));

                    await turnContext.SendActivityAsync($"**TIMERS**\n{alarms}");
                }
                else
                {
                    // Echo back to the user whatever they typed.
                    await turnContext.SendActivityAsync($"You typed \"{text}\"");
                }
            }
            else if (turnContext.Activity.Type == ActivityTypes.Event)
            {
                var value = JsonConvert.SerializeObject(turnContext.Activity.Value);
                await turnContext.SendActivityAsync($"{turnContext.Activity.Name} event detected - Value: {value}");
            }
            else
            {
                await turnContext.SendActivityAsync($"{turnContext.Activity.Type} event detected");
            }
        }
    }
}