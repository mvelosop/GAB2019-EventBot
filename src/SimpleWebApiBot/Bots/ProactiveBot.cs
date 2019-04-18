using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
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

        public ProactiveBot(
            ILogger<ProactiveBot> logger,
            Timers timers)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _timers = timers ?? throw new ArgumentNullException(nameof(timers));
        }

        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            if (turnContext.Activity.Type == ActivityTypes.Message)
            {
                var text = turnContext.Activity.Text.Trim();

                _logger.LogInformation("----- Receiving message activity - Text: {Text}", text);

                if (text.StartsWith("timer", StringComparison.InvariantCultureIgnoreCase))
                {
                    var seconds = Convert.ToInt32(text.Substring(text.IndexOf(" ")));

                    await turnContext.SendActivityAsync($"Starting a timer to go off in {seconds}s");

                    _timers.AddTimer(turnContext.Activity.GetConversationReference(), seconds);
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
            else
            {
                await turnContext.SendActivityAsync($"{turnContext.Activity.Type} event detected");
            }
        }
    }
}