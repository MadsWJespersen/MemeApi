using MemeApi.library.repositories;
using MemeApi.Models.Context;
using MemeApi.Models.Entity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.HttpOverrides;
using System.Net;
using System;
using System.IO;
using System.Reflection;
using MemeApi.library.Services;
using MemeApi.library.Services.Files;
using MemeApi.library;

namespace MemeApi
{
    public class Startup
    {
        public Startup()
        {
            Configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables().Build();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddControllers();
            services.AddDbContext<MemeContext>(options => {
                var connstr = Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING");
                if (connstr != null)
                    options.UseNpgsql(connstr);
                else
                    options.UseInMemoryDatabase("Test");
            });
            services.AddControllers().AddNewtonsoftJson(options =>
                options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
            );

            services.AddDefaultIdentity<User>(options => options.SignIn.RequireConfirmedAccount = false)
                .AddEntityFrameworkStores<MemeContext>();

            services.Configure<IdentityOptions>(options =>
            {
                options.Lockout.MaxFailedAccessAttempts = 1000;
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 1;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.User.AllowedUserNameCharacters =
                    "abcdefghijklmnopqrstuvwxyz���ABCDEFGHIJKLMNOPQRSTUVWXYZ���0123456789-._@+ ";
            });

            services.AddAutoMapper(typeof(Startup));

            services.AddCors();
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Meme API",
                    Description = "An ASP.NET Core Web API for creating memes",
                    Contact = new OpenApiContact
                    {
                        Name = "Contact information",
                        Email = "Contact@mads.monster"
                      
                    },
                });
                var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
            });

            services.AddScoped<IFileSaver, S3FileStorageClient>();
            services.AddScoped<IFileRemover, S3FileStorageClient>();
            services.AddScoped<IFileLoader, WebFileLoader>();
            services.AddScoped<IMailSender, MailSender>();
            services.AddScoped<IMemeRenderingService, MemeRenderingService>();
            services.AddScoped<MailSender>();
            services.AddScoped<MemeRenderingService>();

            services.AddScoped<UserRepository>();
            services.AddScoped<MemeRepository>();
            services.AddScoped<VisualRepository>();
            services.AddScoped<TextRepository>();
            services.AddScoped<VotableRepository>();
            services.AddScoped<TopicRepository>();

            services.AddSingleton<MemeApiSettings>();

            services.AddScoped<IMemeOfTheDayService, MemeOfTheDayService>();
            services.AddHostedService<ConsumeScopedServiceHostedService>();

            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.KnownProxies.Add(IPAddress.Parse("10.0.0.100"));
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            
            var factory = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>();
            using var serviceScope = factory.CreateScope();
            var context = serviceScope.ServiceProvider.GetRequiredService<MemeContext>();

            if (context.Database.EnsureCreated()) // false when db already exists
            {
                context.SaveChanges();
            }

            app.UseMiddleware<SwaggerAuthenticationMiddleware>();
            app.UseSwaggerUI();
            app.UseSwagger();
            app.UseRouting();


            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            app.UseAuthentication();
            app.UseCors(builder =>
            {
                builder
                    .SetIsOriginAllowed(_ => true)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            });
            app.UseAuthorization();

            //app.UseHttpsRedirection();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
