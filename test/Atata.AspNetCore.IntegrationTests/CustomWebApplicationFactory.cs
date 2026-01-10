namespace Atata.AspNetCore.IntegrationTests;

public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    public static KeyValuePair<string, string> CustomSetting =>
        new("CustomWebApplicationFactoryKey", "CustomWebApplicationFactoryValue");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.UseSetting(CustomSetting.Key, CustomSetting.Value);

        builder.ConfigureTestServices(services =>
        {
            // Custom service configuration can be done here.
        });
    }
}
