//  -----------------------------------------------------------------------
//  
//  <copyright file="Program.cs" company="Microsoft">
//  
//  Copyright (c) Microsoft. All rights reserved.
// 
//  </copyright>
// 
//  -----------------------------------------------------------------------
namespace {{ cookiecutter.assembly_name }}.Api
{
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using AspNetCore.Hosting;
    using Autofac.Extensions.DependencyInjection;

    /// <summary>
    /// The program class
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class Program
    {
        /// <summary>
        /// Build web bootstrap
        /// </summary>
        /// <param name="args">String array of arguments</param>
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        /// <summary>
        /// Build web host
        /// </summary>
        /// <param name="args">String array of Arguments</param>
        /// <returns>An <see cref="IWebHost"/></returns>
        public static IWebHost BuildWebHost(string[] args)
        {
            return new WebHostBuilder()
                    .UseKestrel()
                    .ConfigureServices(services => services.AddAutofac())
                    .UseContentRoot(Directory.GetCurrentDirectory())
                    .UseIISIntegration()
                    .UseStartup<Startup>()
                    .Build();
        }
    }
}
