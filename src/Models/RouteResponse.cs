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
        private IList<Instruction> rawInstructions;

        public JObject Route { get; set; }

        public GeoJsonFeatureCollection Instructions { get; set; }

        /// <summary>
        /// Constructor of a RouteResponse
        /// </summary>
        /// <param name="RouteObj">The route object.</param>
        /// <param name="colourCorrection">Indicates if the colours need to be corrected.</param>
        /// <param name="instructions">Indicates if instructions need to be generated.</param>
        /// <param name="language">The language of the instructions.</param>
        public RouteResponse (Route RouteObj, bool colourCorrection, bool instructions, string language = "en") {
            this.RouteObj = RouteObj;


            if (instructions)
            {
                rawInstructions = RouteObj.GenerateInstructions(Languages.GetLanguage(language));
                rawInstructions = rawInstructions.makeContinuous(RouteObj);
                rawInstructions = rawInstructions.simplify(RouteObj);
                if (colourCorrection)
                {
                    RouteObj.correctColours(rawInstructions);
                }
                Instructions = rawInstructions.ToGeoJsonCollection(RouteObj);
            }


            Route = JObject.Parse (RouteObj.ToGeoJson ());
        }

    }
}