using System.Collections.Generic;
using System.Globalization;
using Itinero;
using Itinero.Navigation.Directions;
using Itinero.Navigation.Instructions;
using rideaway_backend.Extensions;

namespace rideaway_backend.Model {
    /// <summary>
    /// Model for an instruction together with its properties in the geoJSON format.
    /// </summary>
    public class InstructionProperties {

        public string type { get; set; }

        public Dictionary<string, string> properties { get; set; }
        public GeoJsonPoint geometry { get; set; }

        private Instruction Instruction { get; set; }

        private DirectionEnum Direction { get; set; }

        private RelativeDirection Angle { get; set; }

        /// <summary>
        /// Constructor of the InstructionProperties.
        /// </summary>
        /// <param name="Instruction">The instruction to convert to a GeoJsonFeature.</param>
        /// <param name="Next">The next instruction.</param>
        /// <param name="route">The Route object.</param>
        public InstructionProperties (Instruction Instruction, Instruction Next, Route route) {
            this.type = "Feature";
            this.properties = new Dictionary<string, string> ();
            this.geometry = new GeoJsonPoint (route.Shape[Instruction.Shape]);
            this.Instruction = Instruction;

            properties.Add ("instruction", Instruction.Text);
            Route.Meta meta = route.ShapeMetaFor (Instruction.Shape);

            properties.Add ("colour", Instruction.GetAttribute ("cyclecolour", route));
            properties.Add ("ref", Instruction.GetAttribute ("cycleref", route));

            float time;
            float dist;
            route.DistanceAndTimeAt (Instruction.Shape, out dist, out time);
            properties.Add ("distance", dist.ToString (new CultureInfo ("en-US")));

            properties.Add ("type", Instruction.Type);

            if (Next != null) {
                Route.Meta nextMeta = route.ShapeMetaFor (Next.Shape);
                properties.Add ("nextColour", Next.GetAttribute ("cyclecolour", route));
                properties.Add ("nextRef", Next.GetAttribute ("cycleref", route));
                
                if (Instruction.Type != "start" && Instruction.Type != "stop") {
                    this.Angle = route.RelativeDirectionAt (Instruction.Shape);
                    properties.Add ("angle", Angle.Direction.ToString ());
                }
            }
        }
    }
}