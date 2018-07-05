using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using rideaway_backend.Instance;

namespace rideaway_backend {
    public class Startup {
        /// <summary>
        /// Startup method for the web application.
        /// </summary>
        /// <param name="env">Environment of the appplication.</param>
        public Startup (IHostingEnvironment env) {
            var builder = new ConfigurationBuilder ()
                .SetBasePath (env.ContentRootPath)
                .AddJsonFile ("appsettings.json", optional : false, reloadOnChange : true)
                .AddJsonFile ($"appsettings.{env.EnvironmentName}.json", optional : true)
                .AddEnvironmentVariables ();
            Configuration = builder.Build ();
        }

        public IConfigurationRoot Configuration { get; }

        /// <summary>
        /// Configure the services for the web api. Adds a CORS policy and initializes 
        /// the router and languages.
        /// </summary>
        /// <param name="services">The services of the application.</param>
        public void ConfigureServices (IServiceCollection services) {
            // Add framework services.
            services.AddMvc ();
            services.AddCors (options => {
                options.AddPolicy ("AllowAnyOrigin",
                    builder => builder.AllowAnyOrigin ().AllowAnyHeader ().WithMethods ("GET"));
            });
            services.AddDirectoryBrowser ();
            services.AddSingleton<IConfiguration> (Configuration);

            RouterInstance.initialize (Configuration);
            Languages.initialize (Configuration);
            ParkingInstance.initialize (Configuration);
            RequestLogger.initialize(Configuration);
        }

        /// <summary>
        /// Configure the application.
        /// </summary>
        /// <param name="app">The builder of the app.</param>
        /// <param name="env">The environment of the app.</param>
        /// <param name="loggerFactory">The factory for logging.</param>
        public void Configure (IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory) {
            loggerFactory.AddConsole (Configuration.GetSection ("Logging"));
            loggerFactory.AddDebug ();
            
            var paths = Configuration.GetSection ("Paths");

            app.UseMvc ();
            app.UseCors ("AllowAnyOrigin");
            app.UseDefaultFiles ();

            app.UseStaticFiles (new StaticFileOptions () {
                FileProvider = new PhysicalFileProvider (
                        Path.Combine (Directory.GetCurrentDirectory (), @"wwwroot",
                            paths.GetValue<string> ("LoggingEndpoint"))),
                    RequestPath = new PathString ("/" + paths.GetValue<string> ("LoggingEndpoint"))
            });
            app.UseStaticFiles (new StaticFileOptions () {
                FileProvider = new PhysicalFileProvider (
                        Path.Combine (Directory.GetCurrentDirectory (), @"wwwroot",
                            paths.GetValue<string> ("CyclenetworkEndpoint"))),
                    RequestPath = new PathString ("/" + paths.GetValue<string> ("CyclenetworkEndpoint"))
            });

            app.UseDirectoryBrowser (new DirectoryBrowserOptions () {
                FileProvider = new PhysicalFileProvider (
                        Path.Combine (Directory.GetCurrentDirectory (), @"wwwroot",
                            paths.GetValue<string> ("LoggingEndpoint"))),
                    RequestPath = new PathString ("/" + paths.GetValue<string> ("LoggingEndpoint"))
            });
        }
    }
}