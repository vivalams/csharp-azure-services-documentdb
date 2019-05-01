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
    using Builders;
    using Clients;
    using Extensions.Configuration;
    using Extensions.DependencyInjection;
    using Factories;
    using Mappers;
    using Microsoft.ApplicationInsights;
    using Microsoft.Extensions.DependencyModel;
    using Microsoft.Extensions.Logging;
    using Microsoft.LearningService.Common.Security;
    using Microsoft.LearningService.DocumentDb;
    using Microsoft.LearningService.EventBus;
    using Microsoft.LearningService.EventBus.Interfaces;
    using Microsoft.LearningService.ServiceBus;
    using Microsoft.LearningService.ServiceBus.Configuration;
    using Microsoft.LearningServices.LearnerRecords.Api.Configuration;
    using Microsoft.LearningServices.LearnerRecords.Api.Events;
    using Microsoft.LearningServices.LearnerRecords.Api.Helpers;
    using Models;
    using Models.GoalsAndTasks;
    using Services;
    using Settings;
    using SharedServices.EventBus.Models.EventModels;
    using Swashbuckle.AspNetCore.Swagger;

    /// <summary>
    /// Startup class for Learner Record endpoints
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
                    Title = "Learner Record service API",
                    Description = "Learner Record service keeps tracks of all the events related to the user"
                });

                c.IncludeXmlComments(this.GetXmlCommentsPath());
            });

            // The scheme is defined by us and we control the token validation. 
            // In its current state the validation on service has been disabled.
            // The token validation is handled by the API gateway before it reaches the services.
            services.AddAuthentication(o =>
            {
                o.DefaultScheme = TokenSchemeName;
            }).AddScheme<TokenAuthenticationOptions, TokenAuthenticationHandler>(TokenSchemeName, o => { });

            // Registrations for DI
            var config = this.BuildConfiguration();

            services.AddTransient<IResourceSettings>((appServices) =>
            {
                return config.GetSection("Resources").Get<AppSettings>();
            });

            services.AddTransient<IServiceBusSettings>((serviceProvider) =>
            {
                return config.GetSection("EventBusConfig").Get<ServiceBusSettings>();
            });

            services.AddTransient<ProfileServiceClientSettings>((appServices) =>
            {
                return config.GetSection("ProfileServiceClient").Get<ProfileServiceClientSettings>();
            });

            // Add application insights
            services.AddApplicationInsightsTelemetry(config.GetSection("ApplicationInsights:InstrumentationKey").Get<string>());
            services.AddTransient<TelemetryClient>();

            // Register Services
            services.AddTransient<IGoalRecordService, GoalRecordService>();

            // Register Helpers
            services.AddTransient<IUserCompletableHelper, UserCompletableHelper>();
            services.AddSingleton<IProfileServiceClient, ProfileServiceClient>();

            // Register Builders
            services.AddTransient<IDynamicQueryBuilder<LearnerRecord, LearnerRecordQueryParameters>, DynamicQueryBuilder<LearnerRecord, LearnerRecordQueryParameters>>();
            services.AddTransient<IDynamicQueryBuilder<LearnerMappingRecord, LearnerRecordMappingQueryParameters>, DynamicQueryBuilder<LearnerMappingRecord, LearnerRecordMappingQueryParameters>>();

            // Register Factory
            services.AddSingleton<IDocumentDbClientFactory, DocumentClientFactory>();

            // Register Mappers
            services.AddTransient<ILearnerRecordMapper, LearnerRecordMapper>();
            services.AddTransient<IMapper<GoalPathDto, GoalPathViewModel>, GoalPathMapper>();
            services.AddTransient<IMapper<SubGoalDto, SubGoalViewModel>, SubGoalMapper>();
            services.AddTransient<IMapper<TaskNodeDto, TaskNodeViewModel>, TaskNodeMapper>();
            services.AddTransient<IMapper<TaskContentOptionDto, TaskContentOptionViewModel>, TaskContentOptionMapper>();

            // Register Clients
            services.AddSingleton<ILearnerRecordDocumentClient, LearnerRecordDocumentClient>((appServices) =>
            {
                var factory = (DocumentClientFactory)appServices.GetService(typeof(IDocumentDbClientFactory));

                // The collection type is used in the factory to determine the appSettings to documentClient mapping
                var learnerConfig = config.GetSection("CosmosDb-Learner").Get<CosmosDbSettings>();
                learnerConfig.CollectionType = typeof(LearnerRecord).ToString();

                return factory.BuildDocumentDbClient(learnerConfig) as LearnerRecordDocumentClient;
            });

            services.AddSingleton<ILearnerRecordMappingDocumentClient, LearnerRecordMappingDocumentClient>((appServices) =>
            {
                var factory = (DocumentClientFactory)appServices.GetService(typeof(IDocumentDbClientFactory));

                // The collection type is used in the factory to determine the appSettings to documentClient mapping
                var learnerConfig = config.GetSection("CosmosDb-Mapping").Get<CosmosDbSettings>();
                learnerConfig.CollectionType = typeof(LearnerMappingRecord).ToString();

                return factory.BuildDocumentDbClient(learnerConfig) as LearnerRecordMappingDocumentClient;
            });

            services.AddSingleton<IGoalAndTasksClient, GoalAndTasksClient>();
            services.AddSingleton<IEventTrackingClient, EventTrackingClient>();
            services.AddSingleton<AccountCloseEventTopicClient>();

            this.AddEventBus(services, config);
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
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Learner Record API V1");
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
            var eventBus = app.ApplicationServices.GetService<IEventBus>();

            if (eventBus != null)
            {
                // Subscribe to the events
                eventBus.Subscribe<AccountLinkLRStartedEvent, AccountLinkLRStartedEventHandler>();
                eventBus.Subscribe<AccountCloseRequestEvent, AccountCloseRequestEventHandler>();
            }
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
            return compilationLibrary.Name.Contains("LearningService", System.StringComparison.OrdinalIgnoreCase) ||
                    compilationLibrary.Dependencies.Any(d => d.Name.Contains("LearningService", System.StringComparison.OrdinalIgnoreCase));
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
            // TODO: When disabled use storage emulator queue
            if (config.GetValue<bool>("AzureServiceBusEnabled"))
            {
                services.AddSingleton<IEventBus, EventBusServiceBus>(sp =>
                {
                    var settings = sp.GetRequiredService<IServiceBusSettings>();
                    var iLifetimeScope = sp.GetRequiredService<ILifetimeScope>();
                    var logger = sp.GetRequiredService<ILogger<Startup>>();
                    var eventBusSubcriptionsManager = sp.GetRequiredService<IEventBusSubscriptionsManager>();

                    return new EventBusServiceBus(
                        settings,
                        eventBusSubcriptionsManager,
                        logger,
                        iLifetimeScope);
                });
            }

            services.AddSingleton<IEventBusSubscriptionsManager, EventBusSubscriptionsManager>();
            services.AddTransient<AccountLinkLRStartedEventHandler>();
            services.AddTransient<AccountCloseRequestEventHandler>();
        }
    }
}
