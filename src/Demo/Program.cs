using System;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyLab.Log.Syslog;

namespace Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging(c =>c.AddSyslog().AddConsole());
            
            var config = new ConfigurationBuilder()
                .AddJsonFile("config.json")                
                .Build();

            serviceCollection.AddSingleton(config);
            serviceCollection.Configure<SyslogLoggerOptions>(config.GetSection("Logging:Syslog"));
            
            var serviceProvider = serviceCollection.BuildServiceProvider();

            var lf = serviceProvider.GetService<ILoggerFactory>();
            var logger = lf.CreateLogger("mycat");
            var log = logger.Dsl();

            Exception ex;
            try
            {
                throw new InvalidOperationException("Точно что-то случилось!");
            }
            catch (Exception e)
            {
                ex = e;
            }

            var sb = new StringBuilder();

            for (int i = 0; i < 10000; i++)
            {
                sb.Append("1234567890-" + i + ",");
            }

            log.Act("Ololo").Write();
            log.Error(new EventId(345, "Видимо что-то случилось"), ex).Write();

            Console.ReadLine();
        }
    }
}
