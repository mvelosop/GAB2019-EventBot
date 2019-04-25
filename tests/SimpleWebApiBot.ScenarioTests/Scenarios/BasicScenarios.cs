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

            // Act ---------------------

            // Assert ------------------

        }

        private T GetService<T>() => _scope.ServiceProvider.GetRequiredService<T>();
    }
}
