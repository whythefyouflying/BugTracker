////////////////////////////////////////////////////////////////////////////////////////////////////
/// <file>  BugTracker_API\Startup.cs </file>
///
/// <copyright file="Startup.cs" company="Dawid Osuchowski">
/// Copyright (c) 2020 Dawid Osuchowski. All rights reserved.
/// </copyright>
///
/// <summary>   Implements the startup class. </summary>
////////////////////////////////////////////////////////////////////////////////////////////////////

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using BugTracker_API.Data;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using NSwag;
using NSwag.Generation.Processors.Security;
using Microsoft.AspNetCore.Http;
using BugTracker_API.Services;

namespace BugTracker_API
{
    ////////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>   A startup. </summary>
    ///
    /// <remarks>   Dawid, 18/06/2020. </remarks>
    ////////////////////////////////////////////////////////////////////////////////////////////////////

    public class Startup
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Constructor. </summary>
        ///
        /// <remarks>   Dawid, 18/06/2020. </remarks>
        ///
        /// <param name="configuration">    The configuration. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Gets the configuration. </summary>
        ///
        /// <value> The configuration. </value>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public IConfiguration Configuration { get; }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        ///
        /// <remarks>   Dawid, 18/06/2020. </remarks>
        ///
        /// <param name="services"> The services. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<DataContext>(opt =>
                opt.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
            services
                .AddControllers()
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.ReferenceLoopHandling =
                    Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                });

            services.AddAutoMapper(typeof(Startup));
            services.AddScoped<IAuthRepository, AuthRepository>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<ISharedService, SharedService>();
            services.Configure<SecretKey>(Configuration.GetSection("AppSettings:Secret"));
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII
                            .GetBytes(Configuration.GetSection("AppSettings:Secret:Key").Value)),
                        ValidateIssuer = false,
                        ValidateAudience = false
                    };
                });

            services.AddSwaggerDocument(config =>
            {
                config.PostProcess = document =>
                {
                    document.Info.Version = "v1";
                    document.Info.Title = "BugTracker API";
                    document.Info.Description = "Simple ASP.NET Core Web API for bug tracking.";
                    document.Info.Contact = new NSwag.OpenApiContact
                    {
                        Name = "Dawid Osuchowski",
                        Email = string.Empty,
                        Url = "https://github.com/tulphoon/bugtracker"
                    };
                    document.Info.License = new NSwag.OpenApiLicense
                    {
                        Name = "MIT License",
                        Url = "https://github.com/tulphoon/BugTracker/blob/master/LICENSE"
                    };
                };

                config.DocumentProcessors.Add(
                    new SecurityDefinitionAppender("JWT",
                    new OpenApiSecurityScheme
                    {
                        Type = OpenApiSecuritySchemeType.ApiKey,
                        Name = "Authorization",
                        In = OpenApiSecurityApiKeyLocation.Header,
                        Description = "Type into the textbox: Bearer {your JWT token}."
                    }));

                config.OperationProcessors.Add(new AspNetCoreOperationSecurityScopeProcessor("JWT"));
            });
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request
        /// pipeline.
        /// </summary>
        ///
        /// <remarks>   Dawid, 18/06/2020. </remarks>
        ///
        /// <param name="app">  The application. </param>
        /// <param name="env">  The environment. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseOpenApi();
            app.UseSwaggerUi3();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
