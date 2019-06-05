using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Logging;
using MyLab.Logging;
using Newtonsoft.Json;
using YamlDotNet.Serialization;

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
            var le = state as LogEntity;
            
            var logTime = le?.Time ?? DateTime.Now;
            
            string messagePayload = le != null  
                ? SerializeLogEntity(le, Options.Format)
                : formatter(state, exception).Replace(Environment.NewLine, "\\r\\n");

            var serializer = new SyslogMessageSerializer(Options)
            {
                EventId = eventId,
                LogTime = logTime,
                Level = logLevel
            };
            
            var sender = CreateLogSender();

            var messagePayloads = CreateMessagePayloads(
                messagePayload, 
                sender.LengthLimit - serializer.GetHeaderLen()
                ).ToArray();

            for (int i = 0; i < messagePayloads.Length; i++)
            {
                try
                {
                    sender.Send(serializer.Serialize(messagePayloads[i])).Wait();
                }
                catch(AggregateException e)
                {
                    throw e.InnerException;
                }
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

            var binPayload = Encoding.UTF8.GetBytes(messagePayload);

            int index = 0;
            bool isLast;

            do
            {
                var left = binPayload.Length - index;
                isLast = left <= senderLengthLimit;

                var chunk = isLast ? left : senderLengthLimit;

                yield return Encoding.UTF8.GetString(binPayload, index, chunk);

                index += chunk;
            } while (!isLast);
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

        string SerializeLogEntity(LogEntity logEntity, string format)
        {   
            switch (format.ToLower())
            {
                case "json":
                {
                    return JsonConvert.SerializeObject(logEntity, new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore
                    });        
                }
                case "yaml":
                {
                    var s = new SerializerBuilder().Build();
                    return s.Serialize(logEntity);
                }
                default: throw new NotSupportedException("Format not supported");
            }   
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