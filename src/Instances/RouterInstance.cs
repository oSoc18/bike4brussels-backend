using System.IO;
using System.Collections.Generic;
using Itinero;
using Itinero.LocalGeo;
using Itinero.Profiles;
using Itinero.Navigation.Instructions;
using rideaway_backend.Extensions;
using rideaway_backend.Exceptions;
using rideaway_backend.FileMonitoring;
using Microsoft.Extensions.Configuration;
using rideaway_backend.Model;

namespace rideaway_backend.Instance
{
    /// <summary>
    /// Static instance of the router.
    /// </summary>
    public static class RouterInstance
    {
        private static Router router;
        private static RouterDb routerDb;
        private static FilesMonitor<FileInfo> monitor;

        /// <summary>
        /// Loads the routerdb into ram and starts the file monitor that automatically checks
        /// for updates in the routerdb and reloads when necessary.
        /// </summary>
        public static void initialize(IConfiguration configuration)
        {
            var paths = configuration.GetSection("Paths");
            using (var stream = new FileInfo(paths.GetValue<string>("RouterdbFile")).OpenRead())
            {
                routerDb = RouterDb.Deserialize(stream);
            }
            router = new Router(routerDb);
            monitor = new FilesMonitor<FileInfo>((f) =>
            {
                using (var stream = new FileInfo(paths.GetValue<string>("RouterdbFile")).OpenRead())
                {
                    routerDb = RouterDb.Deserialize(stream);
                }
                router = new Router(routerDb);
                return true;
            }, new FileInfo(paths.GetValue<string>("RouterdbFile")));
            monitor.Start();
            monitor.AddFile(paths.GetValue<string>("RouterdbFile"));
        }

        /// <summary>
        /// Calculate a route.
        /// </summary>
        /// <param name="profileName">Name of the profile to use.</param>
        /// <param name="from">The starting coordinate.</param>
        /// <param name="to">The ending coordinate.</param>
        /// <returns>A Route object with a route between the two points</returns>
        /// <exception cref="ResolveException">
        /// Thrown when one of the points could not be resolved or when there is no path between the points.!--
        /// </exception>
        public static Route Calculate(string profileName, Coordinate from, Coordinate to)
        {
            Vehicle vehicle = router.Db.GetSupportedVehicle("bicycle");
            int dist = 50;
            var point1 = router.TryResolve(vehicle.Profile(profileName), from, dist);
            while (point1.IsError && dist < 1600)
            {
                dist *= 2;
                point1 = router.TryResolve(vehicle.Profile(profileName), from, dist);
            }
            if (point1.IsError)
            {
                throw new ResolveException("Location 1 could not be resolved");
            }
            dist = 50;
            var point2 = router.TryResolve(vehicle.Profile(profileName), to, dist);
            while (point2.IsError && dist < 1600)
            {
                dist *= 2;
                point2 = router.TryResolve(vehicle.Profile(profileName), from, dist);
            }
            if (point2.IsError)
            {
                throw new ResolveException("Location 2 could not be resolved");
            }
            var result = router.TryCalculate(vehicle.Profile(profileName), point1.Value, point2.Value);
            if (result.IsError)
            {
                throw new ResolveException("No path found between locations");
            }

            return result.Value;
        }

        /// <summary>
        /// Calculate a route.
        /// </summary>
        /// <param name="RouteObj">Name of the profile to use.</param>
        /// <param name="language">The starting coordinate.</param>
        /// <returns>A GeoJsonFeatureCollection object representing the instructions.</returns>
        public static GeoJsonFeatureCollection GenerateInstructions(Route RouteObj, string language = "en")
        {
            IList<Instruction> rawInstructions;
            rawInstructions = RouteObj.GenerateInstructions(routerDb, Languages.GetLanguage(language));
            rawInstructions = rawInstructions.makeContinuous(RouteObj);
            rawInstructions = rawInstructions.simplify(RouteObj);
            try{
                RouteObj.correctColours(rawInstructions);
            } catch {} // If the collourcorrection fails, return the route anyway
            return rawInstructions.ToGeoJsonCollection(RouteObj);
        }
    }
}