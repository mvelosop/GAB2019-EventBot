using Microsoft.Bot.Builder.Integration;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleWebApiBot.Timer
{
    public class Timers
    {
        private readonly IAdapterIntegration _adapter;
        private readonly string _botAppId;

        public Timers(
            IAdapterIntegration adapter,
            IConfiguration configuration)
        {
            _adapter = adapter;

            var appId = (configuration ?? throw new System.ArgumentNullException(nameof(configuration)))["BotWebApiApp:AppId"];

            _botAppId = string.IsNullOrWhiteSpace(appId) ? "*" : appId;
        }

        public List<Timer> List { get; set; } = new List<Timer>();

        public void AddTimer(ConversationReference reference, int seconds)
        {
            var timer = new Timer(_adapter, _botAppId, reference, seconds, List.Count + 1);
            List.Add(timer);

            Task.Run(() => timer.Start());
        }
    }
}