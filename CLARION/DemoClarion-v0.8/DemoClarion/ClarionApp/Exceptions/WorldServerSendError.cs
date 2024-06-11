using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClarionApp.Exceptions
{
    [Serializable()]
    public class WorldServerSendError : System.Exception
    {
        public WorldServerSendError() : base() { }
        public WorldServerSendError(string message) : base(message) { }
        public WorldServerSendError(string message, System.Exception inner) : base(message, inner) { }

        // A constructor is needed for serialization when an
        // exception propagates from a remoting server to the client. 
        protected WorldServerSendError(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) { }
    }
}



