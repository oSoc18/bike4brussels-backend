namespace rideaway_backend.Exceptions {
    /// <summary>
    /// Exception to throw when a part of a coordinate could not be parsed to a float.
    /// </summary>
    public class CoordinateParseException : System.Exception {
        public CoordinateParseException () : base () { }
        public CoordinateParseException (string message) : base (message) { }
        public CoordinateParseException (string message, System.Exception inner) : base (message, inner) { }
    }
}