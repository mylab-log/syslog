using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MyLab.Syslog
{
    [ProviderAlias("Syslog")]
    class SyslogLoggerProvider : ILoggerProvider
    {
        public SyslogLoggerOptions Options { get; }

        public SyslogLoggerProvider()
        {
            
        }

        public SyslogLoggerProvider(IOptions<SyslogLoggerOptions> options)
            : this(options.Value)
        {
            
        }

        public SyslogLoggerProvider(SyslogLoggerOptions options)
        {
            Options = options;
        }

        public ILogger CreateLogger(string categoryName)
        {
            var senderFactory = (Options?.UseTcp ?? false)
                ? (ILogSenderFactory) new TcpLogSenderFactory()
                : new UdpLogSenderFactory();
            return new SyslogLogger(senderFactory, Options);
        }

        public void Dispose()
        {
        }
    }
}