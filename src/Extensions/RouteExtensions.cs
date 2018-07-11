using System.Collections.Generic;
using Itinero;
using Itinero.Navigation.Instructions;

namespace rideaway_backend.Extensions {
    public static class RouteExtensions {
        /// <summary>
        /// Correct the colours of the cyclenetwork on the route so there is only one colour
        /// present that matches the colours in the instructions
        /// </summary>
        /// <param name="Route">The route object</param>
        /// <param name="instructions">The list of instructions to get the colours from</param>
        public static void correctColours (this Route Route, IList<Instruction> instructions) {
            int instructionIndex = 0;
            Instruction currentInstruction = instructions[instructionIndex];

            for (var i = 0; i < Route.ShapeMeta.Length; i++) {
                int currentShape = Route.ShapeMeta[i].Shape;
                if (currentShape == currentInstruction.Shape) {

                    instructionIndex++;
                    if (instructionIndex < instructions.Count - 1) {
                        currentInstruction = instructions[instructionIndex];
                    }
                }
                if (i < Route.ShapeMeta.Length - 1) {
                    Route.ShapeMeta[i + 1].Attributes.AddOrReplace ("colour", currentInstruction.GetAttribute ("cyclecolour", Route));
                }

            }
        }

    }
}