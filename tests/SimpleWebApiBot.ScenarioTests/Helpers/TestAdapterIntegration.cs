using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Integration;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleWebApiBot.ScenarioTests.Helpers
{
    public class TestAdapterIntegration : IAdapterIntegration
    {
        public TestAdapterIntegration(TestAdapter adapter)
        {
            Adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
        }

        public TestAdapter Adapter { get; }

        public Task ContinueConversationAsync(string botId, ConversationReference reference, BotCallbackHandler callback, CancellationToken cancellationToken = default)
        {
            return Adapter.ContinueConversationAsync(botId, reference, callback, cancellationToken);
        }

        public async Task<InvokeResponse> ProcessActivityAsync(string authHeader, Activity activity, BotCallbackHandler callback, CancellationToken cancellationToken)
        {
            await Adapter.ProcessActivityAsync(activity, callback, cancellationToken);

            return new InvokeResponse { Status = 200 };
        }
    }
}
