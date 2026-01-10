namespace Atata.AspNetCore;

internal sealed class AtataWebApplicationFactory<TEntryPoint> : WebApplicationFactory<TEntryPoint>
    where TEntryPoint : class
{
    private Action<IWebHostBuilder>? _configure;

    internal void AddConfiguration(Action<IWebHostBuilder> configure)
    {
        if (_configure is null)
            _configure = configure;
        else
            _configure += configure;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder) =>
        _configure?.Invoke(builder);
}
