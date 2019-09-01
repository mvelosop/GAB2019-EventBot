using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Integration;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleWebApiBot.Setup
{
    public class TestAdapterIntegration : IAdapterIntegration
    {
        private readonly ILogger<TestAdapterIntegration> _logger;

        public TestAdapterIntegration(
            ILogger<TestAdapterIntegration> logger,
            TestAdapter adapter)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            Adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
        }

        public TestAdapter Adapter { get; }

        public Task ContinueConversationAsync(string botId, ConversationReference reference, BotCallbackHandler callback, CancellationToken cancellationToken = default)
        {
            _logger.LogTrace("----- Continuing conversation through TestAdapterIntegration");

            return Adapter.ContinueConversationAsync(botId, reference, callback, cancellationToken);
        }

        public async Task<InvokeResponse> ProcessActivityAsync(string authHeader, Activity activity, BotCallbackHandler callback, CancellationToken cancellationToken)
        {
            _logger.LogTrace("----- Processing activity through TestAdapterIntegration");

            await Adapter.ProcessActivityAsync(activity, callback, cancellationToken);

            return new InvokeResponse { Status = 200 };
        }
    }
}
