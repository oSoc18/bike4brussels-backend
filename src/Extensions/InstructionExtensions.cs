using System.Collections.Generic;
using System.Text.RegularExpressions;
using Itinero;
using Itinero.Navigation.Instructions;
using rideaway_backend.Model;

namespace rideaway_backend.Extensions {
    /// <summary>
    /// Extensions for instructions to set and get attributes of the corresponding metadata.
    /// </summary>
    public static class InstructionExtensions {
        /// <summary>
        /// Get an attribute value from the metadata corresponding to the instruction.
        /// </summary>
        /// <param name="instruction">Instruction to get the attribute from.</param>
        /// <param name="key">The key for the attribute value.</param>
        /// <param name="route">The route object that has the metadata for all the instructions.</param>
        /// <returns></returns>
        public static string GetAttribute (this Instruction instruction, string key, Route route) {
            string value;
            route.ShapeMetaFor (instruction.Shape).Attributes.TryGetValue (key, out value);
            return value;
        }

        /// <summary>
        /// Set an attribute in the metadata corresponding to the instruction.
        /// </summary>
        /// <param name="instruction">Instruction to set the attribute of.</param>
        /// <param name="key">The key for the attribute.</param>
        /// <param name="value">The value to set for the attribute.</param>
        /// <param name="route">The route object that has the metadata for all the instructions</param>
        public static void SetAttribute (this Instruction instruction, string key, string value, Route route) {
            route.ShapeMetaFor (instruction.Shape).Attributes.AddOrReplace (key, value);
        }

        /// <summary>
        /// Converts a list of instructions to a geojson featurecollection.
        /// </summary>
        /// <param name="instructions">instructions to convert</param>
        /// <param name="Route">the route</param>
        /// <returns>a geojson featurecollection containing the instructions</returns>
        public static GeoJsonFeatureCollection ToGeoJsonCollection (this IList<Instruction> instructions, Route Route) {
            IList<InstructionProperties> InstructionProps = new List<InstructionProperties> ();
            Instruction Previous = null;
            foreach (Instruction instruction in instructions) {
                if (Previous == null) {
                    Previous = instruction;
                } else {
                    InstructionProps.Add (new InstructionProperties (Previous, instruction, Route));
                    Previous = instruction;
                }
            }
            InstructionProps.Add (new InstructionProperties (Previous, null, Route));

            return new GeoJsonFeatureCollection (InstructionProps);
        }

        /// <summary>
        /// Makes the instructions continuous by removing gaps, these gaps are likely caused by 
        /// incorrect osm data.
        /// </summary>
        /// <param name="instructions">instructions to make continuous</param>
        /// <param name="Route">the route</param>
        /// <returns>list of continuous instructions</returns>
        public static IList<Instruction> makeContinuous (this IList<Instruction> instructions, Route Route) {
            IList<Instruction> continuous = new List<Instruction> ();
            continuous.Add (instructions[0]);
            if (instructions[1].GetAttribute ("cycleref", Route) == null) {
                instructions[1].Type = "enter";
            }
            continuous.Add (instructions[1]);

            for (var i = 2; i < instructions.Count - 2; i++) {
                if (instructions[i].GetAttribute ("cycleref", Route) != null) {
                    continuous.Add (instructions[i]);
                }
            }
            if (instructions.Count >= 3) {
                if (instructions.Count >= 4) {
                    if (instructions[instructions.Count - 1].GetAttribute ("cycleref", Route) == null) {
                        instructions[instructions.Count - 2].Type = "leave";
                    }
                    continuous.Add (instructions[instructions.Count - 2]);
                }
                continuous.Add (instructions[instructions.Count - 1]);
            }
            return continuous;
        }

        /// <summary>
        /// Simplifies instructions by removing unnecessary instructions and setting the 
        /// reference and colour attributes to one element (sometimes routes overlap).
        /// </summary>
        /// <param name="instructions">the instructions to simplify</param>
        /// <param name="Route">the route</param>
        /// <returns>list of simplified instructions</returns>
        public static IList<Instruction> simplify (this IList<Instruction> instructions, Route Route) {
            IList<Instruction> simplified = new List<Instruction> ();
            string currentRef = null;
            string currentColour = null;
            var c = 0;
            while (instructions[c].Type != "turn") {
                simplified.Add (instructions[c]);
                c++;
            }
            Instruction previous = null;
            Instruction ins = instructions[c];
            while (ins.GetAttribute ("cycleref", Route) != null && c != instructions.Count) {
                if (currentRef == null) {
                    string refs = ins.GetAttribute ("cycleref", Route);
                    string colours = ins.GetAttribute ("cyclecolour", Route);
                    if (refs != null) {
                        currentRef = refs.Split (',')[0];
                        ins.SetAttribute ("cycleref", currentRef, Route);
                        if (colours != null) {
                            currentColour = colours.Split (',')[0];
                        }
                        ins.SetAttribute ("cyclecolour", currentColour, Route);
                        previous = ins;
                    }
                } else {
                    string refs = ins.GetAttribute ("cycleref", Route);
                    string colours = ins.GetAttribute ("cyclecolour", Route);
                    //create regex to check if current ref is contained in the string
                    Regex reg = new Regex (@"^" + currentRef + ",|^" + currentRef + "$|," + currentRef + ",|," + currentRef + "$");
                    if (refs != null && !reg.IsMatch (refs)) {
                        previous.SetAttribute ("cycleref", currentRef, Route);
                        previous.SetAttribute ("cyclecolour", currentColour, Route);
                        currentRef = refs.Split (',')[0];
                        if (colours != null) {
                            currentColour = colours.Split (',')[0];
                        }

                        simplified.Add (previous);
                    }
                }
                previous = ins;
                c++;
                if (c < instructions.Count) {
                    ins = instructions[c];
                }
            }

            if (instructions.Count >= 3) {
                if (instructions.Count >= 4) {
                    previous.SetAttribute ("cycleref", currentRef, Route);
                    previous.SetAttribute ("cyclecolour", currentColour, Route);
                    simplified.Add (previous);

                }
                //if there is a leave instruction add the last instruction as it is not yet added
                if (instructions[instructions.Count - 2].Type == "leave") {
                    simplified.Add (instructions[instructions.Count - 1]);
                }
            }

            return simplified;
        }
    }
}