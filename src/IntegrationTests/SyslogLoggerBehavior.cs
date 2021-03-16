using System;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyLab.Log.Dsl;
using MyLab.Log.Syslog;
using Xunit;

namespace IntegrationTests
{
    public class SyslogLoggerBehavior
    {
        [Fact]
        public void ShouldSendAction()
        {
            //Arrange 
            var log = CreateDslLogger();

            //Act
            log.Action("MyLab.Log.Syslog test action message").Write();

            //Assert
        }

        [Fact]
        public void ShouldSendError()
        {
            //Arrange 
            var log = CreateDslLogger();
            Exception exception;

            try
            {
                throw new Exception("MyLab.Log.Syslog test error message");
            }
            catch (Exception e)
            {
                exception = e;
            }

            //Act
            log.Error(exception).Write();

            //Assert
        }

        [Fact]
        public void ShouldSplitVeryLargeMessage()
        {
            //Arrange 
            var log = CreateDslLogger();
            var longMessage = string.Concat(Enumerable.Repeat("MyLab.Log.Syslog test action message", 1000));

            //Act
            log.Action(longMessage).Write();

            //Assert
        }


        IDslLogger CreateDslLogger()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging(c => c.AddSyslog());

            var config = new ConfigurationBuilder()
                .AddJsonFile("config.json")
                .Build();

            serviceCollection.AddSingleton(config);
            serviceCollection.Configure<SyslogLoggerOptions>(config.GetSection("Logging:Syslog"));

            var serviceProvider = serviceCollection.BuildServiceProvider();

            var lf = serviceProvider.GetService<ILoggerFactory>();
            var logger = lf.CreateLogger("mycat");
            return logger.Dsl();
        }
    }
}
