using System;
using System.Collections.Generic;
using System.IO;
using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;
using Itinero.LocalGeo;
using Newtonsoft.Json;
using rideaway_backend.Structure;
using Microsoft.Extensions.Configuration;
using rideaway_backend.Util;

namespace rideaway_backend.Instance {
    public class ParkingInstance {
        private static ParkingMap parkings;

        /// <summary>
        /// Get a FeatureCollection of the bicycle parkings in the radius of the given location.
        /// </summary>
        /// <param name="location">location</param>
        /// <param name="radius">radius in meters</param>
        /// <returns>FeatureCollection containing the parkings in the radius</returns>
        public static FeatureCollection getParkings (Coordinate location, int radius) {
            Position lambert = Utility.WSG84toLambert72 (location.Latitude, location.Longitude);
            List<Feature> inside = parkings.RadiusFeatures (lambert.Longitude, lambert.Latitude, radius);
            List<Feature> insideWSG = new List<Feature> ();
            foreach (Feature f in inside) {
                Point point = (Point) f.Geometry;
                Point newPoint = new Point (Utility.Lambert72toWGS84 (point.Coordinates.Longitude, point.Coordinates.Latitude));
                Feature newFeature = new Feature (newPoint, f.Properties, f.Id);
                insideWSG.Add (newFeature);
            }
            return new FeatureCollection (insideWSG);
        }

        /// <summary>
        /// Initialize the ParkingInstance.
        /// </summary>
        public static void initialize (IConfiguration configuration) {
            parkings = new ParkingMap ();

            string json = File.ReadAllText (configuration.GetSection("Paths").GetValue<string>("BicycleParkingsFile"));
            FeatureCollection raw = JsonConvert.DeserializeObject<FeatureCollection> (json);
            foreach (Feature f in raw.Features) {
                string type = f.Properties["type_nl"].ToString ();
                if (type == "Gegroepeerde bogen" || type == "Geïsoleerde boog") {
                    parkings.Add (f);
                }
            }
        }
    }

}