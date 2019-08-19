using System.IO;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureKeyVault;

namespace WebApi.CityOfMountJuliet
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
            //BuildWebHost(args).Run();
        }
        //public static IWebHost BuildWebHost(string[] args) =>
        //  WebHost.CreateDefaultBuilder(args)
        //      .ConfigureAppConfiguration((ctx, builder) =>
        //      {
        //          var keyVaultEndpoint = GetKeyVaultEndpoint();
        //          if (!string.IsNullOrEmpty(keyVaultEndpoint))
        //          {
        //              var azureServiceTokenProvider = new AzureServiceTokenProvider();
        //              var keyVaultClient = new KeyVaultClient(
        //                  new KeyVaultClient.AuthenticationCallback(
        //                      azureServiceTokenProvider.KeyVaultTokenCallback));
        //              builder.AddAzureKeyVault(
        //                  keyVaultEndpoint, keyVaultClient, new DefaultKeyVaultSecretManager());
        //          }
        //      }
        //   ).UseStartup<Startup>()
        //    .Build();

        //private static string GetKeyVaultEndpoint() => $"https://{config["azureKeyVault:vault"]}.vault.azure.net";

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(ConfigConfiguration)
                .UseStartup<Startup>();

        static void ConfigConfiguration(WebHostBuilderContext webHostBuilderContext, IConfigurationBuilder configurationBuilder)
        {
            configurationBuilder
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false, true)
                .AddEnvironmentVariables();
            var config = configurationBuilder.Build();
            var keyVaultEndpoint = $"https://{config["azureKeyVault:vault"]}.vault.azure.net/";
            var azureServiceTokenProvider = new AzureServiceTokenProvider();
            var keyVaultClient = new KeyVaultClient(
                new KeyVaultClient.AuthenticationCallback(
                    azureServiceTokenProvider.KeyVaultTokenCallback));
            configurationBuilder.AddAzureKeyVault(
                keyVaultEndpoint, keyVaultClient, new DefaultKeyVaultSecretManager());
        }       
    }
}
