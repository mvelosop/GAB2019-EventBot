using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using SimpleWebApiBot.Timer;

namespace SimpleWebApiBot.Controllers
{
    [ApiController]
    public class BotController : ControllerBase
    {
        private readonly ILogger<BotController> _logger;
        private readonly IAdapterIntegration _adapter;
        private readonly IBot _bot;

        public BotController(
            ILogger<BotController> logger,
            IAdapterIntegration adapter,
            IBot bot)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
            _bot = bot ?? throw new ArgumentNullException(nameof(bot));
        }

        [HttpPost("/simple-bot/messages")]
        public async Task<InvokeResponse> Messages([FromBody]Activity activity)
        {
            _logger.LogTrace("----- BotController - Receiving activity: {@Activity}", activity);

            return await _adapter.ProcessActivityAsync(string.Empty, activity, _bot.OnTurnAsync, default);
        }
    }
}