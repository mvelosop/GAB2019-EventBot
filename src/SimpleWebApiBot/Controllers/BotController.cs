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

        public BotController(
            ILogger<BotController> logger,
            IAdapterIntegration adapter,
            IBot bot,
            IConfiguration configuration,
            Conversations conversations)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
            _bot = bot ?? throw new ArgumentNullException(nameof(bot));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _conversations = conversations ?? throw new ArgumentNullException(nameof(conversations));
        }

        [HttpPost("/simple-bot/messages")]
        public async Task<InvokeResponse> Messages([FromBody]Activity activity)
        {
            _logger.LogTrace("----- BotController - Receiving message activity: {@Activity}", activity);

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
                body = await reader.ReadToEndAsync();
            }

            _logger.LogTrace("----- BotController - Receiving event: \"{EventName}\" - user: \"{UserName}\" ({Body})", eventName, userName, body);

            var conversation = _conversations.Get(userName);

            if (conversation == null)
            {
                return new InvokeResponse
                {
                    Status = 404,
                    Body = body
                };
            }

            var activity = conversation.GetContinuationActivity();

            activity.Name = eventName;
            activity.RelatesTo = null;
            activity.Value = body;
            activity.ValueType = typeof(string).FullName;

            _logger.LogTrace("----- BotController - Craft event activity: {@Activity}", activity);

            var botAppId = _configuration["BotWebApiApp:AppId"];

            if (botAppId == string.Empty)
            {
                await _adapter.ProcessActivityAsync(string.Empty, activity, _bot.OnTurnAsync, default);
            }
            else
            {
                var claimsIdentity = new ClaimsIdentity(new List<Claim>
                {
                    // Adding claims for both Emulator and Channel.
                    new Claim(AuthenticationConstants.AudienceClaim, botAppId),
                    new Claim(AuthenticationConstants.AppIdClaim, botAppId),
                });

                var botFrameworkAdapter = _adapter as BotFrameworkAdapter;

                await botFrameworkAdapter.ProcessActivityAsync(claimsIdentity, activity, _bot.OnTurnAsync, default);
            }

            return new InvokeResponse
            {
                Status = 200,
                Body = body
            };
        }
    }
}