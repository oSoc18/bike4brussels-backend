using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace rideaway_backend {
    public class Program {
        /// <summary>
        /// Main method of the application. Builds the webhost and runs it.
        /// </summary>
        /// <param name="args">arguments.</param>
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                //.UseUrls("http://localhost:5001/")
                .Build();
    }
}