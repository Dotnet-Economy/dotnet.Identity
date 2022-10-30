using System;
using dotnet.Common.HealthChecks;
using dotnet.Common.MassTransit;
using dotnet.Common.Settings;
using dotnet.Identity.Service.Entities;
using dotnet.Identity.Service.Exceptions;
using dotnet.Identity.Service.HostedServices;
using dotnet.Identity.Service.Settings;
using GreenPipes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace dotnet.Identity.Service
{
    public class Startup
    {
        private const string AllowedOriginSettings = "AllowedOrigin";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            BsonSerializer.RegisterSerializer(new GuidSerializer(BsonType.String));
            var serviceSettings = Configuration.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>();
            var mongoDbSettings = Configuration.GetSection(nameof(MongoDbSettings)).Get<MongoDbSettings>();
            var identityServerSettings = Configuration.GetSection(nameof(IdentityServerSettings)).Get<IdentityServerSettings>();


            services.Configure<IdentitySettings>(Configuration.GetSection(nameof(IdentitySettings)))
                    .AddDefaultIdentity<ApplicationUser>()
                    .AddRoles<ApplicationRole>()
                    .AddMongoDbStores<ApplicationUser, ApplicationRole, Guid>(
                        mongoDbSettings.ConnectionString,
                        serviceSettings.ServiceName
                    );

            services.AddMassTransitWithMessageBroker(Configuration, retryConfigurator =>
            {
                retryConfigurator.Interval(3, TimeSpan.FromSeconds(5));
                retryConfigurator.Ignore(typeof(UnknownUserException));
                retryConfigurator.Ignore(typeof(InsufficientFundsException));
            });

            services.AddIdentityServer(options =>
            {
                options.Events.RaiseSuccessEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseErrorEvents = true;
            })
                    .AddAspNetIdentity<ApplicationUser>()
                    .AddInMemoryApiScopes(identityServerSettings.ApiScopes)
                    .AddInMemoryApiResources(identityServerSettings.ApiResources)
                    .AddInMemoryClients(identityServerSettings.Clients)
                    .AddInMemoryIdentityResources(identityServerSettings.IdentityResources)
                    .AddDeveloperSigningCredential();

            //Adds auth in its own REST APIs
            services.AddLocalApiAuthentication();

            services.AddControllers();
            //
            services.AddHostedService<IdentitySeedHostedService>();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "dotnet.Identity.Service", Version = "v1" });
            });

            services.AddHealthChecks()
                    .AddMongoDb();

            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor | Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto;
                options.KnownNetworks.Clear();
                options.KnownProxies.Clear();
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseForwardedHeaders();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "dotnet.Identity.Service v1"));

                app.UseCors(builder =>
                {
                    builder.WithOrigins(Configuration[AllowedOriginSettings])
                            .AllowAnyHeader()
                            .AllowAnyMethod();
                });
            }

            app.UseHttpsRedirection();

            app.Use((context, next) =>
            {
                var identitySettings = Configuration.GetSection(nameof(IdentitySettings)).Get<IdentitySettings>();
                context.Request.PathBase = new Microsoft.AspNetCore.Http.PathString(identitySettings.PathBase);
                return next();
            });

            app.UseStaticFiles();

            app.UseRouting();
            //Add between useRouting() and useAuthorization()
            app.UseIdentityServer();

            app.UseAuthorization();
            //Requirement to get access tokens over http when running microservice in Docker container
            app.UseCookiePolicy(new CookiePolicyOptions { MinimumSameSitePolicy = Microsoft.AspNetCore.Http.SameSiteMode.Lax });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapRazorPages();
                endpoints.MapDotnetEconomyHealthChecks();
            });
        }

    }
}
