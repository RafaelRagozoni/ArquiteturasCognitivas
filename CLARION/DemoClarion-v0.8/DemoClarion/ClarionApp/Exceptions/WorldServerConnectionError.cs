using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClarionApp.Exceptions
{
    [Serializable()]
    public class WorldServerConnectionError : System.Exception
    {
        public WorldServerConnectionError() : base() { }
        public WorldServerConnectionError(string message) : base(message) { }
        public WorldServerConnectionError(string message, System.Exception inner) : base(message, inner) { }

        // A constructor is needed for serialization when an
        // exception propagates from a remoting server to the client. 
        protected WorldServerConnectionError(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) { }
    }
}



