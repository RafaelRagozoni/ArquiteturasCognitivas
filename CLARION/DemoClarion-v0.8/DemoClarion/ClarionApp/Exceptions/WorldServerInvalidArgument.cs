using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClarionApp.Exceptions
{
    [Serializable()]
    public class WorldServerInvalidArgument : System.Exception
    {
        public WorldServerInvalidArgument() : base() { }
        public WorldServerInvalidArgument(string message) : base(message) { }
        public WorldServerInvalidArgument(string message, System.Exception inner) : base(message, inner) { }

        // A constructor is needed for serialization when an
        // exception propagates from a remoting server to the client. 
        protected WorldServerInvalidArgument(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) { }
    }
}



