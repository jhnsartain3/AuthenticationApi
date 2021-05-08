using AutoWrapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using PublicEndpoints.Filters;
using Sartain_Studios_Common.HttpRestApiCalls;
using Sartain_Studios_Common.Interfaces.Token;
using Sartain_Studios_Common.Logging;
using Sartain_Studios_Common.Token;
using Services;

namespace PublicEndpoints
{
    public class Startup
    {
        private const string ApplicationVersion = "0.0.1";
        private readonly string _applicationName;
        private readonly int _authenticationExpirationInMinutes;
        private readonly string _authenticationSecret;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            _authenticationSecret = Configuration["Authentication:Secret"];
            _authenticationExpirationInMinutes = int.Parse(Configuration["Authentication:ExpirationInMinutes"]);
            _applicationName = Configuration["ApplicationInformation:ApplicationName"];
        }

        private static string CorsPolicyName => "CorsOpenPolicy";

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            UseCustomModelStateFilter(services);
            UseExceptionFilter(services);
            AddCrossOriginResourceSharing(services);

            SetupServices(services);

            services.AddControllers();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc(ApplicationVersion,
                    new OpenApiInfo {Title = _applicationName, Version = ApplicationVersion});
            });
        }

        private static void UseCustomModelStateFilter(IServiceCollection services)
        {
            services.Configure<ApiBehaviorOptions>(options => { options.SuppressModelStateInvalidFilter = true; });

            services.AddMvc(options => { options.Filters.Add(typeof(ValidateModelStateAttribute)); });
        }

        private static void UseExceptionFilter(IServiceCollection services) =>
            services.AddMvc(options => { options.Filters.Add(typeof(UnauthorizedAccessExceptionHandlerAttribute)); });

        private static void AddCrossOriginResourceSharing(IServiceCollection services) =>
            services.AddCors(o => o.AddPolicy(CorsPolicyName,
                builder => { builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader(); }));

        private void SetupServices(IServiceCollection services)
        {
            services.AddSingleton(typeof(IAutoWrapperHttpClient<>), typeof(AutoWrapperHttpClient<>));

            services.AddSingleton<IUserService, UserService>();
            services.AddSingleton<IAuthenticationService, AuthenticationService>();

            var logPath = Configuration.GetSection("LogWriteLocation").Value;
            services.AddSingleton<ILoggerWrapper>(new LoggerWrapper(logPath));

            services.AddSingleton<IToken>(new JwtToken(_authenticationSecret, _authenticationExpirationInMinutes));
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseCors(CorsPolicyName);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                    c.SwaggerEndpoint(
                        $"/swagger/{ApplicationVersion}/swagger.json", $"{_applicationName} {ApplicationVersion}"));
            }

            app.UseHttpsRedirection();

            app.UseApiResponseAndExceptionWrapper();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}