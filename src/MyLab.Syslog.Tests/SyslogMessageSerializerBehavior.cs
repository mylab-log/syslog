using System;
using Microsoft.Extensions.Logging;
using Xunit;

namespace MyLab.Syslog.Tests
{
    public class SyslogMessageSerializerBehavior
    {
        [Fact]
        public void ShouldSerializeHeaderWell()
        {
            //Arrange
            var options = new SyslogLoggerOptions
            {
                Hostname = "host",
                AppName = "app",
                ProcId = "proc"
            };

            var logTime = new DateTime(2001, 1, 1, 1, 1, 1,1, DateTimeKind.Local)
                .AddTicks(10);
            
            var serializer = new SyslogMessageSerializer(options)
            {
                 Level = LogLevel.Debug,
                 EventId = new EventId(0),
                 LogTime = logTime
            };
            
            //Act
            var strMsg = serializer.Serialize("foo");
            
            //Assert
            Assert.Equal("<191>1 2001-01-01T01:01:01.001001+03:00 host app proc foo", strMsg);
        }
    }
}