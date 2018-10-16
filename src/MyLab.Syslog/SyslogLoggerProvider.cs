using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MyLab.Syslog
{
    [ProviderAlias("syslog")]
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
            return new SyslogLogger(new UdpLogSenderFactory(), Options);
        }

        public void Dispose()
        {
        }
    }
}