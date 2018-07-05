using Itinero.LocalGeo;

namespace rideaway_backend.Model {
    /// <summary>
    /// Class to represent points in the geoJSON format.
    /// </summary>
    public class GeoJsonPoint {
        public string type { get; set; }
        public float[] Coordinates { get; set; }

        /// <summary>
        /// Constructor of the GeoJsonPoint.
        /// </summary>
        /// <param name="Coordinate">Coordinate of the point.</param>
        public GeoJsonPoint (Coordinate Coordinate) {
            this.type = "Point";
            this.Coordinates = new float[] { Coordinate.Longitude, Coordinate.Latitude };
        }
    }
}