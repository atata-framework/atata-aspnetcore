# Atata.AspNetCore

[![Atata Templates](https://img.shields.io/badge/get-Atata_Templates-green.svg?color=4BC21F)](https://marketplace.visualstudio.com/items?itemName=YevgeniyShunevych.AtataTemplates)\
[![Slack](https://img.shields.io/badge/join-Slack-green.svg?colorB=4EB898)](https://join.slack.com/t/atata-framework/shared_invite/zt-5j3lyln7-WD1ZtMDzXBhPm0yXLDBzbA)
[![Atata docs](https://img.shields.io/badge/docs-Atata_Framework-orange.svg)](https://atata.io)
[![X](https://img.shields.io/badge/follow-@AtataFramework-blue.svg)](https://x.com/AtataFramework)

**Atata.AspNetCore** is a C#/.NET library that adds ASP.NET Core sessions to [Atata](https://github.com/atata-framework/atata) using `WebApplicationFactory<TEntryPoint>`.

*The package targets .NET 8/9 depending on the version of the chosen library.
For example, for ASP.NET Core 8 project, use Atata.AspNetCore.v8.*

## Features

- **Sessions**. Integrates ASP.NET Core session management via `WebApplicationFactory<TEntryPoint>`.
- **Versions**. Provides versioned packages for ASP.NET Core 8, 9, and 10.
- **Kestrel**. Provides Kestrel hosting support API.
- **Logging**. Transmits application logs to Atata logs.

## Installation

Install the package via .NET CLI:

```bash
dotnet add package Atata.AspNetCore.v10
```

Or using Package Manager:

```powershell
Install-Package Atata.AspNetCore.v10
```

*Change v10 to v8 or v9 for older versions.*

## Dependencies

- [Atata](https://www.nuget.org/packages/Atata)
- [Microsoft.AspNetCore.Mvc.Testing](https://www.nuget.org/packages/Microsoft.AspNetCore.Mvc.Testing)
- [Microsoft.Extensions.Diagnostics.Testing](https://www.nuget.org/packages/Microsoft.Extensions.Diagnostics.Testing)

## Dependencies

- .NET 8/9/10 depending on the chosen package version
- ASP.NET Core shared framework matching the project target
- [Atata](https://github.com/atata-framework/atata)
- `Microsoft.AspNetCore.Mvc.Testing`

## Usage

Add web application sessions to `AtataContextBuilder` or `AtataContext` using the provided `AddWebApplication` extension methods.

### Add standard web application session to `AtataContextBuilder`

```cs
builder.Sessions.AddWebApplication(x => x
    .Use<Program>());
```

```cs
builder.Sessions.AddWebApplication(x => x
    .Use(() => new WebApplicationFactory<Program>()));
```

### Add standard web application session to `AtataContext`

```cs
var containerSession = await Context.Sessions.AddWebApplication()
    .Use<Program>()
    .BuildAsync();
```

```cs
var containerSession = await Context.Sessions.AddWebApplication()
    .Use(() => new WebApplicationFactory<Program>())
    .BuildAsync();
```

### Custom `WebApplicationFactory<TEntryPoint>`

```cs
public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.UseSetting("SomeKey", "SomeValue");

        builder.ConfigureTestServices(services =>
        {
            // Custom service configuration can be done here.
        });
    }
}
```

```cs
builder.Sessions.AddWebApplication(x => x
    .Use(new CustomWebApplicationFactory()));
```

### Custom `WebApplicationSession` class

```cs
public class CustomWebApplicationSession : WebApplicationSession
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.ConfigureTestServices(services =>
        {
            // Custom service configuration can be done here.
        });
    }
}
```

```cs
builder.Sessions.AddWebApplication<CustomWebApplicationSession>(x => x
    .Use<Program>());
```

### Get session from `AtataContext`

```cs
var session = Context.Sessions.Get<WebApplicationSession>();
```

## API

### `WebApplicationSessionAtataSessionsBuilderExtensions`

A set of extension methods for `AtataSessionsBuilder` to add and configure `WebApplicationSessionBuilder` and `WebApplicationSessionBuilder<TSession>` session builders.

```cs
public static class WebApplicationSessionAtataSessionsBuilderExtensions
{
    // Adds a new instance of WebApplicationSessionBuilder builder.
    public static AtataContextBuilder AddWebApplication(
        this AtataSessionsBuilder builder,
        Action<WebApplicationSessionBuilder>? configure = null);

    // Adds a new instance of WebApplicationSessionBuilder<TSession> builder.
    public static AtataContextBuilder AddWebApplication<TSession>(
        this AtataSessionsBuilder builder,
        Action<WebApplicationSessionBuilder<TSession>>? configure = null)
        where TSession : WebApplicationSession, new();

    // Configures existing nameless WebApplicationSessionBuilder session builder.
    public static AtataContextBuilder ConfigureWebApplication(
        this AtataSessionsBuilder builder,
        Action<WebApplicationSessionBuilder> configure,
        ConfigurationMode mode = default);

    // Configures existing nameless WebApplicationSessionBuilder<TSession> session builder.
    public static AtataContextBuilder ConfigureWebApplication<TSession>(
        this AtataSessionsBuilder builder,
        Action<WebApplicationSessionBuilder<TSession>> configure,
        ConfigurationMode mode = default)
        where TSession : WebApplicationSession, new();

    // Configures existing WebApplicationSessionBuilder session builder that has the specified name.
    public static AtataContextBuilder ConfigureWebApplication(
        this AtataSessionsBuilder builder,
        string? name,
        Action<WebApplicationSessionBuilder> configure,
        ConfigurationMode mode = default);

    // Configures existing WebApplicationSessionBuilder<TSession> session builder that has the specified name.
    public static AtataContextBuilder ConfigureWebApplication<TSession>(
        this AtataSessionsBuilder builder,
        string? name,
        Action<WebApplicationSessionBuilder<TSession>> configure,
        ConfigurationMode mode = default)
        where TSession : WebApplicationSession, new();
}
```

### `WebApplicationSessionAtataSessionCollectionExtensions`

A set of extension methods for `AtataSessionCollection` to add `WebApplicationSessionBuilder` and `WebApplicationSessionBuilder<TSession>` session builders.

```cs
public static class WebApplicationSessionAtataSessionCollectionExtensions
{
    // Creates a new WebApplicationSessionBuilder and adds it to the collection.
    public static WebApplicationSessionBuilder AddWebApplication(
        this AtataSessionCollection collection,
        Action<WebApplicationSessionBuilder>? configure = null);

    // Creates a new WebApplicationSessionBuilder<TSession> and adds it to the collection.
    public static WebApplicationSessionBuilder<TSession> AddWebApplication<TSession>(
        this AtataSessionCollection collection,
        Action<WebApplicationSessionBuilder<TSession>>? configure = null)
        where TSession : WebApplicationSession, new();
}
```

### `WebApplicationSessionBuilder<TSession, TBuilder>`

A base builder for creating and configuring a `WebApplicationSession`.

```cs
public abstract class WebApplicationSessionBuilder<TSession, TBuilder> : AtataSessionBuilder<TSession, TBuilder>
    where TSession : WebApplicationSession, new()
    where TBuilder : WebApplicationSessionBuilder<TSession, TBuilder>
{
    // Gets or sets a value indicating whether to dispose the WebApplicationFactory<TEntryPoint>
    // when AtataSession.DisposeAsync method is invoked.
    // The default value is true.
    public bool DisposeWebApplicationFactory { get; set; }

    // Gets or sets a value indicating whether to collect application logs.
    // The default value is true.
    public bool CollectApplicationLogs { get; set; }

    // Gets or sets a value indicating whether to transmit application logs to Atata.
    // The default value is true.
    // When true, adds an AtataLoggerProvider instance, with SourceNameForAtataLog as Atata log source
    // and MinimumLogLevelForAtataLog as minimum log level, to the application builder's ILoggingBuilder.
    public bool TransmitApplicationLogsToAtata { get; set; }

    // Gets or sets a value indicating whether to clear predefined application logging providers.
    // The default value is true.
    // When true, calls ClearProviders method for application builder's ILoggingBuilder.
    public bool ClearPredefinedApplicationLoggingProviders { get; set; }

    // Gets or sets the source name for Atata log.
    // The default value is "App".
    public string SourceNameForAtataLog { get; set; }

    // Gets or sets the default application log level.
    // The default value is null, which doesn't change the predefined application configuration.
    // When a value is set, adds "Logging:LogLevel:Default" to the application configuration.
    public MSLogLevel? DefaultApplicationLogLevel { get; set; }

    // Gets or sets the minimum application log level.
    // The default value is null, which doesn't change the predefined application configuration.
    // When a value is set, calls SetMinimumLevel method for application builder's ILoggingBuilder.
    public MSLogLevel? MinimumApplicationLogLevel { get; set; }

    // Gets or sets the minimum log level for Atata log.
    // The default value is MSLogLevel.Trace.
    public MSLogLevel MinimumLogLevelForAtataLog { get; set; }

    // Sets the WebApplicationFactory<TEntryPoint> to use for the session.
    public TBuilder Use<TEntryPoint>()
        where TEntryPoint : class;

    // Sets the specified WebApplicationFactory<TEntryPoint> instance to use for the session.
    public TBuilder Use<TEntryPoint>(WebApplicationFactory<TEntryPoint> webApplicationFactory)
        where TEntryPoint : class;

    // Configures and sets a new WebApplicationFactory<TEntryPoint> instance to use for the session.
    public TBuilder Use<TEntryPoint>(Action<WebApplicationFactory<TEntryPoint>> configure)
        where TEntryPoint : class;

    // Sets the WebApplicationFactory<TEntryPoint> to use for the session using a factory method.
    public TBuilder Use<TEntryPoint>(Func<WebApplicationFactory<TEntryPoint>> webApplicationFactoryCreator)
        where TEntryPoint : class;

    // Adds a configuration action to be applied to the IWebHostBuilder.
    public TBuilder UseConfiguration(Action<IWebHostBuilder> configure);

    // Sets a value indicating whether to use Kestrel as the server.
    // The default value is false.
    public TBuilder UseKestrel(bool useKestrel = true);

    // Configures the builder to use Kestrel as the server on the specified port.
    public TBuilder UseKestrel(int port);

    // Configures the builder to use Kestrel as the server.
    public TBuilder UseKestrel(Action<KestrelServerOptions> configureKestrelOptions);

    // Configures the builder to start only when the Kestrel server port is available.
    public TBuilder UseStartWhenKestrelPortIsAvailable();

    // Sets a value indicating whether to dispose the WebApplicationFactory<TEntryPoint>
    // when AtataSession.DisposeAsync method is invoked.
    // The default value is true.
    public TBuilder UseDisposeWebApplicationFactory(bool disposeFactory);

    // Sets a value indicating whether to collect application logs.
    // The default value is true.
    public TBuilder UseCollectApplicationLogs(bool collectLogs);

    // Sets a value indicating whether to transmit application logs to Atata.
    // The default value is true.
    // When true, adds an AtataLoggerProvider instance, with SourceNameForAtataLog as Atata log source
    // and MinimumLogLevelForAtataLog as minimum log level, to the application builder's ILoggingBuilder.
    public TBuilder UseTransmitApplicationLogsToAtata(bool transmitLogs);

    // Sets a value indicating whether to clear predefined application logging providers.
    // The default value is true.
    // When true, calls ClearProviders method for application builder's ILoggingBuilder.
    public TBuilder UseClearPredefinedApplicationLoggingProviders(bool clearProviders);

    // Sets the source name for Atata log.
    // The default value is "App".
    public TBuilder UseSourceNameForAtataLog(string sourceName);

    // Sets the default application log level.
    // The default value is null, which doesn't change the predefined application configuration.
    // When a value is set, adds "Logging:LogLevel:Default" to the application configuration.
    public TBuilder UseDefaultApplicationLogLevel(MSLogLevel? logLevel);

    // Sets the minimum application log level.
    // The default value is null, which doesn't change the predefined application configuration.
    // When a value is set, calls SetMinimumLevel method for application builder's ILoggingBuilder.
    public TBuilder UseMinimumApplicationLogLevel(MSLogLevel? logLevel);

    /// Sets the minimum log level for Atata log.
    public TBuilder UseMinimumLogLevelForAtataLog(MSLogLevel logLevel);
}
```

### `WebApplicationSessionBuilder<TSession>`

```cs
public class WebApplicationSessionBuilder<TSession> : WebApplicationSessionBuilder<TSession, WebApplicationSessionBuilder<TSession>>
    where TSession : WebApplicationSession, new()
{
}
```

### `WebApplicationSessionBuilder`

```cs
public class WebApplicationSessionBuilder : WebApplicationSessionBuilder<WebApplicationSession, WebApplicationSessionBuilder>
{
}
```

### `WebApplicationSession`

Represents a session for a web application.
Creates and launches an application using `WebApplicationFactory<TStartup>`.

```cs
public class WebApplicationSession : AtataSession
{
    public WebApplicationSession();

    // Gets the Uri of the started web application.
    public Uri Uri { get; }

    // Gets the URL string of the started web application.
    public string Url { get; }

    // Gets the Microsoft.AspNetCore.TestHost.TestServer instance associated with the session.
    // In case of Kestrel server, the property will be null.
    public TestServer? TestServer { get; }

    // Gets the IServiceProvider for resolving application services.
    public IServiceProvider Services { get; }

    // Gets the client options used for HTTP client creation.
    public WebApplicationFactoryClientOptions ClientOptions { get; }

    // Gets the Microsoft.Extensions.Logging.Testing.FakeLogCollector of application.
    public FakeLogCollector FakeLogCollector { get; }

    // Creates WebApplicationSessionBuilder instance for WebApplicationSession configuration.
    public static WebApplicationSessionBuilder CreateBuilder();

    public HttpClient CreateClient();

    public HttpClient CreateClient(WebApplicationFactoryClientOptions options);

    public HttpClient CreateDefaultClient(params DelegatingHandler[] handlers);

    public HttpClient CreateDefaultClient(Uri baseAddress, params DelegatingHandler[] handlers);

    // Configures the web host builder for the session.
    // Override this method to customize the IWebHostBuilder used to create the application.
    protected virtual void ConfigureWebHost(IWebHostBuilder builder);
}
```

## Community

- Slack: [https://atata-framework.slack.com](https://join.slack.com/t/atata-framework/shared_invite/zt-5j3lyln7-WD1ZtMDzXBhPm0yXLDBzbA)
- X: https://x.com/AtataFramework
- Stack Overflow: https://stackoverflow.com/questions/tagged/atata

## Feedback

Any feedback, issues and feature requests are welcome.

If you faced an issue please report it to [Atata.AspNetCore Issues](https://github.com/atata-framework/atata-aspnetcore/issues),
[ask a question on Stack Overflow](https://stackoverflow.com/questions/ask?tags=atata+csharp) using [atata](https://stackoverflow.com/questions/tagged/atata) tag
or use another [Atata Contact](https://atata.io/contact/) way.

## Contact author

Contact me if you need a help in test automation using Atata Framework, or if you are looking for a quality test automation implementation for your project.

- LinkedIn: https://www.linkedin.com/in/yevgeniy-shunevych
- Email: yevgeniy.shunevych@gmail.com
- Consulting: https://atata.io/consulting/

## Contributing

Check out [Contributing Guidelines](CONTRIBUTING.md) for details.

## SemVer

Atata Framework tries to follow [Semantic Versioning 2.0](https://semver.org/) when possible.
Sometimes Selenium.WebDriver dependency package can contain breaking changes in minor version releases,
so those changes can break Atata as well.
But Atata manages its sources according to SemVer.
Thus backward compatibility is mostly followed and updates within the same major version
(e.g. from 2.1 to 2.2) should not require code changes.

## License

Atata is an open source software, licensed under the Apache License 2.0.
See [LICENSE](LICENSE) for details.
