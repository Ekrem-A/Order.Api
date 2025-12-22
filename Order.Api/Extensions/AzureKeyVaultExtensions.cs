using Azure.Identity;

namespace Order.Api.Extensions;

public static class AzureKeyVaultExtensions
{
    public static IHostApplicationBuilder AddAzureKeyVault(this IHostApplicationBuilder builder)
    {
        var keyVaultUri = builder.Configuration["KeyVault:Uri"];

        if (!string.IsNullOrWhiteSpace(keyVaultUri))
        {
            builder.Configuration.AddAzureKeyVault(
                new Uri(keyVaultUri),
                new DefaultAzureCredential());
        }

        return builder;
    }
}

