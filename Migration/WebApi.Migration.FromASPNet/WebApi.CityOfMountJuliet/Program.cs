using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureKeyVault;
using Microsoft.Extensions.Logging;

namespace WebApi.CityOfMountJuliet
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //CreateWebHostBuilder(args).Build().Run();
            BuildWebHost(args).Run();
        }
        public static IWebHost BuildWebHost(string[] args) =>
          WebHost.CreateDefaultBuilder(args)
              .ConfigureAppConfiguration((ctx, builder) =>
              {
                  var keyVaultEndpoint = GetKeyVaultEndpoint();
                  if (!string.IsNullOrEmpty(keyVaultEndpoint))
                  {
                      var azureServiceTokenProvider = new AzureServiceTokenProvider();
                      var keyVaultClient = new KeyVaultClient(
                          new KeyVaultClient.AuthenticationCallback(
                              azureServiceTokenProvider.KeyVaultTokenCallback));
                      builder.AddAzureKeyVault(
                          keyVaultEndpoint, keyVaultClient, new DefaultKeyVaultSecretManager());
                  }
              }
           ).UseStartup<Startup>()
            .Build();

        private static string GetKeyVaultEndpoint() => "https://ps-test-sea-keyvault.vault.azure.net";

        //public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
        //    WebHost.CreateDefaultBuilder(args)
        //        .ConfigureAppConfiguration(ConfigConfiguration)
        //        .UseStartup<Startup>();
        //static void ConfigConfiguration(WebHostBuilderContext webHostBuilderContext, IConfigurationBuilder configurationBuilder)
        //{
        //    configurationBuilder.SetBasePath(Directory.GetCurrentDirectory())
        //        .AddJsonFile("azurekeyvault.json", false, true)
        //        .AddEnvironmentVariables();

        //    var config = configurationBuilder.Build();

        //    configurationBuilder.AddAzureKeyVault(
        //        $"https://{config["azureKeyVault:vault"]}.vault.azure.net/",
        //        config["azureKeyVault:clientId"],
        //        config["azureKeyVault:clientSecret"]
        //    );
        //}
    }
}
