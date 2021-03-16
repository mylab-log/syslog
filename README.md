# MyLab.Syslog

For .NET Core 3.1+

[![NuGet](https://img.shields.io/nuget/v/MyLab.Log.Syslog.svg)](https://www.nuget.org/packages/MyLab.Log.Syslog/)

Sends logging messages to a `syslog` server.

Check out the latest changes in the [changelog](/changelog.md).

## Integration

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

## Configuration

### How to configure

`RemoteHost` and `RemotePort` are required options for the logger. It possible to configure at the place:

```c#
services.AddLogging(c => c.AddSyslog(opt =>
            {
                opt.RemoteHost = "syslog-collector.corp";
                opt.RemotePort = 514;
            }));
```

Or with services collection:

```C#
services.ConfigureSyslog(opt =>
            {
                opt.RemoteHost = "syslog-collector.corp";
                opt.RemotePort = 514;
            });
```

Also there is able to configure `syslog` logger with configuration. The following example presents a config file and code which configure the logger.

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

Use configuration to configure syslog from section with default name `Logging:Syslog`  

```C#
services.ConfigureSyslog(configuration);
```

Or specify custom one:

```c#
services.ConfigureSyslog(configuration, "CustomSectionName"); 
```

### Configuration parameters

```c#
/// <summary>
/// Contains syslog writing options
/// </summary>
public class SyslogLoggerOptions
{
    /// <summary>
    /// Overrides default hostname
    /// </summary>
    public string Hostname { get; set; }

    /// <summary>
    /// Overrides current application name
    /// </summary>
    public string AppName { get; set; }

    /// <summary>
    /// Overrides default process id. Should be generated value.
    /// </summary>
    public string ProcId { get; set; }

    /// <summary>
    /// Gets syslog host
    /// </summary>
    public string RemoteHost { get; set; }

    /// <summary>
    /// Gets syslog port
    /// </summary>
    public int RemotePort { get; set; }

    /// <summary>
    /// Use TCP connection instead a UDP
    /// </summary>
    public bool UseTcp { get; set; } = false;

    /// <summary>
    /// Gets facility. Local7 - by default.
    /// </summary>
    /// <remarks>https://tools.ietf.org/html/rfc5424#section-6.2.1</remarks>
    public int Facility { get; set; } = 23;

    /// <summary>
    /// Include event id in message
    /// </summary>
    public bool IncludeEventId { get; set; } = false;

    /// <summary>
    /// Gets log message format
    /// </summary>
    /// <remarks>json - by default. Available values: json, yaml</remarks>
    public string Format { get; set; } = "json";
}
```

