using System.Collections.Generic;
using Itinero;
using Itinero.Navigation.Instructions;
using Newtonsoft.Json.Linq;
using rideaway_backend.Extensions;
using rideaway_backend.Instance;

namespace rideaway_backend.Model {
    /// <summary>
    /// Model of the response of the api.
    /// </summary>
    public class RouteResponse {
        private Route RouteObj;

        public JObject Route { get; set; }

        public GeoJsonFeatureCollection Instructions { get; set; }

        /// <summary>
        /// Constructor of a RouteResponse
        /// </summary>
        /// <param name="RouteObj">The route object.</param>
        /// <param name="Instructions">The instructions that go with this route.</param>
        public RouteResponse (Route RouteObj, GeoJsonFeatureCollection Instructions) {
            this.RouteObj = RouteObj;
            this.Instructions = Instructions;

            Route = JObject.Parse (RouteObj.ToGeoJson ());
        }

    }
}