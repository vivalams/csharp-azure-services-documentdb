//  -----------------------------------------------------------------------
//  
//  <copyright file="Startup.cs" company="Microsoft">
//  
//  Copyright (c) Microsoft. All rights reserved.
// 
//  </copyright>
// 
//  -----------------------------------------------------------------------
namespace {{ cookiecutter.assembly_name }}.Api
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using AspNetCore.Builder;
    using AspNetCore.Hosting;
    using AspNetCore.Mvc;
    using Autofac;
    using Extensions.Configuration;
    using Extensions.DependencyInjection;
    using Microsoft.ApplicationInsights;
    using Microsoft.Extensions.DependencyModel;
    using Microsoft.Extensions.Logging;
    using Swashbuckle.AspNetCore.Swagger;
	using {{ cookiecutter.assembly_name }}.Api.Settings;

    /// <summary>
    /// Startup class for {{ cookiecutter.assembly_name }} endpoints
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class Startup
    {
        /// <summary>
        /// Custom token authentication scheme name
        /// </summary>
        private readonly string TokenSchemeName = "TokenAuthenticationScheme";

        /// <summary>
        /// Gets the configuration object
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="env">Hosting environment</param>
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                    .SetBasePath(env.ContentRootPath)
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                    .AddEnvironmentVariables();

            this.Configuration = builder.Build();
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services">Collection of Services to add to container</param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(
                options =>
                {
                    options.AddPolicy(
                        "AllowAllHeaders",
                        builder =>
                        {
                            builder.AllowAnyOrigin()
                                   .AllowAnyHeader()
                                   .AllowAnyMethod();
                        });
                });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info
                {
                    Version = "v1",
                    Title = "{{ cookiecutter.service_name }} API",
                    Description = "{{ cookiecutter.assembly_name }} keeps tracks of all the events related to the {{ cookiecutter.service_name }}"
                });

                c.IncludeXmlComments(this.GetXmlCommentsPath());
            });
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app">Application Builder Service</param>
        /// <param name="env">Hosting Environment Service</param>
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseCors("AllowAllHeaders");
            app.UseAuthentication();
            app.UseHttpsRedirection();

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), 
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "{{ cookiecutter.service_name }} API V1");
            });

            app.UseMvc();
            this.ConfigureEventBus(app);
        }

        /// <summary>
        /// ConfigureContainer is where you can register things directly
        /// with Autofac. This runs after ConfigureServices so the things
        /// here will override registrations made in ConfigureServices.
        /// Don't build the container; that gets done for you. If you
        /// need a reference to the container, you need to use the
        /// "Without ConfigureContainer" mechanism shown later.
        /// </summary>
        /// <param name="builder">Service Builder Container</param>
        public void ConfigureContainer(ContainerBuilder builder)
        {
            var assemblies = this.GetAssemblies();
            builder.RegisterAssemblyModules(assemblies);
        }

        /// <summary>
        /// Configure eventbus 
        /// </summary>
        /// <param name="app">Application builder instance</param>
        protected virtual void ConfigureEventBus(IApplicationBuilder app)
        {
          
        }

        /// <summary>
        /// Gets List of Assemblies being used
        /// </summary>
        /// <returns> List of Assemblies being used in the API</returns>
        private Assembly[] GetAssemblies()
        {
            var assemblies = new List<Assembly>();
            var dependencies = DependencyContext.Default.RuntimeLibraries;
            foreach (var library in dependencies)
            {
                if (this.IsCandidateCompilationLibrary(library) && library.Type != "package")
                {
                    var assembly = Assembly.Load(new AssemblyName(library.Name));
                    assemblies.Add(assembly);
                }
            }

            return assemblies.ToArray();
        }

        /// <summary>
        /// Determines if this project contains all the dependent assemblies
        /// </summary>
        /// <param name="compilationLibrary">RuntimeLibrary to check against our current Service</param>
        /// <returns>Boolean of whether the assembly is used in this service</returns>
        private bool IsCandidateCompilationLibrary(RuntimeLibrary compilationLibrary)
        {
             return compilationLibrary.Name.Contains("{{ cookiecutter.assembly_name }}", System.StringComparison.OrdinalIgnoreCase) ||
                    compilationLibrary.Dependencies.Any(d => d.Name.Contains("{{ cookiecutter.assembly_name }}", System.StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Builds the configuration.
        /// </summary>
        /// <returns>An IConfigurationRoot.</returns>
        private IConfiguration BuildConfiguration()
        {
            IConfigurationRoot config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", true)
                .AddEnvironmentVariables()
                .Build();

            var keyVaultConfig = config.GetSection("KeyVaultConfig").Get<KeyVaultConfig>();

            if (keyVaultConfig != null && keyVaultConfig.EnableSecureConfig)
            {
                config = new ConfigurationBuilder()
                    .AddEnvironmentVariables()
                    .AddAzureKeyVault(keyVaultConfig.VaultBaseUrl)
                    .Build();
            }

            return config;
        }

        /// <summary>
        /// Fetches the Xml comments file path for swagger
        /// </summary>
        /// <returns>Xml comments file path as string</returns>
        private string GetXmlCommentsPath()
        {
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            return xmlPath;
        }

        /// <summary>
        /// Add servicebus based on the appsettings
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <param name="config">Configuration</param>
        private void AddEventBus(IServiceCollection services, IConfiguration config)
        {

        }
	
     
    }
}
