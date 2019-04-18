using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration;
using Microsoft.Bot.Schema;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleWebApiBot.Timer
{
    public class Timer
    {
        private readonly IAdapterIntegration _adapter;
        private readonly ILogger _logger;

        public Timer(
            IAdapterIntegration adapter,
            ConversationReference conversationReference,
            int seconds,
            int number)
        {
            _adapter = adapter;
            _logger = Log.ForContext<Timer>();

            ConversationReference = conversationReference;
            Seconds = seconds;
            Number = number;
        }

        public ConversationReference ConversationReference { get; }

        public double Elapsed => ((FinishedAt ?? DateTime.Now) - (StartedAt ?? DateTime.Now)).TotalMilliseconds;

        public DateTime? FinishedAt { get; private set; }

        public int Number { get; }

        public int Seconds { get; }

        public DateTime? StartedAt { get; private set; }

        public string Status { get; private set; } = "Started";

        public async Task Start()
        {
            _logger.Information("----- Timer #{Number} [{Duration}s] started", Number, Seconds);

            StartedAt = DateTime.Now;
            Status = "Running";

            await Task.Delay(Seconds * 1000);

            FinishedAt = DateTime.Now;
            Status = "Finished";

            _logger.Information("----- Timer #{Number} [{Duration}s] finished ({Elapsed:n3}s)", Number, Seconds, Elapsed / 1000);

            await _adapter.ContinueConversationAsync("not-important-for-emulator", ConversationReference, SendMessageAsync);
        }

        private async Task SendMessageAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            await turnContext.SendActivityAsync($"Timer #{Number} finished! ({Seconds})s");
        }
    }
}