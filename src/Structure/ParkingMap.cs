using System;
using System.Collections.Generic;
using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;

namespace rideaway_backend.Structure {
    /// <summary>
    /// Data structure for the parkings, uses a very simple and inefficient way to 
    /// find elements in radius. Would be better to use some kind of spatial index
    /// for this, but could not find any that were compatible.
    /// </summary>
    public class ParkingMap {
        private IList<Feature> features;

        public ParkingMap () {
            features = new List<Feature> ();
        }

        /// <summary>
        /// Add a feature to the map
        /// </summary>
        /// <param name="f">Feature to add</param>
        public void Add (Feature f) {
            features.Add (f);
        }

        /// <summary>
        /// Get all features in a radius around the given x and y coordinates.
        /// </summary>
        /// <param name="x">x coordinate</param>
        /// <param name="y">y coordinate</param>
        /// <param name="radius">radius</param>
        /// <returns></returns>
        public List<Feature> RadiusFeatures (double x, double y, int radius) {

            List<Feature> inside = new List<Feature> ();
            foreach (Feature f in features) {
                if (Distance (f, x, y) < radius) {

                    inside.Add (f);
                }
            }
            return inside;

        }

        /// <summary>
        /// Calculate distance between feature and x, y coordinate
        /// </summary>
        /// <param name="f">Feature</param>
        /// <param name="x">x coordinate</param>
        /// <param name="y">y coordinate</param>
        /// <returns></returns>
        public double Distance (Feature f, double x, double y) {
            Point p = (Point) f.Geometry;
            return Math.Sqrt (Math.Pow (p.Coordinates.Longitude - x, 2) + Math.Pow (p.Coordinates.Latitude - y, 2));
        }

    }
}