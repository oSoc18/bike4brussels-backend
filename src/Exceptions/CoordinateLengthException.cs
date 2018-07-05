namespace rideaway_backend.Exceptions {
    /// <summary>
    /// Exception to throw when a Coordinate could not be parsed because of an incorrect format.
    /// </summary>
    public class CoordinateLengthException : System.Exception {
        public CoordinateLengthException () : base () { }
        public CoordinateLengthException (string message) : base (message) { }
        public CoordinateLengthException (string message, System.Exception inner) : base (message, inner) { }

    }

}