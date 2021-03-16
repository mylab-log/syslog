using System;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace MyLab.Log.Syslog
{
    /// <summary>
    /// Serializes message
    /// </summary>
    public class SyslogMessageSerializer
    {
        private readonly SyslogLoggerOptions _options;

        private readonly Lazy<string> _headerStr;

        /// <summary>
        /// Message level
        /// </summary>
        public LogLevel Level { get; set; }

        /// <summary>
        /// Occurence time
        /// </summary>
        public DateTime LogTime { get; set; }

        /// <summary>
        /// Event identifier
        /// </summary>
        public EventId EventId { get; set; }

        /// <summary>
        /// Initializes a new instance of <see cref="SyslogMessageSerializer"/>
        /// </summary>
        public SyslogMessageSerializer(SyslogLoggerOptions options)
        {
            _options = options;
            
            _headerStr = new Lazy<string>(CreateHeader);
        }

        /// <summary>
        /// Gets header length
        /// </summary>
        public int GetHeaderLen()
        {
            return _headerStr.Value.Length;
        }
        
        /// <summary>
        /// Serializes message with content
        /// </summary>
        public string Serialize(string content)
        {
            return _headerStr.Value + content;
        }

        string CreateHeader()
        {
            int priority = CalcPriority(_options.Facility);
            string hostname = _options?.Hostname ?? Dns.GetHostName();
            string appName = _options?.AppName ?? Assembly.GetEntryAssembly()?.GetName().Name;
            string procId = _options?.ProcId ?? Process.GetCurrentProcess().Id.ToString();

            var eventIdInjection = _options.IncludeEventId
                ? EventId.Id + " "
                : string.Empty;
            
            return $"<{priority}>1 {LogTime:yyyy-MM-dd'T'HH:mm:ss.ffffffK} {hostname} {appName} {procId} {eventIdInjection}"; //\uFEFF
        }
        
        int CalcPriority(int facility)
        {
            int severity;


            switch (Level)
            {
                case LogLevel.Trace:
                case LogLevel.Debug:
                    severity = 7;
                    break;
                case LogLevel.Warning:
                    severity = 4;
                    break;
                case LogLevel.Error:
                    severity = 3;
                    break;
                case LogLevel.Critical:
                    severity = 2;
                    break;
                default:
                    severity = 6;
                    break;
            }

            return facility * 8 + severity;
        }
    }
}