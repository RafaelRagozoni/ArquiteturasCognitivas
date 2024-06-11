using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClarionApp.Exceptions
{
    [Serializable()]
    public class WorldServerErrorProcessingResponse : System.Exception
    {
        public WorldServerErrorProcessingResponse() : base() { }
        public WorldServerErrorProcessingResponse(string message) : base(message) { }
        public WorldServerErrorProcessingResponse(string message, System.Exception inner) : base(message, inner) { }

        // A constructor is needed for serialization when an
        // exception propagates from a remoting server to the client. 
        protected WorldServerErrorProcessingResponse(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) { }
    }
}



