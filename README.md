# MyLab.Syslog

For .NET Core 2.1+

Sends logging messages to a `syslog` server.

## How to use

There is easy to integrate the `MyLab.Syslog` logger into `.NET Core` logging system: just call `AddSyslog` extension method for logging configuration and then configure a logger.

The following example shows how to integrate `syslog` logger:

```C#
public class Startup
{
    // ....

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddMvc();
        services.AddLogging(c => c.AddSyslog());
        
        //Configure here
    }

    // ....
}
```

`RemoteHost` and `RemotePort` are required options for the logger. It possible to configure at the place:

```C#
....
services.Configure<SyslogLoggerOptions>(opt =>
            {
                opt.RemoteHost = "syslog-collector.corp";
                opt.RemotePort = 514;
            });
....
```

Also there is able to configure `syslog` logger through config. The following example represent a config file and code which configure the logger.

```json
{
  "Logging": {
    "Syslog": {
      "RemoteHost": "localhost",
      "RemotePort": "5514"
    }
  }
}
```

```C#
....
services.Configure<SyslogLoggerOptions>(Configuration.GetSection("Logging:Syslog"));
....
```

