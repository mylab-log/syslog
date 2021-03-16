using System;
using System.Text;
using Microsoft.Extensions.Logging;
using MyLab.Log.Serializing;

namespace MyLab.Log.Syslog
{
    internal class SyslogLogger : ILogger
    {
        private const string CroppedSuffix = "... [cropped]";
        private static readonly int CroppedSuffixLen = Encoding.UTF8.GetByteCount(CroppedSuffix);

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
            
            string messageString = le != null  
                ? SerializeLogEntity(le, Options.Format)
                : formatter(state, exception).Replace(Environment.NewLine, "\\r\\n");

            var serializer = new SyslogMessageSerializer(Options)
            {
                EventId = eventId,
                LogTime = logTime,
                Level = logLevel
            };
            
            var sender = CreateLogSender();

            var messagePayload = CreateMessagePayload(
                messageString, 
                sender.LengthLimit - serializer.GetHeaderLen());

            try
            {
                sender.Send(serializer.Serialize(messagePayload)).Wait();
            }
            catch (AggregateException e)
            {
                Console.WriteLine("MyLab.Syslog error.");
                Console.WriteLine(e.ToString());
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

        private string CreateMessagePayload(string messagePayload, int senderLengthLimit)
        {
            if (senderLengthLimit <= 0)
            {
                return "[empty]";
            }

            var binPayload = Encoding.UTF8.GetBytes(messagePayload);

            if (binPayload.Length <= senderLengthLimit)
            {
                return Encoding.UTF8.GetString(binPayload, 0, binPayload.Length);
            }
            else
            {
                return Encoding.UTF8.GetString(binPayload, 0, senderLengthLimit-CroppedSuffixLen) + CroppedSuffix;
            }
        }

        string SerializeLogEntity(LogEntity logEntity, string format)
        {   
            switch (format.ToLower())
            {
                case "json":
                {
                    return logEntity.ToJson();
                }
                case "yaml":
                {
                    return logEntity.ToYaml();
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