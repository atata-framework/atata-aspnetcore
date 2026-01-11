namespace Atata.AspNetCore;

/// <summary>
/// Provides a builder for creating and configuring a <see cref="WebApplicationSession"/>.
/// </summary>
/// <typeparam name="TSession">The type of the session to build, which must inherit from <see cref="WebApplicationSession"/>.</typeparam>
/// <typeparam name="TBuilder">The type of the inherited builder.</typeparam>
public abstract class WebApplicationSessionBuilder<TSession, TBuilder> : AtataSessionBuilder<TSession, TBuilder>
    where TSession : WebApplicationSession, new()
    where TBuilder : WebApplicationSessionBuilder<TSession, TBuilder>
{
    private readonly List<Action<IWebHostBuilder>> _webHostConfigurationActions = [];

    private Action<WebApplicationSession>? _sessionStartAction;

#if NET10_0_OR_GREATER
    private bool _useKestrel;

    private int? _kestrelPort;

    private Action<KestrelServerOptions>? _configureKestrelOptions;
#endif

    /// <summary>
    /// Gets or sets a value indicating whether to dispose the <see cref="WebApplicationFactory{TEntryPoint}"/>
    /// when <see cref="AtataSession.DisposeAsync()"/> method is invoked.
    /// The default value is <see langword="true"/>.
    /// </summary>
    public bool DisposeWebApplicationFactory { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to collect application logs.
    /// The default value is <see langword="true"/>.
    /// </summary>
    public bool CollectApplicationLogs { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to transmit application logs to Atata.
    /// The default value is <see langword="true"/>.
    /// When <see langword="true"/>, adds an <see cref="AtataLoggerProvider"/> instance,
    /// with <see cref="SourceNameForAtataLog"/> as Atata log source
    /// and <see cref="MinimumLogLevelForAtataLog"/> as minimum log level,
    /// to the application builder's <see cref="ILoggingBuilder"/>.
    /// </summary>
    public bool TransmitApplicationLogsToAtata { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to clear predefined application logging providers.
    /// The default value is <see langword="true"/>.
    /// When <see langword="true"/>, calls <c>ClearProviders</c> method for application builder's <see cref="ILoggingBuilder"/>.
    /// </summary>
    public bool ClearPredefinedApplicationLoggingProviders { get; set; } = true;

    /// <summary>
    /// Gets or sets the source name for Atata log.
    /// The default value is <c>"App"</c>.
    /// </summary>
    public string SourceNameForAtataLog { get; set; } = "App";

    /// <summary>
    /// Gets or sets the default application log level.
    /// The default value is <see langword="null"/>, which doesn't change the predefined application configuration.
    /// When a value is set, adds <c>Logging:LogLevel:Default</c> to the application configuration.
    /// </summary>
    public MSLogLevel? DefaultApplicationLogLevel { get; set; }

    /// <summary>
    /// Gets or sets the minimum application log level.
    /// The default value is <see langword="null"/>, which doesn't change the predefined application configuration.
    /// When a value is set, calls <c>SetMinimumLevel</c> method for application builder's <see cref="ILoggingBuilder"/>.
    /// </summary>
    public MSLogLevel? MinimumApplicationLogLevel { get; set; }

    /// <summary>
    /// Gets or sets the minimum log level for Atata log.
    /// The default value is <see cref="MSLogLevel.Trace"/>.
    /// </summary>
    public MSLogLevel MinimumLogLevelForAtataLog { get; set; }

    /// <summary>
    /// Sets the <see cref="WebApplicationFactory{TEntryPoint}"/> to use for the session.
    /// </summary>
    /// <typeparam name="TEntryPoint">The entry point type of the application.</typeparam>
    /// <returns>The same <typeparamref name="TBuilder"/> instance.</returns>
    public TBuilder Use<TEntryPoint>()
        where TEntryPoint : class
        =>
        Use(() => new AtataWebApplicationFactory<TEntryPoint>());

    /// <summary>
    /// Sets the specified <see cref="WebApplicationFactory{TEntryPoint}"/> instance to use for the session.
    /// </summary>
    /// <typeparam name="TEntryPoint">The entry point type of the application.</typeparam>
    /// <param name="webApplicationFactory">The web application factory instance.</param>
    /// <returns>The same <typeparamref name="TBuilder"/> instance.</returns>
    public TBuilder Use<TEntryPoint>(WebApplicationFactory<TEntryPoint> webApplicationFactory)
        where TEntryPoint : class
    {
        Guard.ThrowIfNull(webApplicationFactory);

        return Use(() => webApplicationFactory);
    }

    /// <summary>
    /// Configures and sets a new <see cref="WebApplicationFactory{TEntryPoint}"/> instance to use for the session.
    /// </summary>
    /// <typeparam name="TEntryPoint">The entry point type of the application.</typeparam>
    /// <param name="configure">The action to configure the web application factory.</param>
    /// <returns>The same <typeparamref name="TBuilder"/> instance.</returns>
    public TBuilder Use<TEntryPoint>(Action<WebApplicationFactory<TEntryPoint>> configure)
        where TEntryPoint : class
        =>
        Use(() =>
        {
            AtataWebApplicationFactory<TEntryPoint> webApplicationFactory = new();
            configure?.Invoke(webApplicationFactory);
            return webApplicationFactory;
        });

    /// <summary>
    /// Sets the <see cref="WebApplicationFactory{TEntryPoint}"/> to use for the session using a factory method.
    /// </summary>
    /// <typeparam name="TEntryPoint">The entry point type of the application.</typeparam>
    /// <param name="webApplicationFactoryCreator">The function to create the web application factory.</param>
    /// <returns>The same <typeparamref name="TBuilder"/> instance.</returns>
    public TBuilder Use<TEntryPoint>(Func<WebApplicationFactory<TEntryPoint>> webApplicationFactoryCreator)
        where TEntryPoint : class
    {
        Guard.ThrowIfNull(webApplicationFactoryCreator);

        _sessionStartAction = session =>
        {
            void ConfigureWebApplicationFactory(IWebHostBuilder builder) =>
                ConfigureWebHost(builder, session);

            WebApplicationFactory<TEntryPoint> webApplicationFactory = webApplicationFactoryCreator.Invoke();

            WebApplicationFactoryUtils.AddConfigurationDelegate(webApplicationFactory, ConfigureWebApplicationFactory);

#if NET10_0_OR_GREATER
            bool useKestrel = _useKestrel;

            if (useKestrel)
            {
                webApplicationFactory.UseKestrel();

                if (_kestrelPort is not null)
                    webApplicationFactory.UseKestrel(_kestrelPort.Value);

                if (_configureKestrelOptions is not null)
                    webApplicationFactory.UseKestrel(_configureKestrelOptions);
            }

            webApplicationFactory.StartServer();
#else
            bool useKestrel = false;
#endif

            if (!useKestrel)
            {
                try
                {
                    session.TestServer = webApplicationFactory.Server;
                }
                catch
                {
                    // Unexpected error. Ignore. TestServer property will be null.
                }
            }

            session.Services = webApplicationFactory.Services;
            session.ClientOptions = webApplicationFactory.ClientOptions;
            session.CreateClientFunction = webApplicationFactory.CreateClient;
            session.CreateClientWithOptionsFunction = webApplicationFactory.CreateClient;
            session.CreateDefaultClientFunction = webApplicationFactory.CreateDefaultClient;
            session.CreateDefaultClientWithBaseAddressFunction = webApplicationFactory.CreateDefaultClient;

            session.Uri = webApplicationFactory.ClientOptions.BaseAddress;
            session.Url = webApplicationFactory.ClientOptions.BaseAddress.ToString();

            if (DisposeWebApplicationFactory)
                session.WebApplicationFactoryToDispose = webApplicationFactory;
        };

        return (TBuilder)this;
    }

    /// <summary>
    /// Adds a configuration action to be applied to the <see cref="IWebHostBuilder"/>.
    /// </summary>
    /// <param name="configure">The configuration action.</param>
    /// <returns>The same <typeparamref name="TBuilder"/> instance.</returns>
    public TBuilder UseConfiguration(Action<IWebHostBuilder> configure)
    {
        Guard.ThrowIfNull(configure);

        _webHostConfigurationActions.Add(configure);
        return (TBuilder)this;
    }

#if NET10_0_OR_GREATER
    /// <summary>
    /// Sets a value indicating whether to use Kestrel as the server.
    /// The default value is <see langword="false"/>.
    /// </summary>
    /// <param name="useKestrel">Whether to use Kestrel.</param>
    /// <returns>The same <typeparamref name="TBuilder"/> instance.</returns>
    public TBuilder UseKestrel(bool useKestrel = true)
    {
        _useKestrel = useKestrel;

        return (TBuilder)this;
    }

    /// <summary>
    /// Configures the builder to use Kestrel as the server on the specified <paramref name="port"/>.
    /// </summary>
    /// <param name="port">The port to listen to when the server starts. Use `0` to allow dynamic port selection.</param>
    /// <returns>The same <typeparamref name="TBuilder"/> instance.</returns>
    public TBuilder UseKestrel(int port)
    {
        _useKestrel = true;
        _kestrelPort = port;

        return (TBuilder)this;
    }

    /// <summary>
    /// Configures the factory to use Kestrel as the server.
    /// </summary>
    /// <param name="configureKestrelOptions">A callback handler that will be used for configuring the server when it starts.</param>
    /// <returns>The same <typeparamref name="TBuilder"/> instance.</returns>
    public TBuilder UseKestrel(Action<KestrelServerOptions> configureKestrelOptions)
    {
        _useKestrel = true;
        _configureKestrelOptions = configureKestrelOptions;

        return (TBuilder)this;
    }
#endif

    /// <summary>
    /// Sets a value indicating whether to dispose the <see cref="WebApplicationFactory{TEntryPoint}"/>
    /// when <see cref="AtataSession.DisposeAsync"/> method is invoked.
    /// The default value is <see langword="true"/>.
    /// </summary>
    /// <param name="disposeFactory">Whether to dispose factory.</param>
    /// <returns>The same <typeparamref name="TBuilder"/> instance.</returns>
    public TBuilder UseDisposeWebApplicationFactory(bool disposeFactory)
    {
        DisposeWebApplicationFactory = disposeFactory;
        return (TBuilder)this;
    }

    /// <summary>
    /// Sets a value indicating whether to collect application logs.
    /// The default value is <see langword="true"/>.
    /// </summary>
    /// <param name="collectLogs">Whether to collect logs.</param>
    /// <returns>The same <typeparamref name="TBuilder"/> instance.</returns>
    public TBuilder UseCollectApplicationLogs(bool collectLogs)
    {
        CollectApplicationLogs = collectLogs;
        return (TBuilder)this;
    }

    /// <summary>
    /// Sets a value indicating whether to transmit application logs to Atata.
    /// The default value is <see langword="true"/>.
    /// When <see langword="true"/>, adds an <see cref="AtataLoggerProvider"/> instance,
    /// with <see cref="SourceNameForAtataLog"/> as Atata log source
    /// and <see cref="MinimumLogLevelForAtataLog"/> as minimum log level,
    /// to the application builder's <see cref="ILoggingBuilder"/>.
    /// </summary>
    /// <param name="transmitLogs">Whether to transmit logs to Atata.</param>
    /// <returns>The same <typeparamref name="TBuilder"/> instance.</returns>
    public TBuilder UseTransmitApplicationLogsToAtata(bool transmitLogs)
    {
        TransmitApplicationLogsToAtata = transmitLogs;
        return (TBuilder)this;
    }

    /// <summary>
    /// Sets a value indicating whether to clear predefined application logging providers.
    /// The default value is <see langword="true"/>.
    /// When <see langword="true"/>, calls <c>ClearProviders</c> method for application builder's <see cref="ILoggingBuilder"/>.
    /// </summary>
    /// <param name="clearProviders">Whether to clear providers.</param>
    /// <returns>The same <typeparamref name="TBuilder"/> instance.</returns>
    public TBuilder UseClearPredefinedApplicationLoggingProviders(bool clearProviders)
    {
        ClearPredefinedApplicationLoggingProviders = clearProviders;
        return (TBuilder)this;
    }

    /// <summary>
    /// Sets the source name for Atata log.
    /// The default value is <c>"App"</c>.
    /// </summary>
    /// <param name="sourceName">The source name.</param>
    /// <returns>The same <typeparamref name="TBuilder"/> instance.</returns>
    public TBuilder UseSourceNameForAtataLog(string sourceName)
    {
        SourceNameForAtataLog = sourceName;
        return (TBuilder)this;
    }

    /// <summary>
    /// Sets the default application log level.
    /// The default value is <see langword="null"/>, which doesn't change the predefined application configuration.
    /// When a value is set, adds <c>Logging:LogLevel:Default</c> to the application configuration.
    /// </summary>
    /// <param name="logLevel">The log level.</param>
    /// <returns>The same <typeparamref name="TBuilder"/> instance.</returns>
    public TBuilder UseDefaultApplicationLogLevel(MSLogLevel? logLevel)
    {
        DefaultApplicationLogLevel = logLevel;
        return (TBuilder)this;
    }

    /// <summary>
    /// Sets the minimum application log level.
    /// The default value is <see langword="null"/>, which doesn't change the predefined application configuration.
    /// When a value is set, calls <c>SetMinimumLevel</c> method for application builder's <see cref="ILoggingBuilder"/>.
    /// </summary>
    /// <param name="logLevel">The log level.</param>
    /// <returns>The same <typeparamref name="TBuilder"/> instance.</returns>
    public TBuilder UseMinimumApplicationLogLevel(MSLogLevel? logLevel)
    {
        MinimumApplicationLogLevel = logLevel;
        return (TBuilder)this;
    }

    /// <summary>
    /// Sets the minimum log level for Atata log.
    /// </summary>
    /// <param name="logLevel">The log level.</param>
    /// <returns>The same <typeparamref name="TBuilder"/> instance.</returns>
    public TBuilder UseMinimumLogLevelForAtataLog(MSLogLevel logLevel)
    {
        MinimumLogLevelForAtataLog = logLevel;
        return (TBuilder)this;
    }

    /// <inheritdoc/>
    protected override void ValidateConfiguration()
    {
        base.ValidateConfiguration();

        if (_sessionStartAction is null)
            throw new AtataSessionBuilderValidationException("WebApplicationFactory is not set. Use 'Use' method to set it.");
    }

    /// <inheritdoc/>
    protected override void ConfigureSession(TSession session)
    {
        base.ConfigureSession(session);

        session.StartAction = _sessionStartAction!;
    }

    private void ConfigureWebHost(IWebHostBuilder builder, WebApplicationSession session)
    {
        if (DefaultApplicationLogLevel is not null)
        {
            string defaultLoggingLevelValue = DefaultApplicationLogLevel.Value.ToString();

            builder.ConfigureAppConfiguration(x => x
                .AddInMemoryCollection([new KeyValuePair<string, string?>("Logging:LogLevel:Default", defaultLoggingLevelValue)]));
        }

        builder.ConfigureLogging(loggingBuilder =>
        {
            if (ClearPredefinedApplicationLoggingProviders)
                loggingBuilder.ClearProviders();

            if (MinimumApplicationLogLevel is not null)
                loggingBuilder.SetMinimumLevel(MinimumApplicationLogLevel.Value);

            if (TransmitApplicationLogsToAtata)
                loggingBuilder.AddProvider(new AtataLoggerProvider(session, SourceNameForAtataLog, MinimumLogLevelForAtataLog));

            if (CollectApplicationLogs)
                loggingBuilder.AddFakeLogging();
        });

        session.ConfigureWebHost(builder);

        foreach (Action<IWebHostBuilder> configurationAction in _webHostConfigurationActions)
            configurationAction.Invoke(builder);
    }
}
