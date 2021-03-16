using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MyLab.Log.Syslog
{
    /// <summary>
    /// Extensions to integrate syslog logging
    /// </summary>
    public static class IntegrationExtension
    {
        /// <summary>
        /// Default value for configuration section name
        /// </summary>
        public const string DefaultConfigSectionName = "Logging:Syslog";

        /// <summary>
        /// Adds SYSLOG compatible logger with options
        /// </summary>
        public static ILoggingBuilder AddSyslog(this ILoggingBuilder lBuilder, Action<SyslogLoggerOptions> configure = null)
        {
            if (lBuilder == null) throw new ArgumentNullException(nameof(lBuilder));

            lBuilder.Services.AddSingleton<ILoggerProvider, SyslogLoggerProvider>();

            if(configure != null)
                lBuilder.Services.Configure(configure);

            return lBuilder;
        }

        /// <summary>
        /// Configure MyLab.Log.Syslog
        /// </summary>
        public static IServiceCollection ConfigureSyslog(this IServiceCollection services, Action<SyslogLoggerOptions> configure)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (configure == null) throw new ArgumentNullException(nameof(configure));

            return services.Configure(configure);
        }

        /// <summary>
        /// Configure MyLab.Log.Syslog
        /// </summary>
        public static IServiceCollection ConfigureSyslog(this IServiceCollection services, IConfiguration configuration, string configSectionName = DefaultConfigSectionName)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            if (configSectionName == null) throw new ArgumentNullException(nameof(configSectionName));
            
            return services.Configure<SyslogLoggerOptions>(configuration.GetSection(configSectionName));
        }
    }
}
