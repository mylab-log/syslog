using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;
using Microsoft.Extensions.Logging;
using MyLab.Logging;
using Newtonsoft.Json;

namespace MyLab.Syslog
{
    internal class SyslogLogger : ILogger
    {
        public ILogSenderFactory LogSenderFactory { get; }

        public SyslogLoggerOptions Options { get; }

        public SyslogLogger(ILogSenderFactory logSenderFactory, SyslogLoggerOptions options)
        {
            LogSenderFactory = logSenderFactory ?? throw new ArgumentNullException(nameof(logSenderFactory));
            Options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            DateTime logTime;
            string messagePayload;

            var le = state as LogEntity;

            if (le != null)
            {
                messagePayload = SerializeLogEntity(le);
                logTime = le.Time;
            }
            else
            {
                messagePayload = formatter(state, exception).Replace(Environment.NewLine, "\\r\\n");
                logTime = DateTime.Now;
            }

            int priority = CalcPriority(logLevel);
            string hostname = Options?.Hostname ?? Dns.GetHostName();
            string appName = Options?.AppName ?? Assembly.GetEntryAssembly().GetName().Name;
            string procId = Options?.ProcId ?? Process.GetCurrentProcess().Id.ToString();
            
            string msg = $"<{priority}>1 {logTime:s}Z {hostname} {appName} {procId} {eventId.Id} BOM {messagePayload}";

            SendMessage(msg);
        }

        private void SendMessage(string msg)
            {
            if (string.IsNullOrEmpty(Options.RemoteHost))
                throw new InvalidOperationException("Host name is not specified");
            if (Options.RemotePort == 0)
                throw new InvalidOperationException("Port is not specified");

            var sender = LogSenderFactory.Create(Options.RemoteHost, Options.RemotePort);
            sender.Send(msg).Wait();
        }

        private int CalcPriority(LogLevel logLevel)
        {
            int severity;


            switch (logLevel)
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

            return Options.Facility * 8 + severity;
        }

        string SerializeLogEntity(LogEntity logEntity)
        {
            return JsonConvert.SerializeObject(logEntity, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }
    }
}