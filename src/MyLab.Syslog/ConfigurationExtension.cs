using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MyLab.Syslog
{
    /// <summary>
    /// Extension for logging configuration
    /// </summary>
    public static class ConfigurationExtension
    {
        /// <summary>
        /// Adds SYSLOG compatible logger 
        /// </summary>
        public static ILoggingBuilder AddSyslog(this ILoggingBuilder lBuilder)
        {
            lBuilder.Services.AddSingleton<ILoggerProvider, SyslogLoggerProvider>();
            return lBuilder;
        }

        /// <summary>
        /// Adds SYSLOG compatible logger with options
        /// </summary>
        public static ILoggingBuilder AddSyslog(this ILoggingBuilder lBuilder, Action<SyslogLoggerOptions> configure)
        {
            lBuilder.Services.AddSingleton<ILoggerProvider, SyslogLoggerProvider>();
            lBuilder.Services.Configure(configure);
            return lBuilder;
        }
    }
}
