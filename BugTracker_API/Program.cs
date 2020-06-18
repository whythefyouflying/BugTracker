////////////////////////////////////////////////////////////////////////////////////////////////////
/// <file>  BugTracker_API\Program.cs </file>
///
/// <copyright file="Program.cs" company="Dawid Osuchowski">
/// Copyright (c) 2020 Dawid Osuchowski. All rights reserved.
/// </copyright>
///
/// <summary>   Implements the program class. </summary>
////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureKeyVault;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BugTracker_API
{
    ////////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>   A program. </summary>
    ///
    /// <remarks>   Dawid, 18/06/2020. </remarks>
    ////////////////////////////////////////////////////////////////////////////////////////////////////

    public class Program
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Main entry-point for this application. </summary>
        ///
        /// <remarks>   Dawid, 18/06/2020. </remarks>
        ///
        /// <param name="args"> An array of command-line argument strings. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Creates host builder. </summary>
        ///
        /// <remarks>   Dawid, 18/06/2020. </remarks>
        ///
        /// <param name="args"> An array of command-line argument strings. </param>
        ///
        /// <returns>   The new host builder. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
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
                })

                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Gets key vault endpoint. </summary>
        ///
        /// <remarks>   Dawid, 18/06/2020. </remarks>
        ///
        /// <returns>   The key vault endpoint. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        private static string GetKeyVaultEndpoint() => Environment.GetEnvironmentVariable("ASPNETCORE_HOSTINGSTARTUP__KEYVAULT__CONFIGURATIONVAULT");
    }
}
