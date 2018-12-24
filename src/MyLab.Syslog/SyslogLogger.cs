using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Reflection;
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

            var eventIdInjection = Options.IncludeEventId
                ? eventId.Id + " "
                : string.Empty;
            
            string msgStart = $"<{priority}>1 {logTime:s}Z {hostname} {appName} {procId} {eventIdInjection}"; //\uFEFF

            var sender = CreateLogSender();

            var messagePayloads = CreateMessagePayloads(messagePayload, sender.LengthLimit - msgStart.Length);

            foreach (var payload in messagePayloads)
            {
                sender.Send(msgStart + payload).Wait();
            }
        }

        private ILogSender CreateLogSender()
        {
            if (string.IsNullOrEmpty(Options.RemoteHost))
                throw new InvalidOperationException("Host name is not specified");
            if (Options.RemotePort == 0)
                throw new InvalidOperationException("Port is not specified");
            
            return LogSenderFactory.Create(Options.RemoteHost, Options.RemotePort);
        }

        private IEnumerable<string> CreateMessagePayloads(string messagePayload, int senderLengthLimit)
        {
            if (senderLengthLimit <= 0)
            {
                yield return messagePayload;
                yield break;
            }

            string left = messagePayload;

            while (left.Length > senderLengthLimit)
            {
                yield return left.Substring(0, senderLengthLimit);
                left = left.Substring(senderLengthLimit, left.Length - senderLengthLimit);
            }

            if (left.Length != 0)
                yield return left;
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