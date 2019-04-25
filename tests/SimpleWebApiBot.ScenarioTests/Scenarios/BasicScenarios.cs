using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SimpleWebApiBot.ScenarioTests.Setup;
using System;
using System.Collections.Generic;
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
            var testFlow = CreateTestFlow()
                .Send("HI")
                .AssertReply("You (\"**User1**\") typed \"HI\"");

            // Act ---------------------
            await testFlow.StartTestAsync();

            // Assert ------------------

        }

        private TestFlow CreateTestFlow() => new TestFlow(GetService<TestAdapter>(), GetService<IBot>());

        private T GetService<T>() => _scope.ServiceProvider.GetRequiredService<T>();
    }
}
