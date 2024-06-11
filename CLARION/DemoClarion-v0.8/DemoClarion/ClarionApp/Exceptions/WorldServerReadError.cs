using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClarionApp.Exceptions
{
    [Serializable()]
    public class WorldServerReadError : System.Exception
    {
        public WorldServerReadError() : base() { }
        public WorldServerReadError(string message) : base(message) { }
        public WorldServerReadError(string message, System.Exception inner) : base(message, inner) { }

        // A constructor is needed for serialization when an
        // exception propagates from a remoting server to the client. 
        protected WorldServerReadError(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) { }
    }
}



