IdentityServer uses logging everywhere (well - that's the plan at least, we have some gaps right now).

The logging mechanism and output is determined by the hosting application via setting a `LogProvider` (e.g. in your `startup` class):

```csharp
LogProvider.SetCurrentLogProvider(new DiagnosticsTraceLogProvider());
```

We provide log providers for System.Diagnostics, NLog, Enterprise Library, SeriLog, Loupe and Log4Net (check the `Thinktecture.IdentityServer.Logging` namespace. You can also write your own log providers by implementing `ILogProvider`. If you do that, open source it and let us know so we can link to your provider.

#### Configuring the System.Diagnostics Provider
Add the following snippet to your configuration file to funnel all logging messages to a simple text file. We use [Baretail](https://www.baremetalsoft.com/baretail/) for viewing the log files.

```xml
<system.diagnostics>
  <trace autoflush="true"
         indentsize="4">
    <listeners>
      <add name="myListener"
           type="System.Diagnostics.TextWriterTraceListener"
           initializeData="Trace.log" />
      <remove name="Default" />
    </listeners>
  </trace>
</system.diagnostics>
```

#### Instrumenting your own code
You can also use the logging system in your own extensibility code.

Add a static `ILog` instance to your class
```csharp
private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();
```
Log your messages using the logger
```csharp
Logger.Debug("Getting claims for identity token");
```