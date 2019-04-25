using FluentAssertions;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SimpleWebApiBot.ScenarioTests.Setup;
using SimpleWebApiBot.Timer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace SimpleWebApiBot.ScenarioTests.Scenarios
{
    public class BasicScenarios : IClassFixture<TestingHost>
    {
        private readonly IServiceScope _scope;
        private readonly ILogger<BasicScenarios> _logger;

        public BasicScenarios(TestingHost host)
        {
            _scope = (host ?? throw new ArgumentNullException(nameof(host))).CreateScope();
            _logger = GetService<ILogger<BasicScenarios>>();

            _logger.LogTrace("----- INSTANCE CREATED - {ClassName}", GetType().Name);
        }

        [Fact]
        public async Task BotShouldEchoBack()
        {
            // Arrange -----------------

            // Act ---------------------
            await CreateTestFlow()
                .Send("HI")
                .AssertReply("You (\"**User1**\") typed \"HI\"")
                .StartTestAsync();

            // Assert ------------------

        }

        [Fact]
        public async Task BotShouldCreateTimerAndSendProactiveMessage()
        {
            // Arrange -----------------
            var timers = GetService<Timers>();

            // Act ---------------------
            await CreateTestFlow()
                .Send("TIMER 2")
                .AssertReply("Starting a 2s timer")
                .StartTestAsync();

            await CreateTestFlow()
                .Send("TIMER 3")
                .AssertReply("Starting a 3s timer")
                .AssertReply(activity => activity.AsMessageActivity().Text.Should().StartWith("Timer #1 finished! (2"), null, 2100)
                .AssertReply(activity => activity.AsMessageActivity().Text.Should().StartWith("Timer #2 finished! (3"), null, 3100)
                .StartTestAsync();

            // Assert ------------------
            timers.List.Count.Should().Be(2);

            timers.List.Should().BeEquivalentTo(new[] { new { Number = 1, Seconds = 2 }, new { Number = 2, Seconds = 3 } });

            timers.List[0].Elapsed.Should().BeApproximately(2000, 20);
            timers.List[1].Elapsed.Should().BeApproximately(3000, 20);

        }

        private TestFlow CreateTestFlow() => new TestFlow(GetService<TestAdapter>(), GetService<IBot>());

        private T GetService<T>() => _scope.ServiceProvider.GetRequiredService<T>();
    }
}
