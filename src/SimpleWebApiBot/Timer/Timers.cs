using Microsoft.Bot.Builder.Integration;
using Microsoft.Bot.Schema;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleWebApiBot.Timer
{
    public class Timers
    {
        private readonly IAdapterIntegration _adapter;

        public Timers(IAdapterIntegration adapter)
        {
            _adapter = adapter;
        }

        public List<Timer> List { get; set; } = new List<Timer>();

        public void AddTimer(ConversationReference reference, int seconds)
        {
            var timer = new Timer(_adapter, reference, seconds, List.Count + 1);

            Task.Run(() => timer.Start());

            List.Add(timer);
        }
    }
}