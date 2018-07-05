using System.Collections.Generic;

namespace rideaway_backend.Model {
    /// <summary>
    /// Collection of GeoJsonFeatures.
    /// </summary>
    public class GeoJsonFeatureCollection {
        public string type { get; set; }
        public IList<InstructionProperties> features { get; set; }

        /// <summary>
        /// Constructor of the GeoJsonFeatureCollection.
        /// </summary>
        /// <param name="features">The features of the collection.</param>
        public GeoJsonFeatureCollection (IList<InstructionProperties> features) {
            this.type = "FeatureCollection";
            this.features = features;
        }

    }
}