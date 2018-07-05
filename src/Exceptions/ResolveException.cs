namespace rideaway_backend.Exceptions {
    /// <summary>
    /// Exception to throw when the starting or ending points could not be resolved or when there is 
    /// no path between them.
    /// </summary>
    public class ResolveException : System.Exception {
        public ResolveException () : base () { }
        public ResolveException (string message) : base (message) { }
        public ResolveException (string message, System.Exception inner) : base (message, inner) { }
    }
}