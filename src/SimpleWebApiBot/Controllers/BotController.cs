using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SimpleWebApiBot.Bots;
using SimpleWebApiBot.Timer;

namespace SimpleWebApiBot.Controllers
{
    [ApiController]
    public class BotController : ControllerBase
    {
        private readonly ILogger<BotController> _logger;
        private readonly IAdapterIntegration _adapter;
        private readonly IBot _bot;
        private readonly IConfiguration _configuration;
        private readonly Conversations _conversations;
        private readonly Timers _timers;

        public BotController(
            ILogger<BotController> logger,
            IAdapterIntegration adapter,
            IBot bot,
            IConfiguration configuration,
            Conversations conversations,
            Timers timers)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
            _bot = bot ?? throw new ArgumentNullException(nameof(bot));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _conversations = conversations ?? throw new ArgumentNullException(nameof(conversations));
            _timers = timers ?? throw new ArgumentNullException(nameof(timers));

            _logger.LogTrace("----- INSTANCE CREATED - {ClassName}", GetType().Name);
        }

        [HttpPost("/simple-bot/messages")]
        public async Task<InvokeResponse> Messages([FromBody]Activity activity)
        {
            var entities = JsonConvert.SerializeObject(activity.Entities);

            _logger.LogTrace("----- BotController - Receiving message activity: {@Activity} (Entities: {Entities})", activity, entities);

            var authHeader = HttpContext.Request.Headers["Authorization"];

            return await _adapter.ProcessActivityAsync(authHeader, activity, _bot.OnTurnAsync, default);
        }

        [HttpPost("/simple-bot/events/{eventName}/{userName}")]
        public async Task<InvokeResponse> Events([FromRoute]string eventName, [FromRoute]string userName)
        {
            string body = null;

            userName = WebUtility.UrlDecode(userName);

            using (var reader = new StreamReader(ControllerContext.HttpContext.Request.Body))
            {
                // quick and dirty sanitization
                body = (await reader.ReadToEndAsync())
                    .Replace("script", "", StringComparison.InvariantCultureIgnoreCase)
                    .Replace("href", "", StringComparison.InvariantCultureIgnoreCase);
            }

            _logger.LogTrace("----- BotController - Receiving event: \"{EventName}\" - user: \"{UserName}\" ({Body})", eventName, userName, body);

            var conversation = _conversations.Get(userName);

            if (conversation == null)
            {
                return new InvokeResponse { Status = 404, Body = body };
            }

            var botAppId = _configuration["BotWebApiApp:AppId"];

            await _adapter.ContinueConversationAsync(botAppId, conversation, async (context, token) =>
            {
                context.Activity.Name = eventName;
                context.Activity.RelatesTo = null;
                context.Activity.Value = body;

                _logger.LogTrace("----- BotController - Craft event activity: {@Activity}", context.Activity);

                await _bot.OnTurnAsync(context, token);
            });

            return new InvokeResponse { Status = 200, Body = body };
        }

        [HttpGet("simple-bot/api/timers")]
        public ActionResult<Timers> GetTimers()
        {
            return Ok(_timers.List);
        }
    }
}