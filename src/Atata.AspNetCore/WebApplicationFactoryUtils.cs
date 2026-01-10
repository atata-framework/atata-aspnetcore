namespace Atata.AspNetCore;

internal static class WebApplicationFactoryUtils
{
    private const string ConfigurationFieldName = "_configuration";

#if NET9_0_OR_GREATER
    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = ConfigurationFieldName)]
    private static extern ref Action<IWebHostBuilder> GetConfigurationFieldValueReference<TEntryPoint>(WebApplicationFactory<TEntryPoint> factory)
        where TEntryPoint : class;
#endif

    internal static void AddConfigurationDelegate<TEntryPoint>(
        WebApplicationFactory<TEntryPoint> factory,
        Action<IWebHostBuilder> configurationDelegate)
        where TEntryPoint : class
    {
        if (factory is AtataWebApplicationFactory<TEntryPoint> atataFactory)
        {
            atataFactory.AddConfiguration(configurationDelegate);
        }
        else
        {
#if NET9_0_OR_GREATER
            ref Action<IWebHostBuilder> configuration = ref GetConfigurationFieldValueReference(factory);

            configuration += configurationDelegate;
#else
            Type factoryType = typeof(WebApplicationFactory<TEntryPoint>);

            FieldInfo webApplicationFactoryConfigurationField = factoryType.GetField(
                ConfigurationFieldName,
                BindingFlags.Instance | BindingFlags.NonPublic)
                ?? throw new MissingFieldException(factoryType.FullName, ConfigurationFieldName);

            Action<IWebHostBuilder> configuration = (Action<IWebHostBuilder>)webApplicationFactoryConfigurationField.GetValue(factory)!;

            configuration += configurationDelegate;

            webApplicationFactoryConfigurationField.SetValue(factory, configuration);
#endif
        }
    }
}
