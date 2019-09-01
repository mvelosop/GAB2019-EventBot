using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using SimpleWebApiBot.Bots;
using SimpleWebApiBot.Timer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace SimpleWebApiBot.IntegrationTests.BotController
{
    public class BotControllerTests : IClassFixture<WebApiBotApplicationFactory<IntegrationTestsStartup>>, IDisposable
    {
        private readonly WebApplicationFactory<IntegrationTestsStartup> _testServerFactory;
        private readonly HttpClient _client;
        private readonly IServiceScope _scope;

        public BotControllerTests(WebApiBotApplicationFactory<IntegrationTestsStartup> testServerFactory)
        {
            _testServerFactory = testServerFactory ?? throw new ArgumentNullException(nameof(testServerFactory));
            _client = _testServerFactory.CreateClient();
            _scope = _testServerFactory.Server.Host.Services.CreateScope();
        }

        [Fact]
        public async Task GetTimers_ShouldReturnAnEmptyList_WhenNoTimers()
        {
            // Arrange -----------------
            var client = _testServerFactory.CreateClient();

            // Act ---------------------
            var response = await client.GetAsync("/simple-bot/api/timers");
            var data = await response.Content.ReadAsStringAsync();

            // Assert ------------------
            response.EnsureSuccessStatusCode();
            data.Should().NotBeNull();
        }

        [Theory]
        [InlineData(1, "/simple-bot/messages")]
        [InlineData(2, "/api/messages")]
        public async Task BotShouldEchoBack(int test, string endpoint)
        {
            // Arrange -----------------

            // Act ---------------------
            await SendAsync(endpoint, "HI");

            // Assert ------------------
            await AssertReplyAsync("You (\"**User1**\") typed \"HI\"");
        }

        [Theory]
        [InlineData(1, "/simple-bot/messages")]
        public async Task BotShouldCreateTimerAndSendProactiveMessage(int test, string endpoint)
        {
            // Arrange -----------------
            var timers = GetService<Timers>();
            var conversations = GetService<Conversations>();

            // Act ---------------------
            await SendAsync(endpoint, "TIMER 2");
            await AssertReplyAsync("Starting a 2s timer");

            await SendAsync(endpoint, "TIMER 3");
            await AssertReplyAsync("Starting a 3s timer");

            await AssertReplyAsync(activity => activity.AsMessageActivity().Text
                .Should().StartWith("Timer #1 finished! (2"), 2100);

            await AssertReplyAsync(activity => activity.AsMessageActivity().Text
                .Should().StartWith("Timer #2 finished! (3"), 3100);

            // Assert ------------------
            timers.List.Count.Should().Be(2);

            timers.List.Should().BeEquivalentTo(new[] 
            {
                new { Number = 1, Seconds = 2 }, new { Number = 2, Seconds = 3 }
            });

            timers.List[0].Elapsed.Should().BeApproximately(2000, 20);
            timers.List[1].Elapsed.Should().BeApproximately(3000, 20);

            conversations.Keys.Should().BeEquivalentTo(new[] { "User1" });
        }

        [Fact]
        public async Task BotShouldSendMessage_WhenEventReceived()
        {
            // Arrange -----------------
            var eventsEndpoint = "/simple-bot/events";
            var eventName = "testing-event";
            var eventUser = "User1";
            var eventPayload = "Event payload";
            var eventContent = new StringContent(eventPayload, Encoding.UTF8, "application/json");

            // Act ---------------------
            await SendAsync("/simple-bot/messages", "HI");
            await AssertReplyAsync("You (\"**User1**\") typed \"HI\"");

            var response = await _client.PostAsync($"{eventsEndpoint}/{eventName}/{eventUser}", eventContent);

            await AssertReplyAsync($"**{eventName}** event detected - Payload: {eventPayload}");

            // Assert ------------------
            response.EnsureSuccessStatusCode();
        }

        private async Task SendAsync(string endpoint, string text)
        {
            var botAdapter = GetService<TestAdapter>();
            var activity = botAdapter.MakeActivity(text);
            var activityJson = JsonConvert.SerializeObject(activity);
            var response = await _client.PostAsync(endpoint, new StringContent(activityJson, Encoding.UTF8, "application/json"));

            response.EnsureSuccessStatusCode();
        }

        private Task AssertReplyAsync(string text, int timeout = 3000)
        {
            return AssertReplyAsync(activity => activity.AsMessageActivity().Text.Should().Be(text), timeout);
        }

        private async Task AssertReplyAsync(Action<IActivity> validateActivity, int timeout = 3000)
        {
            var botAdapter = GetService<TestAdapter>();
            var sw = new Stopwatch();

            sw.Start();

            while (sw.ElapsedMilliseconds < timeout)
            {
                var reply = botAdapter.GetNextReply();

                if (reply != null)
                {
                    validateActivity.Invoke(reply);

                    return;
                }

                await Task.Delay(10);
            }

            throw new TimeoutException($"{timeout}ms Timed out waiting for activity");
        }

        private T GetService<T>()
        {
            return _scope.ServiceProvider.GetRequiredService<T>();
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _scope.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~BotControllerTests()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
